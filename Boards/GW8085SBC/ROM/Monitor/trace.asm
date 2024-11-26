RST3		equ	018h
RST3INSTR	equ	0dfh
JMPINSTR	equ	0c3h

;ramVectors - 1, ram located at zero page
;           - 0, rom located at zero page
TraceInit:
	IF RAM_VECTORS
	lxi		h,0018h
	ELSE
	lxi		h,RST3VECT
	ENDIF
trc01:
	lxi		d,Int3Vector
	call	SetVector
	ret

TraceOver:
	lhld	savPC
	mov		a,m
	jmp		trc05

Trace:
	lhld	savPC
	mov		a,m
	call	IsCallJmp
	jnz		trc05

; call or jump detected. set break at call or jump address
	push	h
	inx		h
	mov		e,m
	inx		h
	mov		d,m
	call	addBP
	pop		h
	rnz					; nz means add breakpoint failed.
	jmp	Restore
trc05:
	call	IsReturn
	jnz		trc10
; set breakpoint at return address
	push	h
	lhld	savSP
	mov	e,m
	inx	h
	mov	d,m
	call	addBP
	pop	h
trc10:
	call	numBytesOpcode
	mvi	b,0
	dad	b
	xchg
	call	addBP
	lxi	h,0
	dad	sp
	shld	userSP
	jmp	Restore
trc99:
	lhld	userSP
	sphl
	;remove all break points
	call	restoreBPS
	call	DumpRegisters
	ret

Restore:
	lxi	sp,RegisterFile
	pop	b
	pop	d
	lxi	sp,savFgs
	pop	psw
	lhld	savSP
	sphl
	lhld	savPC
	push	h
	lhld	savHL
	ret

Int3Vector:
	shld	savHL
	pop		h
	dcx		h
	shld	savPC
	push	psw
	pop		h
	shld	savFgs
	lxi		h,0
	dad		sp
	shld	savSP

	mov		l,c
	mov		h,b
	shld	RegisterFile
	mov		l,e
	mov		h,d
	shld	RegisterFile+2
	jmp		trc99

; restore all break points
restoreBPS:
	lda		numBreakPt
	ora		a
	rz						; no breakpoints set.
	mov		c,a
	lxi		h,breakPtAddr
	lxi		d,breakPtByte
revb05:
	push	b
	mov		c,m
	inx		h
	mov		b,m
	inx		h

	ldax	d
	inx		d
	stax	b
	pop		b
	dcr		c
	jnz		revb05
	xra		a
	sta		numBreakPt
revb99:
	ret

clearBP:
	xra		a
	sta		numBreakPt
	ret

; de address
; added check to make sure we are not trying to single step in ROM
; Returns
;   Z - bp added ok
;  NZ - bp added failed. (probably because we are in ROM)
addBP:
	push	d
	ldax	d
	mov		c,a
	mvi		a,RST3INSTR		; rst 3 instruction
	stax	d
; check that the write succeeded
	ldax	d
	pop		d
	cpi		RST3INSTR		; check that the byte written is the same as read
	rnz						; return nz as failed.

	push	d
	lda		numBreakPt
	ral
	mov		e,a
	mvi		d,0
	lxi		h,breakPtAddr
	dad		d
	pop		d

	mov		m,e
	inx		h
	mov		m,d

	lda		numBreakPt
	mov		e,a
	mvi		d,0
	lxi		h,breakPtByte
	dad		d
	mov		m,c
	inr		a
	sta		numBreakPt
	xra		a; 	return z as success.
	ret
