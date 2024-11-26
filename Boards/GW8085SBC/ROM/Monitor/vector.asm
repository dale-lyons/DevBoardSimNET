RST0VECT    equ 0ffe8h
RST1VECT    equ RST0VECT+3
RST2VECT    equ RST1VECT+3
RST3VECT    equ RST2VECT+3
RST4VECT    equ RST3VECT+3
RST5VECT    equ RST4VECT+3
RST6VECT    equ RST5VECT+3
RST7VECT    equ RST6VECT+3

jmpOpcode	equ	0c3h

VectorInit:
	ret

; Input
; HL - vector address to set
; DE - vector value
SetVector:
	push	h
	mvi     m,jmpOpcode
	inx     h
	mov		m,e
	inx     h
	mov		m,d
	pop		h
	ret