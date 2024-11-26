CR			equ	0dh
LF			equ	0ah
SPACE		equ	020h
BACKSPACE	equ	08h
TAB			equ	09h
retOpcode	equ	0c9h

ddtStart:
	lxi		sp,stack
	call    VectorInit
	call	UartInit
	call	PrInitMsg
	call	zeroMemory
	call	TraceInit
main:
	;get a commandline from the user via the serial port and put into commandText
	call	crlf

	mvi		a,'-'
	Rst		1				; uart write

	lxi		h,commandText
	call	getLine
	call	parseParameters
	call	ProcessCommand
	jmp		main

; input
; hl - points to command buffer
getLine:
	push	h
	mvi	b,0
gtl01:
	Rst		2				; uart read
	cpi	CR
	jz	gtl05
	cpi	BACKSPACE
	jz	gtl10

	mov	m,a
	inx	h
	Rst		1				; uart write
	inr	b
	jmp	gtl01
gtl10:
	mov		a,b
	ora		a
	jz		gtl01

	dcr		b
	dcx		h
	mvi		a,BACKSPACE
	Rst		1				; uart write
	jmp		gtl01
gtl05:
	xra		a
	mov		m,a
	pop		h
	ret

; Parse the command parameters
; input
; hl - points to command buffer
parseParameters:
	push	h

; first zero out all paramater slots.
	lxi	h,argv
	lxi	b,(PARM_SIZE*5)
pp01:
	mvi		m,0
	inx		h
	dcx		b
	mov		a,b
	ora		c
	jnz		pp01

	lxi     h,commandText
	mvi	c,0
pp02:
	call	parseParameter
	inr		c
	mov		a,c
	cpi		6
	jc		pp02

	pop		h
	ret

; parse next parameter, put result into parameter slot.
; input
; hl - commandline to parse
; b  - parameter #
parseParameter:
	push	b

	call	scanToNonWS
	mov	a,m
	ora	a
	jz	pp10

	mov	a,c
	rlc
	rlc
	rlc
	rlc

	push	h
	mov	e,a
	mvi	d,0
	lxi	h,argv
	dad	d
	xchg
	pop	h

	mov	a,c
	ora	a
	jnz	pp05

; first parameter
	mov	a,m
	stax	d
	inx	h
	pop	b
	ret
pp05:
	mov	a,m
	call	IsWS
	jz	pp10

	stax	d
	inx	d
	inx	h
	jmp	pp05
pp10:
	pop	b
	ret

scanToNonWS:
	mov	a,m
	ora	a
	rz
	call	IsWS
	rnz
	inx	h
	jmp	scanToNonWS


; process the command in CommandText
; input
; hl - points to command buffer
ProcessCommand:
	lda	param1
	ora	a
	rz

	call	FindCmd
	mov	a,d
	ora	e
	jnz	pc01
	call	crlf
	push	h
	lxi	h,prcs05
	call	printf
	pop	h
	ret
pc01:
	call	crlf
	lxi	b,prc05
	push	b
	xchg
	push	h
	lxi	h, param2
prc05:
	ret
prcs05:	db	'Unrecognized Command', 0

; find command in A
; return command function pointer in DE
FindCmd:
	call	lowerCase
	mov	l,a
	lxi	b,cmdLetters
	mvi	e,0
fc01:
	ldax	b
	cmp	l
	jz	fc02
	inx	b
	inr	e
	mov	a,e
	cpi	numLetters
	jc	fc01
	lxi	d,0
	ret
fc02:
	lxi	h,cmdCode
	mvi	d,0
	dad	d
	dad	d
	mov	e,m
	inx	h
	mov	d,m
	ret

zeroMemory:
	lxi	h,ramStart
	lxi	b,memorySize
zm01:
	mvi	m,0
	inx	h
	dcx	b
	mov	a,b
	ora	c
	jnz	zm01
	ret

cmdLetters:	db	'd'
		db	's'
		db	'c'
		db	'f'
		db	'e'
		db	'g'
		db	'u'
		db	'i'
		db	'o'
		db	'l'
		db	'm'
		db	'r'
		db	't'
		db	'x'
		db	'y'
		db	'h'			; help
		db	'v'			; instruction verifier
numLetters	equ	$-cmdLetters

cmdCode:	dw	Dump
		dw	Search
		dw	Compare
		dw	Fill
		dw	Enter
		dw	Go
		dw	Unassemble
		dw	Input
		dw	Output
		dw	Load
		dw	Move
		dw	Registers
		dw	Trace
		dw	CPM
		dw	TraceOver
		dw	PrintHelp
		dw	Instruction
