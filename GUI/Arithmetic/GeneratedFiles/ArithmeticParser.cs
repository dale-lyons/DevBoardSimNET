//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Arithmetic.g4 by ANTLR 4.7.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace ArithmeticNamespace {

#pragma warning disable 3021

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.1")]
[System.CLSCompliant(false)]
public partial class ArithmeticParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, REG=3, INT=4, MUL=5, DIV=6, ADD=7, SUB=8, WS=9;
	public const int
		RULE_prog = 0, RULE_expr = 1, RULE_reg = 2;
	public static readonly string[] ruleNames = {
		"prog", "expr", "reg"
	};

	private static readonly string[] _LiteralNames = {
		null, "'('", "')'", null, null, "'*'", "'/'", "'+'", "'-'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, "REG", "INT", "MUL", "DIV", "ADD", "SUB", "WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Arithmetic.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static ArithmeticParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}


	    protected const int EOF = Eof;

		public ArithmeticParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public ArithmeticParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}
	public partial class ProgContext : ParserRuleContext {
		public ExprContext[] expr() {
			return GetRuleContexts<ExprContext>();
		}
		public ExprContext expr(int i) {
			return GetRuleContext<ExprContext>(i);
		}
		public ProgContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_prog; } }
		public override void EnterRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.EnterProg(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.ExitProg(this);
		}
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IArithmeticVisitor<TResult> typedVisitor = visitor as IArithmeticVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitProg(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ProgContext prog() {
		ProgContext _localctx = new ProgContext(Context, State);
		EnterRule(_localctx, 0, RULE_prog);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 7;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			do {
				{
				{
				State = 6; expr(0);
				}
				}
				State = 9;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			} while ( (((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << T__0) | (1L << REG) | (1L << INT))) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ExprContext : ParserRuleContext {
		public ExprContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_expr; } }
	 
		public ExprContext() { }
		public virtual void CopyFrom(ExprContext context) {
			base.CopyFrom(context);
		}
	}
	public partial class ParensContext : ExprContext {
		public ExprContext expr() {
			return GetRuleContext<ExprContext>(0);
		}
		public ParensContext(ExprContext context) { CopyFrom(context); }
		public override void EnterRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.EnterParens(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.ExitParens(this);
		}
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IArithmeticVisitor<TResult> typedVisitor = visitor as IArithmeticVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitParens(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class MulDivContext : ExprContext {
		public IToken op;
		public ExprContext[] expr() {
			return GetRuleContexts<ExprContext>();
		}
		public ExprContext expr(int i) {
			return GetRuleContext<ExprContext>(i);
		}
		public MulDivContext(ExprContext context) { CopyFrom(context); }
		public override void EnterRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.EnterMulDiv(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.ExitMulDiv(this);
		}
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IArithmeticVisitor<TResult> typedVisitor = visitor as IArithmeticVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitMulDiv(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class AddSubContext : ExprContext {
		public IToken op;
		public ExprContext[] expr() {
			return GetRuleContexts<ExprContext>();
		}
		public ExprContext expr(int i) {
			return GetRuleContext<ExprContext>(i);
		}
		public AddSubContext(ExprContext context) { CopyFrom(context); }
		public override void EnterRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.EnterAddSub(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.ExitAddSub(this);
		}
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IArithmeticVisitor<TResult> typedVisitor = visitor as IArithmeticVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitAddSub(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class IntContext : ExprContext {
		public ITerminalNode INT() { return GetToken(ArithmeticParser.INT, 0); }
		public IntContext(ExprContext context) { CopyFrom(context); }
		public override void EnterRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.EnterInt(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.ExitInt(this);
		}
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IArithmeticVisitor<TResult> typedVisitor = visitor as IArithmeticVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitInt(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class RegisterContext : ExprContext {
		public RegContext reg() {
			return GetRuleContext<RegContext>(0);
		}
		public RegisterContext(ExprContext context) { CopyFrom(context); }
		public override void EnterRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.EnterRegister(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.ExitRegister(this);
		}
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IArithmeticVisitor<TResult> typedVisitor = visitor as IArithmeticVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitRegister(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ExprContext expr() {
		return expr(0);
	}

	private ExprContext expr(int _p) {
		ParserRuleContext _parentctx = Context;
		int _parentState = State;
		ExprContext _localctx = new ExprContext(Context, _parentState);
		ExprContext _prevctx = _localctx;
		int _startState = 2;
		EnterRecursionRule(_localctx, 2, RULE_expr, _p);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 18;
			ErrorHandler.Sync(this);
			switch (TokenStream.LA(1)) {
			case INT:
				{
				_localctx = new IntContext(_localctx);
				Context = _localctx;
				_prevctx = _localctx;

				State = 12; Match(INT);
				}
				break;
			case REG:
				{
				_localctx = new RegisterContext(_localctx);
				Context = _localctx;
				_prevctx = _localctx;
				State = 13; reg();
				}
				break;
			case T__0:
				{
				_localctx = new ParensContext(_localctx);
				Context = _localctx;
				_prevctx = _localctx;
				State = 14; Match(T__0);
				State = 15; expr(0);
				State = 16; Match(T__1);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			Context.Stop = TokenStream.LT(-1);
			State = 28;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,3,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( ParseListeners!=null )
						TriggerExitRuleEvent();
					_prevctx = _localctx;
					{
					State = 26;
					ErrorHandler.Sync(this);
					switch ( Interpreter.AdaptivePredict(TokenStream,2,Context) ) {
					case 1:
						{
						_localctx = new MulDivContext(new ExprContext(_parentctx, _parentState));
						PushNewRecursionContext(_localctx, _startState, RULE_expr);
						State = 20;
						if (!(Precpred(Context, 5))) throw new FailedPredicateException(this, "Precpred(Context, 5)");
						State = 21;
						((MulDivContext)_localctx).op = TokenStream.LT(1);
						_la = TokenStream.LA(1);
						if ( !(_la==MUL || _la==DIV) ) {
							((MulDivContext)_localctx).op = ErrorHandler.RecoverInline(this);
						}
						else {
							ErrorHandler.ReportMatch(this);
						    Consume();
						}
						State = 22; expr(6);
						}
						break;
					case 2:
						{
						_localctx = new AddSubContext(new ExprContext(_parentctx, _parentState));
						PushNewRecursionContext(_localctx, _startState, RULE_expr);
						State = 23;
						if (!(Precpred(Context, 4))) throw new FailedPredicateException(this, "Precpred(Context, 4)");
						State = 24;
						((AddSubContext)_localctx).op = TokenStream.LT(1);
						_la = TokenStream.LA(1);
						if ( !(_la==ADD || _la==SUB) ) {
							((AddSubContext)_localctx).op = ErrorHandler.RecoverInline(this);
						}
						else {
							ErrorHandler.ReportMatch(this);
						    Consume();
						}
						State = 25; expr(5);
						}
						break;
					}
					} 
				}
				State = 30;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,3,Context);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			UnrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	public partial class RegContext : ParserRuleContext {
		public ITerminalNode REG() { return GetToken(ArithmeticParser.REG, 0); }
		public RegContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_reg; } }
		public override void EnterRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.EnterReg(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IArithmeticListener typedListener = listener as IArithmeticListener;
			if (typedListener != null) typedListener.ExitReg(this);
		}
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IArithmeticVisitor<TResult> typedVisitor = visitor as IArithmeticVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitReg(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public RegContext reg() {
		RegContext _localctx = new RegContext(Context, State);
		EnterRule(_localctx, 4, RULE_reg);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 31; Match(REG);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 1: return expr_sempred((ExprContext)_localctx, predIndex);
		}
		return true;
	}
	private bool expr_sempred(ExprContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0: return Precpred(Context, 5);
		case 1: return Precpred(Context, 4);
		}
		return true;
	}

	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x3', '\v', '$', '\x4', '\x2', '\t', '\x2', '\x4', '\x3', '\t', 
		'\x3', '\x4', '\x4', '\t', '\x4', '\x3', '\x2', '\x6', '\x2', '\n', '\n', 
		'\x2', '\r', '\x2', '\xE', '\x2', '\v', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x5', 
		'\x3', '\x15', '\n', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\a', '\x3', '\x1D', '\n', '\x3', 
		'\f', '\x3', '\xE', '\x3', ' ', '\v', '\x3', '\x3', '\x4', '\x3', '\x4', 
		'\x3', '\x4', '\x2', '\x3', '\x4', '\x5', '\x2', '\x4', '\x6', '\x2', 
		'\x4', '\x3', '\x2', '\a', '\b', '\x3', '\x2', '\t', '\n', '\x2', '%', 
		'\x2', '\t', '\x3', '\x2', '\x2', '\x2', '\x4', '\x14', '\x3', '\x2', 
		'\x2', '\x2', '\x6', '!', '\x3', '\x2', '\x2', '\x2', '\b', '\n', '\x5', 
		'\x4', '\x3', '\x2', '\t', '\b', '\x3', '\x2', '\x2', '\x2', '\n', '\v', 
		'\x3', '\x2', '\x2', '\x2', '\v', '\t', '\x3', '\x2', '\x2', '\x2', '\v', 
		'\f', '\x3', '\x2', '\x2', '\x2', '\f', '\x3', '\x3', '\x2', '\x2', '\x2', 
		'\r', '\xE', '\b', '\x3', '\x1', '\x2', '\xE', '\x15', '\a', '\x6', '\x2', 
		'\x2', '\xF', '\x15', '\x5', '\x6', '\x4', '\x2', '\x10', '\x11', '\a', 
		'\x3', '\x2', '\x2', '\x11', '\x12', '\x5', '\x4', '\x3', '\x2', '\x12', 
		'\x13', '\a', '\x4', '\x2', '\x2', '\x13', '\x15', '\x3', '\x2', '\x2', 
		'\x2', '\x14', '\r', '\x3', '\x2', '\x2', '\x2', '\x14', '\xF', '\x3', 
		'\x2', '\x2', '\x2', '\x14', '\x10', '\x3', '\x2', '\x2', '\x2', '\x15', 
		'\x1E', '\x3', '\x2', '\x2', '\x2', '\x16', '\x17', '\f', '\a', '\x2', 
		'\x2', '\x17', '\x18', '\t', '\x2', '\x2', '\x2', '\x18', '\x1D', '\x5', 
		'\x4', '\x3', '\b', '\x19', '\x1A', '\f', '\x6', '\x2', '\x2', '\x1A', 
		'\x1B', '\t', '\x3', '\x2', '\x2', '\x1B', '\x1D', '\x5', '\x4', '\x3', 
		'\a', '\x1C', '\x16', '\x3', '\x2', '\x2', '\x2', '\x1C', '\x19', '\x3', 
		'\x2', '\x2', '\x2', '\x1D', ' ', '\x3', '\x2', '\x2', '\x2', '\x1E', 
		'\x1C', '\x3', '\x2', '\x2', '\x2', '\x1E', '\x1F', '\x3', '\x2', '\x2', 
		'\x2', '\x1F', '\x5', '\x3', '\x2', '\x2', '\x2', ' ', '\x1E', '\x3', 
		'\x2', '\x2', '\x2', '!', '\"', '\a', '\x5', '\x2', '\x2', '\"', '\a', 
		'\x3', '\x2', '\x2', '\x2', '\x6', '\v', '\x14', '\x1C', '\x1E',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace ArithmeticNamespace
