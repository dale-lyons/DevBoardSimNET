using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using Processors;
using System.Windows.Forms;

namespace Intel8085.InstructionTests
{
    /// <summary>
    /// 2 Register instruction
    /// ie mov r,r
    /// </summary>
    public class RRTest : TestBase
    {
        public RRTest(string name, byte opcode)
        {
            Name = name;
            Opcode = opcode;
        }
        public override void PerformTestFlags(TestSettings testSettings, statusUpdate callback)
        {
        }
    }
}