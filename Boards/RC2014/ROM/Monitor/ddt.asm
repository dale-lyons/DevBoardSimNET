RAM_VECTORS	equ	0
ramStart	equ	08000h
	org		0
	di				; disable interrupts
	jmp	ddtStart

	#INCLUDE "boot.asm"
	INCL	"main.asm"
	INCL 	"uart.asm"
	INCL 	"vector.asm"
	INCL 	"strings.asm"
	INCL 	"dump.asm"
	INCL 	"move.asm"
	INCL 	"compare.asm"
	INCL 	"search.asm"
	INCL 	"fill.asm"
	INCL 	"unassemble.asm"
	INCL 	"OpCode.asm"
	INCL 	"registers.asm"
	INCL 	"load.asm"
	INCL 	"Trace.asm"
	INCL 	"io.asm"
	INCL 	"enter.asm"
	INCL 	"go.asm"
	INCL 	"cpm.asm"
	INCL 	"help.asm"
	INCL	"instruction.asm"
	INCL	"RpTest_8085.asm"
	INCL	"RpTest_Z80.asm"
initmsg:	db	'DDT (Dales debug Tool) for RC2014 (Z80) v1.0', 0

	org		ramStart
	INCL "ram.asm"
	end