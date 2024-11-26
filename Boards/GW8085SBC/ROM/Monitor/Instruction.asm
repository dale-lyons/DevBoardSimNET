ESCAPE			equ		27
JMP_INSTR		equ	0c3h
MVI_INSTR		equ	06h
MVIA_INSTR		equ	03eh
MOV_INSTR		equ	78h
MOVA_INSTR		equ	78h
LXI_INSTR		equ	01h

Instruction:
	lxi		h,msg1
	call	printf
	call	zeroM
load:
	Rst		2				; UartRead
	cpi		':'
	jnz		load
	lxi		h, commandText
    mov     m,a
    inx     h
	Rst		2			; UartRead, read length
    mov     m,a
    inx     h

	mov		c,a		; length in a
	mov		d,a		; checksum start
regt05:
	Rst		2			; UartRead, read byte of payload
	mov		m,a
	add		d
	mov		d,a
	inx		h
	dcr		c
	jnz		regt05

	Rst		2			; UartRead, read checksum from client
	cmp		d		    ; compare with our cs
	jz		mn05		; carry on if match

;;;;;;;;;;; BAD CHECKSUM!!!
	lxi		h,badcs
	call	printf
	ret
msg1:	db	'Waiting for start character:',CR,LF, 0

mn05:
	lda		commandText+2           ; test number
	add		a
	lxi		d,TestTable
	mov		l,a
	mvi		h,0
	dad		d
	xchg
	ldax	d
	mov		l,a
	inx		d
	ldax	d
	mov		h,a
	pchl

zeroM:
	lxi		h,commandText
	mvi		b,INSTRUCTION_TEST_SIZE
zrom05:
	mvi		m,077h
	inx		h
	dcr		b
	jnz		zrom05
	ret

TestTable:
;			dw	RpTest_8085
;			dw	RpTest_Z80

Respond:
	lxi		h,responseText
; create response message and send to pc
	mvi		m,ESCAPE
	inx		h
	mvi		m,ESCAPE
	inx		h
	mvi		m,18				; size
	inx		h
	mvi		m,'z'

	lxi		h,responseText
	mvi		d,0					; checksum
	mvi		c,20
resp05:
	mov		a,m
	inx		h
	Rst		1					; UartWrite
	add		d
	mov		d,a
	dcr		c
	jnz		resp05

	mov		a,d
	Rst		1					; UartWrite
	xra		a
	Rst		1					; UartWrite
	jmp		load
badcs:	db	CR,LF, 'Bad CS', CR, LF, 0
