; Input
; bc - pointer to flag struct
; a set flag:   1
;   clear flag: 0
;;;;           Mask  Clear  Set
;;	db	40h, 'NZ', 'ZR'		; Zero
setFlag:
	ora	a
	lda	savFgs
	mov	e,a
	jz	prf05
;; set flag
	ldax	b
	ora	e
	inx	b
	inx	b
	inx	b
	jmp	prf10
prf05:
;; clear flag
	ldax	b
	cma
	ana	e
	inx	b
prf10:
	sta	savFgs

	ldax	b
	call	UartWrite
	inx	b
	ldax	b
	call	UartWrite
	call	crlf
	ret

; check if string in input buffer is one of the psw flags
; Input
;      hl - ptr to text to test
; output:
;      bc - point to flag entry (null if no flag)
;      a - 0 if clear
;          1 if set
IsPSWFlag:
	push	d
	push	h
	lxi	d,pswFlags
	mvi	c,numflags
ispw05:
	push	d
	mvi	b,0
	inx	d
	push	b
	mvi	c,2
	call	strncmp
	pop	b
	jz	ispw10
	inr	b
	inx	d
	inx	d
	push	b
	mvi	c,2
	call	strncmp
	pop	b
	jz	ispw10

	pop	d
	inx	d
	inx	d
	inx	d
	inx	d
	inx	d
	dcr	c
	jnz	ispw05
	lxi	b,0
	jmp	ispw99
ispw10:
	mov	a,b
	pop	b
ispw99:
	pop	h
	pop	d
	ret


; print a double registers pair to console
; Input a - register number
;           0 - BC
;           1 - DE
;           2 - HL
;           3 - SP
;           4 - PC
prDoubleRegister:
	push	d
	push	h
	mov	e,a
	mvi	d,0
	lxi	h,dreg
	dad	d
	dad	d
	mov	a,m
	call	UartWrite
	inx	h
	mov	a,m
	call	UartWrite
prdr05:
	pop	h
	pop	d
	ret

getByte:
	push	h
	lxi	h, commandText
	call	getline
	lxi	h, commandText
	call	char2bin
	jnc	gtby05
	call	registersError
	stc
; bc has number entered
gtby05:
	pop	h
	ret

; check if the double register specified in the text buffer is a valid one.
; Input: double register in ascii in text buffer to check
; Output: c register number matched 0-4
;          carry set on no match
;         clear on match
IsDoubleReg:
	push	h
	call	strupr
	lxi	d,dreg
	mvi	c,numdreg
isdr05:
	push	b
	mvi	c,2
	call	strncmp
	pop	b
	jz	isdr10
	inx	d
	inx	d
	dcr	c
	jnz	isdr05
	stc
	jmp	isdr99
isdr10:
	mvi	a,numdreg
	sub	c
	mov	c,a
	stc
	cmc
isdr99:
	pop	h
	ret


; Input
; a single register number (0-7)
; Output
; de pre to register in register file
LookupSRegister:
	push	h
	mov	e,a
	mvi	d,0
	lxi	h,sRegisterLookup
	dad	d
	mov	e,m
	mvi	d,0
	lxi	h,RegisterFile
	dad	d
	xchg
	pop	h
	ret

dumpFlags:
	push	h
	lxi	h,intText
	lda	intMask
	call	dumpFlag
	lxi	h,pswFlags
	mvi	c,numflags
drg20:
	lda	savFgs
	call	dumpFlag
	lxi	d,5
	dad	d
	dcr	c
	jnz	drg20
	call	crlf
	pop	h
	ret

;Input
;     hl: flag entry to dump
;     a : flag value
dumpFlag:
	push	b
	push	h
	mov	b,m		; mask into b
	ana	b
	inx	h
	jz	dmpf05
	inx	h
	inx	h
dmpf05:
	mov	a,m
	call	UartWrite
	inx	h
	mov	a,m
	call	UartWrite
	call	outSpace

	pop	h
	pop	b
	ret

