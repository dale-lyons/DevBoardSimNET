; returns length of null terminated string
; hl - ptr to string
; returns a: length of string
strlen:
	push	b
	push	h
	mvi	b,0
strlen05:
	mov	a,m
	cpi	0
	jz	strlen99
	inx	h
	inr	b
	jmp	strlen05
strlen99:
	mov	a,b
	pop	h
	pop	b
	ret

; remove leading and traing whitespace from string
strtrim:
	push	b
	push	h
	push	psw
strtrm05:
	mov	a,m
	ora	a
	jz	strtrm10
	call	IsWS
	jnz	strtrm10
	call	strRemove
	jmp	strtrm05
strtrm10:
	call	strlen
	ora	a
	jz	strtrm99

	push	h
	call	strend
	xchg
	pop	h

	dcx	d
	ldax	d
	call	IsWS
	jnz	strtrm99
	xra	a
	stax	d
	jmp	strtrm10
strtrm99:
	pop	psw
	pop	h
	pop	b
	ret

strRemove:
	push	psw
	push	h
	push	d
	push	h
	pop	d
	inx	d
strrm05:
	ldax	d
	mov	m,a
	inx	d
	inx	h
	ora	a
	jnz	strrm05
	pop	d
	pop	h
	pop	psw
	ret

; convert charcter to upper case
upperCase:
	cpi	'a'
	jc	upc05
	cpi	'z'+1
	jnc	upc05
	sui	020h
upc05:
	ret

; convert charcter to lower case
lowerCase:
	cpi	'A'
	jc	lwc05
	cpi	'Z'
	jnc	lwc05
	adi	020h
lwc05:
	ret

; locate end of string
; hl - str1
; output - hl - pointer to null terminating byte of str1
strend:
	push	d
	call	strlen
	mov	e,a
	mvi	d,0
	dad	d
	pop	d
	ret

; convert string to binary
; hl - pointer to string
; output bc - value
; carry - set: error
char2bin:
	push	h
	lxi	b,0
c2b01:
	mov	a,m
	ora	a
	jz	c2b02
	call	ascii2Bin
	jc	c2b02

	call	shlBC4
	ani	0fh
	ora	c
	mov	c,a
	inx	h
	jmp	c2b01
c2b02:
	pop	h
	ret

ascii2Bin:
	call	IsDigit
	jc	ab01
	sui	'0'
	ora	a
	ret
ab01:
	call	upperCase
	call	IsHexLetter
	rc
	sui	'A'
	adi	0ah
	ora	a
	ret

; shift BC register left 4 bits.
shlBC4:
	push	d
	mvi	e,4
	call	shlBCe
	pop	d
	ret

shlBCe:
	push	psw
	push	d
shl05:
	stc
	cmc
	mov	a,c
	ral
	mov	c,a
	mov	a,b
	ral
	mov	b,a
	dcr	e
	jnz	shl05

	pop	d
	pop	psw
	ret

; Input
;    hl: command with error
cmdError:
	push	h
	lxi	h, errMsg1
	call	printf
	pop	h
	call	printf
	lxi	h, errMsg2
	call	printf
	call	crlf
	ret
errMsg1:	db	'Error in ', 0
errMsg2:	db	' command', 0

outPrompt:
	mvi	a,'-'
	Rst		1				; uart write
	ret
	
; print string to console
; Input
;   HL - string to print (null terminated)
; Output
;   HL byte past end of string
printf:
	mov	a,m
	inx	h
	ora	a
	jnz	prf01
	ret
prf01:
	Rst		1				; uart write
	jmp	printf

outColon:
	push	psw
	mvi	a,':'
	Rst		1				; uart write
	pop	psw
	ret

outSpace:
	push	psw
	mvi	a,' '
	Rst		1				; uart write
	pop	psw
	ret

IsHexDigit:
	call	IsDigit
	rnc
	call	IsHexLetter
	ret
IsDigit:
	cpi	'0'
	rc
	cpi	'9'+1
	cmc
	ret
IsHexLetter:
	cpi	'A'
	rc
	cpi	'Z'
	cmc
	ret
IsWS:
	ora	a
	rz
	cpi	SPACE
	rz
	cpi	TAB
	rz
	cpi	CR
	rz
	cpi	LF
	ret

crlf:
	push	psw
	mvi	a,CR
	Rst		1				; uart write
	mvi	a,LF
	Rst		1				; uart write
	pop	psw
	ret

; hl has number
dhexOut:
	mov	a,h
	call	hexOut
	mov	a,l
	call	hexOut
	ret
hexOut:
	push	psw
	push	b
	mov	c,a
	rar
	rar
	rar
	rar
	ani	0fh
	call	nibbleToAscii
	Rst		1				; uart write
	mov	a,c
	ani	0fh
	call	nibbleToAscii
	Rst		1				; uart write
	pop	b
	pop	psw
	ret

nibbleToAscii:
	cpi		0ah
	jc		nba01
	adi		07
nba01:
	adi		30h
	ret