CR		equ	0dh
LF		equ	0ah
SPACE		equ	020h
BACKSPACE	equ	08h
TAB		equ	09h


	org	08000h

	lxi	sp, 09000h
	call	UartInit

	call	init

	mvi	a,MODE_HOST_1
	call	setMode
	call	checkDrive


	include	sdutil.asm
	include	uart.asm
	include	strings.asm

	end

