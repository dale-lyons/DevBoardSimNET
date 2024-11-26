using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Preferences;
using Processors;
using Parsers;

namespace ParserTasm
{
    public class ParserTasm : IParser
    {
        public IProcessor Processor { get; set; }
        private TasmParserConfig mTasmParserConfig;
        public uint EntryPoint { get { return 0; } }

        public ParserTasm(IProcessor processor)
        {
            Processor = processor;
            mTasmParserConfig = LoadSettings();
        }
        public string ParserName { get { return "ParserTasm.ParserTasm"; } }

        public IList<ASMError> Errors { get; set; }
        public IList<CodeLine> ParseListingFile(string filename) { return null; }

        public OrgSections Sections { get; set; }

        public IList<CodeLine> CodeLines { get; set; }

        public IDictionary<string, ushort> Labels { get; set; }

        public ISystemMemory SystemMemory { get; set; }

        public object Settings { get { return mTasmParserConfig; } }
        public string[] Lines => throw new NotImplementedException();
        public void SaveSettings(object settings)
        {
            PreferencesBase.Save<TasmParserConfig>(settings as TasmParserConfig, TasmParserConfig.Key);
        }
        public TasmParserConfig LoadSettings()
        {
            return PreferencesBase.Load<TasmParserConfig>(TasmParserConfig.Key) as TasmParserConfig;
        }

        static string errorPattern = @"line (\d+): (.*$)";
        List<ASMError> parseErrors(string result)
        {
            var ret = new List<ASMError>();
            var lines = result.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Regex rg = new Regex(errorPattern);
            int lineNumber = 0;

            foreach (var line in lines)
            {
                var match = rg.Match(line);
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out lineNumber))
                        ret.Add(new ASMError { Text = match.Groups[2].Value, Line = lineNumber });
                }
            }
            return ret;
        }

        public void Parse(string filename, ParseSettings parseSettings)
        {
            string workingDir = Path.GetDirectoryName(filename);

            CodeLines = null;
            Errors = null;
            Sections = new OrgSections();
            SystemMemory = new SystemMemory();
            SystemMemory.Default(1024 * 64);

            var result = runAssembler(filename, workingDir);
            Errors = parseErrors(result);

            if (Errors.Count > 0)
                return;

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

        //private static string resultLabel = "Number of errors = ";
        private string runAssembler(string filename, string workingDir)
        {
            //first we must copy the file "TASM80.TAB" from the assembler directory to the target asm file.
            string srcFile = Path.Combine(Path.GetDirectoryName(mTasmParserConfig.AssemblerPath), "TASM80.TAB");
            if (!File.Exists(srcFile))
            {
                return string.Empty;
            }
            string dstFile = Path.Combine(workingDir, "TASM80.TAB");
            File.Copy(srcFile, dstFile, true);

            string fname = Path.GetFileName(filename);

            var startInfo = new ProcessStartInfo(mTasmParserConfig.AssemblerPath);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = string.Format(mTasmParserConfig.AssemblerArguments, fname, fname, Path.GetFileName(filename));
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = workingDir;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            var proc = Process.Start(startInfo);
            var result = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            //now delete the local copy of "TASM80.TAB"
            File.Delete(dstFile);
            return result;
        }

        private CodeLine comment(string line)
        {
            var cl = new CodeLine(line);
            cl.CodeLineType = CodeLine.CodeLineTypes.Comment;
            return cl;
        }

        static string linepattern = @"^(\d+)[\+]*\s+([0-9a-fA-F]+){1,4}\s([0-9a-fA-F][0-9a-fA-F]\s){0,4}.*$";
        private IList<CodeLine> parseListingFile(string filename)
        {
            Sections.TopSection.StartAddress = ushort.MaxValue;
            var ret = new List<CodeLine>();

            Regex rg = new Regex(linepattern);
            string[] lines = File.ReadAllLines(filename);
            for (int ii = 0; ii < lines.Length; ii++)
            {
                string ln = lines[ii];
                var match = rg.Match(ln);
                if (!match.Success)
                    continue;

                var lineNumber = ushort.Parse(match.Groups[1].Value);
                var address = ushort.Parse(match.Groups[2].Value, NumberStyles.HexNumber);
                ushort addr2 = address;

                if (match.Groups[3].Captures.Count == 0)
                {
                    string code2 = (ln.Length >= 24) ? ln.Substring(24) : " ";
                    var cl = new CodeLine(code2);
                    cl.CodeLineType = CodeLine.CodeLineTypes.Comment;
                    ret.Add(cl);
                    continue;
                }

                for (int jj = 0; jj < match.Groups[3].Captures.Count; jj++)
                {
                    byte b = byte.Parse(match.Groups[3].Captures[jj].Value, NumberStyles.HexNumber);
                    SystemMemory[addr2++] = b;
                }

                string code = (ln.Length >= 24) ? ln.Substring(24) : " ";

                var codeLine = new CodeLine(code);
                codeLine.CodeLineType = CodeLine.CodeLineTypes.Code;
                codeLine.Address = address;
                codeLine.Length = addr2 - address;
                ret.Add(codeLine);
                Sections.TopSection.IsDataDefined = true;

                if (address < Sections.TopSection.StartAddress)
                    Sections.TopSection.StartAddress = address;
                if (addr2 > Sections.TopSection.EndAddress)
                    Sections.TopSection.EndAddress = addr2;
                address = addr2;
            }
            return ret;
        }
    }
}