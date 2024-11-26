using System;
using System.IO;
using System.Collections.Generic;

namespace ARM7.Disassembler
{

    // SectionType defines the type of an identifier (normally a label) or expression
    // * Text, Data, Bss describe a label or address within the corresponding region
    // * None describes a pure number
    // * Unknown is used for a label which has not yet been seen; it is OK to have
    //   such labels during Pass 1; after pass 1, it is an error.
    // * Reg is used for an identifier that matches a register name -- attempting to use
    //   such a value as a number or a label is invalid
    // Some arithmetic operations involving these values are valid.
    public enum SectionType { Text = 0, Data = 1, Bss = 2, None = 3, Unknown = 4, Reg = 5 };
    public enum CodeType { TextArm = 0, TextThumb = 1, LtorgData = 2, Data = 3 };

    // one instance of this type is created per source code line to record
    // what the source line translates to.
    public struct SourceLineInfo
    {
        public int NumBytes;
        public int Address;
        public CodeType Content;
        public SourceLineInfo(int n, CodeType ct, int a)
        {
            NumBytes = n; Content = ct; Address = a;
        }
    }

    public abstract class ArmFileInfo
    {
        string fileName;
        IList<string> sourceLine = null;   // source code lines (if available)
        SourceLineInfo[] sourceLineType = null;  // info about the source lines

        int lineNumber = 0;

        IDictionary<string, SyEntry> localSymTable;
        IDictionary<string, SyEntry> globalSymbols;
        IDictionary<string, string> externSymbols;
        int pass;
        public bool trace = false;

        int[] sectionSize;     // contains info only after Pass 1
        int[] sectionAddress;  // contains info only after start of Pass 2

        static string lastFileNamePrinted = null;

        public AssembledProgram ProgramSpace { get; set; }
        public struct NameInt
        {
            public string name;
            public int val;
            public NameInt(string s, int v)
            {
                name = s; val = v;
            }
        }

        public ArmFileInfo(string fileName, IDictionary<string, SyEntry> globalSymbols, IDictionary<string, string> externSymbols)
        {
            this.fileName = fileName;
            this.globalSymbols = globalSymbols;
            this.externSymbols = externSymbols;
            localSymTable = new Dictionary<string, SyEntry>();
            sectionSize = new int[3];
            sectionAddress = new int[4];
            pass = 0;
        }

        // accessor methods

        public int Pass
        {
            get { return pass; }
            set { pass = value; }
        }

        public string FileName { get { return fileName; } }

        public IList<string> SourceLine
        {
            get { return sourceLine; }
            set { sourceLine = value; }
        }

        public SourceLineInfo[] SourceLineType
        {
            get { return sourceLineType; }
            set { sourceLineType = value; }
        }

        public int LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }

        public int[] SectionSize { get { return sectionSize; } }

        public int[] SectionAddress { get { return sectionAddress; } }

        public IDictionary<string, SyEntry> LocalSymTable
        {
            get { return localSymTable; }
            set { localSymTable = value; }
        }

        public IDictionary<string, SyEntry> GlobalSymbols { get { return globalSymbols; } }

        public IDictionary<string, string> ExternSymbols { get { return externSymbols; } }

        //public AssembledProgram ProgramSpace { get; set; }

        // end of accessor methods

        public void DefineSymbol(string name, int lineNum, int value)
        {
            if (pass > 1) return;
            SyEntry newSymbol = new SyEntry(name, lineNum, value);
            localSymTable[name] = newSymbol;
        }

        public void DefineExternal(string name)
        {
            if (globalSymbols.ContainsKey(name)) return;
            if (externSymbols.ContainsKey(name)) return;
            externSymbols[name] = fileName;
        }

        public SyEntry DefineSymbol(string name, int lineNum, int value, SectionType section)
        {
            SyEntry sym;
            if (pass > 1)
            {
                if (!localSymTable.TryGetValue(name, out sym))
                    throw new AsmException("symbol {0} not found", name);
                sym.Value = value;
                return sym;
            }
            if (section != SectionType.None)
            {
                sym = new SyEntry(name, lineNum, section, 0, value);
            }
            else
                sym = new SyEntry(name, lineNum, value);
            localSymTable[name] = sym;
            return sym;
        }

        public virtual SyEntry LookupNumericLabel(string name)
        {
            // overridden in AsmFileInfo subclass
            return null;
        }

        public SyEntry LookupSymbol(string name)
        {
            if (Char.IsDigit(name[0]))
                return LookupNumericLabel(name);
            SyEntry sy;
            if (localSymTable.TryGetValue(name, out sy))
                return sy;
            globalSymbols.TryGetValue(name, out sy);
            return sy;
        }

        public virtual void DefineCommSymbol(string name, int size, int align)
        {
            if (pass > 1) return;
            if (localSymTable.ContainsKey(name)) return;
            if (size <= 0) size = 1;
            // round up alignment to a power of 2
            int al = 1;
            while (al < align)
            {
                al <<= 1;
            }
            align = al;
            SyEntry newsym;
            if (!globalSymbols.TryGetValue(name, out newsym))
            {
                newsym = new SyEntry(name, lineNumber, size, align);
                globalSymbols[name] = newsym;
            }
            else if (newsym.Kind == SymbolKind.CommSymbol)
            {
                // combine new size and alignment with old values
                newsym.Size = size;
                newsym.Align = align;
            } // else ignore the .comm definition because it's
              // already defined as a normal label
            localSymTable[name] = newsym;
        }

        public void ParseError(string msg, params object[] args)
        {
            ParseError(-1, msg, args);
        }

        public void ParseError(int ix, string msg, params object[] args)
        {
            if (lastFileNamePrinted != FileName)
            {
                lastFileNamePrinted = FileName;
                Console.WriteLine("File {0}:", lastFileNamePrinted);
            }
            //AssemblerErrors.AddError(FileName, LineNumber, ix, String.Format(msg, args));
        }

        public abstract void StartPass();

        public void EndPass()
        {
            if (pass == 1)
                endPass1();
            else
                endPass2();
        }

        protected abstract void endPass1();

        protected abstract void endPass2();

        public abstract void DumpInfo();
    }



    public enum SymbolKind { Label, Symbol, CommSymbol };


    public class SyEntry
    {
        string name;
        int lineNumber;
        SymbolKind sykind;
        SectionType section;
        int subSection; // number in range 0 to 8191
        int val;    // if sykind=Label, pos relative to subsection start
                    // if sykind=CommSymbol, size in bytes
        int align;  // if sykind=CommSymbol, size in bytes 

        // Create a Label form of symbol
        public SyEntry(string name, int ln, SectionType section,
                int subSection, int pos)
        {
            this.name = name;
            this.lineNumber = ln;
            sykind = SymbolKind.Label;  // label on text/data/bss loc
            this.section = section;
            this.subSection = subSection;
            this.val = pos;
        }

        // Create a .COMM form of symbol
        public SyEntry(string name, int ln, int size, int align)
        {
            this.name = name;
            this.lineNumber = ln;
            sykind = SymbolKind.CommSymbol;
            this.section = SectionType.Bss;
            this.subSection = 0;
            this.val = size;
            this.align = align;
        }

        public SyEntry(string name, int ln, int val)
        {
            this.name = name;
            this.lineNumber = ln;
            sykind = SymbolKind.Symbol; // symbol
            this.section = SectionType.None;
            this.val = val;
        }

        public int Value
        {
            get { return val; }
            set { val = value; }
        }

        public string Name { get { return name; } }

        public SectionType Section
        {
            get { return section; }
            set { section = value; }
        }

        public int SubSection
        {
            get { return subSection; }
            set { subSection = value; }
        }

        public SymbolKind Kind
        {
            get { return sykind; }
            set { sykind = value; }
        }

        public int LineNumber { get { return lineNumber; } }

        public int Size
        {
            get { return sykind == SymbolKind.CommSymbol ? val : 0; }
            set
            {
                if (sykind == SymbolKind.CommSymbol && value > val)
                    val = value;
            }
        }

        public int Align
        {
            get { return sykind == SymbolKind.CommSymbol ? align : 0; }
            set
            {
                if (sykind == SymbolKind.CommSymbol && value > align)
                    align = value;
            }
        }

        public override string ToString()
        {
            if (sykind == SymbolKind.Label)
                return String.Format("Label {0} [line {4}]: pos {1} in {2} {3}",
                    name, val, section, subSection, lineNumber);
            else if (sykind == SymbolKind.CommSymbol)
                return String.Format("Comm Symbol {0} [line {3}]: size={1}, align={2}",
                    name, val, align, lineNumber);
            else
                return String.Format("Symbol {0} [line {2}] = {1}",
                    name, val, lineNumber);
        }
    }

}  // end of namespace ArmAssembler

