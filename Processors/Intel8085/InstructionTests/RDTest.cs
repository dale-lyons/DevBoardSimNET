using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Ports;

using Processors;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

namespace Processors
{
    /// <summary>
    /// mvi r,val
    /// </summary>
    public class RDTest : TestBase
    {
        public RDTest(string name, byte opcode)
        {
            Name = name;
            Opcode = opcode;
        }
        public override void PerformTestFlags(TestSettings testSettings, statusUpdate callback)
        {

        }
    }
}