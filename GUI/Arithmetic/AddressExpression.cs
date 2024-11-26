using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;
using Antlr4.Runtime;
using System.IO;
using GUI.Arithmetic;
using ArithmeticNamespace;
using Antlr4.Runtime.Tree;
using System.Diagnostics;
#pragma warning disable  CS3021

namespace GUI.Arithmetic
{
    public static class AddressExpression
    {
        public static uint eval(IProcessor processor, string line)
        {
            var stream = new AntlrInputStream(line);
            var lex = new ArithmeticLexer(stream);
            var tokens = new CommonTokenStream(lex);
            var parser = new ArithmeticParser(tokens);
            var arithmeticVisitor = new ArithmeticVisitor(processor);
            return (uint)arithmeticVisitor.VisitProg(parser.prog());
        }
    }
}