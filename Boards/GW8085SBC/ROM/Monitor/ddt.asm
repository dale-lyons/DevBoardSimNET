RAM_VECTORS		equ		1
	org	0c000h
	di					; disable interrupts
	jmp		ddtStart

	INCL "main.asm"
	INCL "uart.asm"
	INCL "vector.asm"
	INCL "strings.asm"
	INCL "dump.asm"
	INCL "move.asm"
	INCL "compare.asm"
	INCL "search.asm"
	INCL "fill.asm"
	INCL "unassemble.asm"
	INCL "OpCode.asm"
	INCL "registers.asm"
	INCL "load.asm"
	INCL "Trace.asm"
	INCL "io.asm"
	INCL "enter.asm"
	INCL "go.asm"
	INCL "cpm.asm"
	INCL "help.asm"
	INCL "instruction.asm"
initmsg:	db	'DDT (Dales debug Tool) for GW8085SBC (8085) v1.0', 0

ramStart	equ	$
	INCL	"ram.asm"
	end