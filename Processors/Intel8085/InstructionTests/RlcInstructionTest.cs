using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class RlcInstructionTest : RotateInstructionTest
    {
        public RlcInstructionTest() : base("Rlc", 0x07) { }
    }
}