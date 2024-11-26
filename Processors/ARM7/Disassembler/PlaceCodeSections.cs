using System;
using System.IO;
using System.Collections.Generic;
using Processors;

namespace ARM7.Disassembler
{

    // After pass 1 of the assembly process has been performed on all the
    // source files, we know the offsets of all labels relative to the
    // start of each section.
    // We now consolidate all these pieces, determining their placements
    // at absolute memory locations.  The offsets of all labels are updated
    // to be absolute memory locations.  (This is work that would normally
    // be performed by a loader.)
    //
    // After this step, the label addresses follow a pattern as shown in
    // the diagram below.  All the text sections are consolidated, all
    // the data sections are consolidated, as are all the bss sections.
    //
    //        program load point  -->  text section for file 1
    //                                 text section for file 2
    //                                 ...
    //                                 data section for file 1
    //                                 data section for file 2
    //                                 ...
    //                                 bss section for file 1
    //                                 bss section for file 2
    //                                 ...
    // The program load point is an address which may be supplied as a
    // parameter to the consolidation process.  If omitted, a default
    // value of 1000 (hex) is used.
    //
    public class PlaceCodeSections
    {
        //static int defaultLoadPoint = 0x1000;
        int loadPoint;
        int nextFreeAddress;
        int textStart, dataStart, bssStart;
        IList<ArmFileInfo> fileInfoList;
        IDictionary<string, SyEntry> globalSymbols;

        private ISystemMemory mSystemMemory;

        public PlaceCodeSections(IList<ArmFileInfo> fileInfoList, IDictionary<string, SyEntry> globalSymbols, int startAddress, ISystemMemory systemMemory)
        {
            loadPoint = startAddress;
            this.fileInfoList = fileInfoList;
            this.globalSymbols = globalSymbols;
            this.mSystemMemory= systemMemory;
            performPlacement();
            processCommEntries();


        }

//        public PlaceCodeSections(IList<ArmFileInfo> fileInfoList, IDictionary<string, SyEntry> globalSymbols)
//            : this(fileInfoList, globalSymbols, defaultLoadPoint) { }

        // accessor methods

        public int LoadPoint { get { return loadPoint; } }

        public int Length { get { return nextFreeAddress - loadPoint; } }

        public int TextStart { get { return textStart; } }

        public int DataStart { get { return dataStart; } }

        public int BssStart { get { return bssStart; } }

        // placement for all sections in all files
        private void performPlacement()
        {
            loadPoint = (int)(((uint)loadPoint + 3) & 0xFFFFFFFC);
            nextFreeAddress = loadPoint;
            textStart = nextFreeAddress;
            performPlacement(SectionType.Text);
            dataStart = nextFreeAddress;
            performPlacement(SectionType.Data);
            bssStart = nextFreeAddress;
            performPlacement(SectionType.Bss);
        }

        // placement for one main section type in all files
        private void performPlacement(SectionType st)
        {
            foreach (ArmFileInfo fileInfo in fileInfoList)
                performPlacement(st, fileInfo);
        }

        // placement for one main section type in one file
        private void performPlacement(SectionType st, ArmFileInfo fileInfo)
        {
            fileInfo.SectionAddress[(int)st] = nextFreeAddress;
            int size = fileInfo.SectionSize[(int)st];
            // place the labels in this section only
            foreach (SyEntry sy in fileInfo.LocalSymTable.Values)
            {
                if (sy.Kind != SymbolKind.Label) continue;
                if (sy.Section != st) continue;
                if (sy.SubSection != 0)
                    throw new AsmException("internal error in performPlacement, 2");
                sy.Value += nextFreeAddress;
            }
            AsmFileInfo asmfi = fileInfo as AsmFileInfo;
            if (asmfi != null)
            {
                if (st == SectionType.Text)
                {
                    // there may be duplicate references to the same entry
                    // first, we delete the duplicates
                    LiteralSet lp = asmfi.LiteralPool;
                    lp.RemoveDuplicates();

                    // place any literals that belong to the text section
                    foreach (AsmLiteral pi in lp)
                    {
                        pi.Subsection = 0;
                        pi.Offset += nextFreeAddress;
                    }
                }
                asmfi.AdjustSubsectionStart(st, nextFreeAddress);
            }
            nextFreeAddress += size;
            nextFreeAddress = (int)(((uint)nextFreeAddress + 3) & 0xFFFFFFFC);
        }

        // all remaining COMM entries after pass 1 are converted to BSS labels
        private void processCommEntries()
        {
            foreach (SyEntry sy in globalSymbols.Values)
            {
                if (sy.Kind != SymbolKind.CommSymbol) continue;
                uint mask = (uint)(-sy.Align);
                int addr = (int)(((uint)nextFreeAddress + (sy.Align - 1)) & mask);
                nextFreeAddress += sy.Size;
                sy.Kind = SymbolKind.Label;
                sy.Section = SectionType.Bss;
                sy.SubSection = 0;
                sy.Value = addr;
            }
            nextFreeAddress = (int)(((uint)nextFreeAddress + 3) & 0xFFFFFFFC);
        }
    }

} // end of namespace ArmAssembler