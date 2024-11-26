	org	0008h		; RST1
	jmp	UartWrite

	org	0010h		; RST2
	jmp	UartRead
		
	org	0018h		; RST3
	jmp	RST3VECT
		
	org	0020h		; RST4
	jmp	RST4VECT

	org	0028h		; RST5
	jmp	RST5VECT

	org	0030h		; RST6
	jmp	RST6VECT

	org	0038h		; RST7
	jmp	RST7VECT