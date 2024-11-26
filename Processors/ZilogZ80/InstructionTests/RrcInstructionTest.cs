using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZilogZ80.InstructionTests
{
    public class RrcInstructionTest : RotateInstructionTest
    {
        public RrcInstructionTest() : base("Rrc", 0xcb, 0x08) { }
    }
}
