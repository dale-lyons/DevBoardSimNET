; Test#0
; dad regt05
RpTest_8085:
	lxi		h,0
	dad		sp
	shld	userSP

	lda		commandText+5				; opcode
	lxi		d,TESTAREA
	stax	d
	inx		d
; check if this test requires immediate data
	lda		commandText+6				; useIMMData
	ora		a
	jz		noImmData
	cpi		1
	jnz		noim16

	lhld	commandText+7
	mov		a,l
	stax	d
	inx		d
	mov		a,h
	stax	d
	inx		d
	jmp		noImmData
noim16:
	lhld	commandText+7
	mov		a,l
	stax	d
	inx		d
noImmData:
	mvi		a,JMP_INSTR
	stax	d
	inx		d
	lxi		h,Int3Vector8085
	mov		a,l
	stax	d
	inx		d
	mov		a,h
	stax	d

; load target memory address and check for 0
	lhld	commandText+19
	mov		a,h
	ora		l
	jz		nomem05

	xchg
	lhld	commandText+21	; memory values to preset
	mov		a,l
	stax	d
	inx		d
	mov		a,h
	stax	d

; no target memory operation
nomem05:
	lxi		sp,commandText+9
	pop		psw
	pop		b
	pop		d
	lhld	commandText+17
	sphl
	lhld	commandText+15
	jmp		TESTAREA

Int3Vector8085:
	shld	responseText+10
	xchg
	shld	responseText+8	    ; DE
	db		038h,0				; ldsi 0
	xchg
	shld	responseText+12
	lhld	userSP
	sphl
	push	psw
	pop		h
	shld	responseText+4
	mov		l,c
	mov		h,b
	shld	responseText+6
	lhld	commandText+19		; get data address
	mov		a,m					; load data resonse
	sta		responseText+16
	inx		h
	mov		a,m
	sta		responseText+17
	jmp		Respond