using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace ARM7.Disassembler
{

    // subclass for Floating Point Data-Processing instructions
    class ArmInstructionFPArith : ArmInstructionTemplate
    {
        static private readonly string threeB = "BBB";      //instructions with 3 register operands of either type
        static private readonly string twoB = "BB";         //instructions with 2 register operands of either type
        static private readonly string oneB = "B";          //instructions with 1 register operand of either type

        //the shift values for each single precision operand register, low bit only
        static readonly int[] lowBitShift = { 22, 5, 7 };

        //the shift values for each single precision operand register, the high 4 bits
        //(Negative shift means shift right)
        static readonly int[] highNibbleShift = { 11, -1, 15 };

        //the shift values for each double precision operand register, all 4 bits
        static readonly int[] registerShift = { 12, 0, 16 };

        //page C3-2 ARM Architecture Reference Manual
        //Format of Floating Point Data Processing instructions
        //cccc 1110 pDqr nnnn dddd aaaa NsM0 mmmm
        //
        // cccc - is the condition code as per the normal instruction set
        // pqrs - the instruction code
        // nnnn - is the first operand register number(double precision)
        //      - the high 4 bits of a single precision register number
        // dddd - is the destination operand register number(double precision)
        //      - the high 4 bits of a single precision register number
        // mmmm - is the second operand register number(double precision)
        //      - the high 4 bits of a single precision register number
        // NDM  - these are the bottom low bit of the Fn,Fd,Fm register if single precision
        // aaaa - the precision of the instruction(1010 - single, 1011 - double)
        static public void initializeClass()
        {
            addOp("FMAC", 0x0E000000, optFPMagnitude, conditions, threeB);
            addOp("FNMAC", 0x0E000040, optFPMagnitude, conditions, threeB);
            addOp("FMSC", 0x0E100000, optFPMagnitude, conditions, threeB);
            addOp("FNMSC", 0x0E100040, optFPMagnitude, conditions, threeB);
            addOp("FMUL", 0x0E200000, optFPMagnitude, conditions, threeB);
            addOp("FADD", 0x0E300000, optFPMagnitude, conditions, threeB);
            addOp("FSUB", 0x0E300040, optFPMagnitude, conditions, threeB);
            addOp("FDIV", 0x0E800000, optFPMagnitude, conditions, threeB);

            addOp("FCPY", 0x0EB00040, optFPMagnitude, conditions, twoB);
            addOp("FABS", 0x0EB000C0, optFPMagnitude, conditions, twoB);
            addOp("FNEG", 0x0EB10040, optFPMagnitude, conditions, twoB);
            addOp("FSQRT", 0x0EB100C0, optFPMagnitude, conditions, twoB);
            addOp("FCMP", 0x0EB40040, optFPMagnitude, conditions, twoB);
            addOp("FCMPE", 0x0EB400C0, optFPMagnitude, conditions, twoB);

            addOp("FCMPZ", 0x0EB50040, optFPMagnitude, conditions, oneB);
            addOp("FCMPEZ", 0x0EB500C0, optFPMagnitude, conditions, oneB);

            addOp("FCVTDS", 0x0EB70AC0, singleton, conditions, "DS");
            addOp("FCVTSD", 0x0EB70BC0, singleton, conditions, "SD");

            addOp("FUITO", 0x0EB80040, optFPMagnitude, conditions, "BS");
            addOp("FSITO", 0x0EB800C0, optFPMagnitude, conditions, "BS");
            addOp("FTOUI", 0x0EBC0040, optFPMagnitude, conditions, "SB");
            addOp("FTOUIZ", 0x0EBC00C0, optFPMagnitude, conditions, "SB");
            addOp("FTOSI", 0x0EBD0040, optFPMagnitude, conditions, "SB");
            addOp("FTOSIZ", 0x0EBD00C0, optFPMagnitude, conditions, "SB");

        }

        public ArmInstructionFPArith(string opname, uint code, string operands) : base(opname, code, operands) { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1, NameValue[] suffix2, string operands)
        {
            foreach (NameValue p1 in suffix1)
            {
                foreach (NameValue p2 in suffix2)
                {
                    string newOP = op + p1.name + p2.name;
                    //System.Diagnostics.Debug.WriteLine("Adding FP op:" + newOP);
                    addOp(new ArmInstructionFPArith(newOP, code | p1.code | p2.code, "#" + operands), 0xFDF00000);
                }
            }
        }

        //This function encodes the register numbers into the opcode.
        //The number of registers to encode is in the input array, so this function can handle 1-3 registers.
        //The static arrays indicate the shift values of the register bits to locate the register number in the opcode.
        //
        //This function uses the operandFormat string to determine if the register is single or double along with the
        //precision of the instruction using the following rules:
        //   1) If the operandFormat is a S, treat as single
        //   2) If the operandFormat is a D, treat as double
        //   3) Otherwise(it must be a B), treat according to the precision of the instruction
        //
        // These rules are applied for each operandFormat char and operand register number.
        //
        // Note that the number of operandFormat chars must be identical to the number of registers in the array.
        //(Minus the leading # symbol)
        private uint encodeCode(params uint[] operands)
        {
            //get a copy of the operandFormat string minus the leading # symbol
            string operandFormats = this.OperandFormats.Substring(1);

            //better be same as number of entries in array
            System.Diagnostics.Debug.Assert(operandFormats.Length == operands.Length);

            //get the base opcode for this instruction
            uint result = code;

            //and determine if this instruction is single or double precision
            bool single = isSingle(this.OPCode);

            //going to loop over the input array registers, get a character from the format string
            int count = 0;
            foreach (char chr in operandFormats)
            {
                //Apply rules here, determine if we treat this register as a double or single precision register
                if ((chr == 'B' && single) || chr == 'S')
                {
                    //make sure single precision register number is sane
                    System.Diagnostics.Debug.Assert(operands[count] <= 32);

                    //single precision register numbers are split between the low bit and high nibble which are
                    //placed in separate areas of opcode. See format in class description for more info.

                    //shift the low bit of the register into the low bit position for this operand.
                    result |= ((operands[count] & 0x01) << lowBitShift[count]);

                    //shift the high 4 bits of the register into the high bits position for this operand.
                    //Note that if the shift is negative means shift right.
                    result |= highNibbleShift[count] >= 0 ? ((operands[count] & 0x1e) << highNibbleShift[count]) : ((operands[count] & 0x1e) >> (-highNibbleShift[count]));

                }
                else
                {
                    //make sure double precision register number is sane
                    System.Diagnostics.Debug.Assert(operands[count] <= 16);

                    //double precision registers are easy, all 4 bits are together, just need a single shift.
                    result |= (uint)(operands[count] << registerShift[count]);
                }
                //next operand
                count++;

            }
            return result;
        }//encodeCode

        public override uint Code(ValCodePair[] opnds, int count, out string msg)
        {
            msg = null;
            switch (count)
            {
                case 3:
                    //Build opcode for FP instructions with 3 operands.
                    //ie FADDD Dd,Dn,Dm
                    return encodeCode((uint)opnds[0].val, (uint)opnds[2].val, (uint)opnds[1].val);
                case 2:
                    //Build opcode for FP instructions with 2 operands.
                    //ie FCPYDS Sd,Sm
                    return encodeCode((uint)opnds[0].val, (uint)opnds[1].val);
                case 1:
                    //Build opcode for FP instructions with 1 operand.
                    //ie FCMPZS Sd
                    return encodeCode((uint)opnds[0].val);
            }//switch
            return 0;
        }//Code
    }//class ArmInstructionFPArith


    // subclass for Floating Point load & store multiple instructions
    class ArmInstructionFPLSMult : ArmInstructionTemplate
    {
        static protected NameValue[] optUnknownMagnitude = {
        new NameValue("",  0x00000000), new NameValue("X",0x00000001)
    };
        static protected NameValue[] fldmModes = {
        new NameValue("IA",0x00800000),new NameValue("DB",0x01000000),
        new NameValue("FD",0x00800000),new NameValue("EA",0x01000000)
    };
        static protected NameValue[] fstmModes = {
        new NameValue("IA",0x01000000),new NameValue("DB",0x00800000),
        new NameValue("FD",0x01000000),new NameValue("EA",0x00800000)
    };

        static public void initializeClass()
        {
            addOp("FSTM", 0x0C000000, fstmModes, optFPMagnitude, conditions);
            addOp("FSTM", 0x0C000B01, fstmModes, optUnknownMagnitude, conditions);
            addOp("FLDM", 0x0C100000, fldmModes, optFPMagnitude, conditions);
            addOp("FLDM", 0x0C100B01, fldmModes, optUnknownMagnitude, conditions);
        }//initializeClass

        public ArmInstructionFPLSMult(string opname, uint code) : base(opname, code, "#UX") { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix0, NameValue[] suffix2, NameValue[] suffix3)
        {
            foreach (NameValue p0 in suffix0)
            {
                foreach (NameValue p2 in suffix2)
                {
                    foreach (NameValue p3 in suffix3)
                    {
                        string newOP = op + p0.name + p2.name + p3.name;
                        //System.Diagnostics.Debug.WriteLine("Adding FP op:" + newOP);
                        addOp(new ArmInstructionFPLSMult(newOP, code | p0.code | p2.code | p3.code), 0xFE100000);
                    }
                }
            }
        }

        //[0]:base register (leftmost bit set means update flag true)
        //[1]:count
        //[2]:start
        public override uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        {
            uint result = code;
            msg = null;

            bool updateFlag;
            uint baseRegister;
            updateFlag = (opnds[0].val < 0);  // encoded by leftmost bit (sign bit)
            baseRegister = (uint)(opnds[0].val & 0x1f);

            uint registerCount = (uint)(opnds[1].val >> 8);
            uint startRegister = (uint)(opnds[1].val & 0x1F);

            if (isSingle(this.OPCode))
            {
                //shift the low bit of the register into the low bit position for this operand.
                result |= ((startRegister & 0x01) << 22);

                //shift the high 4 bits of the register into the high bits position for this operand.
                //Note that if the shift is negative means shift right.
                result |= ((startRegister & 0x1e) << 11);
                result |= registerCount;            // register count
            }
            else
            {
                //double precision registers are easy, all 4 bits are together, just need a single shift.
                result |= (uint)(startRegister << 12);
                result |= registerCount * 2;            // register count
            }
            if (updateFlag)
            {
                result |= (uint)(1 << 21);  // W bit
            }
            result |= (uint)(baseRegister << 16);   // Rn
            return result;
        }//Code
    }//class ArmInstructionFPLSMult



    // subclass for Floating Point load & store single instructions
    class ArmInstructionFPLSSingle : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("FST", 0x0D000000, optFPMagnitude, conditions);
            addOp("FLD", 0x0D100000, optFPMagnitude, conditions);
        }

        public ArmInstructionFPLSSingle(string opname, uint code) : base(opname, code, "#BA") { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
            {
                foreach (NameValue p2 in suffix2)
                {
                    string newOP = op + p1.name + p2.name;
                    //System.Diagnostics.Debug.WriteLine("Adding FP op:" + newOP);
                    addOp(new ArmInstructionFPLSSingle(newOP, code | p1.code | p2.code), 0xFE100000);
                }
            }
        }

        //[0]:fpregister
        //[1]:offset
        public override uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        {
            uint result = code;
            msg = null;

            uint fpRegister = (uint)opnds[0].val;
            uint offset = (uint)opnds[1].val;

            if (isSingle(this.OPCode))
            {
                //shift the low bit of the register into the low bit position for this operand.
                result |= ((fpRegister & 0x01) << 22);

                //shift the high 4 bits of the register into the high bits position for this operand.
                //Note that if the shift is negative means shift right.
                result |= ((fpRegister & 0x1e) << 11);
            }
            else
            {
                //double precision registers are easy, all 4 bits are together, just need a single shift.
                result |= (uint)(fpRegister << 12);
            }
            result |= (uint)offset;

            return result;
        }//Code
    }//class ArmInstructionFPLSSingle


    // subclass for Floating Point transfer instructions
    class ArmInstructionFPTransfer : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            //VFPS9 - extension instructions
            addOp("FMDRR", 0x0C400B10, conditions, "DRR");
            addOp("FMRRD", 0x0C500B10, conditions, "RRD");
            addOp("FMSRR", 0x0C400A10, conditions, "YRR");
            addOp("FMRRS", 0x0C500A10, conditions, "RRY");
            //
            addOp("FMSR", 0x0E000A10, conditions, "SR");
            addOp("FMRS", 0x0E100A10, conditions, "RS");
            addOp("FMDLR", 0x0D000000, conditions, "DR");
            addOp("FMRLD", 0x0D000000, conditions, "RD");
            addOp("FMDHR", 0x0E200B10, conditions, "DR");
            addOp("FMRDH", 0x0D000000, conditions, "RD");
            addOp("FMXR", 0x0EE00A10, conditions, "HR");
            addOp("FMRX", 0x0EF00A10, conditions, "RH");

            addOp("FMSTAT", 0x0EF1FA10, conditions, "");
        }//initializeClass

        public ArmInstructionFPTransfer(string opname, uint code, string operandFormat) : base(opname, code, operandFormat) { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1, string operandFormat)
        {
            foreach (NameValue p1 in suffix1)
            {
                string newOP = op + p1.name;
                //System.Diagnostics.Debug.WriteLine("Adding FP op:" + newOP);
                addOp(new ArmInstructionFPTransfer(newOP, code | p1.code, "#" + operandFormat), 0xFE100000);
            }//foreach
        }

        public override uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        {
            msg = null;
            switch (numOpnds)
            {
                case 2:
                    return encode2(opnds[0].val, opnds[1].val);
                case 3:
                    return encode3(opnds[0].val, opnds[1].val, opnds[2].val);
            }
            return 0;
        }

        private uint encode3(int opnd1, int opnd2, int opnd3)
        {
            uint result = code;
            bool LBit = ((code & 0x00100000) != 0x00000000);

            if (isSingle(this.OPCode))
            {
                if (!LBit)
                {
                    //shift the low bit of the register into the low bit position for this operand.
                    result |= (((uint)opnd1 & 0x01) << 5);
                    //shift the high 4 bits of the register into the high bits position for this operand.
                    //Note that if the shift is negative means shift right.
                    result |= (((uint)opnd1) >> 1);
                    result |= (uint)(opnd2 << 12);
                    result |= (uint)(opnd3 << 16);
                }
                else
                {
                    result |= (uint)(opnd1 << 12);
                    result |= (uint)(opnd2 << 16);
                    //shift the low bit of the register into the low bit position for this operand.
                    result |= (((uint)opnd3 & 0x01) << 5);
                    //shift the high 4 bits of the register into the high bits position for this operand.
                    //Note that if the shift is negative means shift right.
                    result |= (((uint)opnd3) >> 1);
                }
            }
            else
            {
                if (!LBit)
                {
                    result |= (uint)(opnd1);
                    result |= (uint)(opnd2 << 12);
                    result |= (uint)(opnd3 << 16);
                }
                else
                {
                    result |= (uint)(opnd1 << 12);
                    result |= (uint)(opnd2 << 16);
                    result |= (uint)(opnd3);
                }
            }
            return result;
        }

        //offset - opnd3
        //dest - opnd2
        //fpregister - opnd1
        private uint encode2(int opnd1, int opnd2)
        {
            uint result = code;
            switch (this.OperandFormats.Substring(1))
            {
                case "RS":
                    //shift the low bit of the register into the low bit position for this operand.
                    result |= (((uint)opnd2 & 0x01) << 7);

                    //shift the high 4 bits of the register into the high bits position for this operand.
                    //Note that if the shift is negative means shift right.
                    result |= (((uint)opnd2 & 0x1e) << 15);
                    result |= (((uint)opnd1) << 12);
                    break;

                case "SR":
                    //shift the low bit of the register into the low bit position for this operand.
                    result |= (((uint)opnd1 & 0x01) << 7);

                    //shift the high 4 bits of the register into the high bits position for this operand.
                    //Note that if the shift is negative means shift right.
                    result |= (((uint)opnd1 & 0x1e) << 15);
                    result |= (((uint)opnd2) << 12);
                    break;

                case "DR":
                case "HR":
                    //shift the low bit of the register into the low bit position for this operand.
                    result |= (((uint)opnd1) << 16);
                    result |= (((uint)opnd2) << 12);
                    break;

                case "RD":
                case "RH":
                    //shift the low bit of the register into the low bit position for this operand.
                    result |= (((uint)opnd1) << 12);
                    result |= (((uint)opnd2) << 16);
                    break;
            }
            return result;
        } //Code
    } //class ArmInstructionFPTransfer

} // end of namespace