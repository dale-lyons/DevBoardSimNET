using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Processors;

namespace Motorola6502
{
    public class Registers6502 : IRegisters
    {
        public uint PC { get; set; }
        public byte SP { get; set; }

        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }

        object ICloneable.Clone()
        {
            var ret = new Registers6502();
            ret.PC = this.PC;
            ret.SP = this.SP;
            ret.A = this.A;
            ret.X = this.X;
            ret.Y = this.Y;
            return Y;
        }
        private static byte NegativeFlag = 0x80;
        private static byte OverflowFlag = 0x40;
        private static byte DecimalFlag = 0x08;
        private static byte InterruptDisableFlag = 0x04;
        private static byte ZeroFlag = 0x02;
        private static byte CarryFlag = 0x01;
        private static byte B1Flag = 0x10;
        private static byte B2Flag = 0x20;

        public bool Negative { get; set; }
        public bool Overflow { get; set; }
        public bool Decimal { get; set; }
        public bool InterruptDisable { get; set; }
        public bool Zero { get; set; }
        public bool Carry { get; set; }

        public void SetStatusFlags(byte status)
        {
            Negative = (status & NegativeFlag) != 0;
            Overflow = (status & OverflowFlag) != 0;
            Decimal = (status & DecimalFlag) != 0;
            InterruptDisable = (status & InterruptDisableFlag) != 0;
            Zero = (status & ZeroFlag) != 0;
            Carry = (status & CarryFlag) != 0;
        }

        public byte GetStatusFlags(bool push, bool bflag)
        {
            byte ret = (byte)(Negative ? NegativeFlag : 0);
            ret |= (byte)(Overflow ? OverflowFlag : 0);
            ret |= (byte)(Decimal ? DecimalFlag : 0);
            ret |= (byte)(InterruptDisable ? InterruptDisableFlag : 0);
            ret |= (byte)(Zero ? ZeroFlag : 0);
            ret |= (byte)(Carry ? CarryFlag : 0);

            if(push)
                ret |= B1Flag;

            if (bflag)
                ret |= B2Flag;
            return ret;
        }

    bool[] IRegisters.Difference(IRegisters org)
        {
            return null;
        }

        bool IRegisters.FetchFlag(string flag)
        {
            switch(flag)
            {
                case "N":
                    return Negative;
                case "O":
                    return Overflow;
                case "D":
                    return Decimal;
                case "I":
                    return InterruptDisable;
                case "Z":
                    return Zero;
                case "C":
                    return Carry;
            }
            return false;
        }
        public bool TryFetchRegister(string reg, out uint addr) { addr = 0; return false; }

        public uint GetSingleRegister(byte reg) { return 0; }
        public uint GetSingleRegister(string reg)
        {
            switch(reg)
            {
                case "A":
                    return A;
                case "X":
                    return X;
                case "Y":
                    return Y;
            }
            return 0;
        }

        byte GetSingleRegister(int reg)
        {
            throw new NotImplementedException();
        }

        void IRegisters.SetFlag(string flag, bool state)
        {
            throw new NotImplementedException();
        }
        public uint GetDoubleRegister(string reg) { return 0; }
        public uint GetDoubleRegister(byte reg) { return 0; }

        public void SetSingleRegister(string reg, uint val) { }
        public void SetSingleRegister(byte reg, uint data) { }
        public void SetDoubleRegister(string reg, uint val) { }
        public void SetDoubleRegister(byte reg, uint data) { }
    }
}
