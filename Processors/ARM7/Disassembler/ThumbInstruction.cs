using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace ARM7.Disassembler
{

    // Thumb data processing instructions
    class ThumbDP : ArmInstructionTemplate
    {
        string errMsg;

        static public void initializeClass()
        {
            // Format string is $<Format number> followed by codes where
            // format number is defined in section 6.4.2 of ARM manual.
            // The codes are:
            //   r  = register r0-r7
            //   R  = register r0-r15
            //   P  = pc (i.e. register 15)
            //   S  = sp (i.e. register 13)
            //   3,5,8 = immediate constant using 3/5/8 bits
            //   7  = immediate const using 7 bits divisible by 4
            //   0  = immediate const using 8 bits divisible by 4
            addOp("ADD", 0x42220000, "$1rrr$2rr3$3r8$6rS0$6rP0$7S7$8RR$");
            addOp("SUB", 0x33330000, "$1rrr$2rr3$3r8$7S7$");
            addOp("MOV", 0x60000000, "$3r8$8RR$");
            addOp("CMP", 0x51114280, "$3r8$5rr$8RR$");
            addOp("LSL", 0x00004080, "$4rr5$5rr$");
            addOp("LSR", 0x111140C0, "$4rr5$5rr$");
            addOp("ASR", 0x22224100, "$4rr5$5rr$");
            addOp("MVN", 0x000043C0, "$5rr$");
            addOp("CMN", 0x000042C0, "$5rr$");
            addOp("TST", 0x00004200, "$5rr$");
            addOp("ADC", 0x00004140, "$5rr$");
            addOp("SBC", 0x00004180, "$5rr$");
            addOp("NEG", 0x00004240, "$5rr$");
            addOp("MUL", 0x00004340, "$5rr$");
            addOp("ROR", 0x000041C0, "$5rr$");
            addOp("AND", 0x00004000, "$5rr$");
            addOp("EOR", 0x00004040, "$5rr$");
            addOp("ORR", 0x00004300, "$5rr$");
            addOp("BIC", 0x00004380, "$5rr$");
        }

        public ThumbDP(string opname, uint code, string operands)
            : base(opname, code, operands) { }

        static protected void addOp(string op, uint code, string operands)
        {
            addOp(new ThumbDP(op, code, operands));
        }

        public override uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        {
            int pos = 0;
            msg = null;
            bool singletonFormat = true;
            errMsg = null;
            while (pos >= 0)
            {
                uint r = tryCode(opnds, numOpnds, operandFormats, pos);
                if (r != 0xFFFFFFFF) return r;
                pos = operandFormats.IndexOf('$', pos + 1);
                singletonFormat = false;
            }
            if (singletonFormat && errMsg != null)
                msg = String.Format("invalid operand: {0}", errMsg);
            else
                msg = "invalid operand";
            return 0;
        }

        // Result is 0xFFFFFFFF if the format doesn't match, otherwise a 16 bit
        // encoding of the instruction with its operands
        protected uint tryCode(ValCodePair[] opnds, int numOpnds, string s, int ix)
        {
            if (s[ix++] != '$')
                throw new AsmException("bad format for ThumbDP");
            if (ix >= s.Length) return 0xFFFFFFFF;
            char fmtNum = s[ix++];
            for (int i = 0; i < numOpnds; i++)
            {
                char code = s[ix++];  // operand format code
                string ret = matchCode(opnds[i], code);
                if (ret != null)
                {
                    errMsg = ret;
                    return 0xFFFFFFFF;
                }
            }
            // verify that we have all the operands
            if (s[ix] != '$')
            {
                errMsg = "missing operand(s)";
                return 0xFFFFFFFF;
            }
            switch (fmtNum)
            {
                case '1':  // ADD,SUB
                    return 0x1800 | (uint)(opnds[2].val << 6) | (uint)(opnds[1].val << 3)
                        | (uint)(opnds[0].val) | ((this.code >> 7) & 0x0200);
                case '2':  // ADD,SUB
                    return 0x1C00 | (uint)(opnds[2].val << 6) | (uint)(opnds[1].val << 3)
                        | (uint)(opnds[0].val) | ((this.code >> 7) & 0x0200);
                case '3':  // ADD,SUB,MOV,CMP
                    return 0x2000 | (uint)(opnds[0].val << 8) | (uint)(opnds[1].val)
                        | ((this.code >> 5) & 0x1800);
                case '4':  // LSL,LSR,ASR
                    return (uint)(opnds[2].val << 6) | (uint)(opnds[1].val << 3)
                        | (uint)(opnds[0].val) | (uint)((this.code >> 5) & 0x1800);
                case '5':   // MVN,CMP,CMN,TST,ADC,SBC,NEG,MUL,
                            // LSL,LSR,ASR,ROR,AND,EOR,ORR,BIC
                    return this.code | (uint)(opnds[1].val << 3) | (uint)(opnds[0].val);
                case '6':   // ADD
                    return (uint)(opnds[0].val << 8) | (uint)(opnds[2].val >> 2)
                        | (uint)((opnds[1].val == 15) ? 0xA000 : 0xA800);
                case '7':   // ADD,SUB
                    return 0xB000 | (uint)(opnds[1].val >> 2)
                        | ((this.code >> 9) & 0x80);
                case '8':   // MOV,ADD,CMP
                    uint opc = 0x4400 | (uint)((this.code & 0xF000) >> 20);
                    if (opnds[0].val > 7) opc |= 0x80;
                    if (opnds[1].val > 7) opc |= 0x40;
                    return opc | (uint)(opnds[0].val & 0x7) | (uint)((opnds[1].val << 3) & 0x38);
            }
            return 0xFFFFFFFF;
        }

        protected string matchCode(ValCodePair v, char requiredCode)
        {
            char valcode = v.code;  // valCode is what the user provided
            switch (requiredCode)
            {
                case 'r':  // register r0-r7
                    if (valcode != 'R' || v.val > 7)
                        return "r0-r7 operand expected";
                    break;
                case 'R':  // register r0-r15
                    if (valcode != 'R')
                        return "register operand expected";
                    break;
                case 'P':  // pc (i.e. register 15)
                    if (valcode != 'R' || v.val != 15)
                        return "PC operand expected";
                    break;
                case 'S':  // sp (i.e. register 13)
                    if (valcode != 'R' || v.val != 13)
                        return "SP operand expected";
                    break;
                case '3':
                    if (valcode != 'N' || (v.val & 0xFFFFFFF8) != 0)
                        return "numeric operand in range 0 to 7 expected";
                    break;
                case '5':
                    if (valcode != 'N' || (v.val & 0xFFFFFFE0) != 0)
                        return "numeric operand in range 0 to 31 expected";
                    break;
                case '7':  // must be a number divisible by 4, less than 1024
                    if (valcode != 'N' || (v.val & 0x3) != 0 || v.val < 0 || v.val >= 512)
                        return "numeric operand divisible by 4 in range 0 to 508 expected";
                    break;
                case '8':
                    if (valcode != 'N' || (v.val & 0xFFFFFF00) != 0)
                        return "numeric operand in range 0 to 255 expected";
                    break;
                case '0':  // must be a number divisible by 4, less than 1024
                    if (valcode != 'N' || (v.val & 0x3) != 0 || v.val < 0 || v.val >= 1024)
                        return "numeric operand divisible by 4 in range 0 to 1020 expected";
                    break;
                default:
                    throw new AsmException("unexpected Thumb DP format code: {0}", requiredCode);
            }
            return null;
        }
    }

    // Thumb load and store instructions
    class ThumbLoadStore : ArmInstructionTemplate
    {
        string errMsg;

        static public void initializeClass()
        {
            // Format string is #<Format number> followed by codes where
            // format number corresponds to the four versions of LDR in
            // section 7.1 of the ARM manual.
            // The codes are:
            //   r  = register r0-r7
            //   A  = [r,#<immed_5>*X] where X=1,2,4
            //   B  = [r,r]
            //   C  = [PC,#<immed_8>*4]
            //   D  = [SP,#<immed_8>*4]
            addOp("LDR", 0x00006800, "$1rA$2rB$3rC$4rD$");
            addOp("LDRB", 0x00007C00, "$1rA$2rB$");
            addOp("LDRH", 0x00008A00, "$1rA$2rB$");
            addOp("LDRSB", 0x00007600, "$2rB$");
            addOp("LDRSH", 0x00008E00, "$2rB$");
            addOp("STR", 0x00006000, "$1rA$2rB$4rD$");
            addOp("STRB", 0x00007400, "$1rA$2rB$");
            addOp("STRH", 0x00008200, "$1rA$2rB$");
        }

        public ThumbLoadStore(string opname, uint code, string operands)
            : base(opname, code, operands) { }

        static protected void addOp(string op, uint code, string operands)
        {
            addOp(new ThumbLoadStore(op, code, operands));
        }

        public override uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        {
            int pos = 0;
            msg = null;
            bool singletonFormat = true;
            errMsg = null;
            while (pos >= 0)
            {
                uint r = tryCode(opnds, numOpnds, operandFormats, pos);
                if (r != 0xFFFFFFFF) return r;
                pos = operandFormats.IndexOf('$', pos + 1);
                singletonFormat = false;
            }
            if (singletonFormat && errMsg != null)
                msg = String.Format("invalid operand: {0}", errMsg);
            else
                msg = "invalid operand";
            return 0;
        }

        protected int opndSize()
        {
            switch (this.code & 0xF000)
            {
                case 0x6000: return 4;
                case 0x7000: return 1;
                case 0x8000: return 2;
            }
            return 0;
        }

        // Result is 0xFFFFFFFF if the format doesn't match, otherwise a 16 bit
        // encoding of the instruction with its operands
        protected uint tryCode(ValCodePair[] opnds, int numOpnds, string s, int ix)
        {
            if (s[ix++] != '$')
                throw new AsmException("bad format for ThumbLoadStore");
            if (ix >= s.Length) return 0xFFFFFFFF;
            char fmtNum = s[ix++];
            for (int i = 0; i < numOpnds; i++)
            {
                char code = s[ix++];  // operand format code
                string ret = matchCode(opnds[i], code);
                if (ret != null)
                {
                    errMsg = ret;
                    return 0xFFFFFFFF;
                }
            }
            // verify that we have all the operands
            if (s[ix] != '$')
            {
                errMsg = "missing operand(s)";
                return 0xFFFFFFFF;
            }
            uint baseReg = (uint)(opnds[1].val & 0xFF);
            uint offset = (uint)(opnds[1].val >> 8);
            switch (fmtNum)
            {
                case '1':  // LDR,LDRB,LDRH,STR,STRB,STRH
                    int ops = opndSize();
                    if (ops == 4)
                        offset >>= 2;
                    else if (ops == 2)
                        offset >>= 1;
                    return 0x1800 | (uint)(offset << 6) | (uint)(baseReg << 3)
                        | (uint)(opnds[0].val) | (this.code);
                case '2':  // LDR,LDRB,LDRH,LDRSB,LDRSH,STR,STRB,STRH
                    return 0x5000 | (uint)(offset << 6) | (uint)(baseReg << 3)
                        | (uint)(opnds[0].val) | (this.code & 0x0F00);
                case '3':  // LDR
                    return 0x4800 | (uint)(opnds[0].val << 8) | (uint)(offset >> 2);
                case '4':  // LDR,STR
                    return 0x9000 | (uint)(opnds[0].val << 8) | (uint)(offset >> 2)
                        | (this.code & 0x0800);
            }
            return 0xFFFFFFFF;
        }

        protected string matchCode(ValCodePair v, char requiredCode)
        {
            char valcode = v.code;  // valCode is what the user provided
            switch (requiredCode)
            {
                case 'r':  // register r0-r7
                    if (valcode != 'R' || v.val > 7)
                        return "r0-r7 operand expected";
                    break;
                case 'A':  // [r,#<immed_5>*X]
                    int ops = opndSize();
                    if (valcode != 'A')
                        return "memory operand [reg,offset] expected";
                    int vv = v.val >> 8;
                    if (ops == 4)
                    {
                        if ((vv & 0x3) != 0)
                            return "offset must be divisible by 4";
                        vv >>= 2;
                    }
                    else if (ops == 2)
                    {
                        if ((vv & 0x1) != 0)
                            return "offset must be divisible by 2";
                        vv >>= 1;
                    }
                    if (vv < 0 | vv > 255)
                        return "offset does not fit in 8 bits";
                    break;
                case 'B':  // [r,r]
                    if (valcode != 'B')
                        return "memory operand [reg,reg] expected";
                    if ((v.val & 0xF) > 7 || (v.val >> 8) > 7)
                        return "registers must be in range r0 to r7";
                    break;
                case 'C':  // [pc,#<immed_8>*4]
                    if (valcode != 'A')
                        return "memory operand expected";

                    break;
                case 'D':  // [sp,#<immed_8>*4]
                    if (valcode != 'N' || (v.val & 0xFFFFFFFC) != 0)
                        return "numeric operand in range 0 to 7 expected";
                    break;
                default:
                    throw new AsmException("unexpected Thumb DP format code: {0}", requiredCode);
            }
            return null;
        }
    }

    // Thumb load and store instructions
    class ThumbLoadStoreMultiple : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("LDMIA", 0x0000C800, "$1!Z");
            addOp("STMIA", 0x0000C000, "$1!Z");
        }

        public ThumbLoadStoreMultiple(string opname, uint code, string operands)
            : base(opname, code, operands) { }

        static protected void addOp(string op, uint code, string operands)
        {
            addOp(new ThumbLoadStoreMultiple(op, code, operands));
        }

        public override uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        {
            msg = null;
            if (numOpnds < 2)
                msg = "missing operand";
            else
            if (numOpnds > 2)
                msg = "too many operands";
            else
            if (opnds[0].code != '!')
                msg = "first operand must have <reg>! form";
            else
            if (opnds[0].val > 7)
                msg = "first operand must be r0 to r7";
            else
            if (opnds[1].code != 'Z')
                msg = "second operand must be list of registers";
            else
            if (opnds[1].val > 255)
                msg = "register list must contain r0 to r7 only";
            else
                return this.code | (uint)(opnds[0].val << 8) | (uint)(opnds[1].val);
            return 0;
        }
    }

    // Thumb push and pop instructions
    class ThumbPushPop : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("POP", 0x0000BC00, "$2Z");
            addOp("PUSH", 0x0000B400, "$2Z");
        }

        public ThumbPushPop(string opname, uint code, string operands)
            : base(opname, code, operands) { }

        static protected void addOp(string op, uint code, string operands)
        {
            addOp(new ThumbPushPop(op, code, operands));
        }

        public override uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        {
            msg = null;
            string s = null;
            uint r = 0;
            if (numOpnds < 1)
                msg = "missing operand";
            else if (numOpnds > 1)
                msg = "too many operands";
            else if (opnds[0].code != 'Z')
                msg = "operand must be list of registers";
            else if (opnds[0].val == 0)
                msg = "empty list of registers not allowed";
            else
            {
                uint vv = (uint)(opnds[0].val);
                if (this.code == 0xBC00)
                {  // POP
                    if ((vv & 0x80) != 0)
                    {   // list includes PC
                        r = 0x100; vv &= 0xFF7F;
                        s = "list may contain r0 - r7 and pc only";
                    }
                }
                else
                { // PUSH
                    if ((vv & 0x40) != 0)
                    {   // list includes LR
                        r = 0x100; vv &= 0xFFBF;
                        s = "list may contain r0 - r7 and lr only";
                    }
                }
                if (vv <= 255)
                    return this.code | r | (uint)(opnds[0].val);
                msg = s;
            }
            return 0;
        }
    }

    // Thumb branching instructions
    // Each allowed format begins with '$*' (which is a combination checked by
    // the scanner to see if it's a thumb op that can take an address operand).
    // *  T8 means target address specified by a 8 bits offset
    // *  T1 means target address specified by a 11 bits offset
    // *  T2 means target address specified by a 22 bits offset
    // *  R means register in range r0 to r15
    class ThumbBranch : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("BL", 0x0000E000, "$*T2");
            addOp("BLX", 0x0000E000, "$*T2$*R");
            addOp("BX", 0x00008700, "$*R");
            addOp("B", 0x0000E000, "$*T1");
            foreach (NameValue nv in conditions)
            {
                if (nv.name == "") continue;
                addOp("B" + nv.name, 0x0000B000 | (nv.code >> 20), "$*T8");
            }
        }

        public ThumbBranch(string opname, uint code, string operands)
            : base(opname, code, operands) { }

        static protected void addOp(string op, uint code, string operands)
        {
            addOp(new ThumbBranch(op, code, operands));
        }

        public override uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        {
            msg = null;
            if (numOpnds < 1)
            {
                msg = "missing operand"; return 0;
            }
            if (numOpnds > 1)
            {
                msg = "too many operands"; return 0;
            }
            if (opnds[0].code == 'T')
            {
                if (this.operandFormats[2] != 'T')
                {
                    msg = "invalid operand"; return 0;
                }
                // we expect an offset from the instruction address+4
                int offset = opnds[0].val;
                if ((offset & 1) != 0)
                {
                    msg = "unaligned target address"; return 0;
                }
                offset >>= 1;
                switch (this.operandFormats[3])
                {
                    case '8':
                        if (offset >= -128 && offset <= 127)
                            return this.code | (uint)(offset & 0xFF);
                        msg = "target address is out of range";
                        break;
                    case '1':
                        if (offset >= -1024 && offset <= 1023)
                            return this.code | (uint)(offset & 0x7FF);
                        msg = "target address is out of range";
                        break;
                    case '2':
                        // we return two instructions as a 32 bit number,
                        // and we don't both checking if the offset is OK
                        return 0xF0000000 | (uint)((offset << 5) & 0x7FF0000)
                            | this.code | (uint)(offset & 0x7FF);
                }
            }
            else if (opnds[0].code == 'R')
            {
                int reg = opnds[0].val;
                uint h2 = 0;
                if (reg > 7)
                {
                    h2 = 0x40; reg = reg - 8;
                }
                if (this.operandFormats[2] != 'R')
                    h2 |= 0x0080;
                return 0x4700 | h2 | (uint)(reg << 3);
            }
            else
            {
                msg = "invalid operand";
            }
            return 0;
        }
    }

    // Thumb exception generating instructions
    class ThumbExceptionGen : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("BKPT", 0x0000BE00, "$&8");
            addOp("SWI", 0x0000DF00, "$&8");
        }

        public ThumbExceptionGen(string opname, uint code, string operands)
            : base(opname, code, operands) { }

        static protected void addOp(string op, uint code, string operands)
        {
            addOp(new ThumbExceptionGen(op, code, operands));
        }

        public override uint Code(int opnd)
        {
            return this.code | (uint)(opnd & 0xFF);
        }
    }

} // namespace