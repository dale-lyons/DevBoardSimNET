using System;
using System.IO;
using System.Collections.Generic;

namespace ARM7.Disassembler
{

    public class ObjFileInfo : ArmFileInfo
    {
        public ArmElfReader elf;

        public ObjFileInfo(string fileName,
                    IDictionary<string, SyEntry> globalSymbols, IDictionary<string, string> externSymbols)
            : base(fileName, globalSymbols, externSymbols)
        {
            elf = new ArmElfReader(this);
        }

        public ObjFileInfo(FileStream arMember, string libraryName, string member,
                IDictionary<string, SyEntry> globalSymbols, IDictionary<string, string> externSymbols)
            : base(String.Format("{0}({1})", libraryName, member),
                globalSymbols, externSymbols)
        {
            elf = new ArmElfReader(this, arMember);
        }

        // accessor methods

        public ArmElfReader Elf
        {
            get { return elf; }
        }

        // end of accessor methods

        public override void StartPass()
        {
            if (Pass == 1)
            {
                elf.PerformPass1();
            }
            else if (Pass == 2)
            {
                elf.PerformPass2(this.SectionAddress);
            }
        }

        protected override void endPass1() { }

        protected override void endPass2() { }

        public override void DumpInfo() { }
    }

}  // end of namespace ArmAssembler

