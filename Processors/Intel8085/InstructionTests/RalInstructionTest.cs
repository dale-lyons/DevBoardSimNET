using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class RalInstructionTest : RotateInstructionTest
    {
        public RalInstructionTest() : base("Ral", 0x17) { }
    }
}