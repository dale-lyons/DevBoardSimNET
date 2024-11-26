commandText:	ds	32
responseText:	ds	32
TESTAREA:		ds	16
INSTRUCTION_TEST_SIZE	equ		$-commandText

PARM_SIZE	equ	16
argv:
param1:		ds	PARM_SIZE
param2:		ds	PARM_SIZE
param3:		ds	PARM_SIZE
param4:		ds	PARM_SIZE
param5:		ds	PARM_SIZE

dynInOutCmd:
		ds		1
		ds		1
		ds		1
userSP:		ds		2

MAXBPT			equ	8
numBreakPt:		ds	1
breakPtAddr:		ds	MAXBPT*2
breakPtByte:		ds	MAXBPT
lastDump:		ds	2
regEdit:		ds	1
opCodePtr:		ds	2
opCodeTablePtr:		ds	2
numBytes:		ds	1
lastUnassemble:		ds	2
startHex:		ds	2
RegisterFile:
			ds	1		; C
			ds	1		; B
			ds	1		; E
			ds	1		; D
savHL:			ds	1		; L
			ds	1		; H
savSP:			ds	2		; SP
savPC:			ds	2		; PC
savFgs:			ds	1		; Flags
				ds	1		; A
intMask:		ds	1		; Interupt Mask (RIM)
;rx_begin:	db	0
;rx_end:		db	0
;BUFFER_PAGE	equ	(($+255)/ 256)
;	org	BUFFER_PAGE*256
;rx_buffer:	ds	256
memorySize	equ	$-ramStart
		ds	256
stack: