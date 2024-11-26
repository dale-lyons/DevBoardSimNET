RESET_75	equ	03ch

UART_DATA_PORT		equ	00h
UART_STATUS_PORT	equ	01h
UART_TX_READY		equ	01h
UART_RX_READY		equ	02h

UartInit:
	lxi		h,uart
	mvi		c, UART_SIZE
ui01:
	mov		a,m
	out		UART_STATUS_PORT
	inx		h
	dcr		c
	jnz		ui01

; We know here that ram vectors are va;id so it safe to set them in low memory.
; if this is the rc2014 board(Z80) the uart vectors are set in ROM
	lxi		h,0008h
	lxi		d,UartWrite
	call	SetVector

	lxi		h,0010h
	lxi		d,UartRead
	call	SetVector

	call	UartResetBuffer
; set the rst7.5 vector
	lxi		h,RESET_75
	lxi		d,UartRxInt
	call	SetVector

	mvi		a,0bh			; unmask rst 7.5 interrupt
	sim
	ei
	ret
uart:	db	0,0,0,40h,4eh,37h
UART_SIZE	equ	$-uart

UartResetBuffer:
	push	psw
	push	h
	mvi	h,BUFFER_PAGE
	mvi	l,0
uresb05:
	mvi	m,0aah
	inx	h
	mov	a,l
	ora	a
	jnz	uresb05
	sta	rx_begin
	sta	rx_end
	pop	h
	pop	psw
	ret

UartRxInt:
	push	psw
	push	h
	mvi		h,BUFFER_PAGE
	lda		rx_end
	mov		l,a
	in		UART_DATA_PORT
	mov		m,a
	inx		h
	mov		a,l
	sta		rx_end
	pop		h
	pop		psw
	ei
	ret

UartRead:
	push	h
	ei
urd05:
	lhld	rx_begin
	mov		a,l
	cmp		h
	jz		urd05

	di
	push	b
	mvi		h,BUFFER_PAGE
	lda		rx_begin
	mov		l,a
	mov		b,m
	inx		h
	mov		a,l
	sta		rx_begin
	mov		a,b
	pop		b
	ei

	pop		h
	ret

UartWrite:
	push	psw
uw01:
	in		UART_STATUS_PORT
	ani		UART_TX_READY
	jz		uw01
	pop		psw
	out		UART_DATA_PORT
	ret


;UartRead:
;	call	UartStatusRx
;	jz	UartRead
;	in	UART_DATA_PORT
;	ret
UartStatus:
UartStatusRx:
	in	UART_STATUS_PORT
	ani	UART_RX_READY
	ret

rx_begin:	db	0
rx_end:		db	0
BUFFER_PAGE	equ	(($+255)/ 256)
	org	BUFFER_PAGE*256
rx_buffer:	ds	256
