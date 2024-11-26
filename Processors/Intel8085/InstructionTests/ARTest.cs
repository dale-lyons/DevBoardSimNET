using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using System.Threading;
using Processors;
using System.Windows.Forms;

namespace Intel8085.InstructionTests
{
    /// <summary>
    /// Accumulator Register Test
    /// add r,adc r, cmp r, sbb r, sub r, xra r
    /// </summary>
    public class ARTest : TestBase
    {
        private byte[] mFlags;
        public ARTest(string name, byte opcode, byte[] flags)
        {
            mFlags = flags;
            if (mFlags == null)
                mFlags = nullFlags;

            Name = name;
            Opcode = opcode;
        }

    }
}