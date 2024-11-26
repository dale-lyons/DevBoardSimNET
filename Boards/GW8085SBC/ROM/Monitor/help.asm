; Print help screen
PrInitMsg:
	call	crlf
	lxi	h,initmsg
	call	printf
	ret
PrintHelp:
	call	PrInitMsg
	call	crlf
	lxi	h,helpMsg
	call	printf
	ret
helpMsg:	db	'Commands:', CR, LF
		db	'D Dump Memory:D <address> <length>         C Compare Memory: C <address1> < address2> <length>', CR, LF
		db	'M Move Memory:M <address1> <address2> <length>', CR, LF
		db	'F Fill Memory:F <address> <length> <byte>  E Enter Memory:   C <address1> < address2> <length>', CR, LF
		db	'D Dump Memory:D <address> <length>', CR, LF
		db	'I Input Port :I  <port>                    O Output Port:    O <port>', CR, LF
		db	'T  Trace      T                            Y Trace Over      Y', CR, LF
		db	'G  Go         G <address1> <address2>....', CR, LF
		db	'R  Registers R<flag=[1,0]>                 S Search Memory   S address Length value', CR, LF, 0
