using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public class Breakpoint : IBreakpoint
    {
        private uint mBreakpointAddress;
        public bool Temporary { get; }

        public Breakpoint(uint address, bool temporary)
        {
            mBreakpointAddress = address;
            Temporary = temporary;
        }

        public bool HitTest()
        {
            return false;
        }

    }
}
