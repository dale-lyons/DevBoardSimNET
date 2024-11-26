; dump memory
; d [address1] [number of bytes
; address1 - param2
; numbytes - param3
; if address not specified, use the last dump address
; if numbytes not specified, use length of 80h(128)
Dump:
	lxi		h,param2
	mov		a,m
	ora		a
	jnz		dmp01

; no start provided, use last dump addr and default length
	lhld	lastDump
	lxi		b,080h
	call	dMemory
	dad		b
	shld	lastDump		; update last dump
	ret
; start adress provides, parse address
dmp01:
; address field entered
	call	char2bin
	jc		memoryError
	push	b

	lxi		b,80h
	lxi		h,param3
	mov		a,m
	ora		a
	jz		dmp03

; address2 field entered
	call	char2bin
	jc	memoryError

; HL - start address
; BC - number bytes
dmp03:
	pop		h
	call	dMemory
	dad		b
	shld	lastDump
	ret

; dump memory
; HL - address to start
; BC - number bytes
dMemory:
	push	b
	push	h

	call	crlf
dm01:
	call	dhexOut			; output address
	call	outSpace		; a space

	mvi		d,0				; d=0 indicates hex values
	call	dmpLine			; output a line of 16 hex chars
	inr		d				; d=1 indicates ascii chars
	call	dmpLine
	call	crlf

; increment HL by a
	push	b
	mov		c,a
	mvi		b,0
	dad		b
	pop		b

; decrement bc by value in a
	mov		e,a
	mov		a,c
	sub		e
	mov		c,a
	mov		a,b
	sbi		0
	mov		b,a
	jc		dm05
	ora		c
	jnz		dm01
dm05:
	pop	h
	pop	b
	ret

; Dump a line of memory to console
; Input 
; HL - address of memory to dump
; BC - remaining number of bytes to dump(only up to 16 for this line)
; d - 0: print normal hex digits
;   - +1: print ascii characters
; Returns
; a - number of bytes dumped this line
; 0120 __ __ __ xx xx xx xx xx xx xx xx xx xx xx xx xx  ................
dmpLine:
	push	b
	push	d
	push	h

; check if start address is an even 16
	mov	a,l
	ani	0fh
	mov	e,a			; save remainder
	mov	b,a			; save remainder
	jz	dmpl05		; yes, even 16, no need to dump partial line

; output blanks to fill c hex positions
;	push	b
dmpl10:
	call	dmpSpace
	dcr	e
	jnz	dmpl10

dmpl05:
	mvi	a,16
	sub	b
	mov	c,a
	push	b
dmpl15:
	mov	a,d
	ora	a
	jz	dmpl20
; convert to ascii and print
	mov	a,m
	inx	h
	call	bin2ascii
	Rst		1				; uart write
	jmp	dmpl25
dmpl20:
	mov	a,m
	inx	h
	call	hexOut
	call	outSpace
dmpl25:
	dcr	c
	jnz	dmpl15

	pop	b
	mov	a,c
	pop	h
	pop	d
	pop	b
	ret


; input d : 0  normal hex digit (3 spaces)
;       d : +1 asci space(1 space)
dmpSpace:
	push	psw
	call	outSpace
	mov		a,d
	ora	a
	jnz	dmps05
	call	outSpace
	call	outSpace
dmps05:
	pop	psw
	ret

; convert a binary numer to ascii char
; return A '.' if not ascii
bin2ascii:
	cpi	' '
	jc	b2a05
	cpi	7fh
	rc
b2a05:
	mvi	a,'.'
	ret

memoryError:
	lxi		h, dumpErrMsg
	call	cmdError
	ret
dumpErrMsg:	db	'Dump', 0