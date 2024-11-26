CCP		equ	0E000h		; CCP is 2K
BDOS		equ	0E800h		; BDOS is 6K
TEMPM		equ	04000h

CPM:
	lxi	h,cpmMessage1
	call	printf

	mvi	a,13h				; activate ROM, select bank#3
	out	02h
	lxi		h,0f000h
	lxi		d,CCP
	lxi		b,0800h
	call		moveMem

	mvi		a,14h			; activate ROM, select bank#3
	out		02h
	lxi		h,0f000h
	lxi		d,BDOS
	lxi		b,1000h
	call		moveMem

	mvi		a,15h			; activate ROM, select bank#5
	out		02h
	lxi		h,0f000h
	lxi		b,0800h
	call		moveMem

	mvi		a,00H			; turn off ROM
	out		02h

	lxi		h,cpmMessage2
	call		printf
	mvi		c,0
	jmp		BDOS

; Copy memory
; Input
;    HL source
;    DE destination
;	 BC size
moveMem:
	push	b
	push	d
	push	h
	lxi		d,TEMPM
mm01:
	mov		a,m
	stax	d
	inx		d
	inx		h
	dcx		b
	mov		a,b
	ora		c
	jnz		mm01

	pop		h
	pop		d
	pop		b

	push	b
	push	h
	lxi		h,TEMPM
	mvi		a,0
	out		02
mm02:
	mov		a,m
	stax	d
	inx		d
	inx		h
	dcx		b
	mov		a,b
	ora		c
	jnz		mm02

	pop		b
	pop		h
	ret
cpmMessage1:	db	'loading CPM 2.2 ...', CR, LF, 0
cpmMessage2:	db	'Warm booting CPM ...', 0
