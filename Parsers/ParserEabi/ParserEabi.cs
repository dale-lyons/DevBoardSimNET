using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;
using Preferences;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using Parsers;

namespace ParserEabi
{
    public class ParserEabi : IParser
    {
        public IProcessor Processor { get; set; }
        private EabiParserConfig mEabiParserConfig;
        public uint EntryPoint { get; private set; } = 0;
        private Dictionary<string, uint> mSectionAddresses = new Dictionary<string, uint>();

        public ParserEabi(IProcessor processor)
        {
            Processor = processor;
            mEabiParserConfig = LoadSettings();
        }
        public string ParserName { get { return "ParserEabi.ParserEabi"; } }

        public IList<ASMError> Errors { get; set; }
        public IList<CodeLine> ParseListingFile(string filename) { return null; }

        public OrgSections Sections { get; set; }

        public IList<CodeLine> CodeLines { get; set; }

        public IDictionary<string, ushort> Labels { get; set; }

        public ISystemMemory SystemMemory { get; set; }
        public object Settings { get { return mEabiParserConfig; } }
        public string[] Lines => throw new NotImplementedException();
        public void SaveSettings(object settings)
        {
            PreferencesBase.Save<EabiParserConfig>(settings as EabiParserConfig, EabiParserConfig.Key);
        }

        public EabiParserConfig LoadSettings()
        {
            return PreferencesBase.Load<EabiParserConfig>(EabiParserConfig.Key) as EabiParserConfig;
        }

        public void Parse(string filename, ParseSettings parseSettings)
        {
            mEabiParserConfig = LoadSettings();
            string workingDir = Path.GetDirectoryName(filename);

            CodeLines = null;
            Errors = new List<ASMError>();
            Sections = new OrgSections();
            var sm = new SystemMemory();
            sm.Default(1024 * 64);

            var listing = runAssembler(filename, workingDir);

            using (StreamReader sr = new StreamReader(Path.ChangeExtension(filename, ".o")))
            {
                var elf = ELFReader.Load(sr.BaseStream, false);




                var sectionsToLoad = elf.GetSections<ProgBitsSection<uint>>();
                    //.Where(x => x.LoadAddress != 0);












                //var functions = ((ISymbolTable)elf.GetSection(".symtab")).Entries.Where(x => x.Type == SymbolType.Function);
                //var dale = (ISymbolTable)elf.GetSection(".symtab");

                mSectionAddresses[".text"] = mEabiParserConfig.TextAreaAddress;
                mSectionAddresses[".data"] = (uint)elf.GetSection(".text").GetContents().Length;
                mSectionAddresses[".bss"] = (uint)elf.GetSection(".data").GetContents().Length;

                placeCode(elf.GetSection(".text"), sm, mSectionAddresses[".text"]);
                placeCode(elf.GetSection(".data"), sm, mSectionAddresses[".data"]);
                placeCode(elf.GetSection(".bss"), sm, mSectionAddresses[".bss"]);
                fixupLocalSymboles((ISymbolTable)elf.GetSection(".symtab"), sm);
            }
            SystemMemory = sm;
        }


        private void fixupLocalSymboles(ISymbolTable table, SystemMemory sm)
        {
            foreach(ISymbolEntry sym in table.Entries)
            {
                if(!mSectionAddresses.ContainsKey(sym.PointedSection.Name))
                    continue;
                uint offset = mSectionAddresses[sym.PointedSection.Name];



            }
        }

        private void placeCode(ISection section, SystemMemory sm, uint addr)
        {
            var bytes = section.GetContents();
            for(int ii=0; ii< bytes.Length; ii++)
                sm[addr++] = bytes[ii];
        }

        private IList<string> runAssembler(string filename, string workingDir)
        {
            int listingState = 0;
            var listing = new List<string>();
            var symTabListing = new List<string>();
            int errorCnt = 0;

            string fname = Path.GetFileNameWithoutExtension(filename);
            string fname2 = Path.ChangeExtension(fname, ".o");

            var ofile = Path.Combine(workingDir, fname2);
            if (File.Exists(ofile))
                File.Delete(ofile);

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += (object sendingProcess, DataReceivedEventArgs outLine) =>
                {
                    string line = outLine.Data;
                    if (string.IsNullOrEmpty(line))
                        return;
                    switch (listingState)
                    {
                        case 0:
                            if (line.StartsWith("DEFINED SYMBOLS"))
                                listingState = 1;
                            else
                                listing.Add(line);
                            break;
                        case 1:
                            if (line.StartsWith("UNDEFINED SYMBOLS"))
                                listingState = 2;
                            else
                                symTabListing.Add(line);
                            break;
                        case 2:
                            // ignore any further lines
                            break;
                    }
                };

                p.ErrorDataReceived += (object sendingProcess, DataReceivedEventArgs outLine) =>
                {
                    // parse the line of error output; it has the following format
                    //      FILENAME:LINENUMBER: ERROR MESSAGE
                    string line = outLine.Data;
                    if (line == null) return;
                    int lineNum;
                    // look for 2 consecutive colons separated only by decimal digits
                    int colon1pos = -1;
                    int colon2pos = -1;
                    do
                    {
                        colon2pos = line.IndexOf(':', colon1pos + 1);
                        if (colon2pos <= colon1pos) return;
                        lineNum = 0;
                        for (int i = colon1pos + 1; i < colon2pos; i++)
                        {
                            char c = line[i];
                            if (!Char.IsDigit(c))
                            {
                                colon1pos = colon2pos;
                                break;
                            }
                            lineNum = lineNum * 10 + (int)c - (int)'0';
                        }
                    } while (colon1pos == colon2pos);
                    string message = line.Substring(colon2pos + 1);
                    //AssemblerErrors.AddError(fileName, lineNum, 0, message);
                    errorCnt++;
                };

                p.StartInfo.WorkingDirectory = workingDir;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = Path.Combine(mEabiParserConfig.AssemblerPath);
                p.StartInfo.Arguments = string.Format(@"{0} -alns -march=armv7-a -mfpu=vfp -o {1}", Path.GetFileName(filename), fname2);
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
            }
            return listing;
        }
    }
}