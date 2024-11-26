using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZilogZ80.InstructionTests
{
    public class SlaInstructionTest : RotateInstructionTest
    {
        public SlaInstructionTest() : base("Sla", 0xcb, 0x20) { }
    }
}