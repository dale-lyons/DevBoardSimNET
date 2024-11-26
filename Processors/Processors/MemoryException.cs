using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public class MemoryException : Exception
    {
        public uint Address { get; private set; }
        public MemoryException(string msg, uint addr) : base(msg)
        {
            Address = addr;
        }
    }
}