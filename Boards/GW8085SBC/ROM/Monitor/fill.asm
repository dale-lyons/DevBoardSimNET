; fill memory with a single byte
; f ssss llll value
; where: ssss source address
;      : llll length of fill
;      : value is a gingle byte fill value
Fill:
	lxi	h,param2
	ora	a
	jz	fillMemoryError

; source address in param2
	call	char2bin
	jc	fillMemoryError
	push	b

	lxi	h,param3
	ora	a
	jz	fillMemoryError
	call	char2bin
	jc	fillMemoryError
	push	b

	lxi	h,param4
	ora	a
	jz	fillMemoryError
	call	char2bin
	jc	fillMemoryError

	pop	d		; length in de
	pop	h		; start address into hl
				; c has value
fil01:
	mov	m, c
	inx	h
	dcx	d
	mov	a,d
	ora	e
	jnz	fil01
	ret
fillMemoryError:
	lxi	h, fillErrMsg
	call	cmdError
	ret
fillErrMsg:	db	'Fill', 0
