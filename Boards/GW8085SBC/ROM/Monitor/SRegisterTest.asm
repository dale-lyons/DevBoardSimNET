TESTAREA	equ	0a000h
RETINSTR	equ	0c9h
ESCAPE		equ	27
MVI_INSTR	equ	06h
MOVA_INSTR	equ	78h

VERBOSE		equ	1

RegisterTest:
	IF VERBOSE
	lxi	h,msg1
	call	printf
	ENDIF
	lxi	h, commandText
	call	UartRead
	mov	c,a
	mov	m,a
	inx	h

	IF VERBOSE
	push	h
	lxi		h,msg2
	call	printf
	call	hexOut
	call	crlf
	pop	h
	ENDIF

regt05:
	call	UartRead
	mov	m,a
	inx	h

	IF VERBOSE
	push	h
	lxi	h,msg3
	call	printf
	call	hexOut
	call	crlf
	pop	h
	ENDIF

	dcr	c
	jnz	regt05

	lda	commandText+1

	IF VERBOSE
	lxi	h,msg4
	call	printf
	call	hexOut
	call	crlf
	ENDIF

	add	a
	lxi	d,TestTable
	mov	l,a
	mvi	h,0
	dad	d
	xchg
	ldax	d
	mov	l,a
	inx	d
	ldax	d
	mov	h,a
	lxi	b,RegisterTest
	push	b
	pchl

TestTable:	dw	Test0
		dw	Test1
		dw	Test2
		dw	Test3

; Single 8 bit register (Destination)
; dcr e, inr h etc.
; mvi reg,val
; dcr reg
; mov a,reg
; ret
Test3:
	lxi	h,TESTAREA
	lda	commandText+2		; get opcode of instruction to test
	push	psw
	ani	38h
	mvi	b,MVI_INSTR
	ora	b
	mov	m,a
	inx	h
	lda	commandText+3		; get initial value of test register
	mov	m,a
	inx	h

	pop	psw
	ani	038h
	rrc
	rrc
	rrc
	ani	07h			; isolate test register
	mov	b,a

	lda	commandText+2		; get opcode of instruction to test
	mov	m,a
	inx	h
	mvi	a,MOVA_INSTR		; insert mov a,reg instruction
	ora	b
	mov	m,a
	inx	h
	mvi	m,RETINSTR		; add return instruction to the end
	lda	commandText+4
	mov	c,a
	mvi	b,0
	push	b
	pop	psw
	lxi	h,testReturn
	push	h
	lxi	h,TESTAREA
	pchl
Test0:
	lda	commandText+2
	lxi	d,TESTAREA
	stax	d
	inx	d
	mvi	a,RETINSTR
	stax	d

	lda	commandText+3
	mov	b,a
	lda	commandText+4
	mov	c,a
	push	b
	pop	psw
	lxi	h,testReturn
	push	h
	lxi	h,TESTAREA
	pchl
Test1:
	lda	commandText+2
	lxi	d,TESTAREA
	stax	d
	inx	d
	lda	commandText+5
	stax	d
	inx	d
	mvi	a,RETINSTR
	stax	d

	lda	commandText+3
	mov	b,a
	lda	commandText+4
	mov	c,a
	push	b
	pop	psw
	lxi	h,testReturn
	push	h
	lxi	h,TESTAREA
	pchl
Test2:
	mvi	b,MVI_INSTR
	lda	commandText+5
	rlc
	rlc
	rlc
	ora	b
	lxi	d,TESTAREA
	stax	d
	inx	d
	lda	commandText+6			; mvi param
	stax	d
	inx	d
	lda	commandText+2			; opcode
	mov	b,a
	lda	commandText+5			; get register
	ani	07h				; isolate reg bites
	ora	b				; or into opcode
	stax	d
	inx	d
	mvi	a,RETINSTR
	stax	d
	lda	commandText+3
	mov	b,a
	lda	commandText+4
	mov	c,a
	push	b
	pop	psw
	lxi	h,testReturn
	push	h
	lxi	h,TESTAREA
	pchl
testReturn:
	push	psw

	IF VERBOSE
	push	h
	lxi	h,msg5
	call	printf
	pop	h
	ENDIF

	mvi	a,ESCAPE
	call	UartWrite
	call	UartWrite
	mvi	a,3
	call	UartWrite
	mvi	a,'z'
	call	UartWrite

	pop	b
	mov	a,b
	call	UartWrite
	mov	a,c
	call	UartWrite
	mvi	a,0
	call	UartWrite
	ret
msg1:	db	'Waiting for length:',CR,LF, 0
msg2:	db	'got length:',0
msg3:	db	'got a byte:',0
msg4:	db	'Performing Test',0
msg5:	db	'Done Test, sending result',CR,LF,0
