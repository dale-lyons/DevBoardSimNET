; load a hex file from the console into memory
Load:
	lxi	h,0ffffh
	shld	startHex
	lxi	h,ldwaitMsg
	call	printf
	call	crlf
ld01:
	call	ldGetline
	ora	a
	jnz	ld01

	lhld	startHex
	shld	savPC
	shld	lastUnassemble
	ret
ldwaitMsg:	db	'Waiting for hex file upload ....', 0

;; set the load start address if it has not been set yet.
; Input HL - address to set
setStart:
	push	h
	lhld	startHex
	inx	h
	mov	a,h
	ora	l
	pop	h
	rnz
	shld	startHex
	ret

;;;;  :10DC0000C35CDFC358DF7F00436F70797269676858
;;;;:0000000000
; get 1 line of the hex file
; Output
; a: more bytes to get 0 if no
;                      1 if yes
ldGetline:
	Rst	2
;	call	UartRead
	cpi	':'
	jnz	ldGetline

	mvi	a,'.'
	Rst		1				; uart write

	call	ldGetByte
	ora	a
	jnz	hxgt10
; end of file reached, eat the last line
	call	ldGetByte
	call	ldGetByte
	call	ldGetByte
	call	ldGetByte
	xra	a
	ret
hxgt10:
	mov	c,a				; length
	call	ldGetByte
	mov	h,a
	call	ldGetByte
	mov	l,a
	call	setStart
	call	ldGetByte			; discard
hxgt05:
	call	ldGetByte
	mov	m,a
	inx	h
	dcr	c
	jnz	hxgt05
	call	ldGetByte			; checksum
	mvi	a,1
	ret

ldGetByte:
	push	b
	push	h
	lxi		h,commandText
	push	h
   	Rst		2				; uart read
	mov		m,a
	inx		h
   	Rst		2				; uart read
	mov		m,a
	inx		h
	mvi		m,0
	pop		h
	call	char2bin
	mov		a,c
	pop		h	
	pop		b
	ret
