serial_port	equ	80h
serial_control	equ	80h
serial_data	equ	81h

UartInit:
	mvi	a,03h
	out	serial_control	; set serial line high
	nop
	nop
	mvi	a,96h		; rx int disable, rts=0(ready), 8n1
	out	serial_control	; set serial line high
	ret

;UartInt:
;	push	psw
;	push	h
;	ei
;	mvi	h,BUFFER_PAGE
;	lda	rx_end
;	mov	l,a
;	in	UART_DATA_PORT
;	mov	m,a
;	inx	h
;	mov	a,l
;	sta	rx_end
;	pop	h
;	pop	psw
;	ret

UartRead:
	push	b
ur05:
	in	serial_control
	ani	01h
	jz	ur05

	in	serial_data

	pop	b
	ret

UartWrite:
	push	b
	mov	c,a
uw05:
	in	serial_control
	ani	02h
	jz	uw05

	mov	a,c
	out	serial_data

	pop	b
	ret

;resetBuffer:
;	push	h
;	mvi	h,BUFFER_PAGE
;	mvi	l,0
;@@:
;	mvi	m,0aah
;	inx	h
;	mov	a,l
;	ora	a
;	jnz	@B
;	sta	rx_begin
;	sta	rx_end
;	pop	h
;	ret
