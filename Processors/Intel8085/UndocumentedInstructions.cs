using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Processors;

namespace Intel8085
{
    public partial class Intel8085
    {
        private uint Undocumented(byte opcode)
        {
            switch(opcode)
            {
                case (byte)MnemonicEnums.dsub:
                    return dsub(opcode);
                case (byte)MnemonicEnums.arhl:
                    return arhl(opcode);
                case (byte)MnemonicEnums.jk:
                    return jk(opcode);
                case (byte)MnemonicEnums.jnk:
                    return jnk(opcode);
                case (byte)MnemonicEnums.ldhi:
                    return ldhi(opcode);
                case (byte)MnemonicEnums.ldsi:
                    return ldsi(opcode);
                case (byte)MnemonicEnums.lhlx:
                    return lhlx(opcode);
                case (byte)MnemonicEnums.rdel:
                    return rdel(opcode);
                case (byte)MnemonicEnums.rstv:
                    return rstv(opcode);
                case (byte)MnemonicEnums.shlx:
                    return shlx(opcode);
                default:
                    OnInvalidInstruction(opcode, Registers.PC);
                    break;
            }
            return 4;
        }


        private uint dsub(byte opcode)
        {//dsub   hl-bc -> hl
            ushort regHL = Registers8085.HL;
            ushort data = (ushort)-Registers8085.BC;
            Registers8085.HL = add16(regHL, data);
            return 10;
        }

        private uint arhl(byte opcode)
        {//arhl   (hl >> 1 -> hl)
            ushort regHL = Registers8085.HL;
            Registers8085.Carry = ((regHL & 0x0001) != 0);
            ushort sign = (ushort)(regHL & 0x8000);
            regHL >>= 1;
            Registers8085.HL = (ushort)(regHL | sign);
            return 7;
        }

        private uint jk(byte opcode)
        {//jk address
            ushort addr = (ushort)SystemMemory.GetMemory(Registers8085.PC, WordSize.TwoByte, true);
            Registers8085.PC += 2;
            if (!Registers8085.K)
                return 7;
            Registers8085.PC = addr;
            return 10;
        }

        private uint jnk(byte opcode)
        {//jnk address
            ushort addr = (ushort)SystemMemory.GetMemory(Registers8085.PC, WordSize.TwoByte, true);
            Registers8085.PC += 2;
            if (Registers8085.K)
                return 7;
            Registers8085.PC = addr;
            return 10;
        }

        private uint ldhi(byte opcode)
        {//ldhi data8
            byte data = (byte)SystemMemory.GetMemory(Registers8085.PC, WordSize.OneByte, true);
            Registers8085.PC += 1;
            Registers8085.DE = (ushort)(Registers8085.HL + data);
            return 10;
        }

        private uint ldsi(byte opcode)
        {//ldsi data8
            byte data = (byte)SystemMemory.GetMemory(Registers8085.PC, WordSize.OneByte, true);
            Registers8085.PC += 1;
            Registers8085.DE = (ushort)(Registers8085.SP + data);
            return 10;
        }
        private uint lhlx(byte opcode)
        {//lhlx
            ushort addr = Registers8085.DE;
            Registers8085.HL = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, true);
            return 10;
        }
        private uint rdel(byte opcode)
        {//rdel  (rotate de left)
            ushort de = Registers8085.DE;
            bool hibit = ((de & 0x8000) != 0);
            de <<= 1;
            if (Registers8085.Carry)
                de |= 0x0001;
            Registers8085.DE = de;
            Registers8085.Carry = hibit;
            return 10;
        }

        private uint rstv(byte opcode)
        {//rstv
            if (Registers8085.Parity)
            {
                Push((ushort)Registers8085.PC);
                Registers8085.PC = 0x40;
                return 12;
            }
            return 6;
        }
        private uint shlx(byte opcode)
        {//shlx
            ushort addr = Registers8085.DE;
            SystemMemory.SetMemory(addr, WordSize.TwoByte, Registers8085.HL, true);
            return 10;
        }
    }
}