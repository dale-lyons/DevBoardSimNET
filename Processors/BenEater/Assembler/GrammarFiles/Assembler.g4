grammar Assembler;

@header
{
#pragma warning disable 3021
}

options
{
	language = CSharp;
}

prog
   : (line? EOL) + END
   ;

line : ((lbl) | (lbl instruction) | (instruction) | (label directive) | (lbl directive) | (directive))? comment?;


instruction
   : opcode expressionlist?
   ;

opcode
   : OPCODE
   ;

register_
   : REGISTER
   ;

directive
   : assemblerdirective expressionlist?
   ;

assemblerdirective
   : ASSEMBLER_DIRECTIVE
   ;

lbl
   : label ':'
   ;

expressionlist
   : expression (',' expression)*
   ;

label
   : name
   ;

expression
   : argument
   | '(' expression ')'
   | '[' expression ']'
   | unaryop expression
   | NOT expression
   | expression arithop expression
   | expression EXPROPS expression
   ;


arithop:('/'|'*'|'+'|'-');
unaryop:('+' | '-');

TEXT: 
	('\'' (~[\r\n'] | '\'\'')* '\'')
	;

argument
   : number
   | register_
   | dollar
   | name
   | string
   ;

dollar
   :'$'
   ;

string
   :TEXT
   ;

name
   : NAME
   ;

number
   : NUMBER
   ;

comment
   : COMMENT
   ;

NOT
  : (N O T WS)
  ;

END
  : (E N D)
  ;

ASSEMBLER_DIRECTIVE
   : (O R G) | (E Q U) | (D B) | (D W) | (D S) | (I F) | (E N D I F) | (S E T) | (S Y M)
   ;


REGISTER
   : 'A' | 'B' | 'C' | 'D' | 'E' | 'H' | 'L' | 'PC' | 'SP'
   ;


OPCODE
   : (N O P) | (L D A) | (A D D) | (S U B) | (S T A) | (L D I) | (J M P) | (J C) | (O U T) | (H L T)
   ;


EXPROPS
   : (S H L WS) | (S H R WS) | (M O D WS) | (O R WS) | (A N D WS)
   ;

fragment A
   : ('a' | 'A')
   ;


fragment B
   : ('b' | 'B')
   ;


fragment C
   : ('c' | 'C')
   ;


fragment D
   : ('d' | 'D')
   ;

fragment E
   : ('e' | 'E')
   ;


fragment F
   : ('f' | 'F')
   ;


fragment G
   : ('g' | 'G')
   ;


fragment H
   : ('h' | 'H')
   ;


fragment I
   : ('i' | 'I')
   ;


fragment J
   : ('j' | 'J')
   ;


fragment K
   : ('k' | 'K')
   ;


fragment L
   : ('l' | 'L')
   ;


fragment M
   : ('m' | 'M')
   ;


fragment N
   : ('n' | 'N')
   ;


fragment O
   : ('o' | 'O')
   ;


fragment P
   : ('p' | 'P')
   ;


fragment Q
   : ('q' | 'Q')
   ;


fragment R
   : ('r' | 'R')
   ;


fragment S
   : ('s' | 'S')
   ;


fragment T
   : ('t' | 'T')
   ;


fragment U
   : ('u' | 'U')
   ;


fragment V
   : ('v' | 'V')
   ;


fragment W
   : ('w' | 'W')
   ;


fragment X
   : ('x' | 'X')
   ;


fragment Y
   : ('y' | 'Y')
   ;


fragment Z
   : ('z' | 'Z')
   ;

NAME
   : [a-zA-Z@] [a-z_A-Z0-9."@]*
   ;


NUMBER
   : ([0-9a-fA-F]+ ('H' | 'h')?) | ([0-1]+ ('B' | 'b')?) | ([0-7]+ ('Q' | 'q')?)
   ;

COMMENT
   : ';' ~[\r\n]*
   ;

EOL
   : [\r\n] +
   ;

WS
   : [ \t] -> skip
   ;
