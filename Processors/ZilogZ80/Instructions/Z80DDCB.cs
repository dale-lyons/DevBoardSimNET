using System;
using System.Text;
using Microsoft.Win32;
using Processors;

namespace ZilogZ80
{
    public partial class ZilogZ80
    {
        /// <summary>
        /// also handles FDCB
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="indexRegVal"></param>
        /// <param name="setIV"></param>
        /// <returns></returns>
        private uint handleddcb(RegistersZ80 regZ80, byte dd, ushort indexRegVal, setIndexRegDelegate setIV)
        {
            //byte opcode3 = (byte)SystemMemory.GetMemory(Registers.PC + 1, WordSize.OneByte, true);
            byte opcode = (byte)SystemMemory.GetMemory(Registers.PC + 2, WordSize.OneByte, true);
            byte n = (byte)(opcode & 0x0f);

            if (n != 0x06 && n != 0x0e)
                throw new Exception();

            //if (opcode4 == 0x36)
            //    throw new Exception();

            if (opcode < 0x40)
                return handlerotates(regZ80, dd, opcode, indexRegVal, setIV);
            else
                return handlebits(regZ80, dd, opcode, indexRegVal, setIV);
        }

        private uint handlerotates(RegistersZ80 regZ80, byte dd, byte opcode, ushort indexRegVal, setIndexRegDelegate setIV)
        {
            Func<byte, byte> func;
            switch (opcode)
            {
                case 0x06: func = rlc; break;
                case 0x16: func = rl; break;
                case 0x26: func = sla; break;

                case 0x0e: func = rrc; break;
                case 0x1e: func = rar; break;
                case 0x2e: func = sra; break;
                case 0x3e: func = srl; break;
                default:
                    throw new Exception();
            }
            ushort addr = (ushort)(indexRegVal + dd.SignExtend());
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            byte r = func(data);
            SystemMemory.SetMemory(addr, WordSize.OneByte, r, true);
            regZ80.setParityOverflow(r);
            regZ80.setFlagN(false);
            regZ80.setFlagsSZP(r);
            return 23;
        }
        private uint handlebits(RegistersZ80 regZ80,byte dd, byte opcode, ushort indexRegVal, setIndexRegDelegate setIV)
        {
            byte bitpos = (byte)((opcode & 0x38) >> 3);
            ushort addr = (ushort)(indexRegVal + dd.SignExtend());
            byte data = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);

            Func<byte, byte, byte> func;
            switch (opcode & 0xf0)
            {
                case 0x40:
                case 0x50:
                case 0x60:
                case 0x70:
                    testb(bitpos, data);
                    regZ80.AuxCarry = true;
                    return 23;
                case 0x80:
                case 0x90:
                case 0xa0:
                case 0xb0:
                    func = resetb; break;
                case 0xc0:
                case 0xd0:
                case 0xe0:
                case 0xf0:
                    func = setb; break;
                default:
                    throw new Exception();
            }
            byte r = func(bitpos, data);
            SystemMemory.SetMemory(addr, WordSize.OneByte, r, true);
            regZ80.setParityOverflow(r);
            return 23;
        }
    }
}