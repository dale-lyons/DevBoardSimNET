using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;
using Preferences;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using Parsers;

namespace ParserAsm11
{
    public class ParserAsm11 : IParser
    {
        public IProcessor Processor { get; set; }
        private Asm11ParserConfig mAsm11ParserConfig;
        public uint EntryPoint { get { return 0; } }

        public ParserAsm11(IProcessor processor)
        {
            Processor = processor;
            mAsm11ParserConfig = LoadSettings();
        }
        public string ParserName { get { return "ParserAsm11.ParserAsm11"; } }

        public IList<ASMError> Errors { get; set; }
        public IList<CodeLine> ParseListingFile(string filename) { return null; }

        public OrgSections Sections { get; set; }

        public IList<CodeLine> CodeLines { get; set; }

        public IDictionary<string, ushort> Labels { get; set; }

        public ISystemMemory SystemMemory { get; set; }

        public object Settings { get { return mAsm11ParserConfig; } }

        public string[] Lines => throw new NotImplementedException();
        public void SaveSettings(object settings)
        {
            PreferencesBase.Save<Asm11ParserConfig>(settings as Asm11ParserConfig, Asm11ParserConfig.Key);
        }

        public Asm11ParserConfig LoadSettings()
        {
            return PreferencesBase.Load<Asm11ParserConfig>(Asm11ParserConfig.Key) as Asm11ParserConfig;
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
            var startInfo = new ProcessStartInfo(Path.Combine(mAsm11ParserConfig.AssemblerPath));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = string.Format(mAsm11ParserConfig.AssemblerArguments, fname, fname, Path.GetFileName(filename));
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

                var line1 = SafeSubString(lines[ii], 0, 24);
                var line2 = SafeSubString(lines[ii], 24, -1);
                lines[ii].Substring(0, 24);

                if (checkForEnd(line2))
                    break;

                if (line1[0] != ' ')
                {
                    var cl = comment(line2);
                    cl.CodeLineType = CodeLine.CodeLineTypes.Error;
                    ret.Add(cl);
                    Errors.Add(new ASMError { Line = ii, Text = line2[0].ToString() });
                    continue;
                }

                if (string.IsNullOrEmpty(line1.Trim()) || line1.Length < 8)
                {
                    ret.Add(comment(line2));
                    continue;
                }
                string addrS = line1.Substring(3, 4);
                ushort addr = 0;
                if (!UInt16.TryParse(addrS, NumberStyles.HexNumber, null, out addr))
                {
                    ret.Add(comment(line2));
                    continue;
                }
                ushort addr2 = addr;
                string[] sublines = line1.Substring(10, 12).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subline in sublines)
                {
                    if (string.IsNullOrEmpty(subline))
                        break;
                    byte b = byte.Parse(subline, System.Globalization.NumberStyles.HexNumber);
                    SystemMemory[addr2++] = b;
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