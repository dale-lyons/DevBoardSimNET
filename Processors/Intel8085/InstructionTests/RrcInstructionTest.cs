using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class RrcInstructionTest : RotateInstructionTest
    {
        public RrcInstructionTest() : base("Rrc", 0x0f) { }
    }
}