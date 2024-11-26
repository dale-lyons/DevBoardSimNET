using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace M68HC11
{
    public class RegistersM68HC11 : IRegisters
    {
        private byte[] buff2 = new byte[2];

        public uint PC { get; set; }
        public ushort SP { get; set; }

        public byte A { get; set; }
        public byte B { get; set; }

        public ushort D
        {
            get
            {
                buff2[1] = A;
                buff2[0] = B;
                ushort res = BitConverter.ToUInt16(buff2, 0);
                return res;
            }
            set
            {
                byte[] buff = BitConverter.GetBytes(value);
                A = buff[1];
                B = buff[0];
            }
        }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public byte CCR
        {
            get
            {
                byte ret = 0;
                ret |= Carry ?    (byte)0x01:  (byte)0x00;
                ret |= Overflow ? (byte)0x02 : (byte)0x00;
                ret |= Zero ?     (byte)0x04 : (byte)0x00;
                ret |= Negative ? (byte)0x08 : (byte)0x00;
                ret |= IMask ?    (byte)0x10 : (byte)0x00;
                ret |= HalfC ?    (byte)0x20 : (byte)0x00;
                ret |= XMask ?    (byte)0x40 : (byte)0x00;
                ret |= StopDisable ? (byte)0x80 : (byte)0x00;
                return ret;
            }
            set
            {
                Carry =    ((value & 0x01) != 0);
                Overflow = ((value & 0x01) != 0);
                Zero =     ((value & 0x01) != 0);
                Negative = ((value & 0x01) != 0);
                IMask =    ((value & 0x01) != 0);
                HalfC =    ((value & 0x01) != 0);
                XMask =    ((value & 0x01) != 0);
                StopDisable = ((value & 0x01) != 0);
            }
        }

        public bool Carry { get; set; }             //0x01
        public bool Overflow { get; set; }          //0x02
        public bool Zero { get; set; }              //0x04
        public bool Negative { get; set; }          //0x08
        public bool IMask { get; set; }             //0x10
        public bool HalfC { get; set; }             //0x20
        public bool XMask { get; set; }             //0x40
        public bool StopDisable { get; set; }       //0x80

        public object Clone()
        {
            RegistersM68HC11 ret = new RegistersM68HC11();
            ret.PC = PC;
            ret.SP = SP;
            ret.A = A;
            ret.B = B;
            ret.X = X;
            ret.Y = Y;
            ret.CCR = CCR;
            return ret;
        }

        public bool[] Difference(IRegisters org)
        {
            throw new NotImplementedException();
        }

        public bool FetchFlag(string flag)
        {
            throw new NotImplementedException();
        }

        public bool TryFetchRegister(string reg, out uint addr)
        {
            addr = 0;
            if (string.IsNullOrEmpty(reg))
                return false;
            switch(reg.Trim().ToLower())
            {
                case "x": addr = X; return true;
                case "y": addr = Y; return true;
                case "d": addr = D; return true;
                case "pc": addr = PC; return true;
                case "sp": addr = SP; return true;
                default:
                    return false;
            }
        }

        public uint GetSingleRegister(string reg)
        {
            throw new NotImplementedException();
        }

        public uint GetSingleRegister(byte reg)
        {
            throw new NotImplementedException();
        }

        public void SetFlag(string flag, bool state)
        {
            throw new NotImplementedException();
        }

        public void SetSingleRegister(string reg, uint val) { }
        public void SetSingleRegister(byte reg, uint data) { }
        public void SetDoubleRegister(string reg, uint val) { }
        public void SetDoubleRegister(byte reg, uint data) { }
        public uint GetDoubleRegister(string reg) { return 0; }
        public uint GetDoubleRegister(byte reg) { return 0; }
    }
}
