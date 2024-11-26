inOpcode	equ	0dbh
outOpcode	equ	0d3h

; i port
; input from port and put into a
Input:
	lxi	h,param2
	mov	a,m
	ora	a
	jz	ioError

	call	char2bin
	jc	ioError

	lxi	h,dynInOutCmd
	mvi	m,inOpcode
	inx	h
	mov	m,c
	inx	h
	mvi	m,retOpcode

	call	dynInOutCmd
	call	hexOut
	ret

; o port [data]
; output the data value(6 bit) to port
; if data not specified, use register a
Output:
	lxi	h,param2
	mov	a,m
	ora	a
	jz	ioError

	call	char2bin
	jc	ioError
	lxi	h,dynInOutCmd
	mvi	m,outOpcode
	inx	h
	mov	m,c
	inx	h
	mvi	m,retOpcode

	lxi	h,param3
	mov	a,m
	ora	a
	jz	io01

	call	char2bin
	jc	ioError
	mov	a,c
io01:
	jmp	dynInOutCmd

ioError:
	lxi	h, ioErrMsg
	call	cmdError
	ret
ioErrMsg:	db	'IO', 0
