;;;MAX_BPT		equ	8
; g [address] [address] [address] ...
Go:
	call	clearBP
	lxi	h,param2
	mov	a,m
	ora	a
	jz	Restore
	call	char2bin
	jc	goError

	push	b
	pop	h
	shld	savPC

	lxi	h,param3
go01:
	mov	a,m
	ora	a
	jz	Restore
	call	char2bin
	jc	goError

	mov	l,c
	mov	h,b
	call	addBP

	lxi	d,PARM_SIZE
	dad	d
	jmp	go01
goError:
	lxi	h,goErrMsg
	call	cmdError
	ret
goErrMsg:	db	'Go', 0

