SBYTE		equ		2
SWORD		equ		1

; Test#0
; rlc r
RpTest_Z80:
	lxi		d,TESTAREA
	lxi		h,commandText+3				; prebyte size
	mov		a,m
	mov		b,a
	inx		h
	ora		a
	jz		znopre05
	mov		a,m							; prebyte1
	stax	d
	inx		d
	mov		a,b
	cpi		2
	jnz		znopre05
	lda		commandText+27
	stax	d
	inx		d
znopre05:
	inx		h
	mov		a,m							; opcode
	inx		h
	stax	d
	inx		d
; check if this test requires immediate data
	mov		a,m							;useIMMData
	inx		h
	ora		a
	jz		znoImmData
	cpi		SWORD
	jz		zimm16

; 8 bits immediate data
	mov		a,m						; load immediate byte
	stax	d
	inx		d
	jmp		znoImmData

; 16 bits immediate data
zimm16:
	mov		a,m						; load immediate byte low
	inx		h
	stax	d
	inx		d
	mov		a,m						; load immediate byte high
	stax	d
	inx		d
znoImmData:
	mvi		a,JMP_INSTR
	stax	d
	inx		d
	lxi		h,Int3VectorZ80
	mov		a,l
	stax	d
	inx		d
	mov		a,h
	stax	d

; load target memory address and check for 0
	lhld	commandText+19
	mov		a,h
	ora		l
	jz		znomem05

	xchg
	lhld	commandText+21	; memory values to preset
	mov		a,l
	stax	d
	inx		d
	mov		a,h
	stax	d

; no target memory operation
znomem05:
	lhld	commandText+23
	push	h
	db		0ddh, 0e1h		; pop ix
	lhld	commandText+25
	push	h
	db		0fdh, 0e1h		; pop iy

	lxi		sp,commandText+9
	pop		psw
	pop		b
	pop		d
	lhld	commandText+17
	sphl
	lhld	commandText+15
	jmp		TESTAREA

Int3VectorZ80:
	db		08h					; ex af,af' (save primary flags)
	shld	responseText+10
	xchg
	shld	responseText+8	    ; DE
	lxi		h,0
	dad		sp
	shld	responseText+12
	db		08h					; ex af,af' (restore primary flags)
	lxi		sp,stack

	push	psw
	pop		h
	shld	responseText+4

	mov		l,c
	mov		h,b
	shld	responseText+6
	lhld	commandText+19			; Addr
	mov		a,h
	ora		l
	jz		znoadd05

	mov		a,m
	sta		responseText+16
	inx		h
	mov		a,m
	sta		responseText+17

znoadd05:
	db		0ddh, 0e5h			; push ix
	pop		h
	shld	responseText+14

	db		0fdh, 0e5h			; push iy
	pop		h
	shld	responseText+18
	jmp		Respond
