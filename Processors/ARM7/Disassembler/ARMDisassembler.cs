using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace ARM7.Disassembler
{
    public class ARMDisassembler : IDisassembler
    {
        public int Align(uint pos, ISystemMemory code)
        {
            return 0;
        }

        public CodeLine ProcessInstruction(uint startAddr, ISystemMemory code, out uint newAddr)
        {
            uint opcode = code.GetMemory(startAddr, WordSize.FourByte, false);
            var str = ArmInstructionTemplate.Disassemble(opcode, (int)startAddr, null);
            if (str == null)
                str = "Nop";

            newAddr = startAddr + 4;
            var cl = new CodeLine(str, (ushort)startAddr, 4);
            cl.CodeLineType = CodeLine.CodeLineTypes.Code;
            return cl;
        }
    }
}