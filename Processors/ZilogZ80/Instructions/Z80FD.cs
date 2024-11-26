using System;
using Intel8085;
using Processors;

namespace ZilogZ80
{
    public delegate void setIndexRegDelegate(ushort val);
    public delegate byte performArithmeticDelegate8(byte a, byte b);
    public delegate ushort performArithmeticDelegate16(ushort a, ushort b);

    public partial class ZilogZ80
    {
        //private performArithmeticDelegate16[] mFuncsfd16;
//        protected void handleddInit() { }

        private uint handledd(byte prebyte1)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            return handleDDorFD(prebyte1, regZ80.IX, (ushort value) => { regZ80.IX = value; });
        }

        private uint handlefd(byte opcode)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            return handleDDorFD(opcode, regZ80.IY, (ushort value) => { regZ80.IY = value; });
        }

        private uint handleDDorFD(byte prebyte1, ushort indexRegVal, setIndexRegDelegate setIV)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            byte opcode2 = (byte)SystemMemory.GetMemory(Registers.PC + 0, WordSize.OneByte, true);
            byte dd = (byte)SystemMemory.GetMemory(Registers.PC + 1, WordSize.OneByte, true);
            if (opcode2 == 0xcb)
            {
                var ret = handleddcb(regZ80, dd, indexRegVal, setIV);
                Registers.PC += 3;
                return ret;
            }
            ushort addr = add16s(indexRegVal, dd.SignExtend());
            byte opcode4 = (byte)SystemMemory.GetMemory(Registers.PC + 2, WordSize.OneByte, true);
            if ((opcode2 & 0xcf) == 0x09)
            {//add i?,rr    rr = bc, de, ix, sp
                ushort data;
                byte rr = (byte)((opcode2 & 0x30) >> 4);
                if (rr == 0x02)  // i? register
                    data = indexRegVal;
                else
                    data = (ushort)regZ80.GetDoubleRegister(rr);

                ushort r = add16(indexRegVal, data);
                setIV(r);
                Registers.PC++;
                return 15;
            }
            else if (opcode2 == 0x21)
            {//ld i?,nn
                addr = (ushort)SystemMemory.GetMemory((Registers.PC + 1), WordSize.TwoByte, false);
                //setIV((ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte));
                setIV(addr);
                Registers.PC += 3;
                return 14;
            }
            else if (opcode2 == 0x22)
            {//ld (nn),i?
                addr = (ushort)SystemMemory.GetMemory((Registers.PC + 1), WordSize.TwoByte, false);
                SystemMemory.SetMemory(addr, WordSize.TwoByte, indexRegVal);
                Registers.PC += 3;
                return 20;
            }
            else if (opcode2 == 0x23)
            {//inc i?
                setIV((ushort)(indexRegVal + 1));
                Registers.PC++;
                return 10;
            }
            else if (opcode2 == 0x2a)
            {//ld i?,(nn)
                addr = (ushort)SystemMemory.GetMemory((Registers.PC + 1), WordSize.TwoByte, false);
                setIV((ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte));
                Registers.PC += 3;
                return 20;
            }
            else if (opcode2 == 0x2b)
            {//dec i?
                setIV((ushort)(indexRegVal - 1));
                Registers.PC++;
                return 10;
            }
            else if (opcode2 == 0x34)
            {//inc (i?+d)
                byte b = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte);
                byte r = (byte)(b + 1);               // inr
                regZ80.setFlagH(b, 1, r, false);
                SystemMemory.SetMemory(addr, WordSize.OneByte, r, true);
                Registers.PC += 2;
                regZ80.Negative = false;
                regZ80.Carry = false;
                regZ80.setFlagS(r);
                regZ80.setFlagZ(r);
                regZ80.setFlagV(b,1,r,false);
                return 23;
            }
            else if (opcode2 == 0x35)
            {//dec (i?+d)
                byte b = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
                byte r = dcr(b);
                SystemMemory.SetMemory(addr, WordSize.OneByte, r, true);
                Registers.PC += 2;
                regZ80.Negative = true;
                regZ80.Carry = false;
                regZ80.setFlagS(r);
                regZ80.setFlagZ(r);
                regZ80.setFlagV(b, 1, r, true);
                return 23;
            }
            else if (opcode2 == 0x36)
            {//ld (i?+d),n
                SystemMemory.SetMemory(addr, WordSize.OneByte, opcode4, true);
                Registers.PC += 3;
                return 19;
            }
            else if (((opcode2 & 0xc7) == 0x46) && ((opcode2 & 0xc7) != 0x76))
            {//ld r,(i?+d) r = b,c,d,e,h,l,a
                byte r = (byte)((opcode2 & 0x38) >> 3);
                if (r == 6)
                    return 0;
                byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
                regZ80.SetSingleRegister(r, data);
                Registers.PC += 2;
                return 19;
            }
            else if ((opcode2 >= 0x70) && (opcode2 <= 0x77) && (opcode2 != 0x76))
            {//ld (i?+d),r   r = b,c,d,e,h,l,a
                byte reg = (byte)(opcode2 & 0x07);
                byte data = (byte)regZ80.GetSingleRegister(reg);
                SystemMemory.SetMemory(addr, WordSize.OneByte, data, true);
                Registers.PC += 2;
                return 19;
            }
            else if ((opcode2 & 0xc7) == 0x86)
            {//hl = func hl, rr
                int findex = ((opcode2 & 0x38) >> 3);
                //addr = (ushort)(indexRegVal + opcode3);
                byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
                performArithmeticDelegate8 func = mFuncs8[findex];
                regZ80.A = func(regZ80.A, data);
                Registers.PC += 2;
                return 15;
            }
            else if (opcode2 == 0xe1)
            {//pop i?
                setIV(Pop());
                Registers.PC++;
                return 14;
            }
            else if (opcode2 == 0xe3)
            {//ex (sp), i?
                ushort temp = (ushort)SystemMemory.GetMemory(regZ80.SP, WordSize.TwoByte, true);
                SystemMemory.SetMemory(regZ80.SP, WordSize.TwoByte, indexRegVal, true);
                setIV(temp);
                Registers.PC++;
                return 23;
            }
            else if (opcode2 == 0xe5)
            {//push i?
                Push(indexRegVal);
                Registers.PC++;
                return 15;
            }
            else if (opcode2 == 0xe9)
            {//jp (i?)
                regZ80.PC = indexRegVal;
                return 8;
            }
            else if (opcode2 == 0xf9)
            {//ld sp,i?
                regZ80.SP = indexRegVal;
                Registers.PC++;
                return 10;
            }
            throw new Exception();
        }
    }
}