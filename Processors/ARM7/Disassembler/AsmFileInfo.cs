// File AsmFileInfo.cs
//
// Copyright (c) R. Nigel Horspool,  August 2005
// Further improvements: December 2005 - August 2007

using System;
using System.IO;
using System.Collections.Generic;

namespace ARM7.Disassembler
{

    //public enum CodeType { TextArm = 0, TextThumb = 1, LtorgData = 2, Data = 3 };

    // Keeps track of info while processing an assembler file
    //
    // Explanation of the contents of each field
    // -----------------------------------------
    //
    // The assembler file is divided into three main sections: text, data and
    // bss.  The field currSectionType remembers which section we are currently
    // processing.  Each section is divided into subsections numbered 0 to 8191.
    // (The default subsection for the .text, .data and .bss directives is 0.)
    // The field currSubSection remembers the current subsection.
    // As the assembler file is processed, we are advancing through positions
    // in these subsections.  The position array remembers how far we have got
    // in each subsection; e.g. the element position[SectionType.Data][3]
    // returns the position in subsection 3 of the data section.
    // The field currPosition remembers the position in the current subsection
    // of the current section; its value will be stored in the position array
    // only when the section/subsection changes (and at the end of the file).
    //
    // The localSymTable field keeps track of all labels & symbols defined in
    // the current file.  The globalSymbols field is a reference to a table
    // of externally visible labels defined in all assembler files processed
    // so far.
    //
    // The pendingLabels field is a list of label names which have just
    // been seen in the current assembler file but which have not yet been
    // entered into localSymTable.  A brief delay in entering the labels is
    // needed to handle a situation like this:
    //                  .byte 37
    //         label1:
    //         label2:
    //                  .word 0
    // where we want the label1 and label2 values to be word-aligned offsets
    // but we don't know that word alignment is needed until the .word
    // directive is seen.
    //
    // The literalPool field is a list of constants encountered in the
    // instructions and which will be created in the literal pool in the
    // text segment.  The .ltorg directive causes the literal pool to
    // be deposited at the current point in the text segment.  The
    // literal pool is cleared as far as the programmer is concerned,
    // but we leave the entries in the table so that thet are available
    // for access on pass 2 of assembling the program.
    //
    public class AsmFileInfo : ArmFileInfo
    {
        List<int>[] position;
        SectionType currSectionType;
        CodeType currCodeType;
        int currSubSection; // last N used in .text N or .data N or .bss N
        int currPosition;
        bool thumbMode = false;
        List<string> symbolsDeclaredGlobal; // symbols declared as global
        IDictionary<string, int> numericLabelCount;
        List<NameInt> pendingLabels;
        LiteralSet literalPool;
        int ltorgIdentifier;    // unique code for each ltorg

        // the following fields contain info only after Pass 1 is completed
        int[][] subsectionStart;
        int codePosition;

        public AsmFileInfo(string fileName,
                    IDictionary<string, SyEntry> globalSymbols, IDictionary<string, string> externSymbols)
            : base(fileName, globalSymbols, externSymbols)
        {
            readSourceLines();
            position = new List<int>[3];
            for (int i = 0; i < 3; i++)
            {
                position[i] = new List<int>();
                position[i].Add(0);
            }
            currSectionType = SectionType.Text;
            currCodeType = CodeType.TextArm;
            currSubSection = 0;
            currPosition = 0;
            LocalSymTable = new Dictionary<string, SyEntry>();
            symbolsDeclaredGlobal = new List<string>();
            numericLabelCount = new Dictionary<string, int>();
            pendingLabels = new List<NameInt>();
            literalPool = new LiteralSet();
            ltorgIdentifier = 0;
        }

        // accessor methods

        public int LtorgIdentifier { get { return ltorgIdentifier; } }

        public List<int>[] Position { get { return position; } }

        public int DotValue
        {
            get { return (Pass < 2) ? currPosition : codePosition; }
        }

        public int CodePosition
        {
            get
            {
                if (Pass < 2) throw new AsmException("pass 1 not complete");
                return codePosition;
            }
        }

        public SectionType CurrSectionType { get { return currSectionType; } }

        public CodeType CurrCodeType
        {
            get { return currCodeType; }
            set { currCodeType = value; }
        }

        public bool ThumbMode
        {
            get { return thumbMode; }
            set
            {
                thumbMode = value;
                if (currCodeType == CodeType.TextArm && thumbMode)
                    currCodeType = CodeType.TextThumb;
                else if (currCodeType == CodeType.TextThumb && !thumbMode)
                    currCodeType = CodeType.TextArm;
            }
        }

        public int CurrSubSection { get { return currSubSection; } }

        public LiteralSet LiteralPool { get { return literalPool; } }

        public int[][] SubsectionStart
        {
            get
            {
                if (Pass < 2) throw new AsmException("pass 1 not complete");
                return subsectionStart;
            }
        }

        // end of accessor methods

        private void readSourceLines()
        {
            SourceLine = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(FileName))
                {
                    for (; ; )
                    {
                        string s = sr.ReadLine();
                        if (s == null) break;
                        SourceLine.Add(s);
                    }
                }
                SourceLineType = new SourceLineInfo[SourceLine.Count + 2];
                for (int i = 0; i <= SourceLine.Count; i++)
                    SourceLineType[i] = new SourceLineInfo(0, CodeType.TextArm, 0);

            }
            catch (IOException e)
            {
                throw new AsmException("I/O error when reading file {0}: {1}", FileName, e);
            }
        }

        public void SetSection(SectionType st, int subSectNum)
        {
            if (Pass == 1)
            {
                foreach (NameInt label in pendingLabels)
                    DefineLabel(label);
                pendingLabels.Clear();
            }
            List<int> al = position[(int)currSectionType];
            while (subSectNum >= al.Count)
                al.Add(0);
            al[subSectNum] = currPosition;
            currSectionType = st;
            currSubSection = subSectNum;
            al = position[(int)st];
            if (subSectNum > al.Capacity)
            {
                int k = al.Capacity;
                al.Capacity = subSectNum + 1;
                while (k <= subSectNum)
                    al[k++] = 0;
            }
            currPosition = (int)al[subSectNum];
            if (st == SectionType.Text)
                currCodeType = thumbMode ? CodeType.TextThumb : CodeType.TextArm;
            else
                currCodeType = CodeType.Data;
            if (Pass == 2)
                codePosition = currPosition + subsectionStart[(int)st][subSectNum];
        }

        public void AdjustSubsectionStart(SectionType st, int offset)
        {
            int[] starts = subsectionStart[(int)st];
            for (int i = 0; i < starts.Length; i++)
                starts[i] += offset;
        }

        public void Add(IntLiteral iv)
        {
            AddWord(iv.IntValue);
        }

        public void Add(FloatLiteral fv)
        {
            Add(fv.FloatValue);
        }

        public void Add(DoubleLiteral dv)
        {
            Add(dv.DoubleValue);
        }

        public void Add(StringLiteral sv)
        {
            Add(sv.StringValue);
        }

        public void Add(DelayedIntLiteral di)
        {
            Add(0);
        }

        public void Add(string s)
        {
            foreach (char c in s)
                AddByte((int)c);
        }

        public void Add(ulong code)
        {
            AdvancePosition(0, 4, 0);
            currPosition += 8;
            if (Pass == 2)
            {
                //ProgramSpace.AddMap(FileName, LineNumber, codePosition, 8, currCodeType);
                SourceLineType[LineNumber].NumBytes += 8;
                if (SourceLineType[LineNumber].Address == 0)
                    SourceLineType[LineNumber].Address = codePosition;
                // NOTE: we must store the two words in little-endian order
                ProgramSpace.StoreWord(codePosition, (uint)(code & 0xFFFFFFFF));
                ProgramSpace.StoreWord(codePosition + 4, (uint)(code >> 32));
                codePosition += 8;
            }
        }

        public void Add(uint code)
        {
            AdvancePosition(0, 4, 0);
            currPosition += 4;
            if (Pass == 2)
            {
                //ProgramSpace.AddMap(FileName, LineNumber, codePosition, 4, currCodeType);
                SourceLineType[LineNumber].NumBytes += 4;
                if (SourceLineType[LineNumber].Address == 0)
                    SourceLineType[LineNumber].Address = codePosition;
                ProgramSpace.StoreWord(codePosition, code);
                codePosition += 4;
            }
        }

        public void Add(float code)
        {
            uint cc;
            unsafe
            {
                uint* p = (uint*)&code;
                cc = *p;
            }
            Add(cc);
        }

        public void Add(double code)
        {
            ulong cc;
            unsafe
            {
                ulong* p = (ulong*)&code;
                cc = *p;
            }
            Add(cc);
        }

        public void AddWord(int code)
        {
            Add((uint)code);
        }

        public void AddHalfword(int code)
        {
            AddHalfword((uint)code);
        }

        public void AddHalfword(uint code)
        {
            AdvancePosition(0, 2, 0);
            currPosition += 2;
            if (Pass == 2)
            {
                //ProgramSpace.AddMap(FileName, LineNumber, codePosition, 2, currCodeType);
                SourceLineType[LineNumber].NumBytes += 2;
                if (SourceLineType[LineNumber].Address == 0)
                    SourceLineType[LineNumber].Address = codePosition;
                ProgramSpace.StoreHalfword(codePosition, code);
                codePosition += 2;
            }
        }

        public void AddByte(int code)
        {
            currPosition += 1;
            if (Pass == 2)
            {
                //ProgramSpace.AddMap(FileName, LineNumber, codePosition, 1, currCodeType);
                SourceLineType[LineNumber].NumBytes += 1;
                if (SourceLineType[LineNumber].Address == 0)
                    SourceLineType[LineNumber].Address = codePosition;
                ProgramSpace.StoreByte(codePosition, code);
                codePosition += 1;
            }
        }

        // Advances the position in the current section/subsection
        // by size bytes, after first aligning to an address divisible
        // by align.  That aligned address is returned as the result.
        public int AdvancePosition(int size, int align, int fill)
        {
            if (size < 0)
                throw new AsmException("negative size in AdvancePosition");
            if (align > 1)
            {
                currPosition += align - 1;
                currPosition &= (-align);
            }
            if (Pass == 1)
            {
                foreach (NameInt label in pendingLabels)
                    DefineLabel(label);
                pendingLabels.Clear();
            }
            int result = currPosition;
            currPosition += size;
            if (Pass == 2)
            {
                int newCodePosition = currPosition
                    + subsectionStart[(int)currSectionType][currSubSection];
                //ProgramSpace.AddMap(FileName, LineNumber, codePosition,
                //    newCodePosition - codePosition, currCodeType);
                SourceLineType[LineNumber].NumBytes += newCodePosition - codePosition;
                if (SourceLineType[LineNumber].Address == 0)
                    SourceLineType[LineNumber].Address = codePosition;
                while (codePosition < newCodePosition)
                    ProgramSpace.StoreByte(codePosition++, fill);
            }
            return result;
        }

        public int AdvancePosition(int size, int align)
        {
            return AdvancePosition(size, align, 0);
        }

        public void DefineGlobal(string name)
        {
            if (Pass > 1) return;
            if (!(symbolsDeclaredGlobal.Contains(name)))
                symbolsDeclaredGlobal.Add(name);
        }

        public override void DefineCommSymbol(string name, int size, int align)
        {
            if (Pass > 1) return;
            if (symbolsDeclaredGlobal.Contains(name)) return;
            base.DefineCommSymbol(name, size, align);
        }

        public void NewLabel(string name, int lineNum)
        {
            if (Pass > 1) return;
            pendingLabels.Add(new NameInt(name, lineNum));
        }

        // Defines label at current position in the assembler file
        // It should not be invoked directly ... see comments at
        // start of this class.
        protected void DefineLabel(NameInt lab)
        {
            if (Pass > 1) return;
            string name = lab.name;
            if (Char.IsDigit(name[0]))
            {   // it's a numeric label
                int ordinal;
                numericLabelCount.TryGetValue(name, out ordinal);
                numericLabelCount[name] = ordinal + 1;
                name = String.Format("L{0}/{1}", name, ordinal);
            }
            else if (LocalSymTable.ContainsKey(name))
            {
                ParseError("{0} defined more than once", name);
                return;
            }
            SyEntry newLabel = new SyEntry(name, lab.val,
                currSectionType, currSubSection, currPosition);
            LocalSymTable[name] = newLabel;
        }

        public override SyEntry LookupNumericLabel(string name)
        {
            int len = name.Length;
            char fb = name[len - 1];
            if (fb != 'f' && fb != 'b')
            {
                if (!Char.IsDigit(fb))
                    throw new AsmException("invalid numeric label");
                fb = 'f';
            }
            else
                name = name.Substring(0, len - 1);
            int ordinal = 0;
            int pos = (Pass > 1) ? codePosition : currPosition;
            SyEntry result = null;
            // this is a linear search which would need improving
            // if we want to handle large files
            for (; ; )
            {
                SyEntry next;
                LocalSymTable.TryGetValue(String.Format("L{0}/{1}", name, ordinal),
                    out next);
                if (next == null || next.Value >= pos)
                {
                    if (fb == 'f') result = next;
                    break;
                }
                result = next;
                ordinal++;
            }
            if (Pass > 1 && result == null)
                throw new AsmException("Numeric label {0} not found", name);
            return result;
        }

        public int DefineIntConstant(int value)
        {
            if (Pass == 2)
                return literalPool.Find(value, codePosition);
            literalPool.Add(new IntLiteral(value, currSubSection));
            return 0;
        }

        public int DefineStringConstant(string value)
        {
            if (Pass == 2)
                return literalPool.Find(value, codePosition);
            literalPool.Add(new StringLiteral(value, currSubSection));
            return 0;
        }

        public int DefineFloatConstant(float value)
        {
            if (Pass == 2)
                return literalPool.Find(value, codePosition);
            literalPool.Add(new FloatLiteral(value, currSubSection));
            return 0;
        }

        public int DefineDoubleConstant(double value)
        {
            if (Pass == 2)
                return literalPool.Find(value, codePosition);
            literalPool.Add(new DoubleLiteral(value, currSubSection));
            return 0;
        }

        public void ReserveLtorgSpace()
        {
            if (Pass == 2) return;
            literalPool.Add(new DelayedIntLiteral(currSubSection));

        }

        public void ApplyLtorg()
        {
            if (Pass == 1)
                literalPool.ApplyLtorg(this);
            else
            {   // copy literal values into text segment
                CodeType sv = currCodeType;
                currCodeType = CodeType.LtorgData;
                literalPool.CopyLiterals(this);
                currCodeType = sv;
            }
            ltorgIdentifier++;
        }

        public override void StartPass()
        {
            Pass++;
            ltorgIdentifier = 0;
            if (Pass > 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    List<int> al = position[i];
                    for (int k = 0; k < al.Count; k++)
                        al[k] = 0;
                }
                currSectionType = SectionType.Text;
                currSubSection = 0;
                currPosition = 0;
            }
            SetSection(SectionType.Text, 0);
        }

        protected override void endPass1()
        {
            AdvancePosition(0, 0);
            List<int> al = position[(int)SectionType.Text];
            for (int i = 0; i < al.Count; i++)
            {
                SetSection(SectionType.Text, i);
                ApplyLtorg();
            }
            SetSection(SectionType.Text, 0);
            foreach (string name in symbolsDeclaredGlobal)
            {
                SyEntry sy;
                if (!LocalSymTable.TryGetValue(name, out sy))
                {
                    //				ParseError("{0} was declared global but is undefined",name);
                    continue;
                }
                SyEntry globsy;
                if (!GlobalSymbols.TryGetValue(name, out globsy))
                {
                    GlobalSymbols[name] = sy;
                    continue;
                }
                if (globsy.Kind == SymbolKind.CommSymbol)
                {
                    if (sy.Kind == SymbolKind.CommSymbol)
                    {
                        // merge size and alignment of two COMM definitions
                        globsy.Size = sy.Size;
                        globsy.Align = sy.Align;
                        LocalSymTable[name] = globsy;
                    }
                    else if (sy.Kind == SymbolKind.Label)
                    {
                        // an actual label definition always trumps a COMM definition
                        GlobalSymbols[name] = sy;
                    }
                    else
                        ParseError("{0} [line {1}] cannot be both a COMM symbol and a number",
                            name, sy.LineNumber);
                    continue;
                }
                ParseError("{0} [line {1}] defined in two or more files",
                    name, sy.LineNumber);
            }

            // and now we merge all the subsections into one
            subsectionStart = new int[3][];
            mergeSubSections(SectionType.Text);
            mergeSubSections(SectionType.Data);
            mergeSubSections(SectionType.Bss);
            if (trace)
            {
                Console.WriteLine("After pass 1, text size = {0}", position[0][0]);
                Console.WriteLine("After pass 1, data size = {0}", position[1][0]);
                Console.WriteLine("After pass 1,  bss size = {0}", position[2][0]);
            }
        }

        protected override void endPass2()
        {
            SetSection(SectionType.Text, 0);
            ApplyLtorg();
        }

        private void mergeSubSections(SectionType st)
        {
            int nextOffset = 0;
            List<int> al = position[(int)st];
            if (al == null || al.Count == 0)
                return;
            int[] startPositions = new int[al.Count];
            subsectionStart[(int)st] = startPositions;

            // determine start offsets for each subsection
            for (int k = 0; k < al.Count; k++)
            {
                startPositions[k] = nextOffset;
                if (al[k] == 0) continue;
                int size = al[k];
                nextOffset += size;
                nextOffset = (int)(((uint)nextOffset + 3) & 0xFFFFFFFC);
            }
            // adjust offsets of labels for this section only
            foreach (SyEntry sy in LocalSymTable.Values)
            {
                if (sy.Kind != SymbolKind.Label) continue;
                if (sy.Section != st) continue;
                sy.Value += startPositions[sy.SubSection];
                sy.SubSection = 0;
            }
            // record final size of section after combining subsections
            SectionSize[(int)st] = nextOffset;
            if (st != SectionType.Text)
                return;
            // finally move any literals (they belong to the text section)
            foreach (AsmLiteral pi in literalPool)
            {
                pi.Offset += startPositions[pi.Subsection];
                pi.Subsection = 0;
            }
        }

        public override void DumpInfo()
        {
            //Console.WriteLine("File {0}:\n", FileName);
            //Console.WriteLine("  Local Symbol Table:");
            //foreach( object syitem in LocalSymTable.Values )
            //    Console.WriteLine("    {0}", syitem);
            //Console.WriteLine("  Literal Pool:");
            //if (literalPool.Count == 0)
            //    Console.WriteLine("    <<empty>>");
            //else
            //    foreach( object pi in literalPool )
            //        Console.WriteLine("    {0}", pi);
            //if (pendingLabels.Count > 0) {
            //    Console.WriteLine("  Pending (unplaced) labels:");
            //    foreach( NameInt lab in pendingLabels )
            //        Console.WriteLine("    {0} at line {1}", lab.name, lab.val);
            //}
            //for( int i=0;  i<3;  i++ ) {
            //    SectionType st = (SectionType)i;
            //    Console.WriteLine("  Sizes for {0} Section:",
            //        Enum.GetName(typeof(SectionType),i));
            //    List<int> al = position[i];
            //    int[] sl =(Pass == 2)? subsectionStart[i] : null;
            //    for (int k = 0; k < al.Count; k++)
            //    {
            //        if (al[k] == 0) continue;
            //        Console.WriteLine("    subsection {0}: {1}", k, al[k]);
            //        if (sl != null)
            //            Console.WriteLine("    start is at   : {0}", sl[k]);
            //    }
            //}
            //Console.WriteLine();
        }
    }

}  // end of namespace ArmAssembler

