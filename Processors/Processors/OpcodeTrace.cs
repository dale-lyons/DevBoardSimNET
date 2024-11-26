using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public class OpcodeTrace
    {
        private Queue<uint> mLines = new Queue<uint>();

        public void Feed(uint address)
        {
            mLines.Enqueue(address);
            purge();
        }

        private void purge()
        {
            while(mLines.Count > 512)
                mLines.Dequeue();
        }

        public void Clear()
        {
            mLines.Clear();
        }
        public uint Dequeue()
        {
            return mLines.Dequeue();
        }

        public int Count { get { return mLines.Count; } }
    }
}