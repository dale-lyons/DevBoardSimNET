using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intel8085;
using Processors;
using System.Windows.Forms;
using System.Diagnostics;

namespace ZilogZ80.InstructionTests
{
    public class RlcInstructionTest : RotateInstructionTest
    {
        public RlcInstructionTest() : base("Rlc", 0xcb, 0x00) { }
    }
}
