using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZilogZ80.InstructionTests
{
    public class RrInstructionTest : RotateInstructionTest
    {
        public RrInstructionTest() : base("Rr", 0xcb, 0x18) { }
    }
}
