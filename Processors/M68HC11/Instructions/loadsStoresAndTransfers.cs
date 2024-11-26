using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M68HC11
{
    public partial class M68HC11
    {
        public uint loadsStoresAndTransfers(byte opcode)
        {
            ushort addr;
            switch(opcode)
            {
                //LDD
                case 0xcc:
                case 0xdc:
                case 0xfc:
                case 0xec:
                    RegistersM68HC11.D = getAddressForMode(OpcodesAddressingModes.LookupAM(opcode));
                    return 6;
                //Ldab
                case 0xc6:
                case 0xd6:
                case 0xf6:
                case 0xe6:
                    RegistersM68HC11.B = (byte)getAddressForMode(OpcodesAddressingModes.LookupAM(opcode));
                    return 6;
                //Ldaa
                case 0x86:
                case 0x96:
                case 0xa6:
                case 0xb6:
                    RegistersM68HC11.A = (byte)getAddressForMode(OpcodesAddressingModes.LookupAM(opcode));
                    return 6;
            }
            return 0;
        }

        public ushort getAddressForMode(AM mode)
        {
            byte offset = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC, Processors.WordSize.OneByte, true);
            switch (mode)
            {
                case AM.DIR:
                    {
                        ushort addr = (ushort)SystemMemory.GetMemory(RegistersM68HC11.PC, Processors.WordSize.OneByte, true);
                        RegistersM68HC11.PC += 1;
                        return (ushort)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte, true);
                    }
                case AM.IM1:
                    RegistersM68HC11.PC += 1;
                    return offset;
                case AM.INX:
                case AM.INY:
                    {
                        RegistersM68HC11.PC += 1;
                        if (preByte == 0x18)
                            return (ushort)(RegistersM68HC11.Y + offset);
                        else
                            return (ushort)(RegistersM68HC11.X + offset);
                    }
                case AM.EXT:
                    ushort ret = (ushort)SystemMemory.GetMemory(RegistersM68HC11.PC, Processors.WordSize.TwoByte, true);
                    RegistersM68HC11.PC += 2;
                    return ret;
                default:
                    throw new Exception();
            }
        }


    }
}
