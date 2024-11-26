using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using System.IO;

using Processors;

namespace Processors.BenEater.Assembler
{
    public class ParserBenEater : IParser
    {
        public IList<ASMError> Errors { get { return mMyVisitor.Errors; } }
        public IDictionary<string, ushort> Labels { get { return mMyVisitor.Labels; } }
        public IList<CodeLine> CodeLines { get { return mMyVisitor.CodeLines; } }
        public string[] Lines { get; private set; }
        public uint StartAddress { get { return mMyVisitor.StartAddress; } }
        public uint EndAddress { get { return mMyVisitor.EndAddress; } }
        public OrgSections Sections { get; set; } = new OrgSections();

        public string[] SourceLines;
        private MyVisitor mMyVisitor;

        public ISystemMemory SystemMemory { get; private set; }
        private IProcessor mProcessor;
        public ParserBenEater(IProcessor processor)
        {
            SystemMemory = new SystemMemory();
            SystemMemory.Default(16);
            mProcessor = processor;
        }
        public void Parse(string filename, IList<string> lines, ParseSettings parseSettings)
        {
            var newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);
            MemoryStream ms = new MemoryStream();
            foreach (var line in lines)
            {
                var bytes = Encoding.UTF8.GetBytes(line);
                ms.Write(bytes, 0, bytes.Length);
                ms.Write(newLineBytes, 0, newLineBytes.Length);
            }
            ms.Seek(0, SeekOrigin.Begin);
            var stream = new AntlrInputStream(ms);
            var lex = new AssemblerLexer(stream);
            var tokens = new CommonTokenStream(lex);
            var parser = new AssemblerParser(tokens);

            mMyVisitor = new MyVisitor(SystemMemory, lines.ToArray(), mProcessor);
            mMyVisitor.Visit(parser.prog());
            mMyVisitor.Fixups();

            Sections.StartSection.StartAddress = mMyVisitor.StartAddress;
            Sections.StartSection.EndAddress = mMyVisitor.EndAddress;
            Sections.StartSection.IsDataDefined = true;

        }

        public void Parse(string filename, ParseSettings parseSettings)
        {
            var lines = File.ReadAllLines(filename);
            Parse(filename, lines, parseSettings);
        }
    }
}