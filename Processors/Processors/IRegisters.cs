using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public interface IRegisters : ICloneable
    {
        uint PC { get; set; }
        //uint FetchRegister(string reg);
        //bool TryFetchRegister(string reg, out uint addr);
        void SetSingleRegister(string reg, uint val);
        void SetSingleRegister(byte reg, uint val);
        void SetDoubleRegister(string reg, uint val);
        void SetDoubleRegister(byte reg, uint val);

        uint GetSingleRegister(string reg);
        uint GetSingleRegister(byte reg);
        uint GetDoubleRegister(string reg);
        uint GetDoubleRegister(byte reg);

        bool FetchFlag(string flag);
        void SetFlag(string flag, bool state);
        bool[] Difference(IRegisters org);
    }
}