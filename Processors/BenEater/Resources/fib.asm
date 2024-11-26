	org	0
	ldi	1
	sta	[0eh]
	ldi	0
@@:
	out
	add	[0eh]
	sta	[0fh]
	lda	[0eh]
	sta	[0dh]
	lda	[0fh]
	sta	[0eh]
	lda	[0dh]
	jc	0
	jmp	@B
	end