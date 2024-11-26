CR		equ	0dh
LF		equ	0ah
SPACE		equ	020h
BACKSPACE	equ	08h
TAB		equ	09h
;retOpcode	equ	0c9h
;ramStart	equ	08000h

	org	08000h
	lxi	sp,stack
	call	UartInit

	lxi	h,mns01
	call	printf
	lhld	bittim
	call	dhexOut
	call	crlf

	lxi	h,mns02
	call	printf
	lhld	halfbt
	call	dhexOut
	call	crlf

	mvi	a,'-'
	call	UartWrite
@@:
	call	UartRead
	call	UartWrite
	jmp	@B
mns01:	db	'bittim:', 0
mns02:	db	'halfbt:', 0

;	include main.asm
	include uart.asm
	include strings.asm
;	include dump.asm
;	include move.asm
;	include compare.asm
;	include fill.asm
;	include unassemble.asm
;	include OpCode.asm
;	include registers.asm
	include load.asm
;	include Trace.asm
;	include io.asm
;	include enter.asm
;	include go.asm
;	include cpm.asm
;	include SRegisterTest.asm

;	org		ramStart
	include ram.asm
	end
