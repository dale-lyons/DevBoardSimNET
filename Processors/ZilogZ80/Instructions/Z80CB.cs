using System;
using Processors;

namespace ZilogZ80
{
    public partial class ZilogZ80
    {
        private uint handlecb(byte opcode)
        {
            byte opcode2 = (byte)SystemMemory.GetMemory(Registers.PC++, WordSize.OneByte, true);
            switch (opcode2 & 0xc0)
            {
                case 0x00:
                    return handleShiftRotate(opcode2);
                default:
                    return handlebits(opcode2);
            }
        }
        private uint handlebits(byte opcode2)
        {
            bool? Nflag = null;
            bool? Aflag = null;

            RegistersZ80 regZ80 = Registers as RegistersZ80;
            Func<byte, byte, byte> func;
            byte reg = (byte)(opcode2 & 0x07);
            byte bitPos = (byte)((opcode2 & 0x38) >> 3);
            switch (opcode2 & 0xc0)
            {
                case 0x40: func = testb; Nflag = false; Aflag = true; break;
                case 0x80: func = resetb; break;
                case 0xc0: func = setb; break;
                default:
                    return 0;
            }
            if (reg == 0x06)
            {
                byte data = func(bitPos, (byte)SystemMemory.GetMemory(regZ80.HL, WordSize.OneByte, true));
                SystemMemory.SetMemory(regZ80.HL, WordSize.OneByte, data, true);
                if(Nflag.HasValue)
                    regZ80.setFlagN((bool)Nflag);
                if (Aflag.HasValue)
                    regZ80.AuxCarry = (bool)Aflag;
                return 15;
            }
            else
            {
                regZ80.SetSingleRegister(reg, func(bitPos, (byte)regZ80.GetSingleRegister(reg)));
                if (Nflag.HasValue)
                    regZ80.setFlagN((bool)Nflag);
                if (Aflag.HasValue)
                    regZ80.AuxCarry = (bool)Aflag;
                return 8;
            }
            throw new Exception();
        }

        private uint handleShiftRotate(byte opcode2)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            Func<byte, byte> func;
            byte reg = (byte)(opcode2 & 0x07);
            switch (opcode2 & 0xf8)
            {
                case 0x00: func = rlc; break;
                case 0x08: func = rrc; break;
                case 0x10: func = ral; break;
                case 0x18: func = rar; break;
                case 0x20: func = sla; break;
                case 0x28: func = sra; break;
                case 0x38: func = srl; break;
                default:
                    return 0;
            }
            if (reg == 0x06)
            {
                byte a = (byte)SystemMemory.GetMemory(regZ80.HL, WordSize.OneByte, true);
                byte r = func(a);
                SystemMemory.SetMemory(regZ80.HL, WordSize.OneByte, r, true);
                regZ80.setFlagsSZP(r);
                regZ80.setFlagN(false);
                regZ80.AuxCarry = false;
                regZ80.setParityOverflow(r);
                return 15;
            }
            else
            {
                byte a = (byte)regZ80.GetSingleRegister(reg);
                byte r = func(a);
                regZ80.SetSingleRegister(reg, r);
                regZ80.setFlagsSZP(r);
                regZ80.setFlagN(false);
                regZ80.AuxCarry = false;
                regZ80.setParityOverflow(r);
                return 8;
            }
            throw new Exception();
        }
    }
}