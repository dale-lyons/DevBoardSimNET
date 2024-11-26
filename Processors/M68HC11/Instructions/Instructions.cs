using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using Processors;

namespace M68HC11
{
    public partial class M68HC11
    {
        public delegate bool loadStoreDelegate(ref ushort b, int cycles, int bytes = 0);

        //private byte InteruptMask;
        public bool IsHalted { get; private set; }

        protected delegate uint ExecuteInstruction(byte opcode);

        private ExecuteInstruction[] mOpcodeFunctions = new ExecuteInstruction[256];
        //private Tuple<portAccessReadEventHandler, portAccessWriteEventHandler>[] mPortOverrides;
        //private codeAccessReadEventHandler[] mCodeOverrides;

        private void setFlagH(byte a, byte b, byte r)
        {
            RegistersM68HC11.HalfC = (a.Bit3() && b.Bit3())
                                 || (b.Bit3() && !r.Bit3())
                                 || (!r.Bit3() && a.Bit3());

        }
        private void setFlagN(byte r)
        {
            RegistersM68HC11.Negative = r.Bit7();
        }
        private void setFlagZ(byte r)
        {
            RegistersM68HC11.Zero = (r == 0);
        }
        private void setFlagV(byte a, byte b, byte r)
        {
            RegistersM68HC11.Overflow = (a.Bit7() && !b.Bit7() && !r.Bit7()) ||
                                        (!a.Bit7() && b.Bit7() && r.Bit7());
        }
        private void setFlagC(byte a, byte b, byte r)
        {
            RegistersM68HC11.Carry = (a.Bit7() && b.Bit7())
                                  || (b.Bit7() && !r.Bit7()
                                  ||(!r.Bit7() && a.Bit7()));
        }

        private void setFlagsNZ(byte r)
        {
            setFlagN(r);
            setFlagZ(r);
        }

        private void setFlagsNZVC(byte a, byte b, byte r)
        {
            setFlagN(r);
            setFlagZ(r);
            setFlagV(a, b, r);
            setFlagC(a, b, r);
        }
        private void setFlagsHNZVC(byte a, byte b, byte r)
        {
            setFlagsNZVC(a, b, r);
            setFlagH(a, b, r);
        }

        private void setFlagsNZVC16(ushort d, ushort m, ushort r)
        {
            RegistersM68HC11.Negative = r.Bit15();
            RegistersM68HC11.Zero = (r == 0);

            RegistersM68HC11.Overflow = (d.Bit15() && !m.Bit15() && !r.Bit15()) ||
                                       (!d.Bit15() && m.Bit15() && r.Bit15());

            RegistersM68HC11.Carry = ( (!d.Bit15() && m.Bit15())
                                   ||  (m.Bit15() && r.Bit15())
                                   ||  (r.Bit15() && !d.Bit15()) );
        }

        private byte rol(byte a)
        {
            bool c = RegistersM68HC11.Carry;
            RegistersM68HC11.Carry = a.Bit7();
            a <<= 1;
            if (c)
                a |= 0x01;
            return a;
        }

        private byte ror(byte a)
        {
            bool c = RegistersM68HC11.Carry;
            RegistersM68HC11.Carry = a.Bit0();
            a >>= 1;
            if (c)
                a |= 0x80;
            return a;
        }

        private byte asl(byte a)
        {
            RegistersM68HC11.Carry = a.Bit7();
            return (byte)(a << 1);
        }
        public byte lsr(byte a)
        {
            RegistersM68HC11.Carry = a.Bit0();
            return (byte)(a >> 1);
        }

        private byte asr(byte a)
        {
            RegistersM68HC11.Carry = a.Bit0();
            bool msb = a.Bit7();
            a = (byte)(a << 1);
            if (msb)
                a |= 0x80;
            return a;
        }

        private byte neg(byte a)
        {
            return (byte)(0 - a);
        }

        public uint Wai(byte opcode)
        {
            return 0;
        }

        public uint Txs(byte opcode)
        {
            if (preByte == 0)
                RegistersM68HC11.SP = (ushort)(RegistersM68HC11.X + 1);
            if (preByte == 0x18)
                RegistersM68HC11.SP = (ushort)(RegistersM68HC11.Y + 1);
            return 3;
        }

        public uint Tsx(byte opcode)
        {
            if (preByte == 0)
                RegistersM68HC11.X = (ushort)(RegistersM68HC11.SP + 1);
            if (preByte == 0x18)
                RegistersM68HC11.Y = (ushort)(RegistersM68HC11.SP + 1);
            return 3;
        }

        public uint Test(byte opcode)
        {
            return 0;
        }

        public uint Tsta(byte opcode)
        {
            setFlagsNZ(RegistersM68HC11.A);
            return 2;
        }
        public uint Tstb(byte opcode)
        {
            setFlagsNZ(RegistersM68HC11.B);
            return 2;
        }

        public uint Tst(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte r = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            setFlagsNZ(r);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Suba(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = (byte)(RegistersM68HC11.A - data);
            RegistersM68HC11.A = r;
            setFlagsNZ((byte)RegistersM68HC11.A);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                                        (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Swi(byte opcode)
        {

            return 14;
        }

        public uint Subb(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = (byte)(RegistersM68HC11.B - data);
            RegistersM68HC11.B = r;
            setFlagsNZ((byte)RegistersM68HC11.B);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                                        (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Sbca(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = (byte)(RegistersM68HC11.A - data);
            if (RegistersM68HC11.Carry)
                r--;
            RegistersM68HC11.A = r;
            setFlagsNZ((byte)RegistersM68HC11.A);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                                        (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Sbcb(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = (byte)(RegistersM68HC11.B - data);
            if (RegistersM68HC11.Carry)
                r--;
            RegistersM68HC11.B = r;
            setFlagsNZ((byte)RegistersM68HC11.B);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                                        (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Sba(byte opcode)
        {
            RegistersM68HC11.A -= RegistersM68HC11.B;
            setFlagsNZ(RegistersM68HC11.A);
            return 2;
        }

        public uint Rts(byte opcode)
        {
            RegistersM68HC11.PC = PopW();
            return 5;
        }

        public uint Rti(byte opcode)
        {
            return 12;
        }

        public uint Ror(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = ror((byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true));
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            setFlagsNZ(data);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                            (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint RorA(byte opcode)
        {
            RegistersM68HC11.A = ror(RegistersM68HC11.A);
            setFlagsNZ(RegistersM68HC11.A);
            return 2;
        }

        public uint RorB(byte opcode)
        {
            RegistersM68HC11.B = ror(RegistersM68HC11.B);
            setFlagsNZ(RegistersM68HC11.B);
            return 2;
        }

        public uint Psha(byte opcode)
        {
            Push(RegistersM68HC11.A);
            return 3;
        }
        public uint Pshb(byte opcode)
        {
            Push(RegistersM68HC11.B);
            return 3;
        }
        public uint Pshx(byte opcode)
        {
            if (preByte == 0)
                Push(RegistersM68HC11.X);
            else if (preByte == 0x18)
                Push(RegistersM68HC11.Y);
            return (preByte == 0) ? (uint)4 : (uint)5;
        }

        public uint Nop(byte opcode)
        {
            return 2;
        }

        public uint NegA(byte opcode)
        {
            RegistersM68HC11.A = neg(RegistersM68HC11.A);
            setFlagsNZ(RegistersM68HC11.A);
            return 2;
        }
        public uint NegB(byte opcode)
        {
            RegistersM68HC11.B = neg(RegistersM68HC11.B);
            setFlagsNZ(RegistersM68HC11.B);
            return 2;
        }

        public uint Neg(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = neg((byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true));
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Mul(byte opcode)
        {
            RegistersM68HC11.D = (ushort)(RegistersM68HC11.A * RegistersM68HC11.B);
            //ToDo: carry flag?
            return 10;
        }

        public uint Idiv(byte opcode)
        {
            RegistersM68HC11.X = (ushort)(RegistersM68HC11.D / RegistersM68HC11.X);
            ushort remainder = (ushort)(RegistersM68HC11.D % RegistersM68HC11.X);
            RegistersM68HC11.D = remainder;
            return 41;
        }

        public uint Fdiv(byte opcode)
        {
            double res = (RegistersM68HC11.D / RegistersM68HC11.X);
            ushort remainder = (ushort)(RegistersM68HC11.D % RegistersM68HC11.X);
            RegistersM68HC11.X = (ushort)(res * 100000);
            RegistersM68HC11.D = remainder;
            return 41;
        }

        public uint Oraa(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersM68HC11.A |= data;
            setFlagsNZ(RegistersM68HC11.A);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Orab(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersM68HC11.B |= data;
            setFlagsNZ(RegistersM68HC11.B);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint EorA(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersM68HC11.A ^= data;
            setFlagsNZ(RegistersM68HC11.A);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint EorB(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersM68HC11.B ^= data;
            setFlagsNZ(RegistersM68HC11.B);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Des(byte opcode)
        {
            RegistersM68HC11.SP--;
            return 3;
        }
        public uint Ins(byte opcode)
        {
            RegistersM68HC11.SP++;
            return 3;
        }
        public uint Inx(byte opcode)
        {
            if (preByte == 0)
            {
                RegistersM68HC11.X++;
                RegistersM68HC11.Zero = (RegistersM68HC11.X == 0);
            }
            else if (preByte == 0x18)
            {
                RegistersM68HC11.Y++;
                RegistersM68HC11.Zero = (RegistersM68HC11.Y == 0);
            }
            return 3;
        }
        public uint Dex(byte opcode)
        {
            if (preByte == 0)
            {
                RegistersM68HC11.X--;
                RegistersM68HC11.Zero = (RegistersM68HC11.X == 0);
            }
            else if (preByte == 0x18)
            {
                RegistersM68HC11.Y--;
                RegistersM68HC11.Zero = (RegistersM68HC11.Y == 0);
            }
            return 3;
        }

        public uint Bset(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte mask = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC + 1, WordSize.OneByte, false);
            data = (byte)(data | mask);
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint BitA(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = (byte)(RegistersM68HC11.A & data);
            setFlagsNZ(r);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint BitB(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = (byte)(RegistersM68HC11.B & data);
            setFlagsNZ(r);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint IncA(byte opcode)
        {
            RegistersM68HC11.A++;
            setFlagsNZ(RegistersM68HC11.A);
            return 2;
        }
        public uint IncB(byte opcode)
        {
            RegistersM68HC11.B++;
            setFlagsNZ(RegistersM68HC11.B);
            return 2;
        }

        public uint Inc(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            data++;
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            setFlagsNZ(data);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Dec(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            data--;
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            setFlagsNZ(data);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Bclr(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte mask = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC + 1, WordSize.OneByte, false);
            data = (byte)(data & mask);
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            setFlagsNZ(data);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }
        public uint DecA(byte opcode)
        {
            RegistersM68HC11.A--;
            setFlagsNZ(RegistersM68HC11.A);
            return 2;
        }
        public uint DecB(byte opcode)
        {
            RegistersM68HC11.B--;
            setFlagsNZ(RegistersM68HC11.B);
            return 2;
        }

        public uint Bcc(byte opcode)
        {
            return performBranch(!RegistersM68HC11.Carry);
        }

        public uint Brn(byte opcode)
        {
            return 3;
        }

        public uint Bsr(byte opcode)
        {
            Push((ushort)(RegistersM68HC11.PC + 1));
            byte r = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC, WordSize.OneByte, false);
            RegistersM68HC11.PC = incrementPC((ushort)RegistersM68HC11.PC, r);
//            RegistersM68HC11.PC += (ushort)(offset+1);
            return 6;
        }

        public uint Bcs(byte opcode)
        {
            return performBranch(RegistersM68HC11.Carry);
        }
        public uint Bvc(byte opcode)
        {
            return performBranch(!RegistersM68HC11.Overflow);
        }
        public uint Bvs(byte opcode)
        {
            return performBranch(RegistersM68HC11.Overflow);
        }

        public uint Beq(byte opcode)
        {
            return performBranch(RegistersM68HC11.Zero);
        }

        public uint Bge(byte opcode)
        {
            bool branch = !(RegistersM68HC11.Negative ^ RegistersM68HC11.Overflow);
            return performBranch(branch);
        }

        public uint Bls(byte opcode)
        {
            bool branch = (RegistersM68HC11.Carry || RegistersM68HC11.Zero);
            return performBranch(branch);
        }

        public uint Blt(byte opcode)
        {
            bool branch = (RegistersM68HC11.Negative ^ RegistersM68HC11.Overflow);
            return performBranch(branch);
        }

        public uint Bmi(byte opcode)
        {
            return performBranch(RegistersM68HC11.Negative);
        }
        public uint Bpl(byte opcode)
        {
            return performBranch(!RegistersM68HC11.Negative);
        }
        public uint Bra(byte opcode)
        {
            return performBranch(true);
        }

        private ushort incrementPC(ushort pc, byte r)
        {
            return (ushort)(pc + (short)(r.SignExtend() + 1) & 0x0000ffff);
        }
        private uint performBranch(bool branch)
        {
            if (branch)
            {
                byte r = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC, WordSize.OneByte, false);
                if(r > 128)
                {

                }
                RegistersM68HC11.PC = incrementPC((ushort)RegistersM68HC11.PC, r);
                return 3;
            }
            RegistersM68HC11.PC += 1;
            return 3;
        }

        public uint Bne(byte opcode)
        {
            return performBranch(!RegistersM68HC11.Zero);
        }

        public uint Ble(byte opcode)
        {
            bool branch = (RegistersM68HC11.Zero || (RegistersM68HC11.Negative ^ RegistersM68HC11.Overflow));
            return performBranch(branch);
        }

        public uint Bgt(byte opcode)
        {
            bool branch = !(RegistersM68HC11.Zero || (RegistersM68HC11.Negative ^ RegistersM68HC11.Overflow));
            return performBranch(branch);
        }
        public uint Bhi(byte opcode)
        {
            bool branch = !(RegistersM68HC11.Carry || (RegistersM68HC11.Zero));
            return performBranch(branch);
        }

        public uint Asr(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = asr((byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true));
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            setFlagsNZ(data);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                            (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Rola(byte opcode)
        {
            RegistersM68HC11.A = rol(RegistersM68HC11.A);
            setFlagsNZ(RegistersM68HC11.A);
            return 2;
        }
        public uint Rolb(byte opcode)
        {
            RegistersM68HC11.B = rol(RegistersM68HC11.B);
            setFlagsNZ(RegistersM68HC11.B);
            return 2;
        }
        public uint Rol(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = rol((byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true));
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            setFlagsNZ(data);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                            (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint LsrD(byte opcode)
        {
            ushort d = RegistersM68HC11.D;
            RegistersM68HC11.Carry = ((d & 0x01) != 0);
            RegistersM68HC11.D = (ushort)(d >> 1);
            return 3;
        }

        public uint LsrA(byte opcode)
        {
            RegistersM68HC11.A = lsr(RegistersM68HC11.A);
            return 2;
        }

        public uint LsrB(byte opcode)
        {
            RegistersM68HC11.A = lsr(RegistersM68HC11.A);
            return 2;
        }

        public uint Lsr(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = lsr((byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true));
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            setFlagsNZ(data);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                            (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint AsrA(byte opcode)
        {
            RegistersM68HC11.A = asr(RegistersM68HC11.A);
            setFlagsNZ(RegistersM68HC11.A);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                                        (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            return 2;
        }

        public uint AsrB(byte opcode)
        {
            RegistersM68HC11.B = asr(RegistersM68HC11.B);
            setFlagsNZ(RegistersM68HC11.B);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                                        (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            return 2;
        }

        public uint AslA(byte opcode)
        {
            RegistersM68HC11.A = asl(RegistersM68HC11.A);
            setFlagsNZ(RegistersM68HC11.A);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                                        (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            return 2;
        }

        public uint AslB(byte opcode)
        {
            RegistersM68HC11.B = asl(RegistersM68HC11.B);
            setFlagsNZ(RegistersM68HC11.B);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                                        (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            return 2;
        }
        public uint AslD(byte opcode)
        {
            ushort a = RegistersM68HC11.D;
            RegistersM68HC11.Carry = a.Bit15();
            RegistersM68HC11.D = (ushort)(a << 1);
            return 3;
        }

        public uint Asl(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = asl((byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true));
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            setFlagsNZ(data);
            RegistersM68HC11.Overflow = (RegistersM68HC11.Negative & !RegistersM68HC11.Carry) ||
                            (!RegistersM68HC11.Negative & RegistersM68HC11.Carry);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Anda(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersM68HC11.A &= data;
            setFlagsNZ(RegistersM68HC11.A);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Andb(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersM68HC11.B &= data;
            setFlagsNZ(RegistersM68HC11.B);
            RegistersM68HC11.Overflow = false;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Clc(byte opcode)
        {
            RegistersM68HC11.Carry = false;
            return 2;
        }
        public uint Sec(byte opcode)
        {
            RegistersM68HC11.Carry = true;
            return 2;
        }

        public uint Cli(byte opcode)
        {
            RegistersM68HC11.IMask = false;
            return 2;
        }
        public uint Sei(byte opcode)
        {
            RegistersM68HC11.IMask = true;
            return 2;
        }

        public uint Cba(byte opcode)
        {
            byte a = RegistersM68HC11.A;
            byte b = RegistersM68HC11.B;
            byte r = (byte)(a - b);
            setFlagsNZVC(a, b, r);
            return 2;
        }
        public uint Daa(byte opcode)
        {
            byte uh = (byte)(RegistersM68HC11.A >> 4);
            byte lh = (byte)(RegistersM68HC11.A & 0x0f);
            bool hc = RegistersM68HC11.HalfC;
            byte toAdd = 0;
            bool setC = false;
            if (!RegistersM68HC11.Carry)
            {
                if (uh <= 9 && !hc && lh <= 9)
                {
                }
                else if (uh <= 8 && !hc && lh <= 15 && lh >= 10)
                {
                    toAdd = 0x06;
                }
                else if (uh <= 9 && hc && lh <= 3)
                {
                    toAdd = 0x06;
                }
                else if (uh <= 15 && uh >= 10 && !hc && lh <= 9)
                {
                    toAdd = 0x60;
                    setC = true;
                }
                else if (uh <= 15 && uh >= 9 && !hc && lh <= 15 && lh >= 10)
                {
                    toAdd = 0x66;
                    setC = true;
                }
                else if (uh <= 15 && uh >= 10 && hc && lh <= 3)
                {
                    toAdd = 0x66;
                    setC = true;
                }
            }
            else
            {
                if (uh <= 2 && !hc && lh <= 9)
                {
                    toAdd = 0x60;
                    setC = true;
                }
                else if (uh <= 2 && !hc && lh <= 15 && lh >= 10)
                {
                    toAdd = 0x66;
                    setC = true;
                }
                else if (uh <= 3 && hc && lh <= 3)
                {
                    toAdd = 0x66;
                    setC = true;
                }
            }
            if (setC)
                RegistersM68HC11.Carry = true;
            RegistersM68HC11.A += toAdd;
            return 2;
        }
        public uint Cmpa(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            ushort data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = (byte)(RegistersM68HC11.A - data);
            setFlagsNZVC(RegistersM68HC11.A, (byte)data, r);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }
        public uint Cmpb(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            ushort data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = (byte)(RegistersM68HC11.B - data);
            setFlagsNZVC(RegistersM68HC11.B, (byte)data, r);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }
        public uint Cpx(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            ushort data = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
            ushort r;
            if (preByte == 0)
                r = (ushort)(RegistersM68HC11.X - data);
            else if (preByte == 0xcd)
                r = (ushort)(RegistersM68HC11.Y - data);
            else
                throw new Exception();

            RegistersM68HC11.Negative = r.Bit15();
            RegistersM68HC11.Zero = (r == 0);
            //ToDo - overflow and carry
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Cpd(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            ushort data = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
            ushort r = (ushort)(RegistersM68HC11.D - data);
            setFlagsNZVC16(RegistersM68HC11.D, data, r);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint ComA(byte opcode)
        {
            RegistersM68HC11.A = (byte)~RegistersM68HC11.A;
            return 2;
        }
        public uint ComB(byte opcode)
        {
            RegistersM68HC11.B = (byte)~RegistersM68HC11.B;
            return 2;
        }

        public uint Com(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)(~SystemMemory.GetMemory(addr, WordSize.OneByte, true));
            SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Adda(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte b = RegistersM68HC11.A;
            RegistersM68HC11.A += data;
            setFlagsHNZVC(b, data, RegistersM68HC11.A);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }
        public uint Addb(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte b = RegistersM68HC11.B;
            RegistersM68HC11.B += data;
            setFlagsHNZVC(b, data, RegistersM68HC11.B);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }
        public uint Addd(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            ushort data = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
            RegistersM68HC11.D += data;
            RegistersM68HC11.Negative = RegistersM68HC11.D.Bit15();
            RegistersM68HC11.Zero = (RegistersM68HC11.D == 0);
            //ToDo - overflow and carry
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Adca(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersM68HC11.A += data;
            if (RegistersM68HC11.Carry)
                RegistersM68HC11.A++;
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        public uint Adcb(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersM68HC11.B += data;
            if (RegistersM68HC11.Carry)
                RegistersM68HC11.B++;
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        public uint Aba(byte opcode)
        {
            byte a = RegistersM68HC11.A;
            byte b = RegistersM68HC11.B;
            byte r = (byte)(a + b);
            RegistersM68HC11.A = r;
            setFlagsHNZVC(a, b, r);
            return 2;
        }
        public uint Abx(byte opcode)
        {
            RegistersM68HC11.X += RegistersM68HC11.B;
            return 3;
        }

        public uint Staa(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            SystemMemory.SetMemory(addr, WordSize.OneByte, RegistersM68HC11.A, true);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }
        public uint Stab(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            SystemMemory.SetMemory(addr, WordSize.OneByte, RegistersM68HC11.B, true);
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        public uint Std(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            SystemMemory.SetMemory(addr, WordSize.TwoByte, RegistersM68HC11.D, true);
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        public uint Stop(byte opcode)
        {
            this.IsHalted = true;
            return 2;
        }

        public uint Invalid(byte opcode)
        {
            OnInvalidInstruction?.Invoke(opcode, RegistersM68HC11.PC);
            return 0;
        }
        public uint XGDX(byte opcode)
        {
            if (preByte == 0)
            {
                ushort tmp = RegistersM68HC11.X;
                RegistersM68HC11.X = RegistersM68HC11.D;
                RegistersM68HC11.D = tmp;
                return 3;
            }
            else if (preByte == 0x18)
            {
                ushort tmp = RegistersM68HC11.Y;
                RegistersM68HC11.Y = RegistersM68HC11.D;
                RegistersM68HC11.D = tmp;
                return 4;
            }
            throw new Exception();
        }

        public uint Tba(byte opcode)
        {
            RegistersM68HC11.A = RegistersM68HC11.B;
            setFlagsNZ(RegistersM68HC11.B);
            RegistersM68HC11.Overflow = false;
            return 2;
        }

        public uint Tab(byte opcode)
        {
            RegistersM68HC11.B = RegistersM68HC11.A;
            setFlagsNZ(RegistersM68HC11.B);
            RegistersM68HC11.Overflow = false;
            return 2;
        }

        public uint Tap(byte opcode)
        {
            RegistersM68HC11.CCR = RegistersM68HC11.A;
            return 2;
        }
        public uint Tpa(byte opcode)
        {
            RegistersM68HC11.A = RegistersM68HC11.CCR;
            return 2;
        }

        public uint Pula(byte opcode)
        {
            RegistersM68HC11.A = Pop();
            return 3;
        }
        public uint Pulb(byte opcode)
        {
            RegistersM68HC11.B = Pop();
            return 3;
        }
        public uint Pulx(byte opcode)
        {
            if (preByte == 0)
                RegistersM68HC11.X = PopW();
            else if (preByte == 0x18)
                RegistersM68HC11.Y = PopW();

            return (preByte == 0) ? (uint)5 : 6;
        }


        public uint Clr(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            SystemMemory.SetMemory(addr, WordSize.OneByte, 0, true);
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        public uint Clv(byte opcode)
        {
            RegistersM68HC11.Overflow = false;
            return 2;
        }
        public uint Sev(byte opcode)
        {
            RegistersM68HC11.Overflow = true;
            return 2;
        }

        public uint ClrA(byte opcode)
        {
            RegistersM68HC11.A = 0;
            return 2;
        }
        public uint ClrB(byte opcode)
        {
            RegistersM68HC11.B = 0;
            return 2;
        }

        public uint Ldd(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            RegistersM68HC11.D = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        public uint Ldaa(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            RegistersM68HC11.A = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            setFlagsNZ(RegistersM68HC11.A);
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        public uint Ldab(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            RegistersM68HC11.B = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            setFlagsNZ(RegistersM68HC11.B);
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        public uint Lds(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            RegistersM68HC11.SP = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
            RegistersM68HC11.PC += bytes;
            return (uint)Cycles;
        }

        private void Push(byte b)
        {
            SystemMemory.SetMemory(RegistersM68HC11.SP--, WordSize.OneByte, b);
        }
        private byte Pop()
        {
            return (byte)SystemMemory.GetMemory(++RegistersM68HC11.SP, WordSize.OneByte, false);
        }
        private ushort PopW()
        {
            byte[] buff = new byte[2];
            buff[1] = (byte)SystemMemory.GetMemory(++RegistersM68HC11.SP, WordSize.OneByte, false);
            buff[0] = (byte)SystemMemory.GetMemory(++RegistersM68HC11.SP, WordSize.OneByte, false);
            return BitConverter.ToUInt16(buff, 0);
        }

        private void Push(ushort w)
        {
            byte[] bytes = BitConverter.GetBytes(w);
            Push(bytes[0]);
            Push(bytes[1]);
        }
        public uint Jsr(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            Push((ushort)(RegistersM68HC11.PC + bytes));
            RegistersM68HC11.PC = addr;
            return Cycles;
        }

        public uint Jmp(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            //ushort data = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
            RegistersM68HC11.PC = addr;
            return Cycles;
        }

        public uint Brset(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte mask = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC + 1, WordSize.OneByte, true);
            byte broffset = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC + 2, WordSize.OneByte, true);
            bool branch = ((data & mask) != 0);
            if (branch)
                RegistersM68HC11.PC += broffset;
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Brclr(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte mask = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC + 1, WordSize.OneByte, true);
            byte broffset = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC + 2, WordSize.OneByte, true);
            bool branch = ((data & mask) == 0);
            if (branch)
                RegistersM68HC11.PC += broffset;
            RegistersM68HC11.PC += 3;
            return Cycles;
        }

        public uint Ldx(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            ushort data = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
            if (preByte == 0)
                RegistersM68HC11.X = data;
            else if (preByte == 0x18)
                RegistersM68HC11.Y = data;
            else
                throw new Exception();
            RegistersM68HC11.PC += (uint)bytes;
            return (uint)Cycles;
        }

        public uint Stx(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            ushort data;
            if (preByte == 0)
                data = RegistersM68HC11.X;
            else if (preByte == 0x1a)
                data = RegistersM68HC11.Y;
            else
                throw new Exception();

            SystemMemory.SetMemory(addr, WordSize.TwoByte, data, true);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        public uint Sts(byte opcode)
        {
            ushort addr = loadAddressFromMode(opcode, out uint Cycles, out uint bytes);
            SystemMemory.SetMemory(addr, WordSize.TwoByte, RegistersM68HC11.SP, true);
            RegistersM68HC11.PC += bytes;
            return Cycles;
        }

        private ushort loadAddressFromMode(byte opcode, out uint Cycles, out uint bytes)
        {
            Cycles = 2;
            bytes = 1;

            switch (OpcodesAddressingModes.LookupAM(opcode, preByte))
            {
                case AM.INH:
                    return (ushort)RegistersM68HC11.PC;

                case AM.DIR:
                    {
                        ushort addr = (ushort)SystemMemory.GetMemory(RegistersM68HC11.PC, WordSize.OneByte, true);
                        return addr;
                    }
                case AM.INX:
                case AM.INY:
                    {
                        ushort addr = 0;
                        byte offset = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC, WordSize.OneByte, true);
                        if (preByte == 0x18)
                            addr = (ushort)(RegistersM68HC11.Y + offset);
                        else if (preByte == 0 || preByte == 0x1a)
                            addr = (ushort)(RegistersM68HC11.X + offset);
                        else
                            throw new Exception();

                        bytes = 1;
                        return addr;
                    }
                case AM.EXT:
                    {//extended (two bytes absolute address), read 8 bits of data
                        bytes = 2;
                        ushort addr = (ushort)SystemMemory.GetMemory(RegistersM68HC11.PC, WordSize.TwoByte, true);
                        return addr;
                    }
                case AM.IM1:
                    {//immediate, one/two byte(s)
                        Cycles = 3;
                        bytes = 1;
                        return (ushort)RegistersM68HC11.PC;
                    }
                case AM.IM2:
                    {//immediate, one/two byte(s)
                        Cycles = 3;
                        bytes = 2;
                        return (ushort)RegistersM68HC11.PC;
                    }
                default:
                    throw new Exception();
            }
        }
    }
}