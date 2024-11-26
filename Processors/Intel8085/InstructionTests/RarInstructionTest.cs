using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class RarInstructionTest : RotateInstructionTest
    {
        public RarInstructionTest() : base("Rar", 0x1f) { }
    }
}