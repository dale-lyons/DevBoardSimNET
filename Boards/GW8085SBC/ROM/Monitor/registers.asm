; Display register values/set values including flags
; r [reg][flag] [value]
; where reg: bc,de,hl,sp,pc
;         or a,b,c,d,e,h,l
;       value: 16 bit number for double registers
;               8 bit number for single registers
;               0 or 1 for flags
Registers:
	lxi	h,param2
	mov	a,m
	ora	a
	jz	DumpRegisters
;; how many characters in param2?
	mvi	c,-1
rg01:
	inr	c
	mov	a,m
	inx	h
	call	IsWS
	jnz	rg01

	mov	a,c
	cpi	1
	jz	SingleRegisterOrFlag
	cpi	2
	jz	DoubleRegister
registersError:
	lxi	h, regErrMsg
	call	printf
	call	crlf
	ret
regErrMsg:	db	'Error in Registers command', 0

DoubleRegister:
	lxi	h,param2
	call	IsDoubleRegister
	rc

	lxi	h,param3
	call	char2bin
	rc
	mov	a,c
	stax	d
	inx	d
	mov	a,b
	stax	d
	xra	a
	ret

; check if the double register specified at (hl) is a double register
; Input: (hl):
;		double register in ascii in text buffer to check
; Output: c
;         1 nope
;         0 yes
IsDoubleRegister:
	lxi	h,param2
	mov	a,m
	call	upperCase
	mov	b,a
	inx	h
	mov	a,m
	call	upperCase
	mov	c,a

	mvi	l,0
	lxi	d,dreg
idr01:
	ldax	d
	ora	a
	rz

	push	d
	ldax	d
	cmp	b
	jnz	isdr10
	inx	d
	ldax	d
	cmp	c
	jz	idr02
isdr10:
	inr	l
	pop	d
	inx	d
	inx	d
	jmp	idr01
idr02:
	pop	d
	lxi	d,RegisterFile
	mvi	h,0
	dad	h
	dad	d
	xchg
	xra	a
	ret
dreg:	db	'BCDEHLSPPC', 0
numdreg	equ	5

; handle a single register or flag operation
SingleRegisterOrFlag:
	lda	param2
	ora	a
	rz
	call	IsSingleReg
	jnc	SingleRegister
	jmp	Flag

; input a - reg to test
; output:
;        carry  1 nope
;               0 yes
;        hl ptr into register 
IsSingleReg:
	call	upperCase
	lxi	h,sreg
	mov	c,a
	mvi	b,0
isr01:
	mov	a,m
	ora	a
	stc
	rz

	cmp	c
	jz	isr02
	inr	b
	inx	h
	jmp	isr01
isr02:
	mov	e,b
	mvi	d,0
	lxi	h,sRegisterLookup
	dad	d
	mov	e,m
	mvi	d,0
	lxi	h,RegisterFile
	dad	d
	xra	a
issr99:
	ret
sreg:	db	'BCDEHLA', 0
sRegisterLookup:
	db	1		; B
	db	0		; C
	db	3		; D
	db	2		; E
	db	5		; H
	db	4		; L
	db	11		; A

; hl - points to register file
SingleRegister:
	push	h
	lxi	h,param3
	call	char2bin
	pop	h
	mov	m,c
	ret

; flag sepecified on command line
; r [flag] [value]
; where flag: I, S, Z, P ,C
;       value is 0 or 1
Flag:
	lda	param2
	call	IsFlag
	rc

;	ldax	d		; get bit of flag
;	mov	b,a
	lda	param3
	cpi	'0'
	jz	flg05
	cpi	'1'
	jz	flg10
	ret
flg05:
	mov	a,d
	cma
	mov	d,a
	lda	savFgs
	ana	d
	sta	savFgs
	ret
flg10:
	lda	savFgs
	ora	d
	sta	savFgs
	ret


; check if this is a valid flag
; input   a:     flag to check
; output  carry: 1 nope
;                0 all good
;         d:     bit pattern of flag
IsFlag:
	call	upperCase
	lxi	h,flags
	mov	b,a
isf01:
	mov	a,m
	ora	a
	stc
	rz

	cmp	b
	jz	isf02

	inx	h
	inx	h
	jmp	isf01
isf02:
	inx	h
	mov	d,m
	xra	a
	ret
flags:	db	'S', 80h		; Sign
	db	'Z', 40h		; Zero
	db	'K', 20h		; K flag
	db	'A', 10h		; aux carry
	db	'P', 04h		; parity
	db	'V', 02h		; overflow
	db	'Y', 01h		; carry
	db	0

DumpRegisters:
	call	crlf
	call	dumpDoubleRegisters
	call	dumpPsw
	call	crlf
	call	dumpSingleRegisters
	call	dumpFlags
	lhld	savPC
	push	h
	call	unAssembleLine
	pop	h
	shld	lastUnassemble
	ret

dumpFlags:
	lxi	h,flags
dfl01:
	mov	a,m
	ora	a
	jz	dfl02
	Rst		1				; uart write
	inx	h
	mov	b,m
	inx	h
	mvi	c,'1'
	lda	savFgs
	ana	b
	jnz	nfzero
	mvi	c,'0'
nfzero:
	mov	a,c
	Rst		1				; uart write
	call	outSpace
	jmp	dfl01
dfl02:
	call	crlf
	ret

dumpDoubleRegisters:
	push	h
	lxi	h,dreg
	mvi	c,numdreg
	lxi	d,RegisterFile
drg10:
	mov	a,m
	Rst		1				; uart write
	inx	h
	mov	a,m
	Rst		1				; uart write
	inx	h
	mvi	a,'='
	Rst		1				; uart write
	ldax	d
	push	h
	mov	l,a
	inx	d
	ldax	d
	mov	h,a
	inx	d
	call	dhexOut
	call	outSpace
	pop	h
	dcr	c
	jnz	drg10
	pop	h
	ret

dumpPsw:
	push	h
	lxi	h,pswText
	call	printf
	mvi	a,'='
	Rst		1				; uart write
	lhld	savFgs
	call	dhexOut
	pop	h
	ret
pswText:	db	'PSW' ,0

;RegisterFile
dumpSingleRegisters:
	push	b
	push	d
	push	h
	lxi	d,sreg
	mvi	c,0
dsr01:
	ldax	d
	ora	a
	jz	dsr02

	Rst		1				; uart write
	mvi	a,'='
	Rst		1				; uart write
	mvi	b,0
	lxi	h,sRegisterLookup
	dad	b

	push	b
	mov	c,m
	mvi	b,0
	lxi	h,RegisterFile
	dad	b
	pop	b

	mov	a,m
	call	hexOut
	call	outSpace
	inr	c
	inx	d
	jmp	dsr01
dsr02:
	pop	h
	pop	d
	pop	b
	ret

