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

init:
	xra	a
	sta	available

	call	ping
	rc

	mvi	a,1
	sta	available
	ret

setMode:
	mov	b,a
	mvi	a,CH_CMD_SET_USB_MODE
	out	CH_COMMAND_PORT
	mov	a,b
	out	CH_COMMAND_PORT
	out	CH_DATA_PORT
	call	waitForInt
	ret

checkDrive:
	xra	a
	sta	deviceAttached

	in	CH_DATA_PORT




; -----------------------------------------------------------------------------
; ping: Check if the USB host controller hardware is operational
; -----------------------------------------------------------------------------
; Output: Cy = 0 if hardware is operational, 1 if it's not
ping:
	mvi		a,34h
	mov		b,a
	mvi		a,CH_CMD_CHECK_EXIST
	out		CH_COMMAND_PORT
	mov		a,b
	out		CH_DATA_PORT

	in		CH_DATA_PORT
	xri		0ffh
	cmp		b
	stc
	rnz

	ora	a
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


devviceAvailable:	db	0
deviceAttached:		db	0
