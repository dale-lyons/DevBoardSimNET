using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace Intel8085
{
    public class Registers8085 : IRegisters
    {
        private byte[] mBuff2 = new byte[2];

        public byte A { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }

        private const byte SignFlag = 0x80;
        private const byte ZeroFlag = 0x40;
        private const byte KFlag = 0x20;
        private const byte AuxCarryFlag = 0x10;
        private const byte InvalidFlag = 0x08;
        private const byte ParityFlag = 0x04;
        private const byte OverflowFlag = 0x04;
        private const byte NegativeFlag = 0x02;
        private const byte CarryFlag = 0x01;

        public uint PC { get; set; }
        public ushort SP { get; set; }
        public ushort PSW
        {
            get
            {
                mBuff2[1] = A;
                mBuff2[0] = Flags;
                return BitConverter.ToUInt16(mBuff2, 0);
            }
            set
            {
                var bytes = BitConverter.GetBytes(value);
                A = bytes[1];
                Flags = bytes[0];
            }
        }

        public byte Flags
        {
            get
            {
                byte ret = (byte)(Sign ? SignFlag : 0);
                ret |= (byte)(Zero ? ZeroFlag : 0);
                ret |= (byte)(Negative ? NegativeFlag : 0);
                ret |= (byte)(AuxCarry ? AuxCarryFlag : 0);
                ret |= (byte)(Invalid ? InvalidFlag : 0);
                ret |= (byte)(Parity ? ParityFlag : 0);
                ret |= (byte)(Carry ? CarryFlag : 0);
                ret |= (byte)(K ? KFlag : 0);
                return ret;
            }
            set
            {
                Sign = (value & SignFlag) != 0;
                Zero = (value & ZeroFlag) != 0;
                Negative = (value & NegativeFlag) != 0;
                AuxCarry = (value & AuxCarryFlag) != 0;
                Invalid = (value & InvalidFlag) != 0;
                Parity = (value & ParityFlag) != 0;
                Carry = (value & CarryFlag) != 0;
                K = (value & KFlag) != 0;
            }
        }

        //sub C = (!a7 and b7) or (b7 and  r7) or (r7 and !a7)
        public void setFlagsubC(byte a, byte b, byte r)
        {
            Carry = (!a.Bit7() && b.Bit7())
                  || (b.Bit7() && r.Bit7())
                  || (!a.Bit7() && r.Bit7());

        }
        //no negative flag on the 8086
        public virtual void setFlagAuxCarry(bool state) { }
        public virtual void setFlagN(bool state) { }
        public virtual void setFlagsHN() { }

        public void setFlagaddC(byte a, byte b, byte r)
        {
            Carry = isFlagaddC(a, b, r);
        }
        //add C = (a7 and b7) or (b7 and !r7) or(!r7 and a7)
        public static bool isFlagaddC(byte a, byte b, byte r)
        {
            return (a.Bit7() && b.Bit7())
                 || (b.Bit7() && !r.Bit7())
                 || (a.Bit7() && !r.Bit7());
        }

        public void setFlagZ(byte r)
        {
            Zero = (r == 0);
        }
        public void setFlagS(byte r)
        {
            Sign = r.Bit7();
        }
        private static int countBits(byte result)
        {
            int sum = 0;
            while (result != 0)
            {
                if (result.Bit0())
                    sum++;
                result >>= 1;
            }
            return sum;
        }
        public virtual void setFlagP(byte r)
        {
            Parity = ((countBits(r) & 0x01) == 0);
        }

        public void setParityOverflow(byte r)
        {
            Parity = ((countBits(r) & 0x01) == 0);
        }


        public void setFlagsSZP(byte r)
        {
            setFlagS(r);
            setFlagZ(r);
            setFlagP(r);
        }


        public virtual void setFlagV(byte a, byte b, byte r, bool sub) { }

        //half_carry = ((a & 0xf) - (operand & 0xf)) & 0x10;
        public virtual void setFlagH(byte a, byte b, byte r, bool sub)
        {
            if (sub)
            {
                byte b1 = b.OnesComplement();
                AuxCarry = (!a.Bit3() &&  b1.Bit3() && !r.Bit3())
                         || (a.Bit3() && !b1.Bit3() && !r.Bit3())
                         || (a.Bit3() &&  b1.Bit3() && !r.Bit3())
                         || (a.Bit3() &&  b1.Bit3() &&  r.Bit3());
            }
            else
            {
                AuxCarry = (a.Bit3() && b.Bit3())
                         || (a.Bit3() && !r.Bit3())
                         || (b.Bit3() && !r.Bit3());
            }
        }

        public uint GetSingleRegister(string reg)
        {
            string lreg = reg.Trim().ToLower();
            switch (lreg)
            {
                case "a":
                    return A;
                case "flags":
                    return Flags;
                case "b":
                    return B;
                case "c":
                    return C;
                case "d":
                    return D;
                case "e":
                    return E;
                case "h":
                    return H;
                case "l":
                    return L;
                default: throw new Exception();
            }
        }
        public uint GetSingleRegister(byte reg)
        {
            switch (reg)
            {
                case (byte)SingleRegisterEnums.b:
                    return B;
                case (byte)SingleRegisterEnums.c:
                    return C;
                case (byte)SingleRegisterEnums.d:
                    return D;
                case (byte)SingleRegisterEnums.e:
                    return E;
                case (byte)SingleRegisterEnums.h:
                    return H;
                case (byte)SingleRegisterEnums.l:
                    return L;
                case (byte)SingleRegisterEnums.a:
                    return A;
                default: throw new Exception();
            }
        }

        public ushort getDRegister(byte reg, bool usePSW)
        {
            switch (reg)
            {
                case (byte)DoubleRegisterEnums.b: return BC;
                case (byte)DoubleRegisterEnums.d: return DE;
                case (byte)DoubleRegisterEnums.h: return HL;
                case (byte)DoubleRegisterEnums.sp:
                    if (usePSW)
                        return PSW;
                    else
                        return SP;
                default: throw new Exception();
            }
        }
        public void setDRegister(byte reg, ushort val, bool usePSW)
        {
            switch (reg)
            {
                case (byte)DoubleRegisterEnums.b: BC = val; break;
                case (byte)DoubleRegisterEnums.d: DE = val; break;
                case (byte)DoubleRegisterEnums.h: HL = val; break;
                case (byte)DoubleRegisterEnums.sp:
                    if (usePSW)
                        PSW = val;
                    else
                        SP = val;
                    break;
                default: throw new Exception();
            }
        }
        public void SetSingleRegister(string reg, uint data)
        {
            string lreg = reg.Trim().ToLower();
            switch (lreg)
            {
                case "a":
                    A = (byte)data; break;
                case "flags":
                    Flags = (byte)data; break;
                case "b":
                    B = (byte)data; break;
                case "c":
                    C = (byte)data; break;
                case "d":
                    D = (byte)data; break;
                case "e":
                    E = (byte)data; break;
                case "h":
                    H = (byte)data; break;
                case "l":
                    L = (byte)data; break;
                default: throw new Exception();
            }
        }

        public void SetSingleRegister(byte reg, uint data)
        {
            byte r = (byte)data;
            switch (reg)
            {
                case (byte)SingleRegisterEnums.b:
                    B = r; break;
                case (byte)SingleRegisterEnums.c:
                    C = r; break;
                case (byte)SingleRegisterEnums.d:
                    D = r; break;
                case (byte)SingleRegisterEnums.e:
                    E = r; break;
                case (byte)SingleRegisterEnums.h:
                    H = r; break;
                case (byte)SingleRegisterEnums.l:
                    L = r; break;
                case (byte)SingleRegisterEnums.a:
                    A = r; break;
                default: throw new Exception();
            }
        }
        public virtual void SetDoubleRegister(string reg, uint data)
        {
            string lreg = reg.Trim().ToLower();
            switch (lreg)
            {
                case "b":
                    BC = (ushort)data; break;
                case "d":
                    DE = (ushort)data; break;
                case "h":
                    HL = (ushort)data; break;
                case "sp":
                    SP = (ushort)data; break;
                case "psw":
                    PSW = (ushort)data; break;
                default: throw new Exception();
            }
        }

        public void SetDoubleRegister(byte reg, uint data)
        {
            ushort r = (ushort)data;
            switch (reg)
            {
                case (byte)DoubleRegisterEnums.b: BC = r; break;
                case (byte)DoubleRegisterEnums.d: DE = r; break;
                case (byte)DoubleRegisterEnums.h: HL = r; break;
                case (byte)DoubleRegisterEnums.sp: SP = r; break;
                case (byte)DoubleRegisterEnums.psw: PSW = r; break;
                default: throw new Exception();
            }
        }
        public virtual uint GetDoubleRegister(string reg)
        {
            string lreg = reg.Trim().ToLower();
            switch (lreg)
            {
                case "b":
                    return BC;
                case "d":
                    return DE;
                case "h":
                    return HL;
                case "sp":
                    return SP;
                case "psw":
                    return PSW;
                default: throw new Exception();
            }
        }

        public uint GetDoubleRegister(byte reg)
        {
            switch (reg)
            {
                case (byte)DoubleRegisterEnums.b: return BC;
                case (byte)DoubleRegisterEnums.d: return DE;
                case (byte)DoubleRegisterEnums.h: return HL;
                case (byte)DoubleRegisterEnums.sp: return SP;
                case (byte)DoubleRegisterEnums.psw: return PSW;
                default: throw new Exception();
            }
        }
        public ushort BC
        {
            get
            {
                mBuff2[1] = B;
                mBuff2[0] = C;
                return BitConverter.ToUInt16(mBuff2, 0);
            }
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);
                B = bytes[1];
                C = bytes[0];
            }
        }
        public ushort DE
        {
            get
            {
                mBuff2[1] = D;
                mBuff2[0] = E;
                return BitConverter.ToUInt16(mBuff2, 0);
            }
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);
                D = bytes[1];
                E = bytes[0];
            }
        }
        public ushort HL
        {
            get
            {
                mBuff2[1] = H;
                mBuff2[0] = L;
                return BitConverter.ToUInt16(mBuff2, 0);
            }
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);
                H = bytes[1];
                L = bytes[0];
            }
        }

        public bool Zero { get; set; }
        public bool Carry { get; set; }
        public bool Negative { get; set; }
        public bool AuxCarry { get; set; }
        public bool Invalid { get; set; }
        public bool Parity { get; set; }
        public bool Sign { get; set; }
        public bool K { get; set; }

        public virtual object Clone()
        {
            var ret = new Registers8085();
            ret.BC = this.BC;
            ret.DE = this.DE;
            ret.HL = this.HL;
            ret.SP = this.SP;
            ret.PC = this.PC;
            ret.PSW = this.PSW;
            return ret;
        }

        public virtual bool FetchFlag(string text)
        {
            string flag = text.Trim().ToLower();
            switch (flag)
            {
                case "z":
                    return Zero;
                case "c":
                    return Carry;
                case "n":
                    return Negative;
                case "s":
                    return Sign;
                case "p":
                    return Parity;
                case "a":
                    return AuxCarry;
                case "k":
                    return K;
                default:
                    return false;
            }
        }

        public virtual void SetFlag(string text, bool state)
        {
            string flag = text.Trim().ToLower();
            switch (flag)
            {
                case "z":
                    Zero = state;
                    return;
                case "c":
                    Carry = state;
                    return;
                case "n":
                    Negative = state;
                    return;
                case "s":
                    Sign = state;
                    return;
                case "p":
                    Parity = state;
                    return;
                case "a":
                    AuxCarry = state;
                    return;
                case "k":
                    K = state;
                    return;
                default:
                    throw new Exception();
            }
        }


        public virtual bool[] Difference(IRegisters registers)
        {
            Registers8085 org = registers as Registers8085;
            bool[] ret = new bool[15];
            for (byte ii = 0; ii <= 7; ii++)
            {
                if (ii == (byte)SingleRegisterEnums.m)
                    continue;
                ret[ii] = (this.GetSingleRegister(ii) != org.GetSingleRegister(ii));
            }

            ret[8] = this.Sign != org.Sign;
            ret[9] = this.Carry != org.Carry;
            ret[10] = this.Negative != org.Negative;
            ret[11] = this.Parity != org.Parity;
            ret[12] = this.AuxCarry != org.AuxCarry;
            ret[13] = this.Zero != org.Zero;
            ret[14] = this.K != org.K;
            return ret;
        }

        public static SingleRegisterEnums ParseSingleRegister(string r)
        {
            return (SingleRegisterEnums)Enum.Parse(typeof(SingleRegisterEnums), r);
        }

        public static bool IsSingleRegister(string r)
        {
            SingleRegisterEnums result;
            return Enum.TryParse(r, true, out result);
        }

        public static bool IsDoubleRegister(string r)
        {
            DoubleRegisterEnums result;
            return Enum.TryParse(r, true, out result);
        }
        public static DoubleRegisterEnums ParseDoubleRegister(string r)
        {
            return (DoubleRegisterEnums)Enum.Parse(typeof(DoubleRegisterEnums), r);
        }
    }
}