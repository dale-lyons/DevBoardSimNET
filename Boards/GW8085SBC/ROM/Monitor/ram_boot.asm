DDT_START		equ	0c000h
ROM_BOOT_BIT		equ	08h
ROM_ENABLE_BIT		equ	10h
USER_LED_BIT		equ	80h
RAM_BOOT		equ	01000h

	org	RAM_BOOT
	; Rom on, bank 1 user light off
	mvi	a,ROM_ENABLE_BIT OR 1
	out	02

	lxi	h,	DDT_START
	lxi	d,	0f000h
	lxi	b,	1000h
rmb01:
	ldax	d
	mov		m,a
	inx		d
	inx		h
	dcx		b
	mov		a,b
	ora		c
	jnz		rmb01

	; Rom on, bank 2, user light off
	mvi	a,	ROM_ENABLE_BIT OR 2
	out		02h

	lxi	d,	0f000h
	lxi	b,	1000h
rmb02:
	ldax	d
	mov		m,a
	inx		d
	inx		h
	dcx		b
	mov		a,b
	ora		c
	jnz		rmb02

	; ROM off, boot ROM off, select bank#0, user light off
	mvi		a,USER_LED_BIT
	out		02h
	jmp		DDT_START
	end
