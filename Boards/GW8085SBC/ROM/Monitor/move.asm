; move/copy a block of memory from a start address to destination address of length
; move memory address address length
; m ssss dddd llll
; where ssss: start address
;       dddd: destination address
;       llll: length
Move:
	lxi	h,param2
	ora	a
	jz	moveMemoryError

; source address in param2
	call	char2bin
	jc	moveMemoryError
	push	b

	lxi	h,param3
	ora	a
	jz	moveMemoryError
	call	char2bin
	jc	moveMemoryError
	push	b

	lxi	h,param4
	ora	a
	jz	moveMemoryError
	call	char2bin
	jc	moveMemoryError

	pop	d		; dst address into de
	pop	h		; src address into hl
				; bc has length
mvm01:
	mov	a,m
	stax	d
	inx	h
	inx	d
	dcx	b
	mov	a,b
	ora	c
	jnz	mvm01
	ret
moveMemoryError:
	lxi	h, moveErrMsg
	call	cmdError
	ret
moveErrMsg:	db	'Move', 0
