serial_port	equ	40h
data_bit	equ	80h

UartInit:
	mvi	a,80h
	out	serial_port	; set serial line high
	call	brid
	ret
UartRead:
	push	b
	push	h
	call	cin
	mov	a,c
	pop	h
	pop	b
	ret
UartWrite:
	push	b
	push	h
	mov	c,a
	call	cout
	pop	h
	pop	b
	ret


;
;  BRID - Determine the baud rate of the terminal. This routine
; actually finds the proper divisors BITTIM and HALFBT to run CIN
; and COUT properly.
;
;   The routine expects a space. It looks at the 6 zeroes in the
; 20h stream from the serial port and counts time from the start
; bit to the first 1.
;
;  serial_port is the port address of the input data. data_bit
; is the bit mask.
;
brid:
	in	serial_port
	ani	data_bit
	jz	brid		; loop till serial not busy

bri1:
	in	serial_port
	ani	data_bit
	jnz	bri1 		; loop till start bit comes

	lxi	h,-7		; bit count
bri3:
	mvi	e,3
bri4:	
	dcr	e		; 42 machine cycle loop
	jnz	bri4

	nop			; balance cycle counts
	inx	h		; inc counter every 98 cycles
				; while serial line is low
	in	serial_port
	ani	data_bit
	jz	bri3		; loop while serial line low

	push	h		; save count for halfbt computation
	inr	h
	inr	l		; add 101h w/o doing internal carry
	shld	bittim		; save bit time
	pop	h		; restore count

	xra	a		; clear carry
	mov	a,h		; compute hl/2
	rar
	mov	h,a
	mov	a,l
	rar
	mov	l,a		; hl=count/2
	inr	h
	inr	l		; add 101h w/o doing internal carry
	shld	halfbt
	ret

;
; Output the character in C
;
;  Bittime has the delay time per bit, and is computed as:
;
;  <HL>' = ((freq in Hz/baudrate) - 98 )/14
;  BITTIM = <HL>'+101H  (with no internal carry prop between bytes)
;
; and OUT to serial_high sets the serial line high; an OUT
; to serial_low sets it low, regardless of the contents set to the
; port.
;
cout:
	mvi	b,11		; # bits to send (start, 8 data, 2 stop)
	xra	a		; clear carry for start bit
co1:
	jnc	cc1		; if carry, will set line high
	mvi	a,data_bit
	out	serial_port	; set serial line high
	jmp	cc2
cc1:
	mvi	a,0
	out	serial_port	; set serial line low
	jmp	cc2		; idle; balance # cycles with those from setting output high
cc2:
	lhld	bittim		; time per bit
co2:
	dcr	l
	jnz	co2		; idle for one bit time
	dcr	h
	jnz	co2		; idle for one bit time

	stc			; set carry high for next bit
	mov	a,c		; a=character
	rar			; shift it into the carry
	mov	c,a
	dcr	b		; --bit count
	jnz	co1		; send entire character
	ret

;
;  CIN - input a character to C.
;
;  HALFBT is the time for a half bit transition on the serial input
; line. It is calculated as follows:
;   (BITTIM-101h)/2 +101h
;
cin:
	mvi	b,9		; bit count (start + 8 data)
	mvi	c,0
ci1:
	in	serial_port	; read serial line
	ani	data_bit	; isolate serial bit
	jnz	ci1		; wait till serial data comes

	lhld	halfbt		; get 1/2 bit time
ci2:
	dcr	l
	jnz	ci2		; wait till middle of start bit
	dcr	h
	jnz	ci2
ci3:
	lhld	bittim		; bit time
ci4:
	dcr	l
	jnz	ci4		; now wait one entire bit time
	dcr	h
	jnz	ci4

	in	serial_port	; read serial character
	ani	data_bit	; isolate serial data
	jz	ci6		; j if data is 0
	inr	a		; now register A=serial data
ci6:
	rar			; rotate it into carry

	dcr	b		; dec bit count
	jz	ci5		; j if last bit
	mov	a,c		; this is where we assemble char
	rar			; rotate it into the character from carry
	mov	c,a
	nop			; delay so timing matches that in output routine
	jmp	ci3		; do next bit
ci5:
	ret
