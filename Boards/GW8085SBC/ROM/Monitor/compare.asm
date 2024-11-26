; compare 2 blocks of memory and dump result
; c ssss dddd llll
; where: ssss source address
;      : dddd destination address
;      : llll length of compare
Compare:
	lxi	h,param2
	ora	a
	jz	compareErrMsg

; source address in param2
	call	char2bin
	jc	compareErrMsg
	push	b

	lxi	h,param3
	ora	a
	jz	compareErrMsg
	call	char2bin
	jc	compareErrMsg
	push	b

	lxi	h,param4
	ora	a
	jz	compareErrMsg
	call	char2bin
	jc	compareErrMsg

	pop	d		; dst address into de
	pop	h		; src address into hl
				; bc has length
cmp10:
	ldax	d
	cmp	m
	jz	cmp01
	call	prDiff
cmp01:
	inx	h
	inx	b
	dcx	b
	mov	a,b
	ora	c
	jz	cmp01
	ret

; print difference between 2 bytes and their address
; hl - address of byte1
; bc address byte 2
prDiff:
	push	b
	push	d
	push	h

	call	dhexOut
	call	outSpace
	mov	a,m
	call	hexOut
	call	outSpace
	ldax	b
	call	hexOut
	call	outSpace

	mov	l,c
	mov	h,b
	call	dhexOut
	call	crlf

	pop	h
	pop	d
	pop	b
	ret

compareMemoryError:
	lxi	h,compareErrMsg
	call	cmdError
	ret
compareErrMsg:	db	'Compare', 0

