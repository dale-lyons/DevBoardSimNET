// A85.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
//#define _CRT_SECURE_NO_WARNINGS

#include <iostream>


#include "a85.h"
#include <stdlib.h>
#include <string.h>
#include <ctype.h>

static char mIncludeDirectory[256];
static char mIncludePath[256];

int pass = 0;
int eject, filesp, forwd, listhex;
unsigned  address, bytes, errors, listleft, obj[MAXLINE], pagelen, pc;
FILE* filestk[FILES], * source;
TOKEN token;
static int done, ifsp, off;

/* Turbo C has "line" as graphic function, change to lline HRJ */
char errcode, lline[MAXLINE + 1], title[MAXLINE];

/* eternal routines HRJ*/
void asm_line(void);
void lclose(void), lopen(const char*), lputs(void);
void hclose(void), hopen(const char*), hputc(unsigned);
void error(char), fatal_error(char*), warning(char*);
void lerror(void); /* added to list error count HRJ */

void pops(char*), pushc(int), trash(void);
void hseek(unsigned);
void unlex(void);
int isalph(char); /* was int isalph(int) HRJ */

/* these are local but used before defined HRJ */
static void do_label(void), normal_op(void), pseudo_op(void);
static void flush(void);

int main(int argc, char** argv)
{
	//    std::cout << "Hello World!\n";

	SCRATCH unsigned* o;
	int newline(void);
	mIncludeDirectory[0] = 0;

	printf("8085 Cross-Assembler (Portable) Ver 0.2\n");
	printf("Copyright (c) 1985,1987 William C. Colley, III\n");
	printf("fixes for LCC/Windows (c) 2013 Herb Johnson\n");
	printf("Glitch Works modifications (c) 2020 The Glitch Works\n\n");

	while (--argc > 0)
	{
		if (**++argv == '-')
		{
			switch (toupper(*++ * argv))
			{
			case 'I':
				if (!*++ * argv)
				{
					if (!--argc)
					{
						warning((char*)NOINCL);
						break;
					}
					else
						++argv;
				}
				strcpy_s(mIncludeDirectory, sizeof(mIncludeDirectory), *argv);
				break;


			case 'L':
				if (!*++ * argv)
				{
					if (!--argc)
					{
						warning((char*)NOLST);
						break;
					}
					else
						++argv;
				}
				lopen(*argv);
				break;

			case 'O':
				if (!*++ * argv)
				{
					if (!--argc)
					{
						warning((char*)NOHEX);
						break;
					}
					else ++argv;
				}
				hopen((const char *)*argv);
				break;

			default:
				warning((char*)BADOPT);
			}
		}
		else if (filestk[0]) warning((char*)TWOASM);
		else if (!(filestk[0] = fopen(*argv, "r"))) fatal_error((char*)ASMOPEN);
	}
	if (!filestk[0]) fatal_error((char*)NOASM);

	if(strlen(mIncludeDirectory) > 0)
		printf("Include Directory Specified:%s\n", mIncludeDirectory);
	

	while (++pass < 3)
	{
		source = filestk[0];
		if (source == NULL)
			return 1;

		fseek(source, 0L, 0);
		done = FALSE;
		off = FALSE;

		errors = filesp = ifsp = pagelen = pc = 0;  title[0] = '\0';

		while (!done) {
			errcode = ' ';
			if (newline()) {  //reach EOF instead of "END" statement
				error('*');
				strcpy(lline, "\tEND\t ;added by A85\n");
				done = eject = TRUE;  listhex = FALSE;
				bytes = 0;
			}

			else asm_line();
			pc = word(pc + bytes);

			if (pass == 2) {
				if (done) lerror();
				lputs();
				for (o = obj; bytes--; hputc(*o++));
			}
		}
	}

	if (filestk[0] == NULL)
		return 1;

	fclose(filestk[0]);  lclose();  hclose();

	if (errors) printf("%d Error(s)\n", errors);
	else printf("No Errors\n");

	exit(errors);
}

static char label[MAXLINE];
static int ifstack[IFDEPTH] = { ON };

static OPCODE* opcod;

/*  Line assembly routine.  This routine gets expressions and tokens	*/
/*  from the source file using the expression evaluator and lexical	*/
/*  analyzer, respectively.  It fills a buffer with the machine code	*/
/*  bytes and returns nothing.						*/
void asm_line(void)
{
	SCRATCH char* p;
	SCRATCH int i;
	int popc(void);
	OPCODE* find_code(char*), * find_operator(char*);



	address = pc;  bytes = 0;  eject = forwd = listhex = FALSE;
	for (i = 0; i < BIGINST; obj[i++] = NOP);

	label[0] = '\0';
	if ((i = popc()) != ' ' && i != '\n')
	{
		if (isalph((char)i))
		{ //HRJ
			pushc(i);  pops(label);
			/*HRJ need to remove colon from label? */
			for (p = label; *p; ++p);
			if (*--p == ':') *p = '\0';

			if (find_operator(label)) {
				label[0] = '\0';
				error('L');
			}
		}

		else {
			error('L');
			while ((i = popc()) != ' ' && i != '\n');
		}
	}

	trash();
	opcod = NULL;

	if ((i = popc()) != '\n') {
		if (!isalph((char)i)) error('S');

		else {
			pushc(i);  pops(token.sval);
			if (!(opcod = find_code(token.sval))) error('O');
		}

		if (!opcod) {
			listhex = TRUE;
			bytes = BIGINST;
		}
	}

	if (opcod && opcod->attr & ISIF) {
		if (label[0]) error('L');
	}

	else if (off) {
		listhex = FALSE;
		flush();
		return;
	}

	if (!opcod) {
		do_label();
		flush();
	}

	else {
		listhex = TRUE;
		if (opcod->attr & PSEUDO) pseudo_op();
		else normal_op();
		// HRJ this is where ! operator would be seen
		while ((i = popc()) != '\n')
		{
			if (i != ' ')
				error('T');
		}
	}

	source = filestk[filesp];
	return;
}

static void flush(void)
{
	int popc(void);

	while (popc() != '\n');
}

static void do_label(void)
{
	SCRATCH SYMBOL* l;
	SYMBOL* find_symbol(char*), * new_symbol(char*);

	if (label[0]) {
		listhex = TRUE;

		if (pass == 1) {
			if (!((l = new_symbol(label))->attr)) {
				l->attr = FORWD + VAL;
				l->valu = pc;
			}
		}

		else
		{
			if ((l = find_symbol(label))) {
				l->attr = VAL;
				if (l->valu != pc) error('M');
			}

			else error('P');
		}
	}
}

static void normal_op(void)
{
	SCRATCH unsigned attrib, u;
	unsigned expr(void);
	TOKEN* lex(void);
	void do_label(void), unlex(void);

	do_label();
	bytes = (attrib = opcod->attr) & BYTES;

	if (pass < 2) return;

	obj[0] = opcod->valu;  obj[1] = obj[2] = 0;

	while (attrib & ARG1) {
		lex();

		switch (attrib & ARG1) {
		case DATA_16:
			unlex();
			obj[1] = low(u = expr());
			obj[2] = high(u);
			break;

		case DATA_8:
			unlex();

			if ((u = expr()) > 0xff && u < 0xff80) {
				error('V');  u = 0;
			}

			obj[1] = low(u);
			break;

		case PORT:
			unlex();

			if ((u = expr()) > 0xff) {
				error('V');
				u = 0;
			}

			obj[1] = low(u);
			break;

		case RST_NUM:
			unlex();

			if ((u = expr()) > 7) {
				error('V');
				u = 0;
			}

			obj[0] |= u << 3;
			break;

		case LDAX_REG:
			u = BD;
			goto do_reg;

		case DAD_REG:
			u = BDHSP;
			goto do_reg;

		case POP_REG:
			u = BDHPSW;
			goto do_reg;

		case SRC_REG:
			token.valu >>= 3;
			u = BCDEHLMA;
			goto do_reg;

		case DST_REG:
			u = BCDEHLMA;

		do_reg:
			if ((token.attr & TYPE) != REG) {
				error('S');  break;
			}

			if (!(token.attr & u)) {
				error('R');
				break;
			}

			obj[0] |= token.valu;
			break;
		}

		if (((attrib >>= 4) & ARG1) && (lex()->attr & TYPE) != SEP) {
			error('S');  break;
		}

		if (obj[0] == 0x76) error('R');
	}
}

static void pseudo_op(void)
{
	SCRATCH char* s;
	SCRATCH int c;
	SCRATCH unsigned* o, u;
	SCRATCH SYMBOL* l;
	unsigned expr(void);
	SYMBOL* find_symbol(char*), * new_symbol(char*);
	TOKEN* lex(void);

	int popc(void);

	o = obj;

	switch (opcod->valu) {
	case DB:
		do_label();

		do {
			switch (lex()->attr & TYPE) {
			case SEP:
				unlex();
				u = 0;
				goto save_byte;

			case STR:
				trash();
				pushc(c = popc());

				if (c == ',' || c == '\n') {
					for (s = token.sval; *s; *o++ = *s++) ++bytes;
					break;
				}

			default:
				unlex();
				if ((u = expr()) > 0xff && u < 0xff80) {
					u = 0;
					error('V');
				}

			save_byte:
				*o++ = low(u);
				++bytes;
				break;
			}
		} while ((lex()->attr & TYPE) == SEP);

		break;

	case DS:
		do_label();
		u = word(pc + expr());

		if (forwd) error('P');

		else {
			pc = u;
			if (pass == 2) hseek(pc);
		}
		break;

	case DW:
		do_label();

		do {
			lex();
			unlex();

			u = ((token.attr & TYPE) == SEP) ? 0 : expr();

			*o++ = low(u);
			*o++ = high(u);
			bytes += 2;

		} while ((lex()->attr & TYPE) == SEP);

		break;

	case ELSE:
		listhex = FALSE;
		if (ifsp) off = (ifstack[ifsp] = -ifstack[ifsp]) != ON;

		else error('I');
		break;

	case END:
		do_label();
		if (filesp) {
			listhex = FALSE;
			error('*');
		}

		else {
			done = eject = TRUE;

			if (pass == 2) {
				if ((lex()->attr & TYPE) != EOL) {
					unlex();
					hseek(address = expr());
				}
			}

			if (ifsp) error('I');
		}

		break;

	case ENDIF:
		listhex = FALSE;

		if (ifsp) off = ifstack[--ifsp] != ON;

		else error('I');

		break;

	case EQU:
		if (label[0]) {
			if (pass == 1) {
				if (!((l = new_symbol(label))->attr)) {
					l->attr = FORWD + VAL;
					address = expr();

					if (!forwd) l->valu = address;
				}
			}

			else {
				if ((l = find_symbol(label))) {
					l->attr = VAL;
					address = expr();

					if (forwd) error('P');

					if (l->valu != address) error('M');
				}

				else error('P');
			}
		}

		else error('L');

		break;

	case IF:
		if (++ifsp == IFDEPTH) fatal_error((char*)IFOFLOW);

		address = expr();

		if (forwd) {
			error('P');
			address = TRUE;
		}

		if (off) {
			listhex = FALSE;
			ifstack[ifsp] = ZERO;
		} /* was NULL but error HRJ*/

		else {
			ifstack[ifsp] = address ? ON : OFF;
			if (!address) off = TRUE;
		}

		break;

	case INCL:
		listhex = FALSE;
		do_label();

		if ((lex()->attr & TYPE) == STR) {
			if (++filesp == FILES) fatal_error((char*)FLOFLOW);

			if (!(filestk[filesp] = fopen(token.sval, "r")))
			{
				if (strlen(mIncludeDirectory) > 0)
				{
					strcpy(mIncludePath, mIncludeDirectory);
					strcat(mIncludePath, "\\");
					strcat(mIncludePath, token.sval);
					if (!(filestk[filesp] = fopen(mIncludePath, "r")))
					{
						--filesp;
						error('V');
					}
				}
			}
		}
		else
			error('S');
		break;

	case ORG:
		u = expr();
		if (forwd) error('P');

		else {
			pc = address = u;
			if (pass == 2) hseek(pc);
		}

		do_label();
		break;

	case PAGE:
		listhex = FALSE;
		do_label();

		if ((lex()->attr & TYPE) != EOL) {
			unlex();

			if ((pagelen = expr()) > 0 && pagelen < 3) {
				pagelen = 0;
				error('V');
			}
		}

		eject = TRUE;
		break;

	case PRINT:
		listhex = FALSE;
		do_label();

		if ((lex()->attr & TYPE) != STR) error('S');

		if (pass == 1) printf("%s\n", token.sval);

		break;

	case SET:
		if (label[0]) {
			if (pass == 1) {
				if (!((l = new_symbol(label))->attr) || (l->attr & SOFT)) {
					l->attr = FORWD + SOFT + VAL;
					address = expr();

					if (!forwd) l->valu = address;
				}
			}
			else {
				if ((l = find_symbol(label))) {
					address = expr();

					if (forwd) error('P');

					else if (l->attr & SOFT) {
						l->attr = SOFT + VAL;
						l->valu = address;
					}

					else error('M');
				}

				else error('P');
			}
		}

		else error('L');

		break;

	case TITLE:
		listhex = FALSE;
		do_label();

		if ((lex()->attr & TYPE) == EOL) title[0] = '\0';

		else if ((token.attr & TYPE) != STR) error('S');

		else strcpy(title, token.sval);

		break;
	}
	return;
}