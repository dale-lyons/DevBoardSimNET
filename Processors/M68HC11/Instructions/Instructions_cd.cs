using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M68HC11
{
    public partial class M68HC11
    {
        public uint execute_instruction_cd(byte code)
        {
            byte opcode = SystemMemory[RegistersM68HC11.PC];
            uint cycles = 0;
            switch (opcode)
            {
                case 0xce:
                    {//ldy
                        RegistersM68HC11.PC += 1;
                        RegistersM68HC11.Y = loadWord(OpcodesAddressingModes.LookupAM_cd(opcode), opcode, out cycles);
                        return cycles;
                    }
                case 0xee:
                    {//ldx
                        RegistersM68HC11.PC += 1;
                        RegistersM68HC11.X = loadWord(OpcodesAddressingModes.LookupAM_cd(opcode), opcode, out cycles);
                        return cycles;
                    }
                default:
                    throw new Exception();
            }
        }
    }
}