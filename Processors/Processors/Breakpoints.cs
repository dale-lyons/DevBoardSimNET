using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public class BreakPointChangedArgs : EventArgs
    {
    }

    public class Breakpoints
    {
//        public delegate void BreakpointChanged(object sender, BreakPointChangedArgs args);
//        public event BreakpointChanged BreakpointChangedEvent;

        private Dictionary<uint, Breakpoint> mBreakpoints = new Dictionary<uint, Breakpoint>();
        public void Clear() { mBreakpoints.Clear(); }
        private IProcessor mProcessor;

        public Breakpoints(IProcessor processor)
        {
            mProcessor = processor;
        }

        public void Remove(uint addr)
        {
            if (!mBreakpoints.ContainsKey(addr))
                return;
            mBreakpoints.Remove(addr);
        }
        public void Add(uint addr, bool temporary)
        {
            if (mBreakpoints.ContainsKey(addr))
                return;
            mBreakpoints[addr] = new Breakpoint(addr, temporary);

        }

        public Breakpoint Breakpoint(uint addr)
        {
            if (!mBreakpoints.ContainsKey(addr))
                return null;
            return mBreakpoints[addr];
        }

        public bool Contains(uint addr)
        {
            return mBreakpoints.ContainsKey(addr);
        }
        public bool CodeHit(uint addr)
        {
            if (!mBreakpoints.ContainsKey(addr))
                return false;
            if(mBreakpoints[addr].Temporary)
                mBreakpoints.Remove(addr);

            return true;
        }

    }//Breakpoints
}