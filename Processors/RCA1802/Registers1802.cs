using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Processors;

namespace RCA1802
{
    public class Registers1802 : IRegisters
    {
        public uint PC { get; set; }

        public void SetRegister(string reg, uint val) { }





        public object Clone()
        {
            throw new NotImplementedException();
        }

        public bool TryFetchRegister(string reg, out uint addr)
        {
            throw new NotImplementedException();
        }
    }
}
