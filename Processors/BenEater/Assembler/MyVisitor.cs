using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using Processors;

namespace Processors.BenEater.Assembler
{
    public class MyVisitor : AssemblerBaseVisitor<object>
    {
        private bool mIgnoreCode;

        public Dictionary<string, ushort> Labels { get; set; }

        private ushort? mLocalLabel;
        private List<Fixup> mForwardReferences;

        private delegate int HandleOpcodeDelegate(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext context);
        private Dictionary<MnemonicEnums, HandleOpcodeDelegate> instructionMap = new Dictionary<MnemonicEnums, HandleOpcodeDelegate>();
        public List<CodeLine> CodeLines { get; private set; }
        public IList<ASMError> Errors { get; private set; }
        private CodeLine mCodeLine;
        private List<Fixup> mFixups = new List<Fixup>();

        private int mLine;
        //private int mLastLine;
        private ushort mPtr;
        public ISystemMemory SystemMemory { get; private set; }
        private string[] mLines;
        private List<string> mPreProcessedLines = new List<string>();

        public ushort StartAddress { get; private set; }
        public ushort EndAddress { get; private set; }

        private IProcessor mProcessor;
        public MyVisitor(ISystemMemory systemMemory, string[] lines, IProcessor processor)
        {
            SystemMemory = systemMemory;
            mLines = lines;
            mProcessor = processor;
            init();
        }

        public override object VisitProg([NotNull] AssemblerParser.ProgContext context)
        {
            base.VisitProg(context);
            EndAddress = mPtr;
            return null;
        }

        public void Fixups()
        {
            performFixups();
        }
        private void performFixups()
        {
            foreach (var fixup in mFixups)
            {
                if (fixup.Location == 0x2e1e)
                {

                }
                ushort data = 0;
                var response = VisitExpression(fixup.Expression);
                if (response == null)
                {
                    Errors.Add(new ASMError { Text = "Could not locate label:" + fixup.Text });
                }
                else
                {
                    data = Convert.ToUInt16(response);
                    var fixAddr = fixup.Location;
                    if (fixup.WordFixup)
                    {
                        byte[] bytes = BitConverter.GetBytes(data);
                        SystemMemory[fixAddr++] = bytes[0];
                        SystemMemory[fixAddr] = bytes[1];
                    }
                    else
                        SystemMemory[fixAddr++] = (byte)data;
                }
            }
        }//performFixups

        public override object VisitLine([NotNull] AssemblerParser.LineContext ctx)
        {
            mLine = ctx.Start.Line - 1;
            if (mLine >= mLines.Length)
                return null;
            //if (mLine > (mLastLine + 1))
            //{
            //    for (int ii = 0; ii < (mLine - (mLastLine + 1)); ii++)
            //        CodeLines.Add(new CodeLine("", SystemMemory, mPtr, 0, mLine + 1));
            //}
            //mLastLine = mLine;
            string str = mLines[mLine];
            var v4 = ctx.directive();
            if (v4 != null && IsEndifDirective(v4))
                mIgnoreCode = false;

            mCodeLine = new CodeLine(str, mPtr, mLine + 1);
            int count = 0;

            if (!mIgnoreCode)
            {
                string label = extractLabel(ctx);
                if (!string.IsNullOrEmpty(label))
                {
                    if (label == "@@")
                    {
                        if (mForwardReferences != null)
                        {
                            byte[] bytes = BitConverter.GetBytes(mPtr);
                            foreach (var fixup in mForwardReferences)
                            {
                                ushort fixAddr = fixup.Location;
                                SystemMemory[fixAddr++] = bytes[0];
                                SystemMemory[fixAddr] = bytes[1];
                            }
                            mForwardReferences = null;
                        }
                        mLocalLabel = mPtr;
                    }
                    else
                    {
                        addLabel(label, mPtr);
                    }
                }

                var v3 = ctx.instruction();
                if (v3 != null)
                {
                    count = (int)VisitInstruction(v3);
                    mCodeLine.CodeLineType = CodeLine.CodeLineTypes.Code;
                }

                if (v4 != null)
                    count = handleDirective(mCodeLine, v4, label);
            }

            if (mIgnoreCode)
                mCodeLine.Text = ';' + mCodeLine.Text;

            CodeLines.Add(mCodeLine);
            if (count > 0)
                mCodeLine.Length = count;

            return null;
        }

        private string extractLabel(AssemblerParser.LineContext ctx)
        {
            string label = string.Empty;

            var v1 = ctx.label();
            if (v1 != null && v1.Start.Column == 0)
                return v1.GetText();

            var v2 = ctx.lbl();
            if (v2 != null && v2.Start.Column == 0)
            {
                var str = v2.GetText();
                return str.Substring(0, str.Length - 1);
            }

            return string.Empty;
        }

        private bool IsEndifDirective(AssemblerParser.DirectiveContext ctx)
        {
            string str = ctx.assemblerdirective().GetText().Trim().ToLower();
            return (parseDirective(str) == DirectiveEnums.endif);
        }

        private int handleDirective(CodeLine codeLine, AssemblerParser.DirectiveContext ctx, string label)
        {
            string str = ctx.assemblerdirective().GetText();
            var directive = parseDirective(str);
            switch (directive)
            {
                case DirectiveEnums.ifA:
                    {
                        var response = VisitExpressionlist(ctx.expressionlist());
                        var result = Convert.ToUInt16(response);
                        bool condResult = (result != 0);
                        mIgnoreCode = !condResult;
                    }
                    break;
                case DirectiveEnums.endif:
                    mIgnoreCode = false;
                    break;
                case DirectiveEnums.org:
                    {
                        var response = VisitExpressionlist(ctx.expressionlist());
                        mPtr = Convert.ToUInt16(response);
                        codeLine.Address = mPtr;

                        if (StartAddress == ushort.MaxValue)
                            StartAddress = mPtr;
                        return 0;
                    }
                case DirectiveEnums.equ:
                    {//fred equ 12h
                        var el = ctx.expressionlist();
                        var exp = el.expression(0);
                        string str3 = exp.GetText();

                        var response = VisitExpression(exp);
                        ushort value = convertStringResponseToUINT(response);

                        if (!string.IsNullOrEmpty(label))
                            addLabel(label.Trim().ToLower(), value, true);

                        codeLine.CodeLineType = CodeLine.CodeLineTypes.Comment;
                        codeLine.Address = value;

                        return 0;
                    }
                case DirectiveEnums.set:
                    {//set fred 12h
                        return 0;
                    }
                case DirectiveEnums.include:
                    return 0;

                case DirectiveEnums.ds:
                    {// ds 12h
                        codeLine.CodeLineType = CodeLine.CodeLineTypes.Code;
                        var response = VisitExpressionlist(ctx.expressionlist());
                        int num = Convert.ToInt32(response);
                        mPtr = (ushort)(mPtr + num);
                        //for (int ii = 0; ii < num; ii++)
                        //    storeMemoryMapByte(0);
                        return num;
                    }

                case DirectiveEnums.db:
                case DirectiveEnums.dw:
                    {
                        bool byteOp = (directive == DirectiveEnums.db);

                        codeLine.CodeLineType = CodeLine.CodeLineTypes.Code;
                        int index = 0;
                        int count = 0;
                        var expressionList = ctx.expressionlist();
                        var exp = expressionList.expression(index++);
                        do
                        {
                            var response = VisitExpression(exp);
                            if (response is string)
                            {
                                string respStr = response as string;
                                foreach (var c in respStr)
                                    storeMemoryMapByte((byte)c);
                                count += respStr.Length;
                            }
                            else
                            {
                                ushort val = 0;
                                if (response == null)
                                    addFixup(exp, mPtr, !byteOp);
                                else
                                    val = Convert.ToUInt16(response);

                                if (byteOp)
                                    storeMemoryMapByte((byte)val);
                                else
                                    storeMemoryMapWord(val);
                                count += byteOp ? 1 : 2;
                            }
                            exp = expressionList.expression(index++);
                        } while (exp != null);
                        return count;
                    }
                case DirectiveEnums.sym:
                    break;
                //                case DirectiveEnums.macro:
                //                    mMacro = new Macro(ctx, label);
                //break;
                case DirectiveEnums.endm:
                    //                    if (mMacro != null)
                    //                        mMacros.Add(mMacro);
                    //                    mMacro = null;
                    break;
            }//switch
            return 0;
        }//handleDirective

        //public override object VisitChar([NotNull] AssemblerParser.CharContext ctx)
        //{
        //    string str1 = ctx.GetText();
        //    return str1;

        //}

        //public override object VisitChar([NotNull] AssemblerParser.CharContext ctx)
        //{
        //    var str1 = ctx.CHAR().GetText();
        //    str1 = str1.Substring(1, str1.Length - 2);
        //    return (int)str1[0];
        //}
        private ushort convertStringResponseToUINT(object response)
        {
            if (response is ushort)
                return (ushort)response;
            else if (response is int)
                return (ushort)(int)response;
            else if (response is string)
            {
                string str = response as string;
                if (str.Length == 1)
                    return (ushort)str[0];
                else if (str.Length == 2)
                {
                    byte[] bytes = new byte[2] { (byte)str[0], (byte)str[1] };
                    return BitConverter.ToUInt16(bytes, 0);
                }
                else
                    addError("invalid numeric constant", mLine);
            }
            return 0;
        }

        //public override object VisitNotexpr([NotNull] AssemblerParser.NotexprContext ctx)
        //{
        //    string str = ctx.GetText().Trim().ToLower();
        //    var resp = Convert.ToUInt16(VisitExpression(ctx.expression()));
        //    return (ushort)(~resp);
        //}

        public override object VisitExpression([NotNull] AssemblerParser.ExpressionContext ctx)
        {
            string str = ctx.GetText();
            if (mLine == 240)
            {

            }
            var a1 = ctx.argument();
            if (a1 != null)
                return VisitArgument(a1);

            //var notExpr = ctx.NOT();
            //if (notExpr != null)
            //{
            //    var d = (ushort)VisitExpression(notExpr.expression());
            //    return (ushort)(~d);
            //}

            var e1 = ctx.expression(0);
            var result = VisitExpression(e1);
            if (result == null)
                return null;

            ushort resp1 = (ushort)result;
            ushort resp2 = 0;
            var e2 = ctx.expression(1);
            if (e2 == null)
            {
                var uOP = ctx.unaryop();
                if (uOP != null && uOP.GetText() == "-")
                {
                    resp1 = (ushort)-resp1;
                }
                var notOP = ctx.NOT();
                if (notOP != null)
                {
                    if (resp1 == 0)
                        resp1 = (ushort)0xffff;
                    else
                        resp1 = (ushort)0;
                }
                return resp1;
            }
            else
            {
                var t1 = VisitExpression(e2);
                if (t1 == null)
                    return null;

                resp2 = (ushort)t1;

                if (ctx.arithop() != null)
                {
                    var op = ctx.arithop().GetText();
                    if (op == "*")
                        return (ushort)(resp1 * resp2);
                    else if (op == "/")
                        return (ushort)(resp1 / resp2);
                    else if (op == "+")
                        return (ushort)(resp1 + resp2);
                    else if (op == "-")
                        return (ushort)(resp1 - resp2);
                    else
                        addError("unknown operator", mLine);
                }
                else if (ctx.EXPROPS() != null)
                {
                    var op = ctx.EXPROPS().GetText().Trim().ToLower();
                    if (op == "and")
                        return (ushort)(resp1 & resp2);
                    else if (op == "or")
                        return (ushort)(resp1 | resp2);
                    else if (op == "shl")
                        return (ushort)(resp1 << resp2);
                    else if (op == "shr")
                        return (ushort)(resp1 >> resp2);
                    else if (op == "mod")
                        return (ushort)(resp1 % resp2);
                    else
                        addError("unknown operator", mLine);
                }//elseif
            }//else
            addError("unknown operator", mLine);
            return (ushort)0;

        }//VisitExpression

        //public override object VisitArgument([NotNull] AssemblerParser.ArgumentContext ctx)
        //{
        //    return base.VisitArgument(ctx);
        //}

        public override object VisitName([NotNull] AssemblerParser.NameContext ctx)
        {
            string str = ctx.GetText().Trim().ToLower();
            if (string.Compare(str, "@f", true) == 0)
            {
                if (mForwardReferences == null)
                    mForwardReferences = new List<Fixup>();

                var fixup = new Fixup();
                fixup.Location = mPtr;
                fixup.Line = mLine;
                mForwardReferences.Add(fixup);
                return (ushort)0;
            }
            else if (string.Compare(str, "@b", true) == 0)
            {
                if (mLocalLabel == null)
                {
                    addError("reference to local label not found", mLine);
                    return null;
                }
                return mLocalLabel;
            }
            else if (Labels.ContainsKey(str))
                return Labels[str];
            else
                return null;
        }
        private void addFixup(AssemblerParser.ExpressionContext ctx, ushort location, bool wordFixup)
        {
            var fixUp = new Fixup { Expression = ctx, Location = location, WordFixup = wordFixup };
            fixUp.Text = ctx.GetText();
            fixUp.Line = mLine;
            mFixups.Add(fixUp);
        }
        private void addError(string text, int line)
        {
            Errors.Add(new ASMError { Text = text, Line = line });
        }
        private void addLabel(string label, ushort value, bool overRide = false)
        {
            string str = label.Trim().ToLower();
            if (Labels.ContainsKey(str))
            {
                if (!overRide)
                    addError(string.Format("Label {0} already exists", label), mLine);
            }
            Labels[str] = value;
        }//addLabel

        public override object VisitString([NotNull] AssemblerParser.StringContext ctx)
        {
            string str = ctx.GetText();
            str = str.Substring(1, str.Length - 2);

            str = str.Replace("\'\'", "\'");
            if (str.Length == 1)
                return (ushort)str[0];

            return str;
        }

        public override object VisitInstruction([NotNull] AssemblerParser.InstructionContext context)
        {
            var op = (MnemonicEnums)VisitOpcode(context.opcode());
            return instructionMap[op](op, context.expressionlist());
        }

        private int handleMove(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {
            var dstReg = parseSingleRegister(ctx.expression(0).GetText());
            var srcReg = parseSingleRegister(ctx.expression(1).GetText());

            //01DDDSSS
            byte dMask = (byte)((byte)dstReg << 3);
            byte sMask = (byte)srcReg;
            storeMemoryMapByte((byte)((byte)opcode | dMask | sMask));
            return 1;
        }
        private int handleSingleRegSrc(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {//dcr B
            var reg = parseSingleRegister(ctx.expression(0).GetText());
            byte ocode = (byte)opcode;
            ocode = (byte)(ocode | ((byte)reg));
            storeMemoryMapByte(ocode);
            return 1;
        }
        private int handleSingleRegDst(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {//dcr B
            var reg = parseSingleRegister(ctx.expression(0).GetText());
            byte ocode = (byte)opcode;
            ocode = (byte)(ocode | ((byte)reg << 3));
            storeMemoryMapByte(ocode);
            return 1;
        }

        private int handleRST(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {
            //11CCC111
            var exp = ctx.expression(0);
            var response = VisitExpression(exp);
            byte val = (byte)((ushort)response << 3);
            byte ocode = (byte)((byte)opcode | val);

            storeMemoryMapByte(ocode);
            return 1;
        }
        private int handleReg8Data(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {//mvi e,12h
            var reg = parseSingleRegister(ctx.expression(0).GetText());
            //00DDD110;
            byte mask = (byte)reg;
            storeMemoryMapByte((byte)((byte)opcode | (mask << 3)));

            var exp = ctx.expression(1);
            var response = VisitExpression(exp);
            ushort val = 0;
            if (response == null)
                addFixup(exp, mPtr, false);
            else
                val = convertStringResponseToUINT(response);

            storeMemoryMapByte((byte)val);
            return 2;
        }
        private int handle8Data(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {
            storeMemoryMapByte((byte)opcode);
            var exp = ctx.expression(0);
            var response = VisitExpression(exp);

            string str = exp.GetText();
            ushort value = 0;
            if (response == null)
                addFixup(exp, mPtr, false);
            else
                value = convertStringResponseToUINT(response);

            storeMemoryMapByte((byte)value);
            return 2;
        }
        private int handle4Data(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {//lda 5
            var exp = ctx.expression(0);
            var response = VisitExpression(exp);
            ushort value = 0;
            if (response == null)
                addFixup(exp, mPtr, true);
            else
                value = convertStringResponseToUINT(response);

            int v1 = (int)opcode;
            int v2 = v1 | value;

            storeMemoryMapByte((byte)v2);
            return 1;

        }//handle16Data

        private int handle4Addr(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {//lda  [0x5]
            var exp = ctx.expression(0);
            var response = VisitExpression(exp);
            ushort value = 0;
            if (response == null)
                addFixup(exp, mPtr, true);
            else
                value = convertStringResponseToUINT(response);

            int v1 = (int)opcode;
            int v2 = v1 | value;

            storeMemoryMapByte((byte)v2);
            return 1;

        }//handle16Data

        private int handleNoArguments(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {
            storeMemoryMapByte((byte)opcode);
            return 1;
        }

        private void storeMemoryMapByte(byte data)
        {
            SystemMemory[mPtr++] = data;
            if (mPtr > EndAddress)
                EndAddress = mPtr;
        }
        private void storeMemoryMapWord(ushort data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            SystemMemory[mPtr++] = bytes[0];
            SystemMemory[mPtr++] = bytes[1];
        }

        private int handleBranch(MnemonicEnums opcode, AssemblerParser.ExpressionlistContext ctx)
        {
            storeMemoryMapByte((byte)opcode);
            string str = ctx.GetText().Trim().ToLower();
            var response = VisitExpression(ctx.expression(0));

            ushort val = 0;
            if (response == null)
                addFixup(ctx.expression(0), mPtr, true);
            else
                val = Convert.ToUInt16(response);

            storeMemoryMapWord(val);
            return 3;
        }

        public override object VisitNumber([NotNull] AssemblerParser.NumberContext ctx)
        {
            string str = ctx.GetText().Trim().ToLower();

            ushort val = 0;
            if (str.EndsWith("h"))
            {
                if (!ushort.TryParse(str.Substring(0, str.Length - 1), System.Globalization.NumberStyles.AllowHexSpecifier, null, out val))
                {
                    addError("invalid hexadecimal numeric constant", mLine);
                    return null;
                }
            }
            else if (str.EndsWith("b"))
            {
                for (int ii = 0; ii < str.Length - 1; ii++)
                {
                    val = (ushort)(val << 1);
                    int bit = ((str[ii] == '1') ? 1 : 0);
                    val = (ushort)(val | bit);
                }
            }
            else if (str.EndsWith("q"))
            {
                val = 0;
                for (int ii = 0; ii < str.Length - 1; ii++)
                {
                    val *= 8;
                    val += (ushort)(str[ii] - '0');
                }
            }
            else
            {
                if (!ushort.TryParse(str, out val))
                {
                    addError("invalid decimal numeric constant", mLine);
                    val = 0;
                }
            }
            return val;
        }

        public override object VisitDollar([NotNull] AssemblerParser.DollarContext ctx)
        {
            return mPtr;
        }

        public override object VisitOpcode([NotNull] AssemblerParser.OpcodeContext ctx)
        {
            string str = ctx.OPCODE().GetText().Trim().ToLower();
            if (str == "in")
                str = "inA";
            if (str == "out")
                str = "outA";
            MnemonicEnums op = (MnemonicEnums)Enum.Parse(typeof(MnemonicEnums), str);
            return op;
        }

        private SingleRegisterEnums parseSingleRegister(string str)
        {
            return (SingleRegisterEnums)Enum.Parse(typeof(SingleRegisterEnums), str.Trim().ToLower());
        }

        private DirectiveEnums parseDirective(string op)
        {
            string str = op.ToString().Trim().ToLower();
            if (str == "if")
                return DirectiveEnums.ifA;
            else
                return (DirectiveEnums)Enum.Parse(typeof(DirectiveEnums), str);
        }


        private void init()
        {
            instructionMap[MnemonicEnums.nop] = handleNoArguments;
            instructionMap[MnemonicEnums.lda] = handle4Addr;
            instructionMap[MnemonicEnums.add] = handle4Addr;
            instructionMap[MnemonicEnums.sub] = handle4Addr;
            instructionMap[MnemonicEnums.sta] = handle4Addr;
            instructionMap[MnemonicEnums.ldi] = handle4Data;
            instructionMap[MnemonicEnums.jmp] = handle4Data;
            instructionMap[MnemonicEnums.jc] = handle4Data;
            instructionMap[MnemonicEnums.outA] = handleNoArguments;
            instructionMap[MnemonicEnums.hlt] = handleNoArguments;

            Labels = new Dictionary<string, ushort>();
            CodeLines = new List<CodeLine>();
            Errors = new List<ASMError>();
            StartAddress = ushort.MaxValue;
            //mMacros = new Macros();

        }//init

    }//class MyVisitor

    public class Fixup
    {
        public AssemblerParser.ExpressionContext Expression { get; set; }
        public string Text { get; set; }
        public ushort Location { get; set; }
        public bool WordFixup { get; set; }
        public int Line { get; set; }
    }
    //public class ASMError
    //{
    //    public string Text { get; set; }
    //    public int Line { get; set; }
    //}
}