using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;
using Preferences;
using Parsers;

namespace ParserCa65
{
    public class ParserCa65
    {
        private uint? mStartAddress;

        public string DefaultAssembler { get; set; } = @"C:\Projects\DevBoardSimNET\Parsers\ParserCa65\ca65\ca65.exe";
        public IProcessor Processor { get; set; }
        private Ca65ParserConfig mCa65ParserConfig;

        public ParserCa65(IProcessor processor)
        {
            Processor = processor;
            mCa65ParserConfig = LoadSettings();
        }
        public string ParserName { get { return "ParserCa65.ParserCa65"; } }

        public IList<ASMError> Errors { get; set; }
        public IList<CodeLine> ParseListingFile(string filename) { return null; }

        public OrgSections Sections { get; set; }

        public IList<CodeLine> CodeLines { get; set; }

        public IDictionary<string, ushort> Labels { get; set; }

        public ISystemMemory SystemMemory { get; set; }

        public object Settings { get { return mCa65ParserConfig; } }

        public string[] Lines => throw new NotImplementedException();
        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<Ca65ParserConfig>(settings, Ca65ParserConfig.Key);
        }

        public Ca65ParserConfig LoadSettings()
        {
            return PreferencesBase.Load<Ca65ParserConfig>(Ca65ParserConfig.Key) as Ca65ParserConfig;
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
            var startInfo = new ProcessStartInfo(Path.Combine(mCa65ParserConfig.AssemblerPath));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = string.Format(mCa65ParserConfig.AssemblerArguments, fname, fname, Path.GetFileName(filename));
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
            var ret = new List<CodeLine>();
            string[] lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                if (line.StartsWith(";") || string.IsNullOrEmpty(line))
                {
                    var cl = new CodeLine(line);
                    cl.CodeLineType = CodeLine.CodeLineTypes.Comment;
                    ret.Add(cl);
                    continue;
                }
                if (line.StartsWith("="))
                {
                    string[] subs = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    var sb = new StringBuilder(subs[1]);
                    sb.Append('\t');
                    sb.Append(subs[2]);
                    sb.Append('\t');
                    sb.Append(subs[3]);
                    if (subs.Length > 4)
                        sb.Append(subs[4]);

                    var cl = new CodeLine(sb.ToString());
                    cl.CodeLineType = CodeLine.CodeLineTypes.Comment;
                    ret.Add(cl);
                    continue;
                }
                if (line.StartsWith(".") || line.StartsWith(">"))
                {
                    //string[] subs = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    ushort startAddr = ushort.Parse(line.Substring(1, 4), System.Globalization.NumberStyles.HexNumber);
                    ushort addr = startAddr;

                    if (addr == 0xe675)
                    {

                    }

                    if (mStartAddress == null)
                    {
                        var section = new OrgSection(addr);
                        section.IsDataDefined = true;
                        Sections.Sections.Add(section);
                        mStartAddress = addr;
                    }

                    int col = 6;
                    while (true)
                    {
                        if (addr > Sections.TopSection.EndAddress)
                        {
                            Sections.TopSection.EndAddress = addr;
                        }
                        string bs = line.Substring(col, 2).Trim();
                        if (string.IsNullOrEmpty(bs))
                            break;

                        byte b = byte.Parse(bs, System.Globalization.NumberStyles.HexNumber);
                        SystemMemory.SetMemory(addr++, WordSize.OneByte, b, false);
                        col += 2;

                        if (line.Length <= col || line[col] != ' ')
                            break;

                        col++;
                    }
                    var cl = new CodeLine(line.Substring(col).Trim(), startAddr, addr - startAddr);
                    cl.CodeLineType = CodeLine.CodeLineTypes.Code;
                    ret.Add(cl);
                }//if
            }//foreach
            return ret;
        }
    }
}