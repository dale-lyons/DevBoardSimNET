using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;

using Processors;

namespace BenEater
{
    public class RegistersBenEater : IRegisters
    {
        public byte A { get; set; }
        public byte B { get; set; }

        public ushort OUT { get; set; }

        public uint PC { get; set; }

        public bool Carry { get; set; }

        object ICloneable.Clone()
        {
            return new RegistersBenEater();
        }

        public void SetFlag(string flag, bool state)
        {

        }
        public byte GetSingleRegister(int reg)
        {
            return 0;
        }

        public void SetSingleRegister(int reg, byte data) { }

        public bool[] Difference(IRegisters org)
        {
            return null;
        }
        public bool TryFetchRegister(string reg, out uint addr) { addr = 0; return false; }

        public uint FetchRegister(string reg)
        {
            string lreg = reg.Trim().ToLower();
            switch (lreg)
            {
                case "a":
                    return A;
                case "b":
                    return B;
                default: return 0;
            }
        }

        public bool FetchFlag(string flag) { return false; }

        public void SetRegister(string reg, uint val) { }

        //bool IRegisters.TryFetchRegister(string text, out uint addr)
        //{
        //    string reg = text.Trim().ToLower();
        //    switch (reg)
        //    {
        //        case "a":
        //            addr = A;
        //            return true;
        //        case "b":
        //            addr = B;
        //            return true;
        //        case "pc":
        //            addr = (ushort)PC;
        //            return true;
        //    }
        //    addr = 0;
        //    return false;
        //}
    }
}