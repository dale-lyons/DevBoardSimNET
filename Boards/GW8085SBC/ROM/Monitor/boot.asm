DDT_START		equ	0c000h
ROM_BOOT_BIT		equ	08h
ROM_ENABLE_BIT		equ	10h
USER_LED_BIT		equ	80h
RAM_BOOT		equ	01000h

	org	0f000h
start:
	jmp	boot1
boot1:
	; Rom on, bank 0, user light on
	mvi	a,ROM_ENABLE_BIT
	out	02h
	lxi	h,RAM_BOOT
	lxi	d,preload
	lxi	b,1024
boot2:
	ldax		d
	mov		m,a
	inx		d
	inx		h
	dcx		b
	mov		a,b
	ora		c
	jnz		boot2
	jmp		RAM_BOOT
preload:
	end
