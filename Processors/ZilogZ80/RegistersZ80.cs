using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Intel8085;
using Processors;

namespace ZilogZ80
{
    public class RegistersZ80 : Registers8085
    {
        public ushort IX { get; set; }
        public ushort IY { get; set; }
        public byte I { get; set; }
        public byte R { get; set; }

        private ushort AFPrime;
        private ushort BCPrime;
        private ushort DEPrime;
        private ushort HLPrime;

        //public override void setFlagH(byte a, byte b, byte r, bool sub)
        //{
        //    base.setFlagH(a, b, r, sub);
        //}

        public override void setFlagsHN()
        {
            AuxCarry = true;
            Negative = true;
        }

        public override void setFlagV(byte a, byte b, byte r, bool sub)
        {
            if (sub)
                Parity = (a.Bit7() && !b.Bit7() && !r.Bit7())
                     || (!a.Bit7() && b.Bit7() && r.Bit7());
            else
                Parity = (a.Bit7() && b.Bit7() && !r.Bit7())
                     || (!a.Bit7() && !b.Bit7() && r.Bit7());
        }

        //On the Z80, setting the P flag is setting the V(overflow flag)
        public override void setFlagP(byte r) { }

            ////Parity = (a.Bit7() && !b.Bit7() && !r.Bit7())
            ////     || (!a.Bit7() && b.Bit7() && r.Bit7());
            //if (sub)
            //    Parity = (a.Bit7() && !b.Bit7() && !r.Bit7())
            //         || (!a.Bit7() && b.Bit7() && r.Bit7());
            //else
            //    Parity = (a.Bit7() && b.Bit7() && !r.Bit7())
            //         || (!a.Bit7() && !b.Bit7() && r.Bit7());

        public override void setFlagN(bool state)
        {
            Negative = state;
        }

        public override void setFlagAuxCarry(bool state)
        {
            AuxCarry = state;
        }

        //subtract:
        //V = (X7 and !M7 and !R7) or (!X7 and M7 and R7)
        //  Set if a twos complement overflow resulted from the operation; cleared otherwise.
        //add:
        //V = (X7 and M7 and !R7) or (!X7 and !M7 and R7)

        //public override void setFlagP(byte a, byte b, byte r, bool sub)
        //{
        //    if (sub)
        //        Parity = (a.Bit7() && !b.Bit7() && !r.Bit7())
        //             || (!a.Bit7() && b.Bit7() && r.Bit7());
        //    else
        //        Parity = (a.Bit7() && b.Bit7() && !r.Bit7())
        //             || (!a.Bit7() && !b.Bit7() && r.Bit7());
        //}

        public override void setFlagH(byte a, byte b, byte r, bool sub)
        {
            //c->hf = ~(c->a ^ result ^ val) & 0x10;
            if (sub)
            {
                AuxCarry = (!a.Bit3() && b.Bit3())
                        || (b.Bit3() && r.Bit3())
                        || (!a.Bit3() && r.Bit3());

                //byte a1 = (byte)(a & 0x0f);
                //byte b1 = (byte)(b & 0x0f);
                //byte r1 = (byte)(a1 - b1);
                //AuxCarry = ((r1 & 0x10) != 0);
                //byte b1 = (byte)(a ^ r);
                //byte b2 = (byte)(b1 ^ b);
                //byte b3 = (byte)~b2;
                //byte b4 = (byte)(b3 & 0x10);
                //AuxCarry = ((b4 & 0x10) != 0);
                return;
            }
            AuxCarry = (a.Bit3() && b.Bit3())
                     || (a.Bit3() && !r.Bit3())
                     || (b.Bit3() && !r.Bit3());
        }

        public override object Clone()
        {
            var ret = new RegistersZ80();
            ret.BC = this.BC;
            ret.DE = this.DE;
            ret.HL = this.HL;
            ret.SP = this.SP;
            ret.PC = this.PC;
            ret.IX = this.IX;
            ret.IY = this.IY;
            ret.PSW = this.PSW;
            return ret;
        }

        public override uint GetDoubleRegister(string reg)
        {
            switch (reg.Trim().ToLower())
            {
                case "ix":
                    return IX;
                case "iy":
                    return IY;
                default:
                    return base.GetDoubleRegister(reg);
            }
        }

        public override bool[] Difference(IRegisters registers)
        {
            RegistersZ80 org = registers as RegistersZ80;
            bool[] ret = new bool[19];

            for (byte ii = 0; ii <= 7; ii++)
            {
                if (ii == (byte)SingleRegisterEnums.m)
                    continue;
                ret[ii] = (this.GetSingleRegister(ii) != org.GetSingleRegister(ii));
            }

            ret[8] = this.Flags != org.Flags;
            ret[9] = this.SP != org.SP;
            ret[10] = this.PC != org.PC;
            ret[11] = this.IX != org.IX;
            ret[12] = this.IY != org.IY;

            ret[13] = this.Sign != org.Sign;
            ret[14] = this.Carry != org.Carry;
            ret[15] = this.Negative != org.Negative;
            ret[16] = this.Parity != org.Parity;
            ret[17] = this.AuxCarry != org.AuxCarry;
            ret[18] = this.Zero != org.Zero;
            return ret;
        }

        public override void SetDoubleRegister(string reg, uint data)
        {
            switch (reg.Trim().ToLower())
            {
                case "ix":
                    IX = (ushort)data;
                    break;
                case "iy":
                    IY = (ushort)data;
                    break;
                default:
                    base.SetDoubleRegister(reg, data);
                    break;
            }
        }

        public void Switch(bool afOnly)
        {
            ushort tmp;
            if (afOnly)
            {
                tmp = this.PSW;
                this.PSW = AFPrime;
                AFPrime = tmp;
                return;
            }

            tmp = BC;
            BC = BCPrime;
            BCPrime = tmp;

            tmp = this.DE;
            this.DE = DEPrime;
            DEPrime = tmp;

            tmp = this.HL;
            this.HL = HLPrime;
            HLPrime = tmp;
        }//Switch
    }
}