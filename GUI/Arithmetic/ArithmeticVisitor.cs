using ArithmeticNamespace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Processors;

namespace GUI.Arithmetic
{
    public class ArithmeticVisitor : ArithmeticBaseVisitor<int>
    {
        private IProcessor mProcessor;
        public ArithmeticVisitor(IProcessor processor)
        {
            mProcessor = processor;
        }

        public override int VisitInt([NotNull] ArithmeticParser.IntContext context)
        {
            int ret = 0;
            string str = context.GetText();
            if (str.StartsWith("0x"))
                ret = int.Parse(str.Substring(2), System.Globalization.NumberStyles.HexNumber);
            else
                ret = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            return ret;
        }

        public override int VisitAddSub([NotNull] ArithmeticParser.AddSubContext context)
        {
            int left = Visit(context.expr(0));
            int right = Visit(context.expr(1));
            if (context.op.Type == ArithmeticParser.ADD)
            {
                return left + right;
            }
            else
            {
                return left - right;
            }
        }

        public override int VisitMulDiv([NotNull] ArithmeticParser.MulDivContext context)
        {
            int left = Visit(context.expr(0));
            int right = Visit(context.expr(1));
            if (context.op.Type == ArithmeticParser.MUL)
            {
                return left * right;
            }
            else
            {
                return left / right;
            }
        }

        public override int VisitRegister([NotNull] ArithmeticParser.RegisterContext context)
        {
            return (int)mProcessor.Registers.GetDoubleRegister(context.GetText().Substring(1));
        }

        public override int VisitParens([NotNull] ArithmeticParser.ParensContext context)
        {
            return Visit(context.expr());
        }
    }
}