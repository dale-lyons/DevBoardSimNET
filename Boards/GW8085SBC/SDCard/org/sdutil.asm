CH_COMMAND_PORT	 	equ 81h
CH_DATA_PORT		equ 80h

MODE_HOST_1		equ	06h

;--- Commands
CH_CMD_GET_IC_VER	equ 01h
CH_CMD_RESET_ALL	equ 05h
CH_CMD_CHECK_EXIST	equ 06h
CH_CMD_READ_VAR8	equ 0Ah
CH_CMD_SET_RETRY	equ 0Bh
CH_CMD_WRITE_VAR8	equ 0Bh
CH_CMD_READ_VAR32	equ 0Ch
CH_CMD_WRITE_VAR32	equ 0Dh
CH_CMD_DELAY_100US	equ 0Fh
CH_CMD_SET_USB_ADDR	equ 13h
CH_CMD_SET_USB_MODE	equ 15h
CH_CMD_TEST_CONNECT	equ 16h
CH_CMD_ABORT_NAK	equ 17h
CH_CMD_GET_STATUS	equ 22h
CH_CMD_RD_USB_DATA0	equ 27h
CH_CMD_WR_HOST_DATA	equ 2Ch
CH_CMD_WR_REQ_DATA	equ 2Dh
CH_CMD_SET_FILE_NAME	equ 2Fh
CH_CMD_DISK_CONNECT	equ 30h
CH_CMD_DISK_MOUNT	equ 31h
CH_CMD_FILE_OPEN	equ 32h
CH_CMD_FILE_ENUM_GO	equ 33h
CH_CMD_FILE_CREATE	equ 34h
CH_CMD_FILE_ERASE	equ 35h
CH_CMD_FILE_CLOSE	equ 36h
CH_CMD_DIR_INFO_READ	equ 37h
CH_CMD_BYTE_LOCATE	equ 39h
CH_CMD_BYTE_READ	equ 3Ah
CH_CMD_BYTE_RD_GO	equ 3Bh
CH_CMD_BYTE_WRITE	equ 3Ch
CH_CMD_BYTE_WRITE_GO	equ 3Dh
CH_CMD_DIR_CREATE	equ 40h
CH_CMD_SET_ADDRESS	equ 45h
CH_CMD_GET_DESCR	equ 46h
CH_CMD_SET_CONFIG	equ 49h
CH_CMD_ISSUE_TKN_X	equ 4Eh

;--- PIDs
CH_PID_SETUP		equ 0Dh
CH_PID_IN		equ 09h
CH_PID_OUT		equ 01h

;--- Status codes
CH_ST_INT_SUCCESS	equ 14h
CH_ST_INT_CONNECT	equ 15h
CH_ST_INT_DISCONNECT	equ 16h

CH_ST_INT_BUF_OVER	equ 17h
CH_ST_INT_DISK_READ	equ 1Dh
CH_ST_INT_DISK_WRITE	equ 1Eh
CH_ST_INT_DISK_ERR	equ 1Fh
CH_ST_RET_SUCCESS	equ 51h
CH_ST_RET_ABORT		equ 5Fh


; -----------------------------------------------------------------------------
; HW_TEST: Check if the USB host controller hardware is operational
; -----------------------------------------------------------------------------
; Output: Cy = 0 if hardware is operational, 1 if it's not
HW_TEST:
	mvi		a,34h
	call 		HW_TEST_DO
	stc
	rnz

	mvi		a,89h
	call		HW_TEST_DO
	stc
	rnz

	ora	a
	ret

HW_TEST_DO:
	mov		b,a
	mvi		a,CH_CMD_CHECK_EXIST
	out		CH_COMMAND_PORT
	mov		a,b
	xri		0ffh
	out		CH_DATA_PORT
	in		CH_DATA_PORT
	cmp		b
	ret

Reset:
	mvi	a,CH_CMD_RESET_ALL
	out	CH_COMMAND_PORT
	call	sDelay
	ret

sDelay:
	push	b
	lxi	b,0a000h
@@:
	dcx	b
	mov	a,b
	ora	c
	jnz	@B
	pop	b
	ret

ConnectDisk:
	mvi	a,CH_CMD_DISK_CONNECT
	out	CH_COMMAND_PORT
	mvi	a,CH_CMD_GET_STATUS
	out	CH_COMMAND_PORT
	call	waitForInt
	ret

MountDisk:
	mvi	a,CH_CMD_DISK_MOUNT
	out	CH_COMMAND_PORT
	mvi	a,CH_CMD_GET_STATUS
	out	CH_COMMAND_PORT
	call	waitForInt
	ret

OpenFile:
	call	SetFilename
	rc
	mvi	a,CH_CMD_FILE_OPEN
	out	CH_COMMAND_PORT
	call	waitForInt
	ret

;Input - de - ptr to filename to set
;Output cy =0  success
;          = 1 fail
SetFilename:
	push	d
	mvi	a,CH_CMD_SET_FILE_NAME
	out	CH_COMMAND_PORT
@@:
	ldax	d
	out	CH_DATA_PORT
	ora	a
	jz	@F
	inx	d
	jmp	@B
@@:
	pop	d
	call	waitForInt
	ret

fileSize:
	mvi	a,CH_CMD_READ_VAR32
	out	CH_COMMAND_PORT
	mvi	a,068h
	out	CH_COMMAND_PORT
	call	waitForInt
	rc

	in	CH_DATA_PORT
	mov	d,a
	in	CH_DATA_PORT
	mov	e,a
	in	CH_DATA_PORT
	mov	h,a
	in	CH_DATA_PORT
	mov	l,a
	ora	a
	ret

readResult:
	mvi	a,CH_CMD_GET_STATUS
	out	CH_COMMAND_PORT
	in	CH_DATA_PORT
	ani	40h
	jnz	readResult

	in	CH_DATA_PORT
	call	hexOut
	call	crlf
	ret

SetMode:
	mvi	a,CH_CMD_SET_USB_MODE
	out	CH_COMMAND_PORT
	call	waitForInt
	mvi	a,MODE_HOST_1
	call	outData
	call	waitForInt
	ret

readFile:
	push	d
	mvi	a,CH_CMD_BYTE_READ
	call	sendCommand
	mvi	a,80h
	call	outData
	mvi	a,0
	call	outData
	call	sendAndInData
	cpi	01dh
	call	hexOut
	jz	rdf05
	cpi	014h
	jz	rdf10
	jmp	rdf10
rdf05:
	mvi	a,27h
	call	sendCommand
	call	inData
	cpi	80h
	jnc	rdf20
	pop	h
	push	h
	push	psw
	mvi	b,80h
@@:
	mvi	m,0
	inx	h
	dcr	b
	jnz	@B

	pop	psw
rdf20:
	pop	h
	call	inHLRepZ

	push	h
	mvi	a,CH_CMD_BYTE_RD_GO
	call	sendCommand
	mvi	a,CH_CMD_GET_STATUS
	call	sendCommand
	call	inData
	pop	h
	cmp	a
	ret
rdf10:
	pop	d
	ori	01h
	ret

here:
	push	psw
	push	h
	lxi	h,msghere
	call	printf
	pop	h
	pop	psw
	ret
msghere:	db	13,10,'Here!!!', 13, 10, 0

inHLRepZ:
	push	psw
@@:
	in	CH_DATA_PORT
	mov	m,a
	inx	h
	dcr	b
	jnz	@B
	pop	psw
	ret

sendAndInData:
	mvi	a,CH_CMD_GET_STATUS
	call	sendCommand
	call	inData
	ret

sendCommand:
	out	CH_COMMAND_PORT
	call	waitForInt
	ret

inData:
	in	CH_DATA_PORT
	ret

outData:
	out	CH_DATA_PORT
	call	waitForInt
	ret

waitForInt:
	lxi	b,0ea84h
wfi05:
	push	b
	in	CH_COMMAND_PORT
	ani	10h
	jnz	@F
	pop	b
	ora	a
	ret
@@:
	pop	b
	dcx	b
	mov	a,b
	ora	c
	jnz	wfi05
	stc
	ret

