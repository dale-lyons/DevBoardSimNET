; e address
Enter:
	lxi	h,param2
	mov	a,m
	ora	a
	jz	enterError

	call	char2bin
	jc	enterError

	mov	l,c
	mov	h,b
	call	dhexOut
ent01:
	call	outSpace
	mov	a,m
	call	hexOut
	mvi	a,'.'
	Rst		1				; uart write

; bc has address to enter data into
	push	h
	call	getByte2
	pop	h
	mov	m,c
	inx	h
	mov	a,e
	cpi	' '
	jz	ent01
	ret

getByte2:
	lxi		h,commandText
	push	h
gb01:
	Rst		2				; uart read
	call	upperCase
	call	IsHexDigit
	jc	gb01
	mov	m,a
	inx	h
	Rst		1				; uart write
gb02:
	Rst		2				; uart read
	call	upperCase
	call	IsHexDigit
	jc	gb02
	mov	m,a
	inx	h
	Rst		1				; uart write
	Rst		2				; uart read
	mov	m,a
	mov	e,a
	pop	h
	call	char2bin
	ret
enterError:
	lxi	h, enterErrMsg
	call	cmdError
	ret
enterErrMsg:	db	'Enter', 0
