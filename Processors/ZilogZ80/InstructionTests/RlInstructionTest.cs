using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZilogZ80.InstructionTests
{
    public class RlInstructionTest : RotateInstructionTest
    {
        public RlInstructionTest() : base("Rl", 0xcb, 0x10) { }
    }
}
