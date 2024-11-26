; unassemble instructions
; u [address] [length]
Unassemble:
	lxi	h,param2
	mov	a,m
	ora	a
	jnz	una01

	lhld	lastUnassemble
	mvi	c,32
	call	uCode
	mvi	b,0
	dad	b
	shld	lastUnassemble
	ret
una01:
;something entered after u command
;u 1200 [length] (if no length use 32
	call	char2bin
	jc	unassembleError

	push	b
	lxi	h,param3
	mov	a,m
	ora	a
	mvi	c,32
	jz	una02

	call	char2bin
	jc	unassembleError
una02:
	pop	h
	call	uCode
	mvi	b,0
	dad	b
	shld	lastUnassemble
una99:
	ret

unassembleError:
	lxi	h,unassembleErrMsg
	call	printf
	call	crlf
	ret
unassembleErrMsg:	db	'Error in Unassemble command',0

;hl: address to start unassemble
;c: length in bytes
uCode:
	push	b
	push	d
	push	h
uc01:
	call	unAssembleLine
	mov	e,a
	mvi	d,0
	dad	d
	mov	e,a
	mov	a,c
	sub	e
	mov	c,a
	jnc	uc01
ucde99:
	pop	h
	pop	d
	pop	b
	ret

;convert opcode to assembler string and print to console
;Input:	hl: address to unassemble
;Output: a: number of bytes used
unAssembleLine:
	push	b
	push	h
	shld	opCodePtr
	mov	e,m
	mvi	d,0
	lxi	h,unasmTable
	dad	d
	mov	c,m
	mvi	b,0
	mvi	e,3
	call	shlBCe
	lxi	h,opCodeTypes
	dad	b

; hl points to opcode in table opCodeTypes
	push	h
	lxi	d,7
	dad	d
	mov	a,m
	sta	numBytes
	mov	c,a
	mvi	a,3
	sub	c
	mov	d,a

	lhld	opCodePtr
	call	dhexOut
	call	outSpace
ual01:
	mov	a,m
	call	hexOut
	inx	h
	dcr	c
	jnz	ual01
ual02:
	mov	a,d
	ora	a
	jz	ual03
	call	outSpace
	call	outSpace
	dcr	d
	jmp	ual02
ual03:
	call	outSpace
	pop	h
	mvi	c,5
ual04:
	mov	a,m
	Rst		1				; uart write
	inx	h
	dcr	c
	jnz	ual04

	shld	opCodeTablePtr
	push	h
	inx	h
	inx	h
	mov	a,m
	sta	numBytes
	pop	h
	mov	a,m
	cpi	ARGNONE
	jz	unasl99
	lxi	h,unasl99
	push	h
	lhld	opCodePtr
	cpi	ARGDATA8
	cz	argData8s
	cpi	ARGREG8S
	cz	argReg8Ss
	cpi	ARGREG8D
	cz	argReg8Ds
	cpi	ARGREG16
	cz	argReg16s
	cpi	ARGDATA16
	cz	argData16s
	cpi	ARGRST
	cz	argRSTINSTR
	pop	h

	lhld	opCodeTablePtr
	inx	h
	mov	a,m
	cpi	ARGNONE
	jz	unasl99

	lxi	h,unasl99
	push	h
	lhld	opCodePtr
	mov	e,a
	mvi	a,','
	Rst		1				; uart write
	mov	a,e
	cpi	ARGDATA16
	cz	argData16s
	cpi	ARGDATA8
	cz	argData8s
	cpi	ARGREG8S
	cz	argReg8Ss
	cpi	ARGREG8D
	cz	argReg8Ds
	pop	h
unasl99:
	call	crlf
	lda	numBytes
	pop	h
	pop	b
	ret




argRSTINSTR:
	push	psw
	mov	a,m
	rar
	rar
	rar
	ani	07h
	call	nibbleToAscii
	Rst		1				; uart write
	pop	psw
	ret

argReg8Ds:
	push	psw
	mov	a,m
	rar
	rar
	rar
	ani	07h
	mov	e,a
	mvi	d,0
	lxi	h,sreg2
	dad	d
	mov	a,m
	Rst		1				; uart write
	pop	psw
	ret

argReg8Ss:
	push	psw
	mov	a,m
	ani	07h
	mov	e,a
	mvi	d,0
	lxi	h,sreg2
	dad	d
	mov	a,m
	Rst		1				; uart write
	pop	psw
	ret
sreg2:	db	'bcdehlma'

; 00RP0011
; where RP 00-b
;          01 d
;          10 h
;          11 sp
argReg16s:
	push	psw
	mov	a,m
	rar
	rar
	rar
	rar
	mov	b,a
	ani	03h
	cpi	03h
	jnz	ar1605

	mov	a,b
	rar
	rar
	ani	03h
	lxi	h,spStr
	jz	ar1603
	lxi	h,pswStr
ar1603:
	call	printf
	pop	psw
	ret
ar1605:
	mov	e,a
	mvi	d,0
	lxi	h,mydreg
;;	dad	d
	dad	d
	mov	a,m
	Rst		1				; uart write
	pop	psw
	ret
mydreg:	db	'bdhsp'

argData16s:
	push	psw
	inx	h
	mov	e,m
	inx	h
	mov	d,m
	xchg
	call	dhexOut
	pop	psw
	ret

argData8s:
	push	psw
	inx	h
	mov	a,m
	call	hexOut
	pop	psw
	ret
spStr:	db	's','p',0
pswStr:	db	'p','s','w',0
