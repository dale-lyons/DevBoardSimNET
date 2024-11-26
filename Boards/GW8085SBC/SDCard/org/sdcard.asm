CR		equ	0dh
LF		equ	0ah
SPACE		equ	020h
BACKSPACE	equ	08h
TAB		equ	09h


	org	08000h

	lxi	sp, 09000h
	call	UartInit
	call	Reset
	call	HW_TEST
	jc	fail
pass:
	call	SetMode
	jc	fail

	call	ConnectDisk
	jc	fail

	call	MountDisk
	jc	fail

	lxi	d, file
	call	OpenFile
	jc	fail

	lxi	d,fileBuff
	mvi	c,80h
	call	readFile
	jc	fail

	lxi	h,fileBuff
	mvi	b,80h
@@:
	mov	a,m
;	call	hexOut
	inx	h
	dcr	b
	jnz	@B
	jmp	done
fail:
	lxi	h,no
	call	printf
done:
	lxi	h,alldone
	call	printf
@@:	
	jmp	@B
alldone:	db	13, 10, 'AllDone!', 13, 10, 0
no:	db	13, 10, 'test failed!', 13, 10, 0
file:	db	'/s.aaa', 0

fileBuff:
	ds	128
	ds	32

	include	sdutil.asm
	include	uart.asm
	include	strings.asm

	end

