grammar Arithmetic;

@header
{
#pragma warning disable 3021
}

@parser::members
{
    protected const int EOF = Eof;
}
 
@lexer::members
{
    protected const int EOF = Eof;
    protected const int HIDDEN = Hidden;
}

options
{
	language = CSharp;
}

/*
 * Parser Rules
 */
 
prog: expr+ ;
 
expr : expr op=('*'|'/') expr   # MulDiv
     | expr op=('+'|'-') expr   # AddSub
     | INT                      # int
     | reg                      # register
     | '(' expr ')'             # parens
     ;
 
reg
   : REG
   ;

REG
   : '#' (('a' .. 'z') | ('A' .. 'Z'))+
   ;

/*
 * Lexer Rules
 */
INT : ('0x')?[a-fA-F0-9]+;
MUL : '*';
DIV : '/';
ADD : '+';
SUB : '-';
WS
    :   (' ' | '\r' | '\n') -> channel(HIDDEN)
    ;