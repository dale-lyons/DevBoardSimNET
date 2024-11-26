using System;
using Intel8085;
using Processors;

namespace ZilogZ80
{
    public partial class ZilogZ80
    {
        private performArithmeticDelegate16[] mFuncsed16;
        private int mInteruptMode = 0;
        protected void handleedInit()
        {
            mFuncsed16 = new performArithmeticDelegate16[]
            {
               sbc16, adc16, sbc16, adc16, sbc16, adc16, sbc16, adc16
            };
        }
        private uint handleed(byte opcode)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            byte opcode2 = (byte)SystemMemory.GetMemory(regZ80.PC, WordSize.OneByte, true);

            if ((opcode2 & 0xc7) == 0x40)
            {//in r,(c)
                byte reg = (byte)((opcode2 & 0x38) >> 3);
                var item = mPortOverrides[regZ80.C];
                byte data = 0;
                if (item != null && item.Item1 != null)
                    data = (byte)item.Item1(this, new PortAccessReadEventArgs(regZ80.BC));
                regZ80.SetSingleRegister(reg, data);
                Registers.PC += 1;
                return 12;
            }
            else if ((opcode2 & 0xc7) == 0x41)
            {//out (c),c
                byte reg = (byte)((opcode2 & 0x38) >> 3);
                var item = mPortOverrides[regZ80.C];
                if (item != null && item.Item2 != null)
                    item.Item2(this, new PortAccessWriteEventArgs(regZ80.C, (byte)regZ80.GetSingleRegister(reg)));
                Registers.PC += 1;
                return 12;
            }
            else if ((opcode2 & 0xc7) == 0x42)
            {//hl = func hl, rr
                int findex = ((opcode2 & 0x38) >> 3);
                byte reg = (byte)((opcode2 & 0x30) >> 4);
                ushort data = (ushort)regZ80.GetDoubleRegister(reg);
                performArithmeticDelegate16 func = mFuncsed16[findex];
                regZ80.HL = func(regZ80.HL, data);
                Registers.PC += 1;
                return 15;
            }
            else if ((opcode2 & 0xcf) == 0x43)
            {//ld (nn), rr
                byte reg = (byte)((opcode2 & 0x30) >> 4);
                if (reg == 0x02)
                {
                    Registers.PC += 1;
                    return 0;
                }
                ushort data = (ushort)regZ80.GetDoubleRegister(reg);
                ushort addr = (ushort)SystemMemory.GetMemory(regZ80.PC+1, WordSize.TwoByte, true);
                SystemMemory.SetMemory(addr, WordSize.TwoByte, data, true);
                Registers.PC += 3;
                return 15;
            }
            else if ((opcode2 & 0xcf) == 0x4b)
            {//ld rr, (nn)
                byte reg = (byte)((opcode2 & 0x30) >> 4);
                if (reg == 0x02)
                {
                    Registers.PC += 1;
                    return 0;
                }
                uint addr = regZ80.PC + 1;
                ushort data = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
                ushort data2 = (ushort)SystemMemory.GetMemory(data, WordSize.TwoByte, true);
                regZ80.SetDoubleRegister(reg, data2);
                Registers.PC += 3;
                return 20;
            }

            switch (opcode2)
            {
                case 0x44:
                    {//neg
                        regZ80.A = (byte)(0 - regZ80.A);
                        Registers.PC += 1;
                        return 8;
                    }
                case 0x45:
                    {//retn
                        //todo - implement iff1/iff2
                        Registers.PC = Pop();
                        return 14;
                    }
                case 0x46:
                    {//im 0
                        mInteruptMode = 0;
                        Registers.PC += 1;
                        return 9;
                    }
                case 0x47:
                    {//ld i,a
                        regZ80.I = regZ80.A;
                        Registers.PC += 1;
                        return 9;
                    }
                case 0x4d:
                    {//reti
                        Registers.PC = Pop();
                        return 14;
                    }
                case 0x4f:
                    {//ld r,a
                        regZ80.R = regZ80.A;
                        Registers.PC += 1;
                        return 9;
                    }
                case 0x56:
                    {//im1
                        mInteruptMode = 1;
                        Registers.PC += 1;
                        return 9;
                    }
                case 0x57:
                    {//ld a,i
                        regZ80.A = regZ80.I;
                        Registers.PC += 1;
                        return 9;
                    }
                case 0x5b:
                    {//ld de,(nn)
                        uint addr = (ushort)SystemMemory.GetMemory(regZ80.PC + 1, WordSize.TwoByte, true);
                        regZ80.DE = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
                        Registers.PC += 3;
                        return 17;
                    }
                case 0x5e:
                    {//im 2
                        mInteruptMode = 2;
                        Registers.PC += 1;
                        return 9;
                    }
                case 0x5f:
                    {//ld a,r
                        regZ80.A = regZ80.R;
                        Registers.PC += 1;
                        return 9;
                    }
                case 0x67:
                    {//rrd
                        byte data = (byte)SystemMemory.GetMemory(regZ80.HL, WordSize.OneByte, true);
                        byte a = (byte)(regZ80.A & 0x0f);
                        byte b = (byte)(data >> 4);
                        byte c = (byte)(data & 0x0f);

                        byte tmp = a;
                        a = b;
                        b = c;
                        c = tmp;

                        data = (byte)(b << 4 | c);
                        regZ80.A &= 0xf0;
                        regZ80.A |= a;
                        SystemMemory.SetMemory(regZ80.HL, WordSize.OneByte, data, true);

                        Registers.PC += 1;
                        return 18;
                    }
                case 0x6f:
                    {//rld
                        byte data = (byte)SystemMemory.GetMemory(regZ80.HL, WordSize.OneByte, true);
                        byte a = (byte)(regZ80.A & 0x0f);
                        byte b = (byte)(data >> 4);
                        byte c = (byte)(data & 0x0f);

                        byte tmp = a;
                        a = b;
                        b = c;
                        c = tmp;

                        data = (byte)(b << 4 | c);
                        regZ80.A &= 0xf0;
                        regZ80.A |= a;
                        SystemMemory.SetMemory(regZ80.HL, WordSize.OneByte, data, true);

                        Registers.PC += 1;
                        return 18;
                    }
                case 0xa0:
                    {//ldi
                        byte data = (byte)SystemMemory.GetMemory(regZ80.HL++, WordSize.OneByte, true);
                        SystemMemory.SetMemory(regZ80.DE++, WordSize.OneByte, data, true);
                        regZ80.BC--;
                        return 16;
                    }
                case 0xa1:
                    {//cpi
                        byte a = regZ80.A;
                        byte b = (byte)~SystemMemory.GetMemory(regZ80.HL, WordSize.OneByte, true);
                        sub8(a, b);
                        regZ80.BC--;
                        return 16;
                    }
                case 0xa2:
                    {//ini
                        var item = mPortOverrides[regZ80.C];
                        byte data = 0;
                        if (item != null && item.Item1 != null)
                            data = (byte)item.Item1(this, new PortAccessReadEventArgs(regZ80.C));

                        SystemMemory.SetMemory(regZ80.HL++, WordSize.OneByte, data, true);
                        regZ80.B -= 1;
                        regZ80.Zero = (regZ80.B == 0);
                        return 16;
                    }
                case 0xa3:
                    {//outi
                        byte data = (byte)(SystemMemory.GetMemory(regZ80.HL++, WordSize.OneByte, true) + 1);
                        var item = mPortOverrides[regZ80.C];
                        if (item != null && item.Item1 != null)
                            item.Item2(this, new PortAccessWriteEventArgs(regZ80.C, data));
                        regZ80.B -= 1;
                        regZ80.Zero = (regZ80.B == 0);
                        return 16;
                    }
                case 0xa8:
                    {//ldd
                        byte b = (byte)SystemMemory.GetMemory(regZ80.HL--, WordSize.OneByte, true);
                        SystemMemory.SetMemory(regZ80.DE--, WordSize.OneByte, b, true);
                        regZ80.BC -= 1;
                        return 16;
                    }
                case 0xa9:
                    {//cpd
                        byte a = regZ80.A;
                        byte b = (byte)(SystemMemory.GetMemory(regZ80.HL, WordSize.OneByte, true));
                        byte r = (byte)(a - b);
                        regZ80.setFlagZ(r);
                        regZ80.setFlagS(r);
                        regZ80.setFlagH(a , b, r, true);
                        regZ80.HL--;
                        regZ80.BC--;
                        regZ80.Parity = (regZ80.BC != 0);
                        return 16;
                    }
                case 0xaa:
                    {//ind
                        var item = mPortOverrides[regZ80.C];
                        byte data = 0;
                        if (item != null && item.Item1 != null)
                            data = (byte)item.Item1(this, new PortAccessReadEventArgs(regZ80.C));
                        SystemMemory.SetMemory(regZ80.HL--, WordSize.OneByte, data, true);
                        regZ80.B -= 1;
                        regZ80.Zero = (regZ80.B == 0);
                        return 16;
                    }
                case 0xab:
                    {//outd
                        byte data = (byte)(SystemMemory.GetMemory(regZ80.HL--, WordSize.OneByte, true));
                        var item = mPortOverrides[regZ80.C];
                        if (item != null && item.Item2 != null)
                            item.Item2(this, new PortAccessWriteEventArgs(regZ80.C, data));
                        regZ80.B -= 1;
                        regZ80.Zero = (regZ80.B == 0);
                        return 16;
                    }
                case 0xb0:
                    {//ldir
                        while (true)
                        {
                            byte data = (byte)SystemMemory.GetMemory(regZ80.HL++, WordSize.OneByte, true);
                            SystemMemory.SetMemory(regZ80.DE++, WordSize.OneByte, data, true);
                            regZ80.BC--;
                            if (regZ80.BC == 0)
                                break;
                        }
                        regZ80.PC++;
                        return 16;
                    }
                case 0xb1:
                    {//cpir
                        byte a = regZ80.A;
                        while (true)
                        {
                            byte b = (byte)SystemMemory.GetMemory(regZ80.HL, WordSize.OneByte, true);
                            cpi8(a, b);
                            regZ80.BC--;
                            if (regZ80.BC == 0 || regZ80.Zero)
                                break;
                            regZ80.HL++;
                        }
                        regZ80.PC++;
                        return 21;
                    }
                case 0xb2:
                    {//inir
                        while (true)
                        {
                            var item = mPortOverrides[regZ80.C];
                            byte b = 0;
                            if (item != null)
                                b = (byte)item.Item1(this, new PortAccessReadEventArgs(regZ80.C));
                            SystemMemory.SetMemory(regZ80.HL, WordSize.OneByte, b, true);
                            regZ80.B -= 1;
                            if (regZ80.B == 0)
                                break;
                        }
                        regZ80.Zero = true;
                        return 21;
                    }
                case 0xb3:
                    {//otir
                        while (true)
                        {
                            byte b = (byte)SystemMemory.GetMemory(regZ80.HL++, WordSize.OneByte, true);
                            regZ80.B -= 1;
                            var item = mPortOverrides[regZ80.C];
                            if (item != null)
                                item.Item2(this, new PortAccessWriteEventArgs(regZ80.C, b));
                            if (regZ80.B == 0)
                                break;
                        }
                        return 21;
                    }
                case 0xb8:
                    {//lddr
                        while (true)
                        {
                            byte b = (byte)SystemMemory.GetMemory(regZ80.HL--, WordSize.OneByte, true);
                            SystemMemory.SetMemory(regZ80.DE--, WordSize.OneByte, b, true);
                            regZ80.BC -= 1;
                            if (regZ80.BC == 0)
                                break;
                        }
                        return 21;
                    }
                case 0xb9:
                    {//cpdr
                        byte a = regZ80.A;
                        while (true)
                        {
                            byte b = (byte)SystemMemory.GetMemory(regZ80.HL, WordSize.OneByte, true);
                            byte r = (byte)(a - b);
                            regZ80.setFlagH(a, b, r, true);
                            regZ80.setFlagsSZP(r);
                            regZ80.HL--;
                            regZ80.BC--;
                            if (regZ80.BC == 0 || regZ80.Zero)
                                break;
                        }
                        return 21;
                    }
                case 0xba:
                    {//indr
                        while (true)
                        {
                            byte b = 0;
                            var item = mPortOverrides[regZ80.C];
                            if (item != null)
                                b = (byte)item.Item1(this, new PortAccessReadEventArgs(regZ80.C));
                            SystemMemory.SetMemory(regZ80.HL, WordSize.OneByte, b, true);

                            regZ80.B -= 1;
                            if (regZ80.B == 0)
                                break;
                        }
                        regZ80.Zero = true;
                        return 21;
                    }
                case 0xbb:
                    {//otdr
                        while (true)
                        {
                            byte b = (byte)SystemMemory.GetMemory(regZ80.HL--, WordSize.OneByte, true);
                            regZ80.B -= 1;
                            var item = mPortOverrides[regZ80.C];
                            if (item != null)
                                item.Item2(this, new PortAccessWriteEventArgs(regZ80.C, b));
                            if (regZ80.B == 0)
                                break;
                        }
                        return 21;
                    }
                default:
                    throw new Exception();
            }//switch
        }
    }
}