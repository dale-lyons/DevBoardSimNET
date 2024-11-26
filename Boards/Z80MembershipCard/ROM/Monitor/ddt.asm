CR		equ	0dh
LF		equ	0ah
SPACE		equ	020h
BACKSPACE	equ	08h
TAB		equ	09h
retOpcode	equ	0c9h
ramStart	equ	08000h

	org	0
	di			; disable interrupts
	jmp	reset

	org	0008h		; RST	0x08
	ret

	org	0010h		; RST	0x10
	ret
		
	org	0018h		; RST	0x18
	ret
		
	org	0020h		; RST	0x20
	ret

	org	0028h		; RST	0x28
	ret

	org	0030h		; RST	0x30
	ret

	org	0038h		; RST	0x38
	ret

reset:
	lxi	sp,stack
	call	UartInit
	call	PrInitMsg
	call	zeroMemory
	call	TraceInit
main:
	;get a commandline from the user via the serial port and put into commandText
	call	crlf

	mvi	a,'-'
	call	UartWrite

	lxi	h,CommandText
	call	getLine
	call	parseParameters
	call	ProcessCommand
	jmp	main
initmsg:	db	'DDT (Dales debug Tool) for Membership Card (Z80) v1.0', 0

	include main.asm
	include uart.asm
	include strings.asm
	include dump.asm
	include move.asm
	include compare.asm
	include search.asm
	include fill.asm
	include unassemble.asm
	include OpCode.asm
	include registers.asm
	include load.asm
	include Trace.asm
	include io.asm
	include enter.asm
	include go.asm
	include cpm.asm
	include help.asm

	org		ramStart
	include ram.asm
	end
