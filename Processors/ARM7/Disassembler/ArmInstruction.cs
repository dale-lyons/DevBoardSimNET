using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace ARM7.Disassembler
{
    enum ShiftKind { NONE, LSL, LSR, ASR, ROR, RRX };

    struct NameValue
    {
        public string name;	// a component of the opcode name
        public uint code;	// bits to set in the assembled instruction
        public NameValue(string n, uint v)
        {
            name = n; code = v;
        }
    };

    public struct ValCodePair
    {
        public int val;
        public char code;
    }

    // used for disassembling
    struct ArmEncoding
    {
        public uint mask;
        public uint value;
        public ArmInstructionTemplate op;
    };

    // There is one instance of this class for each ARM instruction.
    // The static method Lookup can be used to find the appropriate instance.
    // Once found, the instance contains a string, operandFormats, which
    // specifies how many operands are expected by the instruction and
    // what syntactic forms for the operands are accepted.
    // The operand codes for ARM instructions (not Thumb instructions) are:
    //		R:  register
    //		Z:  register optionally followed by !
    //      X:  register list
    //		P:  operand 2 format (see ARM ref)
    //		L:  label
    //		I:  immediate constant
    //		A:  memory loc (address mode 2)
    //		a:  memory address in format [Rn]
    //		B:	memory loc (address mode 3)
    //		C:	CPSR or SPSR
    //		F:	fields of CPSR or SPSR
    //		f:	immediate constant or register (as used by MSR)
    class ArmInstructionTemplate
    {
        static protected IDictionary<string, ArmInstructionTemplate> opcodeNames;
        static protected IDictionary<string, ArmInstructionTemplate> thumbNames;
        static protected NameValue[] conditions = {
	    new NameValue("",  0xE0000000), new NameValue("EQ",0x00000000),
	    new NameValue("NE",0x10000000), new NameValue("CS",0x20000000),
	    new NameValue("HS",0x20000000), new NameValue("CC",0x30000000),
	    new NameValue("LO",0x30000000), new NameValue("MI",0x40000000),
	    new NameValue("PL",0x50000000), new NameValue("VS",0x60000000),
	    new NameValue("VC",0x70000000), new NameValue("HI",0x80000000),
	    new NameValue("LS",0x90000000), new NameValue("GE",0xA0000000),
	    new NameValue("LT",0xB0000000), new NameValue("GT",0xC0000000),
	    new NameValue("LE",0xD0000000), new NameValue("AL",0xE0000000)
	};

        //added for floating point instructions. Indicates if single or double
        //precision instruction. - dale
        //ie:  fadds - single
        //     faddd - double
        static protected NameValue[] optFPMagnitude = {
	    new NameValue("S",  0x00000A00), new NameValue("D",0x00000B00)
	};
        static protected NameValue[] optsigned = {
	    new NameValue("",  0x00000000), new NameValue("S", 0x00100000)
	};
        static protected NameValue[] addressMode2 = {
		new NameValue("",  0x00000000), new NameValue("B", 0x00400000),
		new NameValue("BT",0x00600000), new NameValue("T", 0x00200000)
	};
        static protected NameValue[] addressMode3L = {
		new NameValue("H",  0x000000B0), new NameValue("SB", 0x000000D0),
		new NameValue("SH",0x000000F0)
	};
        static protected NameValue[] addressMode3S = {
		new NameValue("H",  0x000000B0)
	};
        static protected NameValue[] singleton = {
		new NameValue("",  0x00000000)
	};
        static protected string[] register = {
		"r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7",
		"r8", "r9", "sp",  "fp",  "ip",  "sp",  "lr",  "pc"
	};
        static protected string[] shiftKind = {
		"lsl", "lsr", "asr", "ror"
	};

        // static constructor
        static ArmInstructionTemplate()
        {
            // Initialize all the subclasses so that their
            // opcodes get entered into the lookup table
            // We are also building a table used for disassembly.
            ArmInstructionNop.initializeClass(); // must precede MOV
            ArmInstructionMov.initializeClass();
            ArmInstructionTstCmp.initializeClass();
            ArmInstructionArith.initializeClass();
            ArmInstructionLoadStore2.initializeClass();
            ArmInstructionLoadStore3.initializeClass();
            ArmInstructionLSMult.initializeClass();
            ArmInstructionBranch.initializeClass();
            ArmInstructionBX.initializeClass();
            ArmInstructionAdr.initializeClass();
            ArmInstructionAdrl.initializeClass();
            ArmInstructionMul.initializeClass();
            ArmInstructionMla.initializeClass();
            ArmInstructionUmlal.initializeClass();
            ArmInstructionClz.initializeClass();
            ArmInstructionSwp.initializeClass();
            ArmInstructionMrs.initializeClass();
            ArmInstructionMsr.initializeClass();
            ArmInstructionSwi.initializeClass();

            //Floating point instructions - dale
            ArmInstructionFPArith.initializeClass();
            ArmInstructionFPLSMult.initializeClass();
            ArmInstructionFPLSSingle.initializeClass();
            ArmInstructionFPTransfer.initializeClass();

            //Thumb instructions
            //The opcodes go into a separate Thumb opcode table
            ThumbDP.initializeClass();
            ThumbLoadStore.initializeClass();
            ThumbLoadStoreMultiple.initializeClass();
            ThumbPushPop.initializeClass();
            ThumbBranch.initializeClass();
            ThumbExceptionGen.initializeClass();

            //Debug.WriteLine(String.Format(" {0} opcode combinations created", nextSlot));
        }

        // calling this (or accessing any static member)
        // causes the static constructor to be executed
        static public void ForceInitialization() { }

        static public bool ThumbMode = false;  // affects opcode lookup

        protected string opname;
        protected string operandFormats;
        protected uint code;	// bit pattern for this instruction
        protected ArmInstructionTemplate invertedOp;
        protected ArmInstructionTemplate negatedOp;
        protected ArmInstructionTemplate equivOp;

        // Given an address in memory, this function finds the nearest preceding label
        // and returns a symbolic address relative to that label
        public static string SymbolicOffset(int dest, AddressLabelPair[] cl, int dotPos)
        {
            if (cl == null) return null;
            int len = cl.Length;
            uint d = (uint)dest;
            int last = -1;  // index of the nearest label so far
            int min = 0;
            // binary search of the list of labels
            while (min < len)
            {
                int mid = (min + len) >> 1;
                uint midval = cl[mid].Address;
                if (midval > d)
                {
                    len = mid;
                    continue;
                }
                if (midval == d)
                    return cl[mid].Label;
                last = mid;
                min = mid + 1;
            }
            if (last < 0) return null;
            if ((uint)dotPos > cl[last].Address && dotPos <= dest)
                return String.Format(".+0x{0:X}", dest - dotPos);
            return String.Format("{0}+0x{1:X}", cl[last].Label, d - cl[last].Address);
        }

        public ArmInstructionTemplate(string opname, uint code, string fmt)
        {
            this.opname = opname;
            this.code = code;
            this.operandFormats = fmt;
            invertedOp = negatedOp = equivOp = null;
        }

        // ---------  Properties ------------------

        public string Name { get { return opname; } }

        public uint OPCode { get { return code; } }

        public ArmInstructionTemplate InvertedOp
        {
            get { return invertedOp; }
            set { invertedOp = value; }
        }

        public ArmInstructionTemplate NegatedOp
        {
            get { return negatedOp; }
            set { negatedOp = value; }
        }

        public ArmInstructionTemplate EquivOp
        {
            get { return equivOp; }
            set { equivOp = value; }
        }

        public string OperandFormats { get { return operandFormats; } }

        // ---------------- public methods -----------------------

        public static ArmInstructionTemplate LookupOp(string opname)
        {
            ArmInstructionTemplate result;
            if (opname == null) return null;
            string op = opname.ToUpper();
            if (ThumbMode)
            {
                if (thumbNames == null)
                {
                    Console.WriteLine("thumbNames table unset!\n");
                    throw new Exception("Aargh!");
                }
                thumbNames.TryGetValue(op, out result);
            }
            else
            {
                if (opcodeNames == null)
                {
                    Console.WriteLine("opcodeNames table unset!\n");
                    throw new Exception("Aargh!");
                }
                opcodeNames.TryGetValue(op, out result);
            }
            return result;
        }

        //increased size to 3500 - dale
        static protected ArmEncoding[] reverseMap = new ArmEncoding[5000];
        static protected int nextSlot = 0;  // next unused spot in reverseMap

        // version of addOp for regular ARM instructions
        static protected void addOp(ArmInstructionTemplate at, uint mask)
        {
            if (opcodeNames == null)
                opcodeNames = new Dictionary<string, ArmInstructionTemplate>();
            opcodeNames[at.Name] = at;
            reverseMap[nextSlot].mask = mask;
            reverseMap[nextSlot].value = at.code;
            reverseMap[nextSlot].op = at;
            nextSlot++;
        }

        // version of addOp for Thumb instructions 
        static protected void addOp(ArmInstructionTemplate at)
        {
            if (thumbNames == null)
                thumbNames = new Dictionary<string, ArmInstructionTemplate>();
            thumbNames[at.Name] = at;
        }

        static protected void addNegations(string op1, string op2,
                NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                {
                    string name1 = op1 + p1.name + p2.name;
                    string name2 = op2 + p1.name + p2.name;
                    ArmInstructionTemplate at1 = LookupOp(name1);
                    ArmInstructionTemplate at2 = LookupOp(name2);
                    at1.NegatedOp = at2;
                    at2.NegatedOp = at1;
                }
        }

        static protected void addInversions(string op1, string op2,
                NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                {
                    string name1 = op1 + p1.name + p2.name;
                    string name2 = op2 + p1.name + p2.name;
                    ArmInstructionTemplate at1 = LookupOp(name1);
                    ArmInstructionTemplate at2 = LookupOp(name2);
                    at1.InvertedOp = at2;
                    at2.InvertedOp = at1;
                }
        }

        static protected void addEquivalences(string op1, string op2,
                NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                {
                    string name1 = op1 + p1.name + p2.name;
                    string name2 = op2 + p1.name + p2.name;
                    ArmInstructionTemplate at1 = LookupOp(name1);
                    ArmInstructionTemplate at2 = LookupOp(name2);
                    at1.equivOp = at2;
                    at2.equivOp = at1;
                }
        }


        // each subclass overrides one of the following
        public virtual uint Code()
        { internalError(); return 0x0; }
        public virtual uint Code(uint opnd1)
        { internalError(); return 0x0; }
        public virtual uint Code(int opnd1)
        { internalError(); return 0x0; }
        public virtual uint Code(int opnd1, uint opnd2)
        { internalError(); return 0x0; }
        public virtual uint Code(int ia, int da)
        { internalError(); return 0x0; }
        public virtual uint Code(int opnd1, int opnd2, uint opnd3)
        { internalError(); return 0x0; }
        public virtual uint Code(int opnd1, bool opnd2, uint opnd3)
        { internalError(); return 0x0; }
        public virtual uint Code(int opnd1, int opnd2, int opnd3)
        { internalError(); return 0x0; }
        public virtual uint Code(ValCodePair[] opnds, int numOpnds, out string msg)
        { internalError(); msg = null; return 0x0; }
        public virtual uint Code(int o1, int o2, int o3, int o4)
        { internalError(); return 0x0; }
        public virtual uint Code(int opnd1, int instrAddress, int destAddress, out bool OK)
        { internalError(); OK = true; return 0x0; }
        protected virtual string disassemble(uint value, int pc, AddressLabelPair[] cl)
        { return "<<disassembly not available>>"; }

        // this static method searches the opcodes to find which one has
        // the right opcode bit pattern and then dispatches to that routine.
        static public string Disassemble(uint value, int pc, AddressLabelPair[] cl)
        {
            string result = null;
            int i = 0;
            do
            {
                ArmEncoding ae = reverseMap[i++];
                if (ae.op == null)
                    break;
                else if ((ae.mask & value) == ae.op.code)
                    result = ae.op.disassemble(value, pc, cl);
            } while (result == null);
            return result;
        }

        static protected string shifterOperand(uint val)
        {
            if ((val & 0x02000000) != 0)
            {
                // it's a 32-bit immediate operand
                int shift = (int)((val & 0xf00) >> 7);
                val &= 0xff;
                // rotate val right by shift places
                while (shift > 0)
                {
                    if ((val & 0x1) != 0)
                        val = (val >> 1) | 0x80000000;
                    else
                        val >>= 1;
                    shift--;
                }
                return String.Format("#{0}", val);
            }
            string Rm = register[(int)(val & 0x0f)];
            if ((val & 0xff0) == 0)  // direct register operand
                return Rm;
            string sk = shiftKind[(int)((val >> 5) & 0x3)];
            if ((val & 0x10) == 0)
            {
                // it's an immediate shift
                int shiftImm = (int)((val & 0xf80) >> 7);
                if (shiftImm == 0)
                {
                    if ((val & 0x60) == 0x60)
                        return String.Format("{0}, rrx", Rm);
                    shiftImm = 32;
                }
                return String.Format("{0}, {1} #{2}", Rm, sk, shiftImm);
            }
            else
            {
                // it's a register shift
                string Rs = register[(int)((val >> 8) & 0xf)];
                return String.Format("{0}, {1} {2}", Rm, sk, Rs);
            }
        }

        // Decodes Addressing Modes 2 and 3
        static protected string addressingMode(uint val)
        {
            string pm = ((val & 0x800000) == 0) ? "-" : "";
            string Rn = register[(int)((val >> 16) & 0xf)];
            string wb = ((val & 0x200000) == 0) ? "" : "!";
            string Rm = register[(int)(val & 0xf)];
            int offset = (int)(val & 0xfff);
            int shiftImm = (int)((val >> 7) & 0x1f);
            string sk = shiftKind[(int)((val >> 5) & 0x3)];

            // We will select an appropriate format string for the
            // String.Format function.  In this format, we will assume
            // the necessary arguments have been set, assuming
            //    {0} == Rn
            //    {1} == pm == +/- (sign of the additive offset / reg)
            //    {2} == offset
            //    {3} == wb == ! or nothing (for writeback)
            //    {4} == Rm
            //    {5} == sk == shiftkind (one of lsl, lsr ... etc)
            //    {6} == shiftImm == shift immediate

            string fmt = null;

            // assume addressing mode 2 for now
            if ((val & 0x2000000) == 0)
            {
                // immediate offset/index
                fmt = ((val & 0x1000000) != 0) ?
                    "[{0}, #{1}{2}]{3}" : "[{0}], #{1}{2}";
            }

            if (fmt == null && (val & 0xff0) == 0)
            {
                // register offset/index
                fmt = ((val & 0x1000000) != 0) ?
                    "[{0}, {1}{4}]{3}" : "[{0}], {1}{4}";
            }

            if (fmt == null && (val & 0x10) == 0)
            {
                // scaled register offset/index
                if ((val & 0x1000000) != 0)
                {
                    fmt = "[{0}, {1}{4}, {5} #{6}]{3}";
                    if (shiftImm == 0)
                    {
                        if ((val & 0x60) == 0x60)
                            fmt = "[{0}, {1}{4} RRX]{3}";
                        else
                            shiftImm = 32;
                    }
                }
                else
                {
                    fmt = "[{0}], {1}{4}, {5}{6}";
                    if (shiftImm == 0)
                    {
                        if ((val & 0x60) == 0x60)
                            fmt = "[{0}], {1}{4}, RRX";
                        else
                            shiftImm = 32;
                    }
                }
            }

            // Addressing mode 3
            if (fmt == null)
            {
                if ((val & 0x60) == 0x0 || (val & 0x100040) == 0x04)
                    return null;	// extended instruction space
                if ((val & 0x400000) != 0)
                {
                    offset = (int)((val >> 4) & 0xf0 | (val & 0x0f));
                    fmt = ((val & 0x1000000) != 0) ?
                         "[{0}, #{1}{2}]{3}" : "[{0}], #{1}{2}";
                }
                else
                    fmt = ((val & 0x1000000) != 0) ?
                        "[{0}, {1}{4}]{3}" : "[{0}], {1}{4}";
            }
            if (fmt != null)
                return String.Format(fmt, Rn, pm, offset, wb, Rm, sk, shiftImm);
            return null;
        }

        // Decodes Addressing Modes 2 and 3 when address is relative to pc
        static protected string symbolicAddress(uint val, int dotPos, AddressLabelPair[] cl)
        {
            int rn = (int)((val >> 16) & 0xf);
            if (rn != 15) return null;
            bool isNeg = (val & 0x800000) == 0;
            int offset;

            if ((val & 0xE0F0000) == 0x40F0000)
            {
                // Addressing mode 2, immediate offset/index
                offset = (int)(val & 0xfff);
            }
            else
                if ((val & 0xE4F0090) == 0x04F0090)
                {
                    // Addressing mode 3, immediate offset/index
                    offset = (int)((val >> 4) & 0xf0 | (val & 0x0f));
                }
                else
                    return null;
            return SymbolicOffset(isNeg ? dotPos + 8 - offset : dotPos + 8 + offset, cl, dotPos);
        }

        static protected string addressingMode4(uint val)
        {
            uint rb = 0x1;
            int rn = 0;
            StringBuilder sb = new StringBuilder(20);
            string sep = "{";
            val |= 0x50000;  // sentinel value
            val &= 0x5ffff;
            for (; ; )
            {
                // find first reg in range
                while ((val & rb) == 0)
                {
                    rb <<= 1; rn++;
                }
                if (rn > 15) break;
                int rm = rn;
                // find last reg in range
                while ((val & rb) != 0)
                {
                    rb <<= 1; rn++;
                }
                if (rn > 15) rn = 16;
                sb.Append(sep);
                sb.Append(register[rm]);
                if ((rn - rm) > 1)
                {
                    sb.Append((rn - rm) > 2 ? "-" : ",");
                    sb.Append(register[rn - 1]);
                }
                sep = ",";
            }
            if (sb.Length == 0)
                return "{}";
            sb.Append("}");
            return sb.ToString();
        }

        public static bool isSingle(uint opcode)
        {
            return ((opcode & 0x00000f00) == 0x00000a00);
        }

        protected void internalError()
        {
            throw new AsmException(
                "instruction={0}, this method should not be called", opname);
        }
    }

    // subclass for ... instructions
    class ArmInstructionRP : ArmInstructionTemplate
    {
        public ArmInstructionRP(string opname, uint code) :
            base(opname, code, "RP") { }
    }

    // subclass for ... instructions
    class ArmInstructionR : ArmInstructionTemplate
    {
        public ArmInstructionR(string opname, uint code) :
            base(opname, code, "R") { }
    }

    // subclass for MOV and MVN instructions
    class ArmInstructionMov : ArmInstructionRP
    {
        static public void initializeClass()
        {
            addOp("MOV", 0x01A00000, conditions, optsigned);
            addOp("MVN", 0x01E00000, conditions, optsigned);
            addInversions("MOV", "MVN", conditions, optsigned);
        }

        public ArmInstructionMov(string opname, uint code) :
            base(opname, code) { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                {
                    addOp(new ArmInstructionMov(
                        op + p1.name + p2.name, code | p1.code | p2.code), 0xfdf00000);
                }
        }

        public override uint Code(int opnd1, uint opnd2)
        {
            // special logic for MOV and MVN operands!!
            uint result = code;
            result |= (uint)(opnd1 << 12);
            result |= opnd2;
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            if ((val & 0x02000090) == 0x00000090) return null;  // extended op
            return String.Format("{0,-7} {1}, {2}",
                opname, register[(int)((val >> 12) & 0xf)], shifterOperand(val));
        }
    }

    // subclass for test and compare instructions
    class ArmInstructionTstCmp : ArmInstructionRP
    {
        static public void initializeClass()
        {
            addOp("TST", 0x01100000, conditions, optsigned);
            addOp("TEQ", 0x01300000, conditions, optsigned);
            addOp("CMP", 0x01500000, conditions, singleton);
            addOp("CMN", 0x01700000, conditions, singleton);
            addNegations("CMP", "CMN", conditions, singleton);
        }

        public ArmInstructionTstCmp(string opname, uint code) :
            base(opname, code) { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                    addOp(new ArmInstructionTstCmp(op + p1.name + p2.name,
                        code | p1.code | p2.code), 0xFDF00000);
        }


        public override uint Code(int opnd1, uint opnd2)
        {
            uint result = code;
            result |= (uint)(opnd1 << 16);	// Rn
            result |= opnd2;
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            if ((val & 0x02000090) == 0x00000090) return null;  // extended op
            return String.Format("{0,-7} {1}, {2}",
                opname, register[(int)((val >> 16) & 0xf)], shifterOperand(val));
        }
    }

    // subclass for most arithmetic & logical instructions
    class ArmInstructionArith : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("ADD", 0x00800000, conditions, optsigned);
            addOp("ADC", 0x00A00000, conditions, optsigned);
            addOp("SUB", 0x00400000, conditions, optsigned);
            addOp("SBC", 0x00C00000, conditions, optsigned);
            addOp("RSB", 0x00600000, conditions, optsigned);
            addOp("RSC", 0x00E00000, conditions, optsigned);
            addOp("AND", 0x00000000, conditions, optsigned);
            addOp("EOR", 0x00200000, conditions, optsigned);
            addOp("ORR", 0x01800000, conditions, optsigned);
            addOp("BIC", 0x01c00000, conditions, optsigned);		// corrected 21-Dec-05
            addNegations("ADD", "SUB", conditions, optsigned);
            addInversions("ADC", "SBC", conditions, optsigned);	// corrected 2-Sept-05
            addInversions("AND", "BIC", conditions, optsigned);
        }

        public ArmInstructionArith(string opname, uint code) :
            base(opname, code, "RRP") { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                    addOp(new ArmInstructionArith(op + p1.name + p2.name,
                        code | p1.code | p2.code), 0xFDF00000);
        }

        public override uint Code(int opnd1, int opnd2, uint opnd3)
        {
            uint result = code;
            result |= (uint)(opnd2 << 16);	// Rn
            result |= (uint)(opnd1 << 12);	// Rd
            result |= opnd3;				// shifter operand
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            if ((val & 0x02000090) == 0x00000090) return null;  // extended op
            return String.Format("{0,-7} {1}, {2}, {3}",
                opname, register[(int)((val >> 12) & 0xf)],
                register[(int)((val >> 16) & 0xf)], shifterOperand(val));
        }
    }

    // subclass for load & store instructions which use Addressing Mode 2
    // for their memory operands
    class ArmInstructionLoadStore2 : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("LDR", 0x04100000, conditions, addressMode2);
            addOp("STR", 0x04000000, conditions, addressMode2);
            addEquivalences("LDR", "MOV", conditions, singleton);
        }

        public ArmInstructionLoadStore2(string opname, uint code) :
            base(opname, code, "RA") { }

        // returns true if opcode is one of LDRBT, LDRT, STRBT, STRT
        // -- these instructions have restrictions on the addressing mode
        public bool postIndexedOnly
        {
            get { return ((code & 0x00200000) != 0); }
        }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                    addOp(new ArmInstructionLoadStore2(op + p1.name + p2.name,
                        code | p1.code | p2.code), 0xFC500000);
        }

        public override uint Code(int opnd1, uint opnd2)
        {
            uint result = code;
            // LDR, LDRB, LDRBT, LDRT,
            // STR, STRB, STRBT, STRT instructions
            result |= (uint)(opnd1 << 12);	// Rd
            result |= opnd2;				// Rn + addr_mode
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            if ((val & 0x02000090) == 0x00000090) return null;  // extended op
            string am = symbolicAddress(val, pc, cl);
            if (am == null)
                am = addressingMode(val);
            if (am == null)
                return null;
            return String.Format("{0,-7} {1}, {2}",
                opname, register[(int)((val >> 12) & 0xf)], am);
        }
    }

    // subclass for load & store instructions which use Addressing Mode 3
    // for their memory operands
    class ArmInstructionLoadStore3 : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("LDR", 0x00100000, conditions, addressMode3L);
            addOp("STR", 0x00000000, conditions, addressMode3S);
        }

        public ArmInstructionLoadStore3(string opname, uint code) :
            base(opname, code, "RB") { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                    addOp(new ArmInstructionLoadStore3(op + p1.name + p2.name,
                        code | p1.code | p2.code), 0xFC5000F0);
        }

        public override uint Code(int opnd1, uint opnd2)
        {
            uint result = code;
            // LDRH, LDRSB, LDRSH, STRH instructions
            result |= (uint)(opnd1 << 12);	// Rd
            result |= opnd2;				// Rn + addr_mode
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            if ((val & 0x02000090) == 0x00000090) return null;  // extended op
            string am = symbolicAddress(val, pc, cl);
            if (am == null)
                am = addressingMode(val);
            if (am == null)
                return null;
            return String.Format("{0,-7} {1}, {2}",
                opname, register[(int)((val >> 12) & 0xf)], am);
        }
    }


    // subclass for load & store multiple instructions
    class ArmInstructionLSMult : ArmInstructionTemplate
    {
        static protected NameValue[] lsModes = {
	    new NameValue("IA",0x08800000), new NameValue("IB",0x09800000),
	    new NameValue("DA",0x08000000), new NameValue("DB",0x09000000)
	};
        static protected NameValue[] stmModes = {
	    new NameValue("FA",0x09800000), new NameValue("FD",0x09000000),
	    new NameValue("EA",0x08800000), new NameValue("ED",0x08000000)
	};
        static protected NameValue[] ldmModes = {
	    new NameValue("FA",0x08100000), new NameValue("FD",0x08900000),
	    new NameValue("EA",0x09100000), new NameValue("ED",0x09900000)
	};

        static public void initializeClass()
        {
            addOp("LDM", 0x08100000, conditions, ldmModes);
            addOp("LDM", 0x08100000, conditions, lsModes);
            addOp("STM", 0x08000000, conditions, stmModes);
            addOp("STM", 0x08000000, conditions, lsModes);
        }

        public ArmInstructionLSMult(string opname, uint code) :
            base(opname, code, "ZX") { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                    addOp(new ArmInstructionLSMult(op + p1.name + p2.name,
                        code | p1.code | p2.code), 0xFE100000);
        }

        public override uint Code(int opnd1, bool opnd2, uint opnd3)
        {
            uint result = code;
            result |= (uint)(opnd1 << 16);	// Rn
            if (opnd2)
                result |= (uint)(1 << 21);	// W bit
            result |= (uint)opnd3;			// register list
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            string wb = (val & 0x200000) != 0 ? "!" : "";
            return String.Format("{0,-7} {1}{2}, {3}",
                opname, register[(int)((val >> 26) & 0xf)], wb, addressingMode4(val));
        }
    }

    // subclass for BX and BLX instruction
    // Note: only the second format for BLX (taking a register operand) is supported
    class ArmInstructionBX : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("BX", 0x012FFF10, conditions);
            addOp("BLX", 0x012FFF30, conditions);
        }

        public ArmInstructionBX(string opname, uint code) :
            base(opname, code, "R") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionBX(op + p1.name, code | p1.code), 0xfffffff0);
        }

        public override uint Code(int regNum)
        {
            return code | ((uint)regNum & 0x0f);
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return String.Format("{0,-7} {1}",
                opname, register[(int)(val & 0xf)]);
        }
    }

    // subclass for branch instructions
    class ArmInstructionBranch : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("B", 0x0A000000, conditions);
            addOp("BL", 0x0B000000, conditions);
        }

        public ArmInstructionBranch(string opname, uint code) :
            base(opname, code, "L") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionBranch(
                    op + p1.name, code | p1.code), 0xfe000000);
        }

        public override uint Code(int instrAddress, int destAddress)
        {
            int offset = destAddress - instrAddress - 8;
            offset >>= 2;
            uint i24 = ((uint)offset & 0x00FFFFFF);
            return code | i24;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            int offset = (int)((val & 0xffffff) << 8);
            offset >>= 6;
            offset += pc + 8;
            string target = SymbolicOffset(offset, cl, pc);
            return String.Format(
                (target == null) ? "{0,-7} 0x{1:X}" : "{0,-7} {2}",
                opname, offset, target);
        }
    }

    // subclass for ADR pseudoinstructions
    class ArmInstructionAdr : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("ADR", 0x00000000, conditions);
        }

        public ArmInstructionAdr(string opname, uint code) :
            base(opname, code, "RL") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionAdr(op + p1.name, code | p1.code), 0);
        }

        public override uint Code(int opnd1, int instrAddress, int destAddress, out bool OK)
        {
            uint result = code;
            int offset = destAddress - instrAddress - 8;
            if (offset < 0)
            {	// use SUB
                result |= 0x024F0000;	// SUB *,PC,#0
                offset = -offset;
            }
            else				// use ADD
                result |= 0x028F0000;	// ADD *,PC,#0
            if ((offset & 0x3) == 0)
            {
                result |= 0xF00;	// rotate right by 30 places
                offset >>= 2;		// use word offset
            }
            if (offset > 255)
            {
                OK = false;
                return result;
            }
            result |= (uint)(opnd1 << 12);		// Rd
            result |= (uint)(offset & 0xFF);	// #offset
            OK = true;
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return null;
        }
    }

    // subclass for ADRL pseudoinstructions
    class ArmInstructionAdrl : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("ADRL", 0x00000000, conditions);
        }

        public ArmInstructionAdrl(string opname, uint code) :
            base(opname, code, "RL") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionAdrl(op + p1.name, code | p1.code), 0);
        }

        public ulong CodeLong(int opnd1, int instrAddress, int destAddress, out bool OK)
        {
            uint result1 = code;
            uint result2 = code;
            int offset = destAddress - instrAddress - 8;
            if (offset < 0)
            {	// use two SUBs
                result1 |= 0x024F0C00;	// SUB *,PC,#0,24
                result2 |= 0x02400000;	// SUB *,*,#0
                offset = -offset;
            }
            else
            {			// use two ADDs
                result1 |= 0x028F0C00;	// ADD *,PC,#0,24
                result2 |= 0x02800000;	// ADD *,*,#0
            }
            if ((offset & 0x3) == 0)
            {
                result1 ^= 0xC00;
                result1 |= 0xB00;	// becomes ADD/SUB *,PC,#0,22
                result2 |= 0xF00;	// becomes ADD/SUB *,*,#0,30
                offset >>= 2;		// use word offset
            }
            if (offset > 32767)
            {
                OK = false;
                return (ulong)0;
            }
            if (offset <= 255)
            {
                result1 = 0x01A00000;	// first op becomes a NOP
                result2 |= 0xF0000;		// second op uses PC
            }
            else
            {
                result1 |= (uint)(opnd1 << 12);			// Rd
                result1 |= (uint)(offset >> 8) & 0xFF;	// # high 8 bits of offset
                result2 |= (uint)(opnd1 << 16);			// Rd
            }
            result2 |= (uint)(opnd1 << 12);			// Rd
            result2 |= (uint)(offset & 0xFF);		// # low 8 bits of offset
            OK = true;
            return ((ulong)result2 << 32) | result1;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return null;
        }
    }

    // subclass for multiply instruction
    class ArmInstructionMul : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("MUL", 0x00000090, conditions, optsigned);
        }

        public ArmInstructionMul(string opname, uint code) :
            base(opname, code, "RRR") { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                    addOp(new ArmInstructionMul(op + p1.name + p2.name,
                        code | p1.code | p2.code), 0xfff000f0);
        }

        public override uint Code(int opnd1, int opnd2, int opnd3)
        {
            uint result = code;
            result |= (uint)(opnd1 << 16);	// Rd
            result |= (uint)(opnd3 << 8);	// Rs
            result |= (uint)opnd2;			// Rm
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return String.Format("{0,-7} {1}, {2}, {3}",
                opname, register[(int)((val >> 16) & 0xf)],
                register[(int)(val & 0xf)], register[(int)((val >> 8) & 0xf)]);
        }
    }

    // subclass for MLA instruction
    class ArmInstructionMla : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("MLA", 0x00200090, conditions, optsigned);
            addOp("UMLAL", 0x00A00090, conditions, optsigned);
        }

        public ArmInstructionMla(string opname, uint code) :
            base(opname, code, "RRRR") { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                    addOp(new ArmInstructionMla(op + p1.name + p2.name,
                        code | p1.code | p2.code), 0xfff000f0);
        }

        public override uint Code(int opnd1, int opnd2, int opnd3, int opnd4)
        {
            uint result = code;
            result |= (uint)(opnd1 << 16);	// Rd
            result |= (uint)(opnd4 << 12);	// Rn
            result |= (uint)(opnd3 << 8);	// Rs
            result |= (uint)opnd2;		// Rm
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return String.Format("{0,-7} {1}, {2}, {3}, {4}",
                opname, register[(int)((val >> 16) & 0xf)],
                register[(int)(val & 0xf)], register[(int)((val >> 8) & 0xf)],
                register[(int)((val >> 12) & 0xf)]);
        }
    }

    // subclass for UMLAL, UMULL instructions
    class ArmInstructionUmlal : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("UMLAL", 0x00A00090, conditions, optsigned);
            addOp("UMULL", 0x00800090, conditions, optsigned);
            addOp("SMLAL", 0x00e00090, conditions, optsigned);
            addOp("SMULL", 0x00c00090, conditions, optsigned);
        }

        public ArmInstructionUmlal(string opname, uint code) :
            base(opname, code, "RRRR") { }

        // two suffixes combined with the basic opcode
        static protected void addOp(string op, uint code,
                    NameValue[] suffix1, NameValue[] suffix2)
        {
            foreach (NameValue p1 in suffix1)
                foreach (NameValue p2 in suffix2)
                    addOp(new ArmInstructionUmlal(op + p1.name + p2.name,
                        code | p1.code | p2.code), 0xfff000f0);
        }

        public override uint Code(int opnd1, int opnd2, int opnd3, int opnd4)
        {
            uint result = code;
            result |= (uint)(opnd2 << 16);	// RdHi
            result |= (uint)(opnd1 << 12);	// RdLo
            result |= (uint)(opnd4 << 8);	// Rs
            result |= (uint)opnd3;		// Rm
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return String.Format("{0,-7} {1}, {2}, {3}, {4}",
                opname, register[(int)((val >> 12) & 0xf)],
                register[(int)((val >> 16) & 0xf)], register[(int)(val & 0xf)],
                register[(int)((val >> 8) & 0xf)]);
        }
    }

    // subclass for CLZ instruction
    class ArmInstructionClz : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("CLZ", 0x016f0F10, conditions);
        }

        public ArmInstructionClz(string opname, uint code) :
            base(opname, code, "RR") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionClz(
                    op + p1.name, code | p1.code), 0xffff0ff0);
        }

        public override uint Code(int opnd1, int opnd2)
        {
            uint result = code;
            result |= (uint)(opnd1 << 12);	// Rd
            result |= (uint)opnd2;			// Rm
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return String.Format("{0,-7} {1}, {2}",
                opname, register[(int)((val >> 12) & 0xf)], register[(int)(val & 0xf)]);
        }
    }

    // subclass for SWP instruction
    class ArmInstructionSwp : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("SWP", 0x01000090, conditions);
        }

        public ArmInstructionSwp(string opname, uint code) :
            base(opname, code, "RRa") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionSwp(op + p1.name, code | p1.code), 0xffb00ff0);
        }

        public override uint Code(int opnd1, int opnd2, int opnd3)
        {
            uint result = code;
            result |= (uint)(opnd1 << 12);	// Rd
            result |= (uint)(opnd3 << 16);	// Rn
            result |= (uint)opnd2;			// Rm
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return String.Format("{0,-7} {1}, {2}, [{3}]",
                opname, register[(int)((val >> 12) & 0xf)], register[(int)(val & 0xf)],
                    register[(int)((val >> 16) & 0xf)]);
        }
    }

    // subclass for MRS instruction
    class ArmInstructionMrs : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("MRS", 0x010F0000, conditions);
        }

        public ArmInstructionMrs(string opname, uint code) :
            base(opname, code, "RC") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionMrs(op + p1.name, code | p1.code), 0xffbf0fff);
        }

        // r == 0 to specify CPSR, and r == 1 for SPSR
        public override uint Code(int opnd1, int r)
        {
            uint result = code;
            result |= (uint)(opnd1 << 12);	// Rd
            result |= (uint)(r << 22);	// CPSR/SPSR selector
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return String.Format("{0,-7} {1}, {2}",
                opname, register[(int)((val >> 12) & 0xf)],
                    (val & 0x400000) == 0 ? "CPSR" : "SPSR");
        }
    }

    // subclass for MSR instruction
    class ArmInstructionMsr : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            //modified base opcode below. One bit was off - dale
            //addOp("MSR", 0x0320F000, conditions);
            addOp("MSR", 0x0120F000, conditions);
        }

        public ArmInstructionMsr(string opname, uint code) :
            base(opname, code, "Ff") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionMsr(op + p1.name, code | p1.code), 0xfdb0f000);
        }

        // note: the R value is included in fieldMask
        public override uint Code(int fieldMask, int opnd1)
        {
            uint result = code;
            result |= (uint)(fieldMask << 16);
            result |= (uint)opnd1;	// register or immediate operand
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            StringBuilder sb = new StringBuilder(20);
            sb.Append(String.Format("{0,-7} ", opname));
            sb.Append((val & 0x400000) == 0 ? "CPSR_" : "SPSR_");
            if ((val & 0x10000) != 0) sb.Append("c");
            if ((val & 0x20000) != 0) sb.Append("x");
            if ((val & 0x40000) != 0) sb.Append("s");
            if ((val & 0x80000) != 0) sb.Append("f");
            sb.Append(", ");
            if ((val & 0x2000000) == 0)
                sb.Append(register[(int)(val & 0xf)]);
            else
                sb.Append(addressingMode(val));
            return sb.ToString(); ;
        }
    }

    // subclass for SWI instruction
    class ArmInstructionSwi : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("SWI", 0x0F000000, conditions);
        }

        public ArmInstructionSwi(string opname, uint code) :
            base(opname, code, "I") { }

        // one suffix combined with the basic opcode
        static protected void addOp(string op, uint code, NameValue[] suffix1)
        {
            foreach (NameValue p1 in suffix1)
                addOp(new ArmInstructionSwi(op + p1.name, code | p1.code), 0xff000000);
        }

        public override uint Code(int opnd1)
        {
            uint result = code;
            result |= (uint)opnd1;			// immed_24
            return result;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return String.Format("{0,-7} 0x{1:X}", opname, (int)(val & 0xffffff));
        }
    }

    // subclass for NOP instruction
    class ArmInstructionNop : ArmInstructionTemplate
    {
        static public void initializeClass()
        {
            addOp("NOP", 0x01A00000);  // i.e. MOV R0,R0
        }

        public ArmInstructionNop(string opname, uint code) :
            base(opname, code, "") { }

        static void addOp(string op, uint code)
        {
            addOp(new ArmInstructionNop(op, code), 0xffffffff);
        }

        public override uint Code()
        {
            return code;
        }

        protected override string disassemble(uint val, int pc, AddressLabelPair[] cl)
        {
            return opname;
        }
    }
}