; search a block of memory for a single value.
; s ssss llll value
; where: ssss source address
;      : llll length of compare
;      : value value to search for
Search:
	lxi	h,param2
	ora	a
	jz	searchMemoryError

; source address in param2
	call	char2bin
	jc	searchMemoryError
	push	b

	lxi	h,param3
	ora	a
	jz	searchMemoryError
	call	char2bin
	jc	searchMemoryError
	push	b

	lxi	h,param4
	ora	a
	jz	searchMemoryError
	call	char2bin
	jc	searchMemoryError

	pop	h		; hl has address
	pop	d		; de has length
				; c value to seach for.
src01:
	mov	a,m
	cmp	c
	jnz	src02
	call	prSrchDiff
src02:
	inx	h
	dcx	d
	mov	a,d
	ora	e
	jz	src01
	ret

; print a search find
; hl - address of byte
prSrchDiff:
	call	dhexOut
	call	crlf
	ret

searchMemoryError:
	lxi	h,searchErrMsg
	call	cmdError
	ret
searchErrMsg:	db	'Search', 0

