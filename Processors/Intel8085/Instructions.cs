using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace Intel8085
{
    public partial class Intel8085
    {
        protected delegate uint ExecuteInstruction(byte opcode);

        private byte[] mBuff2 = new byte[2];
        protected bool InterruptsEnabled;
        private bool mEnableEINext;

        public ISystemMemory SystemMemory { get; set; }

        private byte InteruptMask;

        public enum CPUFlags : byte
        {
            Zero = 0x0c,
            Carry = 0x0d,
            Parity = 0x0e,
            Sign = 0x0f
        }

        protected ExecuteInstruction[] mOpcodeFunctions = new ExecuteInstruction[256];
        protected Tuple<portAccessReadEventHandler, portAccessWriteEventHandler>[] mPortOverrides;
        protected codeAccessReadEventHandler[] mCodeOverrides;

        private uint Nop(byte opcode) { return 4; }

        //lxi h,0x1234
        private uint Lxi(byte opcode)
        {
            byte reg = (byte)((opcode >> 4) & 0x03);
            ushort data = (ushort)SystemMemory.GetMemory(Registers.PC, WordSize.TwoByte, true);
            Registers8085.PC += 2;
            Registers8085.SetDoubleRegister(reg, data);
            return 10;
        }

        private uint LdStax(byte opcode)
        {
            int reg = ((opcode >> 4) & 0x01);
            ushort addr = reg == 0 ? Registers8085.BC : Registers8085.DE;
            if ((opcode & 0x0f) == 0x02)
                SystemMemory.SetMemory(addr, WordSize.OneByte, Registers8085.A, true);
            else
                Registers8085.A = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);

            return 7;
        }

        private uint DoubleRegister(byte opcode)
        {
            //00RP0011
            byte reg = (byte)((opcode >> 4) & 0x03);
            switch (opcode & 0xcf)
            {
                case 0x09:                                  //dad reg (sets carry flag)
                    {
                        ushort regHL = Registers8085.HL;
                        ushort data = (ushort)Registers8085.GetDoubleRegister(reg);
                        Registers8085.HL = add16(regHL, data);
                        return 10;
                    }
                case 0x03:
                    {//inx reg ( sets no flags)
                        ushort result = (ushort)(Registers8085.GetDoubleRegister(reg) + 1);
                        Registers8085.K = (result == 0);
                        Registers8085.SetDoubleRegister(reg, result);
                        return 6;
                    }
                case 0x0b:
                    {//dcx reg ( sets no flags)
                        ushort result = (ushort)(Registers8085.GetDoubleRegister(reg) - 1);
                        Registers8085.K = (result == 0);
                        Registers8085.SetDoubleRegister(reg, result);
                        return 6;
                    }
            }//switch
            return 1;
        }//DoubleRegister

        private uint SingleRegDst(byte opcode)
        {//inr m, dcr m???
            byte reg = (byte)((opcode >> 3) & 0x07);
            byte a   = (reg == 0x06) ? (byte)SystemMemory.GetMemory(Registers8085.HL, WordSize.OneByte, true) :
                                        (byte)Registers8085.GetSingleRegister(reg);
            byte b = 1;
            uint cycles = 4;
            byte r = 0;

            switch (opcode & 0xc7)
            {
                case 0x05:                              //dcr
                    r = (byte)(a - b);
                    Registers8085.setFlagH(a, b, r, true);
                    Registers8085.setFlagV(a, b, r, true);
                    break;
                case 0x04:
                    r = (byte)(a + b);               // inr
                    Registers8085.setFlagH(a, b, r, false);
                    Registers8085.setFlagV(a, b, r, false);
                    break;
                default:
                    throw new Exception();
            }

            if (reg == 0x06)
            {
                SystemMemory.SetMemory(Registers8085.HL, WordSize.OneByte, r, true);
                cycles = 10;
            }
            else
                Registers8085.SetSingleRegister(reg, r);

            Registers8085.setFlagsSZP(r);
            return cycles;
        }

        private uint Mvi(byte opcode)
        {
            //00DDD110;
            byte reg = (byte)((opcode >> 3) & 0x07);
            byte data = (byte)SystemMemory.GetMemory(Registers8085.PC++, WordSize.OneByte, true);
            if (reg == 0x06)
                SystemMemory.SetMemory(Registers8085.HL, WordSize.OneByte, data, true);
            else
                Registers8085.SetSingleRegister(reg, data);

            return (reg == 6) ? (uint)10 : (uint)7;
        }

        protected byte rl(byte data)
        {
            byte cBit = (byte)(Registers8085.Carry ? 0x01 : 0x00);
            Registers8085.Carry = data.Bit7();
            data <<= 1;
            data |= cBit;
            return data;
        }

        protected byte rlc(byte data)
        {
            Registers8085.Carry = data.Bit7();
            byte cBit = (byte)(Registers8085.Carry ? 0x01 : 0x00);
            data <<= 1;
            data |= cBit;
            Registers8085.setFlagN(false);
            Registers8085.setFlagAuxCarry(false);
            return data;
        }
        protected byte ral(byte data)
        {
            byte cBit = (byte)(Registers8085.Carry ? 0x01 : 0x00);
            Registers8085.Carry = data.Bit7();
            data <<= 1;
            data |= cBit;
            Registers8085.setFlagN(false);
            Registers8085.setFlagAuxCarry(false);
            return data;
        }

        protected byte rrc(byte data)
        {
            Registers8085.Carry = data.Bit0();
            byte cBit = (byte)(Registers8085.Carry ? 0x80 : 0x00);
            data >>= 1;
            data |= cBit;
            Registers8085.setFlagN(false);
            Registers8085.setFlagAuxCarry(false);
            return data;
        }
        protected byte rar(byte data)
        {
            byte cBit = (byte)(Registers8085.Carry ? 0x80 : 0x00);
            Registers8085.Carry = data.Bit0();
            data >>= 1;
            data |= cBit;
            Registers8085.setFlagN(false);
            Registers8085.setFlagAuxCarry(false);
            return data;
        }
        protected byte sla(byte data)
        {
            Registers8085.Carry = data.Bit7();
            data <<= 1;
            return data;
        }
        protected byte sra(byte data)
        {
            Registers8085.Carry = data.Bit0();
            bool msbOn = data.Bit7();
            data >>= 1;
            if (msbOn)
                data |= 0x80;
            return data;
        }

        protected byte srl(byte data)
        {
            Registers8085.Carry = data.Bit0();
            data >>= 1;
            return data;
        }
        protected byte setb(byte bitPos, byte data)
        {
            byte mask = (byte)(1 << bitPos);
            data |= mask;
            return data;
        }
        protected byte resetb(byte bitPos, byte data)
        {
            byte mask = (byte)~(1 << bitPos);
            data &= mask;
            return data;
        }
        protected byte testb(byte bitPos, byte data)
        {
            byte mask = (byte)(1 << bitPos);
            bool Z = (data & mask) == 0;
            Registers8085.Zero = Z;
            return data;
        }

        private uint NoParams(byte opcode)
        {
            //ushort temp = 0;
            switch (opcode)
            {
                case 0x17://ral - rotate left thru carry
                    {
                        Registers8085.A = ral(Registers8085.A);
                        return 4;
                    }
                case 0x1f://rar - rotate right thru carry
                    {
                        Registers8085.A = rar(Registers8085.A);
                        return 4;
                    }
                case 0x07://rlc - rotate left
                    {
                        Registers8085.A = rlc(Registers8085.A);
                        return 4;
                    }
                case 0x0f://rrc - rotate right
                    {
                        Registers8085.A = rrc(Registers8085.A);
                        return 4;
                    }
                case 0x20:                  //rim
                    Registers8085.A = InteruptMask;
                    return 4;
                case 0x30:                  //sim
                    InteruptMask = Registers8085.A;
                    return 4;
                case 0x37:                  //stc
                    Registers8085.Carry = true;
                    return 4;
                case 0x27:                  //daa
                    Registers8085.A = daa(Registers8085.A);
                    return 4;
                case 0x2f:                  //cma
                    Registers8085.A = cma(Registers8085.A);
                    return 4;
                case 0x3f:                   //cmc
                    Registers8085.setFlagAuxCarry(Registers8085.Carry);
//                    Registers8085.AuxCarry = Registers8085.Carry;
                    Registers8085.Carry = !Registers8085.Carry;
                    Registers8085.setFlagN(false);
                    return 4;
                case 0x76:                  //hlt
                    IsHalted = true;
                    return 5;
                case 0xf3:                  //di
                    InterruptsEnabled = false;
                    return 4;
                case 0xfb:                  //ei
                    mEnableEINext = true;
                    return 4;
            }
            return 1;
        }
        private byte cma(byte a)
        {
            byte r = (byte)~a;
            Registers8085.setFlagsHN();
            return r;
        }

        private byte daa(byte a)
        {
            byte orgA = a;
            if (a.Lsn() > 9 || Registers8085.AuxCarry)
            {
                a += 6;
                Registers8085.Carry |= Registers8085.isFlagaddC(orgA, 6, a);
                Registers8085.setFlagH(orgA, 6, a, false);
            }
            else
                Registers8085.AuxCarry = false;

            if (a.Msn() > 9 || Registers8085.Carry)
            {
                a += 0x60;
                Registers8085.Carry = true;
            }
            else
                Registers8085.Carry = false;
            Registers8085.setFlagsSZP(a);
            Registers8085.setParityOverflow(a);
            return a;
        }

        // sta 0x1234
        private uint Data16(byte opcode)
        {
            ushort addr = (ushort)SystemMemory.GetMemory(Registers8085.PC, WordSize.TwoByte, true);
            Registers8085.PC += 2;

            switch (opcode)
            {
                case 0x32: SystemMemory.SetMemory(addr, WordSize.OneByte, Registers8085.A, true); return 13;  //sta addr
                case 0x3a: Registers8085.A = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true); return 13;  //lda addr
                case 0x2a: Registers8085.HL = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true); return 16;  //lhld addr
                case 0x22: SystemMemory.SetMemory(addr, WordSize.TwoByte, Registers8085.HL); return 16;  //shld addr
                default: break;
            }//switch
            return 1;
        }

        //src or dst reg == 0x06 indicates M register(HL)
        private uint Mov(byte opcode)
        {
            byte dreg = DstReg(opcode);
            byte sreg = SrcReg(opcode);
            uint cycles = ((dreg == 0x06) || (sreg == 0x06)) ? (uint)7 : (uint)4;
            byte sData = 0;
            if (sreg == 0x06)
                sData = (byte)SystemMemory.GetMemory(Registers8085.HL, WordSize.OneByte, true);
            else
                sData = (byte)Registers8085.GetSingleRegister(sreg);

            if (dreg == 0x06)
                SystemMemory.SetMemory(Registers8085.HL, WordSize.OneByte, sData, true);
            else
                Registers8085.SetSingleRegister(dreg, sData);

            return cycles;
        }

        private uint SingleRegSrc(byte opcode)
        {
            byte reg = (byte)(opcode & 0x07);
            byte a = Registers8085.A;
            byte b = (reg == 0x06) ? (byte)SystemMemory.GetMemory(Registers8085.HL, WordSize.OneByte, true) :
                                     (byte)Registers8085.GetSingleRegister(reg);
            uint cycles = (reg == 0x06) ? (uint)7 : 4;

            switch (opcode & 0xf8)
            {
                case 0xb8:                      //cmp reg
                    cpi8(a, b);
                    break;
                case 0xb0:                      //ora reg
                    Registers8085.A = or8(a, b);
                    break;
                case 0xa8:                      //xra reg
                    Registers8085.A = xor8(a, b);
                    break;
                case 0xa0:                      //ana reg
                    Registers8085.A = and8(a, b);
                    break;
                case 0x80:                      //add reg
                    Registers8085.A = add8(a, b);
                    break;
                case 0x90:                      //sub reg
                    Registers8085.A = sub8(a, b);
                    break;
                case 0x88:                      //adc reg
                    Registers8085.A = adc8(a, b);
                    break;
                case 0x98:                      //sbb reg
                    Registers8085.A = sbb8(a, b);
                    break;
                default:
                    throw new Exception();
            }
            return cycles;
        }

        private uint Returns(byte opcode)
        {
            bool unconditional = (opcode == 0xc9);
            bool actionNeeded = false;
            bool polarity = ((opcode & 0x08) != 0);

            if (!unconditional)
            {
                switch (opcode & 0x0f0)
                {
                    case 0xc0:                  //zero flag
                        actionNeeded = (Registers8085.Zero == polarity);
                        break;
                    case 0xd0:                  //carry flag
                        actionNeeded = (Registers8085.Carry == polarity);
                        break;
                    case 0xe0:                  //parity flag
                        actionNeeded = (Registers8085.Parity == polarity);
                        break;
                    case 0xf0:                  //sign flag
                        actionNeeded = (Registers8085.Sign == polarity);
                        break;
                }
                if (!actionNeeded)
                    return 6;
            }
            Registers8085.PC = Pop();
            return 12;
        }

        private uint PushPop(byte opcode)
        {
            byte reg = (byte)((opcode >> 4) & 0x03);
            bool pop = ((opcode & 0x0f) == 0x01);
            if (pop)
                Registers8085.setDRegister(reg, Pop(), true);
            else
                Push(Registers8085.getDRegister(reg, true));
            return pop ? (uint)10 : (uint)12;
        }

        protected void Push(ushort data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            SystemMemory.SetMemory(--Registers8085.SP, WordSize.OneByte, bytes[1], true);
            SystemMemory.SetMemory(--Registers8085.SP, WordSize.OneByte, bytes[0], true);
        }

        protected ushort Pop()
        {
            mBuff2[0] = (byte)SystemMemory.GetMemory(Registers8085.SP++, WordSize.OneByte, true);
            mBuff2[1] = (byte)SystemMemory.GetMemory(Registers8085.SP++, WordSize.OneByte, true);
            return BitConverter.ToUInt16(mBuff2, 0);
        }

        private uint Jump(byte opcode)
        {
            ushort addr = (ushort)SystemMemory.GetMemory(Registers8085.PC, WordSize.TwoByte, true);
            Registers8085.PC += 2;

            bool unConditional = (opcode == 0xc3);
            if (!unConditional)
            {
                CPUFlags condition = (CPUFlags)((opcode >> 4) & 0x0f);
                bool actionTrue = ((opcode & 0x08) != 0);
                bool takeAction = false;
                switch (condition)
                {
                    case CPUFlags.Zero: takeAction = actionTrue == Registers8085.Zero; break;
                    case CPUFlags.Carry: takeAction = actionTrue == Registers8085.Carry; break;
                    case CPUFlags.Parity: takeAction = actionTrue == Registers8085.Parity; break;
                    case CPUFlags.Sign: takeAction = actionTrue == Registers8085.Sign; break;
                }
                if (!takeAction)
                    return 7;
            }//if
            Registers8085.PC = addr;
            return 10;
        }

        private uint Invalid(byte opcode)
        {
            throw new Exception("invalid opcode");
            //return 0;
        }

        private uint Call(byte opcode)
        {
            ushort addr = (ushort)SystemMemory.GetMemory(Registers8085.PC, WordSize.TwoByte, true);
            Registers8085.PC += 2;

            bool unConditional = (opcode == 0xcd);
            if (!unConditional)
            {
                CPUFlags condition = (CPUFlags)((opcode >> 4) & 0x0f);
                bool actionTrue = ((opcode & 0x08) != 0);
                bool takeAction = false;
                switch (condition)
                {
                    case CPUFlags.Zero: takeAction = actionTrue == Registers8085.Zero; break;
                    case CPUFlags.Carry: takeAction = actionTrue == Registers8085.Carry; break;
                    case CPUFlags.Parity: takeAction = actionTrue == Registers8085.Parity; break;
                    case CPUFlags.Sign: takeAction = actionTrue == Registers8085.Sign; break;
                }
                if (!takeAction)
                    return 9;
            }//if
            Push((ushort)Registers8085.PC);
            Registers8085.PC = addr;

            return 18;
        }

        private uint Data8(byte opcode)
        {
            byte a = Registers8085.A;
            byte b = (byte)SystemMemory.GetMemory(Registers8085.PC++, WordSize.OneByte, true);

            switch (opcode)
            {
                case 0xdb:                                 //in data
                    {
                        var item = mPortOverrides[b];
                        if (item != null)
                            Registers8085.A = (byte)item.Item1(this, new PortAccessReadEventArgs(b));
                        else
                            Registers8085.A = 0;

                    }
                    return 10;
                case 0xd3:                                       //out data
                    {
                        var item = mPortOverrides[b];
                        if (item != null && item.Item2 != null)
                            item.Item2(this, new PortAccessWriteEventArgs(b, a));
                    }
                    return 10;
                case 0xf6:                                                  //ori data
                    Registers8085.A = or8(a, b);
                    break;
                case 0xee:                                                  //xri data
                    Registers8085.A = xor8(a, b);
                    break;
                case 0xe6:                                                  //ani data
                    Registers8085.A = and8(a, b);
                    break;
                case 0xfe:                                                  //cpi data
                    cpi8(a, b);
                    break;
                case 0xc6:                                                  //adi data
                    Registers8085.A = add8(a, b);
                    break;
                case 0xd6:                                                  //sui data
                    Registers8085.A = sub8(a, b);
                    break;
                case 0xce:                                                  //aci data
                    Registers8085.A = adc8(a, b);
                    break;
                case 0xde:                                                  //sbi data
                    Registers8085.A = sbc8(Registers8085.A, b);
                    break;
            }
            return 7;
        }

        private uint NoParamXCHGRegs(byte opcode)
        {
            switch (opcode)
            {
                case 0xe9:                  //pchl
                    Registers8085.PC = Registers8085.HL;
                    return 6;
                case 0xf9:                  //sphl
                    Registers8085.SP = Registers8085.HL;
                    return 6;
                case 0xeb:                  //xchg
                    {
                        ushort data = Registers8085.HL;
                        Registers8085.HL = Registers8085.DE;
                        Registers8085.DE = data;
                        return 4;
                    }
                case 0xe3:                  //xthl
                    {
                        ushort data = Pop();
                        Push(Registers8085.HL);
                        Registers8085.HL = data;
                        return 16;
                    }
            }
            return 1;
        }

        private uint Rst(byte opcode)
        {
            Push((ushort)Registers8085.PC);
            int level = ((opcode >> 3) & 0x07);
            Registers8085.PC = (ushort)(level << 3);
            return 12;
        }

        //protected static bool carryResult8(ushort data)
        //{
        //    return ((data & 0xff00) != 0);
        //}
        protected static bool carryResult16(uint data)
        {
            return ((data & 0xffff0000) != 0);
        }

        //protected static bool auxCarryResult(byte a, byte b, byte c, bool plusTwo)
        //{
        //    int temp = lsn(a) + lsn(b) + lsn(c) + (plusTwo ? 1 : 0);
        //    return (temp & 0xfffffff0) != 0;
        //}

        //protected static bool auxCarryResult(byte a, byte b, bool plusOne)
        //{
        //    int temp = lsn(a) + lsn(b) + (plusOne ? 1 : 0);
        //    return (temp & 0xfffffff0) != 0;
        //}

        protected static byte DstReg(byte opcode) { return (byte)((opcode >> 3) & 0x07); }
        protected static byte SrcReg(byte opcode) { return (byte)(opcode & 0x07); }
        //protected static bool msb(ushort data) { return ((data & 0x8000) != 0); }

        protected byte add8(byte a, byte b)
        {
            byte r = (byte)(a + b);
            Registers8085.setFlagaddC(a, b, r);
            Registers8085.setFlagH(a, b, r, false);
            Registers8085.setFlagsSZP(r);
            Registers8085.setFlagV(a, b, r, false);
            return r;
        }

        protected byte adc8(byte a, byte b)
        {
            byte c = Registers8085.Carry ? (byte)1 : (byte)0;
            byte r = (byte)(a + b + c);
            Registers8085.setFlagaddC(a, b, r);
            Registers8085.setFlagH(a, b, r, false);
            Registers8085.setFlagsSZP(r);
            Registers8085.setFlagV(a, b, r, false);
            return r;
        }
        protected byte sub8(byte a, byte b)
        {
            byte r = (byte)(a - b);
            Registers8085.setFlagsubC(a, b, r);
            Registers8085.setFlagN(true);
            Registers8085.setFlagH(a, b, r, true);
            Registers8085.setFlagsSZP(r);
            Registers8085.setFlagV(a, b, r, true);
            return r;
        }
        protected byte sbb8(byte a, byte b)
        {
            byte c = Registers8085.Carry ? (byte)0x01 : (byte)0x00;
            byte r = (byte)(a - b - c);
            Registers8085.setFlagsubC(a, b, r);
            Registers8085.setFlagH(a, b, r, true);
            Registers8085.setFlagN(true);
            Registers8085.setFlagsSZP(r);
            Registers8085.setFlagV(a, b, r, true);
            return r;
        }

        protected byte sbc8(byte a, byte b)
        {
            byte data = (Registers8085.Carry ? (byte)(b + 1) : b);
            data = data.TwosComplement();
            byte r = (byte)(a + data);
            Registers8085.setFlagsubC(a, b, r);
            Registers8085.setFlagN(true);
            Registers8085.setFlagH(a, b, r, true);
            Registers8085.setFlagsSZP(r);
            Registers8085.setFlagV(a, b, r, true);
            return r;
        }

        protected byte and8(byte a, byte b)
        {
            byte r = (byte)(a & b);
            Registers8085.Carry = false;
            Registers8085.AuxCarry = true;
            Registers8085.setFlagsSZP(r);
            Registers8085.setParityOverflow(r);
            return r;
        }

        protected byte or8(byte a, byte b)
        {
            byte r = (byte)(a | b);
            Registers8085.Carry = false;
            Registers8085.AuxCarry = false;
            Registers8085.setFlagsSZP(r);
            Registers8085.setParityOverflow(r);
            return r;
        }

        protected byte xor8(byte a, byte b)
        {
            byte r = (byte)(a ^ b);
            Registers8085.Carry = false;
            Registers8085.AuxCarry = false;
            Registers8085.setFlagsSZP(r);
            Registers8085.setParityOverflow(r);
            return r;
        }
        protected byte cpi8(byte a, byte b)
        {
            sub8(a, b);
            return 0;
        }

        //protected byte inc(byte a)
        //{
        //    byte r = (byte)(a + 1);
        //    Registers8085.setFlagaddC(a, 1, r);
        //    Registers8085.setFlagH(a, 1, r, false);
        //    return r;
        //}
        protected byte dcr(byte a)
        {
            byte r = (byte)(a - 1);
            Registers8085.setFlagsubC(a, 1, r);
            Registers8085.setFlagH(a, 1, r, true);
            return r;
        }

        protected ushort add16s(ushort a, ushort b)
        {
            short result = (short)(a + b);
            return (ushort)result;
        }

        protected ushort add16(ushort a, ushort b)
        {
            uint result = (uint)(a + b);
            Registers8085.Carry = ((result & 0xffff0000) != 0);
            Registers8085.AuxCarry = false;
            return (ushort)result;
        }

    }
}