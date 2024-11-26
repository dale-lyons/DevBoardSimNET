ARGNONE		equ	0
ARGDATA8	equ	1
ARGREG8D	equ	2
ARGREG8S	equ	3
ARGDATA16	equ	4
ARGREG16	equ	5
ARGRST		equ	6

; Input a opcode
; Output
; Z - set if yes
;    clear if no
IsCallJmp:
	call	IsCall
	rz
	call	IsJmp
	ret

IsCall:
	cpi	0cdh
	rz
	mov	b,a
	ani	0c7h
	cpi	0c4h
	jnz	iscl05
	xra	a
iscl05:
	mov	a,b
	ret
IsJmp:
	cpi	0c3h
	rz
	mov	b,a
	ani	0c7h
	cpi	0c2h
	jnz	isjp05
	xra	a
isjp05:
	mov	a,b
	ret

; Input a opcode
; Output
; Z - set if yes
;    clear if no
IsReturn:
	cpi	0c9h
	rz
	mov	b,a
	ani	0c7h
	cpi	0c0h
	jnz	isrt05
	xra	a
isrt05:
	mov	a,b
	ret

; a opcode
; Output
; c: number of bytes for this opcode
numBytesOpcode:
	push	psw
	push	d
	push	h

	mov	e,a
	mvi	d,0
	lxi	h,unasmTable
	dad	d
	mov	c,m
	mvi	b,0
	mvi	e,3
	call	shlBCe
	lxi	h,opCodeTypes
	dad	b
	lxi	d,7
	dad	d
	mov	c,m

	pop	h
	pop	d
	pop	psw
	ret

unasmTable:
	db	0		;nop			;0x00
	db	1		;Lxi
	db	2		;stax	b
	db	3		;inx	b
	db	4		;inr	b
	db	21		;dcr	b
	db	5		;mvi	b,data
	db	6		;rlc
	db	80		; dsub *** undocumented opcode
	db	7		;dad	b
	db	16		;ldax	b
	db	20		;dcx	b
	db	4		;inr	c
	db	21		;dcr	c
	db	5		;mvi	c,data
	db	8		;rrc
	db	81		; arhl *** undocumented opcode			;0x10
	db	1		;lxi	d,data
	db	2		;stax	d
	db	3		;inx	d
	db	4		;inr	d
	db	21		;dcr	d
	db	5		;mvi	d,data
	db	9		;ral
	db	88		; rdel *** undocumented opcode			;0x18
	db	7		;dad	d
	db	16		;ldax	d
	db	20		;dcx	d
	db	4		;inr	e
	db	21		;dcr	e
	db	5		;mvi	e,data
	db	10		;rar
	db	11		;rim			;0x20
	db	1		;lxi	h,data
	db	12		;shld	addr
	db	3		;inx	h
	db	4		;inr	h
	db	21		;dcr	h
	db	5		;mvi	h,data
	db	13		;daa
	db	84		; ldhi *** undocumented opcode			;0x28
	db	7		;dad	h
	db	14		;lhld	addr
	db	20		;dcx	h
	db	4		;inr	l
	db	21		;dcr	l
	db	5		;mvi	l,data
	db	15		;cma
	db	17		;sim			;0x30
	db	1		;lxi	sp,data
	db	79		;sta	addr
	db	3		;inx	sp
	db	4		;inr	m
	db	21		;dcr	m
	db	5		;mvi	m,data
	db	18		;stc
	db	85		; ldsi *** undocumented opcode			;0x38
	db	7		;dad	sp
	db	19		;lda	addr
	db	20		;dcx	sp
	db	4		;inr	a
	db	21		;dcr	a
	db	5		;mvi	a,data
	db	22		;cmc
	db	23		;mov	b,b		;0x40
	db	23		;mov	b,c
	db	23		;mov	b,d
	db	23		;mov	b,e
	db	23		;mov	b,h
	db	23		;mov	b,l
	db	23		;mov	b,m
	db	23		;mov	b,a
	db	23		;mov	c,b
	db	23		;mov	c,c
	db	23		;mov	c,d
	db	23		;mov	c,e
	db	23		;mov	c,h
	db	23		;mov	c,l
	db	23		;mov	c,m
	db	23		;mov	c,a
	db	23		;mov	d,b		;0x50
	db	23		;mov	d,c
	db	23		;mov	d,d
	db	23		;mov	d,e
	db	23		;mov	d,h
	db	23		;mov	d,l
	db	23		;mov	d,m
	db	23		;mov	d,a
	db	23		;mov	e,b
	db	23		;mov	e,c
	db	23		;mov	e,d
	db	23		;mov	e,e
	db	23		;mov	e,h
	db	23		;mov	e,l
	db	23		;mov	e,m
	db	23		;mov	e,a
	db	23		;mov	h,b		;0x60
	db	23		;mov	h,c
	db	23		;mov	h,d
	db	23		;mov	h,e
	db	23		;mov	h,h
	db	23		;mov	h,l
	db	23		;mov	h,m
	db	23		;mov	h,a
	db	23		;mov	l,b
	db	23		;mov	l,c
	db	23		;mov	l,d
	db	23		;mov	l,e
	db	23		;mov	l,h
	db	23		;mov	l,l
	db	23		;mov	l,m
	db	23		;mov	l,a
	db	23		;mov	m,b		;0x70
	db	23		;mov	m,c
	db	23		;mov	m,d
	db	23		;mov	m,e
	db	23		;mov	m,h
	db	23		;mov	m,l
	db	24		;hlt
	db	23		;mov	m,a
	db	23		;mov	a,b
	db	23		;mov	a,c
	db	23		;mov	a,d
	db	23		;mov	a,e
	db	23		;mov	a,h
	db	23		;mov	a,l
	db	23		;mov	a,m
	db	23		;mov	a,a
	db	25		;add	b		;0x80
	db	25		;add	c
	db	25		;add	d
	db	25		;add	e
	db	25		;add	h
	db	25		;add	l
	db	25		;add	m
	db	25		;add	a
	db	26		;adc	b
	db	26		;adc	c
	db	26		;adc	d
	db	26		;adc	e
	db	26		;adc	h
	db	26		;adc	l
	db	26		;adc	m
	db	26		;adc	a
	db	27		;sub	b		;0x90
	db	27		;sub	c
	db	27		;sub	d
	db	27		;sub	e
	db	27		;sub	h
	db	27		;sub	l
	db	27		;sub	m
	db	27		;sub	a
	db	28		;sbb	b
	db	28		;sbb	c
	db	28		;sbb	d
	db	28		;sbb	e
	db	28		;sbb	h
	db	28		;sbb	l
	db	28		;sbb	m
	db	28		;sbb	a
	db	29		;ana	b		;0xa0
	db	29		;ana	c
	db	29		;ana	d
	db	29		;ana	e
	db	29		;ana	h
	db	29		;ana	l
	db	29		;ana	m
	db	29		;ana	a
	db	30		;xra	b
	db	30		;xra	c
	db	30		;xra	d
	db	30		;xra	e
	db	30		;xra	h
	db	30		;xra	l
	db	30		;xra	m
	db	30		;xra	a
	db	31		;ora	b		;0xb0
	db	31		;ora	c
	db	31		;ora	d
	db	31		;ora	e
	db	31		;ora	h
	db	31		;ora	l
	db	31		;ora	m
	db	31		;ora	a
	db	32		;cmp	b
	db	32		;cmp	c
	db	32		;cmp	d
	db	32		;cmp	e
	db	32		;cmp	h
	db	32		;cmp	l
	db	32		;cmp	m
	db	32		;cmp	a
	db	34		;rnz			;0xc0
	db	42		;pop	b	
	db	44		;jnz	addr
	db	43		;jmp	addr
	db	53		;cnz	addr
	db	61		;push	b
	db	62		;adi	data
	db	70		;rst	0
	db	35		;rz
	db	33		;ret
	db	45		;jz	addr
	db	89		; rstv *** undocumented opcode			;0xcb
	db	54		;cz	addr
	db	52		;call	addr
	db	63		;aci	data
	db	70		;rst	1
	db	36		;rnc			;0xd0
	db	42		;pop	d
	db	46		;jnc	addr
	db	71		;out	data
	db	55		;cnc	addr
	db	61		;push	d
	db	64		;sui	data
	db	70		;rst	2
	db	37		;rc
	db	87		; shlx *** undocumented opcode			;0xd9
	db	47		;jc	addr
	db	72		;in	data
	db	56		;cc	addr
	db	82		; jnk *** undocumented opcode			;0xdd
	db	65		;sbi	data
	db	70		;rst	3
	db	39		;rpo			;0xe0
	db	42		;pop	h
	db	49		;jpo	addr
	db	73		;xthl
	db	58		;cpo	addr
	db	61		;push	h
	db	66		;ani	data
	db	70		;rst	4
	db	38		;rpe
	db	74		;pchl
	db	48		;jpe	addr
	db	76		;xchg
	db	57		;cpe	addr
	db	86		; lhlx *** undocumented opcode			;0xed
	db	67		;xri	data
	db	70		;rst	5
	db	41		;rp			;0xf0
	db	42		;pop	psw
	db	51		;jp	addr
	db	77		;di
	db	60		;cp	addr
	db	61		;push	psw
	db	68		;ori	data
	db	70		;rst	6
	db	40		;rm
	db	75		;sphl
	db	50		;jm	addr
	db	78		;ei
	db	59		;cm	addr
	db	83		; jk *** undocumented opcode			;0xfd
	db	69		;cpi	data
	db	70		;rst	7
numops	equ	$-unasmTable

opCodeTypes:
	db	'Nop  ', ARGNONE,	ARGNONE,	1		; 0
	db	'Lxi  ', ARGREG16,	ARGDATA16,	3		; 1
	db	'Stax ', ARGREG16,	ARGNONE,	1		; 2
	db	'Inx  ', ARGREG16,	ARGNONE,	1		; 3
	db	'Inr  ', ARGREG8D,	ARGNONE,	1		; 4
	db	'Mvi  ', ARGREG8D,	ARGDATA8,	2		; 5
	db	'Rlc  ', ARGNONE,	ARGNONE,	1		; 6
	db	'Dad  ', ARGREG16,	ARGNONE,	1		; 7
	db	'Rrc  ', ARGNONE,	ARGNONE,	1		; 8
	db	'Ral  ', ARGNONE,	ARGNONE,	1		; 9
	db	'Rar  ', ARGNONE,	ARGNONE,	1		; 10
	db	'Rim  ', ARGNONE,	ARGNONE,	1		; 11
	db	'Shld ', ARGDATA16,	ARGNONE,	3		; 12
	db	'Daa  ', ARGNONE,	ARGNONE,	1		; 13
	db	'Lhld ', ARGDATA16,	ARGNONE,	3		; 14
	db	'Cma  ', ARGNONE,	ARGNONE,	1		; 15
	db	'Ldax ', ARGREG16,	ARGNONE,	1		; 16
	db	'Sim  ', ARGNONE,	ARGNONE,	1		; 17
	db	'Stc  ', ARGNONE,	ARGNONE,	1		; 18
	db	'Lda  ', ARGDATA16,	ARGNONE,	3		; 19
	db	'Dcx  ', ARGREG16,	ARGNONE,	1		; 20
	db	'Dcr  ', ARGREG8D,	ARGNONE,	1		; 21
	db	'Cmc  ', ARGNONE,	ARGNONE,	1		; 22
	db	'Mov  ', ARGREG8D,	ARGREG8S,	1		; 23
	db	'Hlt  ', ARGNONE,	ARGNONE,	1		; 24
	db	'Add  ', ARGREG8S,	ARGNONE,	1		; 25
	db	'Adc  ', ARGREG8S,	ARGNONE,	1		; 26
	db	'Sub  ', ARGREG8S,	ARGNONE,	1		; 27
	db	'Sbb  ', ARGREG8S,	ARGNONE,	1		; 28
	db	'Ana  ', ARGREG8S,	ARGNONE,	1		; 29
	db	'Xra  ', ARGREG8S,	ARGNONE,	1		; 30
	db	'Ora  ', ARGREG8S,	ARGNONE,	1		; 31
	db	'Cmp  ', ARGREG8S,	ARGNONE,	1		; 32
	db	'Ret  ', ARGNONE,	ARGNONE,	1		; 33
	db	'Rnz  ', ARGNONE,	ARGNONE,	1		; 34
	db	'Rz   ', ARGNONE,	ARGNONE,	1		; 35
	db	'Rnc  ', ARGNONE,	ARGNONE,	1		; 36
	db	'Rc   ', ARGNONE,	ARGNONE,	1		; 37
	db	'Rpe  ', ARGNONE,	ARGNONE,	1		; 38
	db	'Rpo  ', ARGNONE,	ARGNONE,	1		; 39
	db	'Rm   ', ARGNONE,	ARGNONE,	1		; 40
	db	'Rp   ', ARGNONE,	ARGNONE,	1		; 41
	db	'Pop  ', ARGREG16,	ARGNONE,	1		; 42
	db	'Jmp  ', ARGDATA16,	ARGNONE,	3		; 43
	db	'Jnz  ', ARGDATA16,	ARGNONE,	3		; 44
	db	'Jz   ', ARGDATA16,	ARGNONE,	3		; 45
	db	'Jnc  ', ARGDATA16,	ARGNONE,	3		; 46
	db	'Jc   ', ARGDATA16,	ARGNONE,	3		; 47
	db	'Jpe  ', ARGDATA16,	ARGNONE,	3		; 48
	db	'Jpo  ', ARGDATA16,	ARGNONE,	3		; 49
	db	'Jm   ', ARGDATA16,	ARGNONE,	3		; 50
	db	'Jp   ', ARGDATA16,	ARGNONE,	3		; 51
	db	'Call ', ARGDATA16,	ARGNONE,	3		; 52
	db	'Cnz  ', ARGDATA16,	ARGNONE,	3		; 53
	db	'Cz   ', ARGDATA16,	ARGNONE,	3		; 54
	db	'Cnc  ', ARGDATA16,	ARGNONE,	3		; 55
	db	'Cc   ', ARGDATA16,	ARGNONE,	3		; 56
	db	'Cpe  ', ARGDATA16,	ARGNONE,	3		; 57
	db	'Cpo  ', ARGDATA16,	ARGNONE,	3		; 58
	db	'Cm   ', ARGDATA16,	ARGNONE,	3		; 59
	db	'Cp   ', ARGDATA16,	ARGNONE,	3		; 60
	db	'Push ', ARGREG16,	ARGNONE,	1		; 61
	db	'Adi  ', ARGDATA8,	ARGNONE,	2		; 62
	db	'Aci  ', ARGDATA8,	ARGNONE,	2		; 63
	db	'Sui  ', ARGDATA8,	ARGNONE,	2		; 64
	db	'Sbi  ', ARGDATA8,	ARGNONE,	2		; 65
	db	'Ani  ', ARGDATA8,	ARGNONE,	2		; 66
	db	'Xri  ', ARGDATA8,	ARGNONE,	2		; 67
	db	'Ori  ', ARGDATA8,	ARGNONE,	2		; 68
	db	'Cpi  ', ARGDATA8,	ARGNONE,	2		; 69
	db	'Rst  ', ARGRST,	ARGNONE,	1		; 70
	db	'Out  ', ARGDATA8,	ARGNONE,	2		; 71
	db	'In   ', ARGDATA8,	ARGNONE,	2		; 72
	db	'Xthl ', ARGNONE,	ARGNONE,	1		; 73
	db	'Pchl ', ARGNONE,	ARGNONE,	1		; 74
	db	'Sphl ', ARGNONE,	ARGNONE,	1		; 75
	db	'Xchg ', ARGNONE,	ARGNONE,	1		; 76
	db	'Di   ', ARGNONE,	ARGNONE,	1		; 77
	db	'Ei   ', ARGNONE,	ARGNONE,	2		; 78
	db	'Sta  ', ARGDATA16,	ARGNONE,	3		; 79
	db	'Dsub ', ARGNONE,	ARGNONE,	1		; 80
	db	'Arhl ', ARGNONE,	ARGNONE,	1		; 81
	db	'Jnk  ', ARGDATA16,	ARGNONE,	3		; 82
	db	'Jk   ', ARGDATA16,	ARGNONE,	3		; 83
	db	'Ldhi ', ARGDATA8,	ARGNONE,	2		; 84
	db	'Ldsi ', ARGDATA8,	ARGNONE,	2		; 85
	db	'Lhlx ', ARGNONE,	ARGNONE,	1		; 86
	db	'Shlx ', ARGNONE,	ARGNONE,	1		; 87
	db	'Rdel ', ARGNONE,	ARGNONE,	1		; 88
	db	'Rstv ', ARGNONE,	ARGNONE,	1		; 89