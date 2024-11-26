using System;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Preferences;
using Processors;
using Parsers;

namespace ParserZasm
{
    public class ParserZasm : IParser
    {
        public IProcessor Processor { get; set; }
        private ZAsmParserConfig mZAsmParserConfig;
        public uint EntryPoint { get { return 0; } }

        public ParserZasm(IProcessor processor)
        {
            Processor = processor;
            mZAsmParserConfig = LoadSettings();
        }
        public string ParserName { get { return "ParserZasm.ParserZasm"; } }

        public IList<ASMError> Errors { get; set; }
        public IList<CodeLine> ParseListingFile(string filename) { return null; }

        public OrgSections Sections { get; set; }

        public IList<CodeLine> CodeLines { get; set; }

        public IDictionary<string, ushort> Labels { get; set; }

        public ISystemMemory SystemMemory { get; set; }

        public object Settings
        {
            get
            {
                return mZAsmParserConfig;
            }
        }

        public string[] Lines => throw new NotImplementedException();
        public void SaveSettings(object settings)
        {
            PreferencesBase.Save<ZAsmParserConfig>(settings as ZAsmParserConfig, ZAsmParserConfig.Key);
        }

        public ZAsmParserConfig LoadSettings()
        {
            return PreferencesBase.Load<ZAsmParserConfig>(ZAsmParserConfig.Key) as ZAsmParserConfig;
        }

        public void Parse(string filename, ParseSettings parseSettings)
        {
            string workingDir = Path.GetDirectoryName(filename);

            CodeLines = null;
            Errors = new List<ASMError>();
            Sections = new OrgSections();
            SystemMemory = new SystemMemory();
            SystemMemory.Default(1024 * 64);

            var result = runAssembler(filename, workingDir);
            string parsingFile = Path.Combine(workingDir, Path.ChangeExtension(filename, "lst"));

            if (!File.Exists(parsingFile))
                throw new Exception(result);

            CodeLines = parseListingFile(parsingFile);
            if (CodeLines == null || CodeLines.Count == 0)
            {
                Errors.Add(new ASMError { Text = "No output lines created!" });
                return;
            }
        }

        private string runAssembler(string filename, string workingDir)
        {
            string fname = Path.GetFileNameWithoutExtension(filename);

            //var binDir = Assembly.GetExecutingAssembly().Location;
            var startInfo = new ProcessStartInfo(Path.Combine(mZAsmParserConfig.AssemblerPath));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = string.Format(mZAsmParserConfig.AssemblerArguments, fname, fname, Path.GetFileName(filename));
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = workingDir;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            var proc = Process.Start(startInfo);
            var result = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return result;
        }

        private bool checkForEnd(string line)
        {
            return line.Trim().ToLower().StartsWith("end");
        }

        private CodeLine comment(string line)
        {
            var cl = new CodeLine(line);
            cl.CodeLineType = CodeLine.CodeLineTypes.Comment;
            return cl;
        }

        private IList<CodeLine> parseListingFile(string filename)
        {
            Sections.TopSection.StartAddress = ushort.MaxValue;
            var ret = new List<CodeLine>();
            string[] lines = File.ReadAllLines(filename);
            for (int ii = 0; ii < lines.Length; ii++)
            {
                if (string.IsNullOrEmpty(lines[ii].Trim()))
                {
                    ret.Add(comment(lines[ii]));
                    continue;
                }

                var line1 = SafeSubString(lines[ii], 0, 16);
                var line2 = SafeSubString(lines[ii], 16, -1);

                if (checkForEnd(line2))
                    break;

                if (string.IsNullOrEmpty(line1.Trim()) || line1.Length < 8)
                {
                    ret.Add(comment(line2));
                    continue;
                }

//                if (line1[0] != ' ')
//                {
//                    var cl = comment(line2);
//                    cl.CodeLineType = CodeLine.CodeLineTypes.Error;
//                    ret.Add(cl);
//                    Errors.Add(new ASMError { Line = ii, Text = line2[0].ToString() });
//                    continue;
//                }

                string addrS = line1.Substring(0, 4);
                ushort addr = 0;
                if (!UInt16.TryParse(addrS, NumberStyles.HexNumber, null, out addr))
                {
                    ret.Add(comment(line2));
                    continue;
                }
                ushort addr2 = addr;
                int yy = 0;
                while(yy<4)
                {
                    var byteS = SafeSubString(line1, yy * 2 + 6, 2).Trim();
                    if (string.IsNullOrEmpty(byteS))
                        break;
                    byte b = byte.Parse(byteS, System.Globalization.NumberStyles.HexNumber);
                    SystemMemory[addr2++] = b;
                    yy++;
                }
                if (addr2 > addr)
                {
                    var codeLine = new CodeLine(line2);
                    codeLine.CodeLineType = CodeLine.CodeLineTypes.Code;
                    codeLine.Address = addr;
                    codeLine.Length = addr2 - addr;
                    ret.Add(codeLine);
                    Sections.TopSection.IsDataDefined = true;

                    if (addr < Sections.TopSection.StartAddress)
                        Sections.TopSection.StartAddress = addr;
                    if (addr2 > Sections.TopSection.EndAddress)
                        Sections.TopSection.EndAddress = addr2;
                    addr = addr2;
                }
                else
                {
                    var cl = new CodeLine(line2);
                    cl.CodeLineType = CodeLine.CodeLineTypes.EQU;
                    ret.Add(cl);
                }
                continue;
            }
            return ret;
        }
        public static string SafeSubString(string s, int start, int length)
        {
            if (length > 0)
            {
                if (s.Length < (start + length))
                    return string.Empty;
            }
            else
            {
                if (s.Length < start)
                    return string.Empty;
            }
            if (length > 0)
                return s.Substring(start, length);
            else
                return s.Substring(start);
        }
    }
}