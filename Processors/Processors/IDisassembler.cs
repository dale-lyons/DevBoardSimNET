using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public interface IDisassembler
    {
        int Align(uint pos, ISystemMemory code);
        CodeLine ProcessInstruction(uint startAddr, ISystemMemory code, out uint newAddr);
    }
}