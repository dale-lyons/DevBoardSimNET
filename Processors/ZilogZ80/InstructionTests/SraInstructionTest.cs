using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZilogZ80.InstructionTests
{
    public class SraInstructionTest : RotateInstructionTest
    {
        public SraInstructionTest() : base("Sra", 0xcb, 0x28) { }
    }
}