using Parsers;
using Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Code
{
    public class ProgramImage
    {
        private IDisassembler mDisassembler;
        private Dictionary<uint, uint> mAddressToLine = new Dictionary<uint, uint>();
        private IList<CodeLine> mLines = new List<CodeLine>();
        public IProcessor Processor { get; private set; }

        public ProgramImage(IProcessor processor)
        {
            Processor = processor;
        }

        public bool ContainsKey(uint pc)
        {
            return mAddressToLine.ContainsKey(pc);
        }

        public uint LineAtAddress(uint pc)
        {
            return (uint)(mAddressToLine[pc] - 1);
        }

        public void AddCompiledCode(IParser parser, bool loadBinary)
        {
            if (loadBinary)
            {
                uint start = parser.Sections.StartAddress;
                var sm = parser.SystemMemory;
                Processor.SystemMemory.SetReadWriteOverride(0, true);
                //Transfer memory from parser to processor systsem memory.
                foreach (var section in parser.Sections.Sections)
                {
                    if (!section.IsDataDefined)
                        continue;

                    for (uint ii = section.StartAddress; ii <= section.EndAddress; ii++)
                    {
                        byte b = (byte)sm.GetMemory(ii, WordSize.OneByte, false);
                        Processor.SystemMemory.SetMemory(ii, WordSize.OneByte, b);
                    }
                }
                CodeLine.Processor = Processor;
                Processor.SystemMemory.SetReadWriteOverride(0, false);
                //this.SetPC(start);
            }
            int count = mLines.Count + 1;
            foreach (var line in parser.CodeLines)
            {
                line.Line = count++;
                mLines.Add(line);
                if (line.CodeLineType == CodeLine.CodeLineTypes.Code)
                    mAddressToLine[line.Address] = (uint)line.Line;
            }
        }

        public IList<CodeLine> GetLinesByLineNo(int topLine, ISystemMemory sm, uint count)
        {
            List<CodeLine> ret = new List<CodeLine>();
            int yy = 0;
            for (int ii = topLine; ii < mLines.Count && yy < count; ii++, yy++)
                ret.Add(mLines[ii]);
            return ret;
        }

        public IList<CodeLine> GetLinesByAddr(uint addr, ISystemMemory sm, uint count)
        {
            List<CodeLine> ret = new List<CodeLine>();
            var disasm = Processor.CreateDisassembler((int)addr);
            int ii = 0;
            while (ii++ < count)
            {
                var cl = disasm.ProcessInstruction(addr, sm, out addr);
                ret.Add(cl);
            }
            return ret;
        }
    }
}