;Z80 Membership Card Firmware with SD support, Version 1.5, July 23, 2017
;File: ZMCv15.asm
;
;	MACRO 	VERSION_MSG
;	DB	CR,LF,"Z80 MEMBERSHIP CARD MICRO-SD, v1.5beta July 23, 2017",CR,LF,EOS
;	ENDM
;	Table of Contents
;	Preface_i	Acknowledgments, Revisions, notes
;	Preface_ii	Description, Operation
;	Preface_iii	Memory Mapping, I/O Mapping
;	Chapter_1	Page 0 interrupt & restart locations
;	Chapter_2	Startup Code
;	Chapter_3	Main Loop, MENU selection
;	Chapter_4	Menu operations. Loop back, Memory Enter/Dump/Execute, Port I/O
;	Chapter_5	Supporting routines. GET_BYTE, GET_WORD, PUT_BYTE, PUT_HL, PRINT, DELAY, GET/PUT_REGISTER
;	Chapter_6	Menu operations. ASCII HEXFILE TRANSFER
;	Chapter_7	Menu operations. XMODEM FILE TRANSFER
;	Chapter_8	Menu operations. RAM TEST
;	Chapter_9	Menu operations. DISASSEMBLER - Deleted
;	Chapter_10	BIOS.  PUT_CHAR (RS-232 & LED), GET_CHAR (RS-232), IN_KEY (Keyboard)
;	Chapter_11	ISR.  RS-232 Receive, LED & Keyboard scanning, Timer tic counting
;	Chapter_12	SD-CARD
;	Chapter_13	FILE operations
;	Chapter_14	FILE Support Routines
;	Chapter_15	SD Memory Card Routines, Mid Level, Send/Recieve Data Sectors (Writes out Dirty Data)
;	Chapter_16	General Support Routines, 32 Bit stuff and other math
;	Chapter_17	High RAM routines
;	Appendix_A	LED FONT
;	Appendix_B	Future Use
;	Appendix_C	RAM. System Ram allocation (LED_Buffer, KEY_Status, RX Buffer, etc)
;	Appendix_D	HOOK LOCATIONS
;	Appendix_E	Z80 Instruction Reference


;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;	Preface_i - Acknowledgments, Revisions, notes
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;
;Assemble using ver 2.0 of the ASMX assembler by Bruce Tomlin
;
;Command to assemble:
;
;   asmx20 -l -o -e -C Z80 ZMCv12.asm
;
;
;Z80 Membership Card hardware by Lee Hart.
;
;V1.x -Operation, Documentation and Consultation by Herb Johnson
;
;Firmware by Josh Bensadon. Date: Feb 10, 2014
;
;FP LED & Keyboard operation concepts adapted from the Heathkit H8 computer.
;
;Revision.
;
;
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;	Preface_ii - Description, Operation
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;
;- - - HARDWARE - - -
;
;The Hardware is comprised of up to three boards, the CPU board, SIO board and Front Panel (FP) board.
;
;CPU Board:
; Z80 CPU, 4Mhz Clock, 5V Regulator, 32K EPROM (w/firmware), 32K RAM, 8 bit input port, 8 bit output port
;
;SIO Board:
; Additional RAM, up to 512K bytes to bank switch the lower 32K ROM on CPU Board
; ACE 8250 UART with RS-232 or FTDI232 (5V TTL Level) connection
; Micro SD Card Slot
;
;Front Panel Board:
; Terminal for Power & RS-232 connection, Timer for 1mSec interrupt, LED Display Driver & Keyboard Matrix.
;
; LED Display: 7 x 7-Segment displays (d1 to d7) and 7 annunciator leds (x1 to x7) below the 7 digits.
;
;    d1   d2   d3   d4   d5   d6   d7
;    _    _    _    _    _    _    _
;   |_|  |_|  |_|  |_|  |_|  |_|  |_|
;   |_|  |_|  |_|  |_|  |_|  |_|  |_|
;    _   _      _   _     _   _     _
;    x1  x2     x3  x4    x5  x6    x7
;
; Keyboard: 16 keys labeled "0" to "F" designated as a HEX keyboard
; The "F" key is wired to a separate input line, so it can be used as a "Shift" key to produce an extended number of key codes.
; The "F" and "0" keys are also wired directly to an AND gate, so that pressing both these keys produces a HARD reset.
;
;- - - FIRMWARE - - -
;
;The Firmware provides a means to control the system through two interfaces.
;Control is reading/writing to memory, registers, I/O ports; having the Z80 execute programs in memory or halting execution.
;The two interfaces are:
; 1. The Keyboard and LED display
; 2. A terminal (or PC) connected at 9600,N,8,1 to the RS-232 port.
;
;- - - The Keyboard and LED display interface - - -
;
;While entering commands or data, the annunciator LED's will light according to the state of the operation or system as follows:
;
; x1 = Enter Register
; x2 = Enter Memory Location
; x3 = Alter Memory/Register
; x4 = Send Data to Output Port
; x5 = Monitor Mode (Default Mode upon Power up)
; x6 = Run Mode
; x7 = Beeper (on key press)
;
;Keyboard Functions:
;
; "F" & "0" - Force a HARD reset to the Z80 and restarts the system.  See System Starting for additional details.
;
; "0" - Display a Register.  x1 lights and you have a few seconds to select which register to display.
; "E" - Display Memory.  x2 lights and you have a few seconds to enter a memory location.
; "5" - Display Input Port.  x2 lights and you have a few seconds to enter a port address.
; "6" - Output Port. x2 lights and you have a few seconds to enter a port address,
;	then x4 lights and you can enter data to output, new data may be sent while x4 remains lit.
; "A" - Advance Display Element.  Advances to next Register, Memory address or Port address.
; "B" - Backup Display Element.  Backs up to previous Register, Memory address or Port address.
; "4" - Go. Preloads all the registers including the PC, thus causes execution at the current PC register.
; "7" - Single Step.
; "D" - Alter/Output.  Depending on the display, Selects a different Register, Memory Address, Port or Sends Port Output.
;	Note, "D" will only send to that Output Port, to change port, reuse Command 6.
; "F" & "E" - Does a SOFT reset when Running.
;
;
;- - - The Terminal interface - - -
;
;Through a Terminal, there are more features you can use.  Entering a question mark (?) or another unrecognized command will display a list of available commands.
;Most commands are easy to understand, given here are the few which could use a better explaination.
;
; C - Continous Dump.	Works like the D command but without pausing on the page boundaries.  This is to allow the text capturing of a dump.
;			The captured file can then be later sent back to the system by simply sending the text file through an ASCII upload.
; M - Multiple Input.	Allows the entering of data in a format that was previously sent & saved in an ASCII text file.
; R - Register.		Entering R without specifiying the register will display all the registers.
;			A specific register can be displayed or set if specified.  eg. R HL<CR>, R HL=1234<CR>
; T - Test RAM		Specify the first and last page to test, eg T 80 8F will test RAM from 8000 to 8FFF.
; X - Xmodem Transfers	Transfers a binary file through the XModem protocol.  Enter the command, then configure your PC to receive or send a file.
;			eg. X U 8000<CR> will transfer a file from your PC to the RAM starting at 8000 for the length of the file (rounded up to the next 128 byte block).
;			eg. X D 8000 0010 will transfer a file from RAM to your PC, starting at 8000 for 10 (16 decimal) blocks, hence file size = 2K.
; : - ASCII HEX Upload	The ":" character is not entered manually, it is part of the Intel HEX file you can upload through ASCII upload.
;			eg. While at the prompt, just instruct your terminal program to ASCII upload (or send text file) a .HEX file.
;
;
;- - - System Starting - - -
;When the Z80 starts execution of the firmware at 0000, all the registers are saved to memory for examination or modification.
;There are many ways the Z80 can come to execute at 0000.  The firmware tries to deterimine the cause of the start up and will respond differently.
;Regardless of why, the firmware first saves all the registers to RAM and saves the last Stack word *assuming* it was the PC.
;A test is done to check if the FP board or SIO board is present.
;-If there is no FP board, then the firmware will either RUN code in RAM @8002 (if there's a valid signature of 2F8 @8000) or HALT.
;Next, 8 bytes of RAM is tested & set for/with a signature.
;-If there isn't a signature, it is assumed the system is starting from a powered up condition (COLD Start), no further testing is done.
;When the signature is good (WARM Start), more tests are done as follows:
;Test Keyboard for "F"&"E" = Soft Reset from Keyboard
;Test Keyboard for "F"|"0" = Hard Reset from Keyboard
;Test Last instruction executed (assuming PC was on Stack) for RST 0 (C7) = Code Break
;Test RS-232 Buffer for Ctrl-C (03) = Soft Reset from Terminal
;If cause cannot be deterimined, it is assumed an external source asserted the RESET line.
;
;The Display will indicate the cause of reset as:
;	"COLD 00"  (Power up detected by lack of RAM Signature)
;	"SOFT ##"  (F-E keys pressed)
;	"STEP ##"  (Single Step)
;	"^C   ##"  (Ctrl-C)
;	"HALT ##"  (HALT Instruction executed)
;	"F-0  ##"  (F-0 Hard Reset)
;	"RST0 ##"  (RST0 Instruction executed)
;	"HARD ##"  (HARD Reset by other)
;
;Where the number after the reset shows the total number of resets.
;
;The PC will be changed to 8000 on Cold resets.
;
;
;- - - Firmware BIOS - - -
;
;There are routines which can be called from your program to access the RS-232 Bit banging interface, Keyboard or Display inteface or Timer interrupt services.
;
;Label		Addr.	Description
;Put_Char	xxxx	Sends the ASCII character in A to the RS-232 port or LED Display (no registers, including A, are affected)
;Put_HEX	xxxx	Converts the low nibble of A to an ASCII character 0 to F and sends to RS-232 or LED Display
;Put_Byte	xxxx	Converts/sends both high and low nibbles of A (sends 2 ASCII Character) to RS-232 or LED Display

;Warning: FCB's must never cross page boundaries.



; Z80 - Registers
;
; A F   A' F'
; B C   B' C'
; D E   D' E'
; H L   H' L'
;    I R
;    IX
;    IY
;    SP
;    PC


;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;	Preface_iii- Memory Mapping, I/O Mapping
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

;String equates
CR		equ	0x0D
LF		equ	0x0A
EOS		equ	0x00


;Memory Mapping
;
;0x0000 - 0x7FFF	EPROM or RAM (BANK SWITCHED)
;0x8000 - 0xFFFF	RAM

; Notes on Z80MC CPU board.
; Memory Mapping
; Holding /ROM high will inhibit the ROM from being enabled on the Data bus.
; Holding /RAM high will inhibit the RAM from being enabled on the Data bus.
; /ROM is loosely pulled low on Memrq & A15 low (ie, ROM 0000-7FFF)
; /RAM is loosely pulled low on Memrq & A15 high (ie, RAM 8000-FFFF)
; 
; 
; Notes on Z80 SRS board.
; Memory (SDC-RAM)
; The RAM on the SD card and the /ROM signal for the CPU board are both generated by U4.
; The Y output is the value of the addressed Data input pin or Low if strobe (G) is high.
; The W output is always the inverted value of Y.
; Y goes to /ROM and W goes to RAM /CE.  
; Therefore, when Y is high /ROM is inhibited and RAM /CE is low
; When Y is low /ROM is unhindered (and ROM is allowed to be accessed on A15=0)
; 
; When NOT requesting any Memory operation, /MREQ=1, Y is LOW.  This is irrelevant to
; the CPU ROM and RAM, but it ensures that the SDC-RAM is NOT enabled.
; With A15=0:
;  >Reading, Y is controled by Q1 (0=ROM, 1=RAM)
;  >Writing, Y is high, thus SDC-RAM is enabled to accept the write
;  
; With A15=1:
;  >Reading or Writing, Y is low, thus SDC-RAM is disabled and CPU-RAM is enabled
 

; Notes on Z80MC CPU board.
; I/O Mapping
; /OUT is loosely pulled low on A7 low, A6 high, A5 low and /WR low  (ie, 40-5F)
; /IN  is loosely pulled low on A7 low, A6 high, A5 low and /RD low  (ie, 40-5F)
;I/O
;0x40	Input/Output
;	Output bits	*Any write to output will clear /INT AND advance the Scan/Column Counter U2A.
;	0 = Segment D OR LED7       --4--
;	1 = Segment E OR LED6      2|   |3
;	2 = Segment F OR LED5       |   |
;	3 = Segment B OR LED4       --5--
;	4 = Segment A OR LED3      1|   |6
;	5 = Segment G OR LED2       |   |
;	6 = Segment C OR LED1       --0--
;	7 = RS-232 TXD (Bit Banged) = 1 when line idle, 0=start BIT
;
;	Input Bits
;	0 = Column Counter BIT 0 (Display AND Keyboard)
;	1 = Column Counter BIT 1 (Display AND Keyboard)
;	2 = Column Counter BIT 2 (Display AND Keyboard)
;	3 = 0 when Keys 0-7 are pressed (otherwise = 1), Row 0
;	4 = 0 when Keys 8-E are pressed (otherwise = 1), Row 1
;	5 = 1 when Key F is pressed (otherwise = 0), Key F is separate so it may be used as A Shift Key
;	6 = 1 when U2B causes an interrupt, Timer Interrupt (Send Output to reset)
;	7 = RS-232 RXD (Bit Banged) = 1 when not connected OR line idle, 0=first start BIT
;
;	Bit 5 allows Key F to be read separately to act as A "Shift" key when needed.
;	Bits 0-2 can be read to ascertain the Display Column currently being driven.
;
; Notes on Z80 SIO board.
; I/O 
; C0-C8, write single bit (D0) to BIT LATCH U5.
; C0=Q0 SPI_CLK
; C1=Q1 BANK 0=ROM, 1=RAM
; C2=Q2
; C3=Q3
; C4=Q4 SD /CS
; C5=Q5 SD MOSI
; C6=Q6 
; C7=Q7 ACE MASTER RESET (1=RESET UART)
; 
; Q0  SPI_CLK
; 0   HIGH (Always)
; 1   LOW and PULSED HIGH BY I/O 88  (NO NEED TO I/O TWICE HIGH/LOW FOR A PULSE)
;
;
; IN C0 = SD Clock Pulse
;  
; 	LD	A,1
; 	OUT	(0xC0),A	;ENABLE CLK LOW
; 	
; 				;SINGLE I/O TO PULSE SPI_CLK
; 	IN	A,(0xC0)	;PULSE SPI_CLK HIGH WITH A SINGLE I/O
; 
; 
; C8-CF, write/read bytes to 8250 UART
; C8 TXBUFFER/RXBUFFER
; C9 Interrupt enable reg
; CA Interrupt ID reg
; CB Line  Control Reg (LCR) word len, stop bits, parity
; CC Modem Control Reg (MCR) 0, 0, 0, Loop, OUT2, OUT1, RTS, DTR
; CD Line Status Reg (LSR) TX/RX Buffer Empty
; CE Modem Status Reg (MSR) DCD, RI, DSR, CTS, DDCD, TERI, DDSR, DCTS
; CF Scratch Register (SCR) General read/write
; 
; Outputs in Modem Control Reg B (MCR) control the High 4 address bits of the
; extended RAM. These 4 bits are inverted and on a MR, the outputs all go high.
; CC.0  /DTR  B15
; CC.1  /RTS  B16
; CC.2  /OUT1 B17
; CC.3  /OUT2 B18
; 
; Inputs from Modem Status Reg 
; CE.7  /DCD  MISO (Inverted Data from SD Card)
; 
; Inputs from Line Status Register indicate UART condition
; CD.0  Data Read 1=RX Data ready to read
; CD.5  Data TX Holding Register Empty 1=Empty (ie ok to send next byte) Mask 0x20
;

Port40		equ	0x40	;LED DISPLAY, RS-232 TXD/RXD AND KEYBOARD

				;U5 - 74LS259 Bit Addressable Latch
SDCLK		equ	0xC0	;SD Clk and Clk Pulse
RAMROM		equ	0xC1	;RAM /ROM selection
BITS_Q2		equ	0xC2	;PIN 6
BITS_Q3		equ	0xC3	;PIN 7
SDCS		equ	0xC4	;SD Card /CS
SDTX		equ	0xC5	;SD Card TX Data
GREEN_LED	equ	0xC6	;GREEN_LED, PIN 11
ACE_RESET	equ	0xC7	;ACE Reset

ACE_DATA	equ	0xC8	;ACE TX and RX register
ACE_BAUD0	equ	0xC8	;ACE Baudrate Low
ACE_BAUD1	equ	0xC9	;ACE Baudrate High
ACE_LCR		equ	0xCB	;ACE Word len/bit setup
ACE_OUT		equ	0xCC	;ACE RAM Bank Selection.  Lower 4 bits map to Bank Select.
ACE_STATUS	equ	0xCD	;ACE RX/TX status
ACE_MSR		equ	0xCE	;ACE Modem Status Register
ACE_SCRATCH	equ	0xCF	;ACE Scratch Register (not used, because not available on all UARTs)

;	Chapter_1	Page 0 interrupt & restart locations
;
;                        *******    *******    *******    *******
;                       *********  *********  *********  *********
;                       **     **  **     **  **     **  **     **
;                       **     **  **     **  **     **  **     **
;---------------------  **     **  **     **  **     **  **     **  ---------------------
;---------------------  **     **  **     **  **     **  **     **  ---------------------
;                       **     **  **     **  **     **  **     **
;                       **     **  **     **  **     **  **     **
;                       *********  *********  *********  *********
;                        *******    *******    *******    *******


		org	0x0000
					; Z80 CPU LDRTS HERE
		DI			; Disable Interrupts
		JP	RESETLDRT

		org	0x0008		; RST	0x08
		RET

		org	0x0010		; RST	0x10
		RET
		
		org	0x0018		; RST	0x18
		RET
		
		org	0x0020		; RST	0x20
		RET

		org	0x0028		; RST	0x28
		RET

		org	0x0030		; RST	0x30
		RET

;Interrupts:
;The Serial bit banger uses an interrupt on the fall of the start bit, therefore we must count
;clock cycles to the data bits.  Interrupts can happen in the middle of any instruction.  
;With most common instructions being between 4 and 16 tc, the count starts at 10 tc
;An Interrupt is really a RST 38 instruction (with 2 extra wait cycles)
		;Previous Instruction	;10
		;RST	0x38		;13  (11 + 2 wait cycles)
		org	0x0038
		EX	AF,AF'		;4
		EXX			;4
		LD	HL,(INT_VEC)	;16 Typical Calls are to ISR_DISPATCH
		JP	(HL)		;4

RST38_LEN	EQU	$-0x0038

;What the stack looks like on Interrupts...
;
;OLD-STACK    ISR-STACK
;   PC	
;		HL
;		AF
;SP		PC	(Call to ISR_DISPATCH from High Ram Dispatch)
;		PC	(Call to GET_REGISTER from ISR)

		org	0x0066		; NMI Service Routine
		PUSH	HL
		LD	HL,(NMI_VEC)
		JP	(HL)
;NMI_VEC:	RETN			;



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_2	Startup Code
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

		ORG	0x0100
;-------------------------------------------------------------------------------- RESET LDRTUP CODE
RESETLDRT:
;Save Registers & SET sp
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		IM	1		; Interrupts cause RST 0x38

					;No need to select ROM, we are already here.
		LD	(RSHL),HL

		POP	HL		;Fetch PC
		LD	(RPC),HL	;Save the PC
		LD	(RSSP),SP	;Save the SP

		LD	SP,RSDE+2	;Set Stack to save registers DE,BC,AF
		PUSH	DE
		PUSH	BC
		PUSH	AF

		LD	A,1
		OUT	(ACE_RESET),A	;RESET ACE

		EX	AF,AF'		;Save Alternate register set
		EXX			;Save Alternate register set
		LD	SP,RSHL2+2	;Set Stack to save registers HL',DE',BC',AF'
		PUSH	HL
		PUSH	DE
		PUSH	BC
		PUSH	AF
		EX	AF,AF'
		EXX

		LD	A,I		;Fetch IR
		LD	B,A
		LD	A,R
		LD	C,A
		PUSH	BC		;Save IR

		PUSH	IY
		PUSH	IX

		LD	SP, StackTop	; Stack = 0xFF80 (Next Stack Push Location = 0xFF7F,0xFF7E)


;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Save Input State, Set default Output state
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		LD	C,Port40	;Select I/O Port
		LD	E,0x80		;Advance Column		
		LD	B,8		; 8 Tries to get to Column 0
RSIS_LP		OUT	(C),E		; Clear LED Display & Set RS-232 TXD to inactive state
		IN	A,(C)		;Fetch Column
		LD	D,A		;Save IN D (For RESET Test)
		AND	7		;Mask Column only
		JR   Z,	RSIS_OK		;When 0, exit Test Loop
		DJNZ	RSIS_LP
RSIS_OK					;Input State upon reset saved IN Register D


		LD	HL,BIT_TABLE	;BIT_TABLE = 0x01,0x02,0x04,0x08,0x10,0x20,0x40,0x80
		LD	A,1
FILL_BT		LD	(HL),A
		INC	HL
		RLCA
		JR  NC,	FILL_BT

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Test Hardware - FP Board Present?
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

					;Verify FP board is present
		LD	BC,0x1800	; Several Loops through Column 0 proves FP board is working
		LD	E,10		; 10 Retries if not expected Column

RTHW_LP		IN	A,(Port40)	;Fetch Column
		AND	7
		CP	C
		JP  Z,	RTHW_OK		;Jump if Column = expected value

		DEC	E		;If not expected, count the errors.
		JR  Z,	RTHW_EXIT	;If error chances down to zero, there's no FP
		JR	RTHW_ADV

RTHW_OK		INC	C		; Advance expected value
		RES	3,C		; Limit expected value to 0-7
RTHW_ADV	LD	A,0x80		;Advance Column
		OUT	(Port40),A	; Clear LED Display & Set RS-232 TXD to inactive state
		DJNZ	RTHW_LP
		LD	E,A
RTHW_EXIT				;E=80 FP present, (D still holding input state)
					;E=00 NO FP


;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Test Hardware - SIO Board Present?
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		XOR	A
		OUT	(ACE_RESET),A	;Cancel RESET of ACE
		
		LD	B,0		;Count 256 times	
		LD	A,0x80		;Set baud rate
		OUT	(ACE_LCR),A

RTHW_SIOT_LP	LD	A,B		;Use the counter to test for the presence 
		OUT	(ACE_BAUD0),A	;of the ACE SCRATCH Register
		IN	A,(ACE_BAUD0)
		CP	B
		JR NZ,	RTHW_SIO_EXIT
		DJNZ	RTHW_SIOT_LP
		INC	E		;All tests OK, Advance E so the LSD is 1
		CALL	SD_DESELECT	;DISABLE SD CARD, LED's OFF
RTHW_SIO_EXIT	LD	A,E
		RLCA
		LD	(HW_LIST),A	;Save HW List
					;00 NO Boards
					;01 FP only
					;02 SIO only
					;03 FP & SIO


		JR 	CHK_RESET	;Ignore EXEC_RAM_2F8

;		OR	A
;		JR NZ,	CHK_RESET	;Jump if any board is present


;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Execute RAM program if NO FP Board Present
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
					;If scan counter not running, then either Execute RAM OR HALT
EXEC_RAM_2F8	LD	HL,(0x8000)	;Address of RAM Valid Signature
		LD	BC,0x2F8	;(FP board probably not present)
		XOR	A		;Verify RAM valid with 2F8 signature at 0x8000
		SBC	HL,BC
		JP	Z,0x8002	;Execute RAM
		JP	$		;Or HALT

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Determine Reason for RESET ie entering Monitor Mode (D=Key Input from save state section)
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
					;Determine why the CPU is executing A RESET.
					;  -Power on (RAM signature will be wrong)
					;  -F-0 Reset Switch (one of the switches will still be pressed)
					;  -RST 0 (look for C7 at previous location given by stack)
					;  -External /RESET OR User program branches to 0000
					;
					;RC_TYPE
					;0	COLD RESET (NO RAM SIGNATURE)
					;1	F-E Soft Reset (SOFT_RST_FLAG = FE)
					;2	Single Step (SOFT_RST_FLAG = D1)
					;3	<Ctrl>-C (SOFT_RST_FLAG = CC)
					;4	HALT (SOFT_RST_FLAG = 76)
					;5	F-0 (F or 0 key on FP still down)
					;6	RST0 ( M(PC-1) = C7)
					;7	HARD RESET (Default if SOFT_RST_FLAG not set)

					;Test RAM Signature for Cold Start Entry
CHK_RESET	LD	HL,RAMSIGNATURE		
		LD	A,0xF0		;First signature byte expected
		LD	B,8		;#bytes in signature (loop)
RAMSIG_LP	CP	(HL)
		JR  NZ,	COLD_START
		INC	L
		SUB	0xF
		DJNZ	RAMSIG_LP
		JR	WARM_START

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
COLD_START	LD	HL,RC_TYPE
		LD	B,CS_CLR_LEN
		CALL	CLEAR_BLOCK

		;DB	0		;(RC_TYPE)
		;DB	0		;(RC_SOFT)
		;DB	0		;(RC_STEP)
		;DB	0		;(RC_CC)
		;DB	0		;(RC_HALT)
		;DB	0		;(RC_F0)
		;DB	0		;(RC_RST0)
		;DB	0		;(RC_HARD)
		;DB	0		;(RegPtr)
		;DW	0		;(ABUSS)
		;DB	0		;(IoPtr)
		;DB	0		;(RX_ERR_LDRT)
		;DB	0		;(RX_ERR_STOP)
		;DB	0		;(RX_ERR_OVR)
		
		LD	A,12
		LD	(ACE_BAUD),A

		LD	HL,UiVec_RET
		LD	(UiVec),HL

		LD	A,(HW_LIST)	;Fetch HW List
		AND	3		;00 NO FP
		LD	C,A		;01 FP only
		CALL	SET_IO		;02 SIO only
					;03 SIO & FP
		
		LD	HL,StackTop-2
		LD	(RSSP),HL
		LD	HL,RAM_LDRT
		LD	(RPC),HL
		JR	INIT_SYSTEM

					;Determine Warm Start condition
					;Input: Various tests
					;(D=Key Input from save state section)
					;Output: HL=Pointer to Reset Codes (RC_????)
WARM_START	LD	HL,RC_SOFT	;HL=RC_SOFT
		LD	A,(SOFT_RST_FLAG)
		CP	0xFE		;'FE' is the keyboard code for holding F & E
		JR  Z,	WS_SET
		INC	L		;HL=RC_STEP
		CP	0xD1		;'D1' is the code for Do One step
		JR  Z,	WS_SET
		INC	L		;HL=RC_CC
		CP	0xCC		;'CC' is the code for <Ctrl>-C
		JR  Z,	WS_SET
		INC	L		;HL=RC_HALT
		CP	0x76		;'76' is the code and opcode for HALT
		JR  Z,	WS_SET
		INC	L		;HL=RC_F0
		
		LD	A,(HW_LIST)
		RRA
		JR  NC,	WS_NOFP
		
		LD	A,D		;Fetch Input of Column 0
		BIT	5,A		;
		JR NZ,	WS_SET		;Jump if F switch pressed
		BIT	3,A		;
		JR  Z,	WS_SET		;Jump if 0 switch pressed
		
WS_NOFP		INC	L		;HL=RC_RST0
		LD	DE,(RPC)
		DEC	DE
		LD	A,(DE)
		CP	0xC7		;Did we get here by a RESTART 0 instruction?
		JR  Z,	WS_SET		;Jump if RST 0 Instruction
		INC	L		;HL=RC_HARD

WS_SET		LD	A,L		;Low address of Pointer & 7 = Reset code type
		LD	(SOFT_RST_FLAG),A ;Nuke flag until next time
		AND	7
		LD	(RC_TYPE),A
		CALL	TINC		;Advance the reset counter

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Init all System RAM, enable interrupts
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
INIT_SYSTEM	LD	HL,RXBUFFER
		LD	B,0
		CALL	CLEAR_BLOCK

		LD	HL,CLEARED_SPACE
		LD	B,CLEARED_LEN
		CALL	CLEAR_BLOCK

		LD	HL,(DISPMODE)
		LD	(SDISPMODE),HL

		CALL	WRITE_BLOCK
		DW	BEEP_TO		;Where to write
		DW	34		;Bytes to write
		DB	1		;(BEEP_TO)
		DB	0x84		;(ANBAR_DEF) = MON MODE
		DW	GET_REG_MON	;(GET_REG)
		DW	PUT_REG_MON	;(PUT_REG)
		DW	CTRL_C_RET	;(CTRL_C_CHK)
		DW	IDISP_RET	;(LDISPMODE)
		DW	IDISP_RET	;(DISPMODE)
		DW	IMON_CMD	;(KEY_EVENT) Initialize to Command Mode
		DB	1		;(IK_TIMER)
		DB	0x90		;(KEYBFMODE) HEX Keyboard Mode (F on release)
		DB	1		;(DISPLABEL)
		DB	-1		;(IK_HEXST)
		DW	LED_DISPLAY	;(HEX_CURSOR) @d1
		DW	HEX2ABUSS	;(HEX_READY)
		DW	LED_DISPLAY	;(LED_CURSOR)
		DW	RXBUFFER	;(RXBHEAD)
		DW	RXBUFFER	;(RXBTAIL)
		DW	ISR_DISPATCH	;(INT_VEC)
		DW	LED_DISPLAY	;(SCAN_PTR)
		DW	DO_HALT_TEST	;(HALT_TEST)

		LD	A,0x80		;Advance Column / Clear Counter for Interrupt
		OUT	(Port40),A	; Clear LED Display & Set RS-232 TXD to inactive state
		CALL	DELAY_100mS

		LD	HL,LED_SPLASH_TBL
		LD	A,(RC_TYPE)
		RLCA
		RLCA
		RLCA
		CALL	ADD_HL_A
		CALL	LED_PRINT

		LD	A,(RC_TYPE)
		OR	A
		JR  Z,	LSPLASH_CNT
		LD	HL, RC_TYPE	;Print count of reset type
		CALL	ADD_HL_A
		LD	A,(HL)
		LD	HL,LED_DISPLAY+5
		CALL	LED_PUT_BYTE_HL
LSPLASH_CNT	LD	(HL),0x80	;Annunciator LED's OFF

		JR	SKIP_TABLE1

LED_SPLASH_TBL	DB	"-DELAY-",EOS
		DB	"Soft   ",EOS
		DB	"StEp   ",EOS
		DB	"^C     ",EOS
		DB	"HALt   ",EOS
		DB	"F-0    ",EOS
		DB	"Rst0   ",EOS
		DB	"HARD   ",EOS

SKIP_TABLE1	
		
		CALL	SET_BANK	;Bank 0
		
		CALL	LOAD_HIGH_RAM	;Copy subroutines to HIGH RAM

		CALL	ACE_SET_BAUD	;Set Baud & Word to saved setting (might have been changed from 9600)

		EI			;************** Interrupts ON!!!!

;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_3	Main Loop, RS-232 MONITOR, MENU selection
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

		LD	A,0x80		;
		LD	(SCAN_LED),A
		OUT	(Port40),A	;Reset Timer interrupt

		LD	A,(RC_TYPE)
		;CALL	PRINTI
		;DB	CR,LF,"RT_TYPE=",EOS		
		;CALL	PUT_BYTE
		OR	A
		JR NZ,	NOT_COLD

		LD	C,20		;2 Seconds
		CALL	DELAY_C		;Delay in 100mSEC

		CALL	LED_HOME_PRINTI
		DB	"COLD 00",EOS
		
		LD	HL,RAMSIGNATURE
		LD	A,0xF0		;First signature byte expected
		LD	B,8		;#bytes in signature (loop)
RAMSIGN_LP	LD	(HL),A		;Save Signature
		INC	L
		SUB	0xF
		DJNZ	RAMSIGN_LP
		XOR	A
		
NOT_COLD	RLCA
		LD	HL,RS232_SPLASH
		CALL	ADD_HL_A
		CALL	LD_HL_HL
		CALL	PUT_NEW_LINE		
		CALL	PRINT
		LD	A,(RC_TYPE)
		OR	A
		JR   Z,	SPLASH_VERSION
		LD	HL, RC_TYPE	;Print count of reset type
		CALL	ADD_HL_A
		LD	A,(HL)
		CALL	SPACE_PUT_BYTE
		CALL	REG_DISP_ALL
		JR	SKIP_TABLE2

RS232_SPLASH	DW	R_COLD
		DW	R_SOFT
		DW	R_STEP
		DW	R_CC
		DW	R_HALT
		DW	R_F0
		DW	R_RST0
		DW	R_HARD

R_COLD		DB	"Cold Start",CR,LF,EOS
R_SOFT		DB	"Soft Restart",EOS
R_STEP		DB	"Step",EOS
R_CC		DB	"<Ctrl>-C",EOS
R_HALT		DB	"CPU HALT",EOS
R_F0		DB	"F-0 Reset",EOS
R_RST0		DB	"<Break>",EOS
R_HARD		DB	"Hard Reset",EOS

SPLASH_VERSION	CALL 	PUT_VERSION

SKIP_TABLE2	LD	A,(RC_TYPE)
		CP	2		;If returning from Single Step, restore Monitor Display
		JR NZ,	WB_NOT_STEP
		LD	HL,(SDISPMODE)
		LD	(LDISPMODE),HL
		LD	(DISPMODE),HL
WB_NOT_STEP	


;                 ******       *****      *****    *********
;                 *******     *******    *******   *********
;                 **    **   ***   ***  ***   ***     ***
;                 **    **   **     **  **     **     ***
;                 *******    **     **  **     **     ***
;                 *******    **     **  **     **     ***
;                 **    **   **     **  **     **     ***
;                 **    **   ***   ***  ***   ***     ***
;                 *******     *******    *******      ***
;                 ******       *****      *****       ***




		LD	A,(RC_TYPE)	;Auto boot option for cold start or RESET only
		OR	A
		JR Z,	AUTO_BOOT_MENU
		CP	7
		JR NZ,	MAIN_MENU

	;HW_LIST: 00 NO FP, 01 FP only, 02 SIO only, 03 BOTH SIO & FP

AUTO_BOOT_MENU	LD	A,(HW_LIST)	;Auto boot if no card or SD card
		OR	A
		JR   Z,	AUTO_BOOT_DO
		AND	2
		JR   Z,	MAIN_MENU	;No Option to boot from SD Card
		
AUTO_BOOT_DO	CALL	PRINTI		;Monitor Start, Display Welcome Message
		DB	CR,LF,"Press M for Monitor",EOS
		
		LD	B,30
AUTO_BOOT_LP	CALL	DOT_GETCHAR	;C=1 if dots timed out or <TAB>, or C=0 and A=char
		JR  C,	AUTO_BOOT_GO
		CP	'M'
		JR  Z,	MAIN_MENU	;M? Go Monitor
		JR	AUTO_BOOT_LP
		
AUTO_BOOT_GO	LD	HL,RC_TYPE	;Flag auto boot
		SET	7,(HL)		
		LD	A,(HW_LIST)	;Auto boot to BASIC if no board
		OR	A
		JP   Z,	GO_BASIC
		CALL	WRITE_BLOCK
		DW	FILENAME	;Where to write
		DW	11		;Bytes to write
		DB	'Z80MC_GOHEX'
		CALL	GO_SD_CARD	;Do a CALL, let any returns go to Monitor
		
		
		

;************************************************************************************
;************************************************************************************
;************************************************************************************
;************************************************************************************
;
;
;                 ***        ***    *********    **       **     **       **
;                 ****      ****    *********    ***      **     **       **
;                 ** **    ** **    **           ****     **     **       **
;                 **  **  **  **    **           ** **    **     **       **
;                 **   ****   **    *******      **  **   **     **       **
;                 **    **    **    *******      **   **  **     **       **
;                 **          **    **           **    ** **     **       **
;                 **          **    **           **     ****     ***     ***
;                 **          **    *********    **      ***      ********* 
;                 **          **    *********    **       **        *****  


;Monitor
;Functions:
; -Dump, Edit & Execute Memory.
; -Input Port and Output Port.
; -RAM Test
; -ASCII Upload intel HEX file
; -XMODEM up/down load to Memory
;
; D XXXX YYYY	Dump memory from XXXX to YYYY
; E XXXX	Edit memory starting at XXXX (type an X and press enter to exit entry)
; G XXXX	GO starting at address XXXX (Monitor program address left on stack)
; I XX		Input from I/O port XX and display as hex
; O XX YY	Output to I/O port XX byte YY
; L		Loop back test
; X U XXXX	XMODEM Upload to memory at XXXX (CRC or CHECKSUM)
; X D XXXX CCCC	XMODEM Download from memory at XXXX for CCCC number of 128 byte blocks
; :ssHHLLttDDDDDD...CS   -ASCII UPLOAD Intel HEX file to Memory.  Monitor auto downloads with the reception of a colon.
; R XX YY	RAM TEST from pages XX to YY
; V		Report Version


;----------------------------------------------------------------------------------------------------; MAIN MENU
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;----------------------------------------------------------------------------------------------------; MAIN MENU

MAIN_MENU:	LD	SP, StackTop	; Reset Stack = 0xFF80
		XOR	A
			
		LD	(FILENAME),A	;Nuke any Auto Run
		LD	HL, MAIN_MENU	;Push Mainmenu onto stack as default return address
		PUSH	HL
		
		CALL	PURGE
		
		CALL	PRINTI		;Monitor Start, Display Welcome Message
		DB	CR,LF,"Main Menu >",EOS
		
		LD	A,0xFF
		CALL	SET_ECHO	;Echo on

		CALL 	GET_CHAR	;get char

		CP 	'1'
		JP Z,	SW2_BIT
		CP 	'2'
		JP Z,	SW2_ACE
		CP 	'3'
		JP Z,	SW2_BOTH		

		AND 	0x5F		;to upper case
		CP 	'B'		;
		JP Z, 	GO_BASIC	; B = GO BASIC
		CP 	'C'		;
		JP Z, 	MEM_DUMP	; C = Memory Dump (Continuous)
		CP 	'D'		;
		JP Z, 	MEM_DUMP_PAGED	; D = Memory Dump
		CP 	'E'
		JP Z, 	MEM_EDIT	; E = Edit Memory
		CP 	'G'
		JP Z, 	MEM_EXEC	; G = Go (Execute at)
		CP	'H'
		JP Z,	GETHEXFILE	; H = START HEX FILE LOAD
		CP 	'I'
		JP Z, 	PORT_INP	; I = Input from Port
		CP 	'L'
		JP Z,	LOOP_BACK_TEST	; L = Loop Back Test
		CP 	'M'
		JP Z,	MEM_ENTER	; M = ENTER INTO MEMORY
		CP 	'O'
		JP Z, 	PORT_OUT	; O = Output to port
		CP 	'P'
		JP Z,	PORT_SPEED	; P = Port Speed for ACE
		CP 	'R'
		JP Z,	REG_MENU	; R = REGISTER OPERATIONS
		CP 	'S'		;
		JP Z, 	GO_SD_CARD	; S = BOOT SD CARD
		CP 	'T'
		JP Z,	RAM_TEST	; T = RAM TEST
		CP 	'U'
		JP Z,	USE_RAM		; U = USE RAM/ROM
		CP 	'V'
		JP Z,	PUT_VERSION	; V = Version
		CP 	'W'
		JP Z, 	GO_SINGLE	; W = Single Step
		CP 	'X'
		JP Z, 	XMODEM		; X = XMODEM
		CP 	'Y'
		JP Z,	WHICH_PORT	; Y = Display Which port

PRINT_MENU	CALL 	PRINTI		;Display Help when input is invalid
		DB	CR,LF,"HELP"		
		DB	CR,LF,"D XXXX YYYY    Dump memory from XXXX to YYYY"
		DB	CR,LF,"C XXXX YYYY    Continous Dump (no pause)"
		DB	CR,LF,"E XXXX         Edit memory starting at XXXX"
		DB	CR,LF,"M XXXX YY..YY  Enter many bytes into memory at XXXX"
		DB	CR,LF,"G [XXXX]       GO (PC Optional)"
		DB	CR,LF,"W              Single Step"
		DB	CR,LF,"I XX           Input from I/O"
		DB	CR,LF,"O XX YY        Output to I/O"
		DB	CR,LF,"R rr [=xx]     Register"
		DB	CR,LF,"L              Loop back test"
		DB	CR,LF,"T XX YY        RAM TEST from pages XX to YY"
		DB	CR,LF,"V              Version"
		DB	CR,LF,"H              UPLOAD Intel HEX file"
		DB	CR,LF,"X U XXXX       XMODEM Upload to memory at XXXX"
		DB	CR,LF,"X D XXXX CCCC  XMODEM Download from XXXX for CCCC #of 128 byte blocks"
		DB	CR,LF,"P              Port Speed (ACE Only)"
		DB	CR,LF,"1,2,3          Switch Console Input"
		DB	CR,LF,"Y              Identify Port"
		DB	CR,LF,"U              Use ROM or RAM BANK"
		DB	CR,LF,"S              SD CARD BOOT"
		DB	CR,LF,"B              BASIC - DAVE DUNFIELD"
		DB	CR,LF,EOS
		RET


;1,2,3          Switch Console Input"

;A
;B BASIC
;C Continous Dump (no pause)
;D Dump memory
;E Edit memory
;F
;G GO (PC Optional)
;H UPLOAD Intel HEX file
;I Input from I/O
;J
;K
;L Loop back test
;M Enter many bytes
;N
;O Output to I/O
;P Port Speed
;Q
;R Register
;S SD CARD BOOT
;T RAM TEST
;U Use ROM or RAM BANK
;V Version
;W Single Step
;X XMODEM
;Y Identify Port
;Z



SW2_BIT		LD	C,1
		CALL	SET_IO
		RET  C
		JR	PUT_IOMSG

SW2_ACE		LD	C,2
		CALL	SET_IO
		RET  C
		JR	PUT_IOMSG

SW2_BOTH	LD	C,3
		CALL	SET_IO
		RET  C

PUT_IOMSG	CALL	PUT_NEW_LINE
		LD	A,(HW_SETIO)
		DEC	A
		RLCA
		RLCA
		LD	HL,IO_MSG
		CALL	ADD_HL_A
		CALL	PRINT
		RET

IO_MSG		DB	"BIT",EOS
		DB	"ACE",EOS
		DB	"BOTH",EOS


WHICH_PORT	CALL	LED_HOME_PRINTI		
		DB	"LED ",EOS
		LD	A,(HW_SETIO)
		LD	B,A
		LD	C,1
		CALL	SET_IO
		JR C,	WP_NOBIT
		CALL	PRINTI		
		DB	CR,LF,"BIT",EOS
WP_NOBIT	LD	C,2
		CALL	SET_IO
		JR C,	WP_NOACE
		CALL	PRINTI		
		DB	CR,LF,"ACE",EOS		
WP_NOACE	LD	C,B
		CALL	SET_IO
		RET

PORT_SPEED	CALL	PRINTI		
		DB	CR,LF,"(0C=9600) BAUD:",EOS
		LD	A,(ACE_BAUD)
		CALL	PUT_BYTE
		CALL	SPACE_GET_BYTE
		RET  C
		LD	(ACE_BAUD),A
		CALL	ACE_SET_BAUD
		RET

USE_RAM		CALL	PRINTI		
		DB	CR,LF,"Currently using:",EOS
		CALL	DISP_RRBANK
		CALL	PRINTI		
		DB	CR,LF,"Enter 0-F,R ",EOS
		CALL	GET_HEX
		JR   C,	UR_NOTRAM
		RLCA
		INC	A
		JR	UR_RET
UR_NOTRAM	AND	0x5F		;To upper case
		SUB	'R'		;If ='R', Use ROM (A=0)
		JR  NZ,	UR_RET1		
UR_RET		LD	(READ_RAMROM),A
UR_RET1		CALL	DISP_RRBANK
SET_BANK	LD	A,(READ_RAMROM)
		RRA
		AND	0xF
		OUT	(ACE_OUT),A	;SET Bank
		RET
				
DISP_RRBANK	LD	A,(READ_RAMROM)
		RRA
		JR  NC,	DRR_ROM
		CALL	PRINTI
		DB	" RAM BANK:",EOS		
		CALL	PUT_HEX
		RET
DRR_ROM		CALL	PRINTI
		DB	" ROM",EOS
		RET
		



		

;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_4	Menu operations
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;=============================================================================
;Display Version
;-----------------------------------------------------------------------------
PUT_VERSION	CALL	PRINTI
	DB	CR,LF,"Z80 MEMBERSHIP CARD MICRO-SD, v1.5beta July 23, 2017",CR,LF,EOS
;		VERSION_MSG
		CALL	PRINTI		
		DB	"Hardware: ",EOS	
		LD	A,(HW_LIST)	;00 NO FP
		CALL	PUT_BYTE	;01 FP only
		OR	A		;02 SIO only
		JR NZ,	PVH_0		;03 FP & SIO
		CALL	PRINTI		
		DB	" None",EOS
		RET
		
PVH_0		RRCA
		LD	C,' '
		JR NC,	PVH_1
		LD	C,','
		CALL	PRINTI		
		DB	" FP",EOS
PVH_1		RRCA
		RET NC
		LD	A,C
		CALL	PUT_CHAR
		CALL	PRINTI		
		DB	" SIO",EOS
		RET			
					
				

;=============================================================================
;Register Display/Set
;-----------------------------------------------------------------------------
REG_MENU	CALL	PUT_SPACE
		CALL	GET_CHAR
		CP	CR
		JR  NZ,	RM_NOTALL

;12345678901234567890123456789012345678901234567890123456789012345678901234567890  80 COLUMNS
;AF=xxxx  BC=xxxx  DE=xxxx  HL=xxxx  AF'=xxxx  BC'=xxxx  DE'=xxxx  HL'=xxxx
;IX=xxxx  IY=xxxx  IR=xxxx  PC=xxxx  SP=xxxx

REG_DISP_ALL				;Dump ALL registers
		LD	B,13		;13 Registers to dump
RM_LP		LD	HL,REGORDER
		LD	A,B
		DEC	A
		CALL	ADD_HL_A
		LD	A,(HL)
		OR	A
		CALL M,	PUT_NEW_LINE
		AND	0xF
		CALL	GET_REGNAME
		CALL	PRINT
		CALL	RM_DUMP_REG
		CALL	PRINTI
		DB	'  ',EOS
		DJNZ	RM_LP
		RET

REGORDER	DB	0		;Registers to dump (Numbers shifted left)
		DB	5		;LSB will indicate a NEW LINE
		DB	8
		DB	7
		DB	6 + 0x80
		DB	12
		DB	11
		DB	10
		DB	9 + 0x80
		DB	4
		DB	3
		DB	2
		DB	1 + 0x80	;First Register to Dump

RM_NOTALL	CALL	TO_UPPER
		CALL	IS_LETTER	;A contains first letter of register
		JR  C,	RM_ERR		;abort if not a letter
		LD	E,A		;E=first letter of register
		CALL	GET_CHAR	;input 2nd letter of register
		CALL	TO_UPPER
		CALL	IS_LETTER	;
		JR  C,	RM_ERR		;Abort if 2nd char is not a letter
		LD	D,A		;D=2nd letter

		LD	L,0
RM_2		CALL	GET_CHAR	;Get the 3rd char command
		CP	0x27		;Apostrophe Char
		JR  NZ,	RM_3		;if 3rd char is apostrophe, 
		LD	L,1		;L=1 if Alternate Register
		JR	RM_2		;and loop back for a 4th char command
		
RM_3		RR	L		;Put Alternate flag into CARRY
		PUSH	AF		;Save last key input before proceeding to Search for Register

		LD	B,13		;Loop through all 13 registers
RM_4		LD	A,B
		DEC	A		;adjust to base 0
		CALL	GET_REGNAME	;HL=PTR TO NAME
		CALL	LD_HL_HL	;HL = ASCII name of Register
		OR	A		;CLEAR CARRY
		SBC	HL,DE		;Test if DE=HL
		JR  Z,	RM_5		;Jump if NAME FOUND
		DJNZ	RM_4

		POP	AF
RM_ERR		LD	A,'?'		;Register Name not found
		CALL	PUT_CHAR
		RET
		
RM_5		LD	C,B		;C=Register Ptr
		DEC	C
		POP	AF		;Restore saved command (and alternate register selection)
		LD	D,A		;D=Command
		JR  C,	RM_6		;Jump if Alternate (Selection would be correct)
		LD	A,C
		CP	9
		JR  C,	RM_6		;Jump if NOT Registers AF,BC,DE or HL
		SUB	8		;Subtract 8 to convert AF' to AF, etc
		LD	C,A
RM_6		LD	A,D		;RESUME Decoding command line
		CP	CR		;if CR, dump Register
		JR  Z,	RM_DUMP_REG_C
		CP	'='		;if '=', then Assign new value to Register
		JR  NZ,	RM_ERR

		CALL	GET_WORD	;DE = Word from Command
		LD	A,C
		CALL	PUT_REGISTER

RM_DUMP_REG_C	LD	A,C
RM_DUMP_REG	CALL	PRINTI
		DB	'=',0
		CALL	GET_REGISTER
		CALL	PUT_HL
		RET


;=============================================================================
;Loop back test
;-----------------------------------------------------------------------------
LOOP_BACK_TEST	LD	A,0		;Turn off LED update & enable key entry
		CALL	LED_UPDATE

		CALL	PRINTI
		DB	CR,LF
		DB	'LOOP BACK, <Esc>-EXIT',CR,LF,0
		CALL	LED_HOME_PRINTI
		DB	'LOOPBAC',0
		CALL	LED_HOME
		
		
LOOP_BACK_LP	CALL	IN_CHAR	;Test for any RS-232 input
		JR   C,	LB_0	;Jump if no input
		CP	27	;<Esc> to quit
		JR   Z,	LB_RET
		JR	LB_OUT	;Display
		
LB_0		CALL 	IC_KEY		;Test regular HEX input
		JP   Z,	LOOP_BACK_LP	;

LB_1		BIT	4,A
		JR   Z,	LB_2	;Jump if NOT shifted
		PUSH	AF		;If Shifted, then output a carret before the key
		LD	A,'^'
		CALL	Put_Char
		POP	AF

LB_2		CALL	HEX2ASC	;Convert Keypad input to ASCII

LB_OUT		CALL	PUT_CHAR
		LD	C,A
		CALL	PC_LED
		JR	LOOP_BACK_LP

LB_RET		LD	A,1
		CALL	LED_UPDATE
		RET


;=============================================================================
;MEMORY ENTER.  M XXXX YY..YY,  ENTERS AS MANY BYTES AS THERE ARE ON THE LINE.
;-----------------------------------------------------------------------------
MEM_ENTER	LD	BC,0		;Clear count
MEM_ENTER_NEXTL	CALL	GET_WORD	;DE = Word from console, A=non-hex character following word (space)
		CP	' '	;Test delimiting character, must be a space
		RET 	NZ		;Main Menu
		EX	DE,HL		;HL = Start
MEN_LP		CALL	GET_BYTE	;A = Byte or A=non-hex character (Carry Set)
		JR C,	MEN_CHK		;Jump if non-hex input
		LD	(HL),A		;else, save the byte
		INC	HL		;advance memory pointer
		INC	BC		;count bytes
		JR	MEN_LP		;repeat for next byte input

MEN_RET		CALL	GET_CHAR_NE	;ignore rest of line before returning to main menu
		CALL	PUT_CHAR
MEN_CHK		CP	0x0D		;wait until we get the <CR>
		JR Z,	MEN_1
		CP	0x0A		;wait until we get the <LF>
		JR NZ,	MEN_RET
MEN_1		LD	A,4		;Wait up to 2 seconds for another M command or return to main menu
		CALL	TIMED_GETCHAR	;
		CALL	PUT_CHAR
		CP	'M'		;If another M command comes in, process it
		JR Z,	MEM_ENTER_NEXTL
		CP	0x0A		;Skip LF
		JR Z,	MEN_1
		CALL	PRINTI
		DB	CR,LF,"BYTES ENTERED=",EOS
		CALL	PUT_BC
		RET			;If not, return to main menu prompt


;=============================================================================
;MEMORY DUMP - Continous
;-----------------------------------------------------------------------------
MEM_DUMP:	LD	B,0xFF		;Continuous Dump, No pausing
MEM_DUMP_0	CALL	SPACE_GET_WORD	;Input start address
		EX	DE,HL			;HL = Start
		CALL	SPACE_GET_WORD	;Input end address (DE = end)

MEM_DUMP_LP:	CALL	PUT_NEW_LINE
		CALL	DUMP_LINE	;Dump 16 byte lines (advances HL)
		RET Z			;RETURN WHEN HL=DE
		CALL	IN_CHAR		;Exit on <Esc>
		CP	27
		RET	Z
		LD	A,L
		OR	B
		JR  NZ,	MEM_DUMP_LP	;Dump 1 Page, then prompt for continue
		CALL	GET_CONTINUE
		JR	MEM_DUMP_LP
;=============================================================================
;MEMORY DUMP - Paged
;-----------------------------------------------------------------------------
MEM_DUMP_PAGED	LD	B,0		;Paused Dump
		JR	MEM_DUMP_0

;-----------------------------------------------------------------------------
GET_CONTINUE	CALL	PUT_NEW_LINE
		CALL	PRINTI
		DB	"Press any key to continue",EOS
		CALL	GET_CHAR
		CP	27
		RET NZ
		POP	HL		;Scrap return address
		RET


;-----------------------------------------------------------------------------
;DUMP_LINE -- Dumps a line
;xxx0:  <pre spaces> XX XX XX XX XX After spaces | ....ASCII....
;-----------------------------------------------------------------------------
DUMP_LINE:	PUSH	BC		;+1
		PUSH	HL		;+2 Save H for 2nd part of display
		PUSH	HL		;+3 Start line with xxx0 address
		LD	A,'M'
		CALL	Put_Char
		CALL	PUT_HL		;Print Address
		CALL	PUT_SPACE
		POP	HL		;-3
		LD	A,L
		AND	0x0F		;Fetch how many prespaces to print
		LD	C,A
		LD	B,A		;Save count of prespaces for part 2 of display
		CALL	PUT_3C_SPACES

DL_P1L		CALL	GET_MEM
		CALL	SPACE_PUT_BYTE
		CALL	CP_HL_DE
		JR Z,	DL_P1E
		INC	HL
		LD	A,L
		AND	0x0F
		JR  NZ,	DL_P1L
		JR	DL_P2

DL_P1E		LD	A,L
		CPL
		AND	0x0F
		LD	C,A
		CALL	PUT_3C_SPACES

DL_P2		CALL	PRINTI		;Print Seperator between part 1 and part 2
		DB	" ; ",EOS

DL_PSL2		LD	A,B		;Print prespaces for part 2
		OR	A
		JR Z,	DL_PSE2
		CALL	PUT_SPACE
		DEC	B
		JR	DL_PSL2
DL_PSE2
		POP	HL		;-2
		POP	BC		;-1
DL_P2L		CALL	GET_MEM
		CP	' '		;A - 20h	Test for Valid ASCII characters
		JR NC,	DL_P2K1
		LD	A,'.'				;Replace with . if not ASCII
DL_P2K1		CP	0x7F		;A - 07Fh
		JR C,	DL_P2K2
		LD	A,'.'
DL_P2K2		CALL	Put_Char

		CALL	CP_HL_DE
		RET Z
		INC	HL
		LD	A,L
		AND	0x0F
		JR  NZ,	DL_P2L

;-----------------------------------------------------------------------------
;Compare HL with DE
;Exit:		Z=1 if HL=DE
;		M=1 if DE > HL
CP_HL_DE	LD	A,H
		CP	D		;H-D
		RET NZ			;M flag set if D > H
		LD	A,L
		CP	E		;L-E
		RET


PUT_3C_SPACES	INC	C		;Print 3C Spaces
PUT_3C_SPACES_L	DEC	C		;Count down Prespaces
		RET Z
		CALL	PRINTI		;Print pre spaces
		DB "   ",EOS
		JR	PUT_3C_SPACES_L


;-----------------------------------------------------------------------------
;EDIT MEMORY
;Edit memory from a starting address until X is pressed.
;Display mem loc, contents, and results of write.
;-----------------------------------------------------------------------------
MEM_EDIT:	CALL	SPACE_GET_WORD	;Input Address
		EX	DE,HL			;HL = Address to edit
ME_LP		CALL	PUT_NEW_LINE
		CALL	PUT_HL		;Print current contents of memory
		CALL	PUT_SPACE
		LD	A, ':'
		CALL	Put_Char
		CALL	GET_MEM
		CALL	SPACE_PUT_BYTE
		CALL	SPACE_GET_BYTE	;Input new value or Exit if invalid
		RET C			;Exit to Command Loop
		LD	(HL), A		;or Save new value
		CALL	GET_MEM
		CALL	SPACE_PUT_BYTE
		INC	HL		;Advance to next location
		JR	ME_LP		;repeat input


;=============================================================================
;	MEM_EXEC - Execute at
;	Get an address and jump to it
;-----------------------------------------------------------------------------
MEM_EXEC:	CALL	SPACE_GET_WORD	;Input address
		JP NC,	ME_1		;Jump if no hex input
		CP	27
		RET Z			;Exit if <ESC> pressed
		LD	(RPC),DE

		CALL	PRINTI
		DB	' PC=',EOS
		CALL	PUT_DE
		JR	GO_EXEC_T

ME_1		CP	13		;No hex input (user just typed G and something not HEX)
		RET NZ			;Exit if NOT <CR> pressed
		
GO_EXEC_T	LD	HL,(RSSP)	;20 Fetch SP
		DEC	HL
		DEC	HL
		LD	A,0xAA
		LD	(HL),A
		CP	(HL)
		JR NZ,	GE_STACKFAIL
		CPL
		LD	(HL),A
		CP	(HL)
		JR NZ,	GE_STACKFAIL

		DI			;
GO_EXEC		LD	A,0x82		;7  (ANBAR_DEF) = RUN MODE
		LD	(ANBAR_DEF),A	;13
		LD	HL,GET_REG_RUN	;10
		LD	(GET_REG),HL	;16
		LD	HL,PUT_REG_RUN	;10
		LD	(PUT_REG),HL	;16
		LD	HL,CTRL_C_TEST	;10
		LD	(CTRL_C_CHK),HL	;16
		
		LD	A,(ANBAR_DEF)	;13 Refresh Display
		LD	(LED_ANBAR),A	;13 
					;\\\ 124

		EX	AF,AF'		;4  Fetch Alternate register set
		LD	SP,RSAF2	;10 Set Stack to get register AF'
		POP	AF		;10
		EX	AF,AF'		;4
		EXX			;4  Fetch Alternate register set
		LD	BC,(RSBC2)	;20
		LD	DE,(RSDE2)	;20
		LD	HL,(RSHL2)	;20
		EXX			;4

		LD	BC,(RSIR)	;20 Fetch IR
		LD	A,C		;4
		LD	R,A		;9
		LD	A,B		;4
		LD	I,A		;9 
					;\\\ 142
					
		LD	A,0xC3		;7  Set Jump instruction
		LD	(HR_EXE_GO),A	;13
		LD	HL,(RPC)	;16 Fetch PC
		LD	(HR_EXE_GO+1),HL;16

		LD	SP,RSAF		;10 Set Stack to Fetch register AF
		POP	AF		;10

		LD	SP,(RSSP)	;20 Fetch SP
		INC	SP		;6
		INC	SP		;6
		LD	HL,0		;10 Set Default return address as 0000 (Restart/Save Registers)
		PUSH	HL		;11   & Put on stack
					;\\\ 125
		
		LD	BC,(RSBC)	;20
		LD	DE,(RSDE)	;20
		LD	HL,(RSHL)	;20
		LD	IX,(RSIX)	;20
		LD	IY,(RSIY)	;20

		EI			;4
		JP	HR_EXE_GO	;10 PC=(STACK)   
		
;HR_EXE_GO	JP	(RSPC)		;10 Final jump to code
					;\\\ 124
					;Total = 515

GE_STACKFAIL	CALL	PRINTI
		DB	' STACK NOT IN RAM',EOS
		CALL	LED_HOME_PRINTI
		DB	'SP ERR ',EOS
		RET
		

;===============================================
;Input from port, print contents
PORT_INP:	CALL	SPACE_GET_BYTE
		LD	C, A
		IN	A,(C)
		CALL	SPACE_PUT_BYTE
		RET

;Get a port address, write byte out
PORT_OUT:	CALL	SPACE_GET_BYTE
		LD	C, A
		CALL	SPACE_GET_BYTE
		OUT	(C),A
		RET



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_5	Supporting routines. GET_BYTE, GET_WORD, PUT_BYTE, PUT_WORD
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;


CLEAR_BLOCK	LD	(HL),0
		INC	HL
		DJNZ	CLEAR_BLOCK
		RET

;-----------------------------------------------------------------------------------------------------
;	FILL_BLOCK, Fills a block of RAM with value in A
;	Input:	A = value
;		HL = Start Address
;		B = Length of Fill (MAX = 0 = 256 bytes)
;-----------------------------------------------------------------------------------------------------
FILL_BLOCK	PUSH	HL
FB_LP		LD	(HL),A
		INC	HL
		DJNZ	FB_LP
		POP	HL
		RET
					;Critical Timing in effect for GO_EXEC
WRITE_BLOCK	EX	(SP),HL		;19 HL=PC Total = 137 + 21 * BC
		PUSH	DE		;11
		PUSH	BC		;11
		LD	E,(HL)		;7
		INC	HL		;6
		LD	D,(HL)		;7
		INC	HL		;6
		LD	C,(HL)		;7
		INC	HL		;6
		LD	B,(HL)		;7
		INC	HL		;6
		LDIR			;21/16  21*BC-5
		POP	BC		;10
		POP	DE		;10
		EX	(SP),HL		;19 PC=HL
		RET			;10
		
GET_REGNAME	PUSH	AF
		RLCA
		RLCA
		LD	HL,REGNAMES
		CALL	ADD_HL_A
		POP	AF
		RET

REGNAMES	DB	'SP ',0		;0
		DB	'AF ',0		;1
		DB	'BC ',0		;2
		DB	'DE ',0		;3
		DB	'HL ',0		;4
		DB	'PC ',0		;5
		DB	'IX ',0		;6
		DB	'IY ',0		;7
		DB	'IR ',0		;8
		DB	'AF',0x27,0	;9
		DB	'BC',0x27,0	;10
		DB	'DE',0x27,0	;11
		DB	'HL',0x27,0	;12

GET_REGISTER	LD	HL,(GET_REG)	;16
		JP	(HL)		;4

GET_REG_MON	LD	HL,RSSP		;
		RLCA
		CALL	ADD_HL_A	;17+18 HL=Where to find Register
		CALL	LD_HL_HL	;HL=(HL)
		RET

GET_REG_RUN	LD	HL,GRR_TBL	;10
		JP	SHORTNWAY	;10

PUT_REGISTER	LD	HL,(PUT_REG)
		JP	(HL)

PUT_REG_MON	LD	HL,RSSP
		RLCA
		CALL	ADD_HL_A	;HL=Where to find Register
PURRS_RET	LD	(HL),E
		INC	HL
		LD	(HL),D
		RET

PUT_REG_RUN	LD	HL,PURR_TBL
					;40 to get here
SHORTNWAY	AND	0xF		;7
		CALL	ADD_HL_A	;14+18
		LD	A,(HL)		;7
		LD	HL,GRR_SUB	;10
		CALL	ADD_HL_A	;17+18
VCALL_HL	JP	(HL)		;4  st=138

GRR_TBL		DB	0
		DB	GRR_SUB_AF - GRR_SUB
		DB	GRR_SUB_BC - GRR_SUB
		DB	GRR_SUB_DE - GRR_SUB
		DB	GRR_SUB_HL - GRR_SUB
		DB	GRR_SUB_PC - GRR_SUB
		DB	GRR_SUB_IX - GRR_SUB
		DB	GRR_SUB_IY - GRR_SUB
		DB	GRR_SUB_IR - GRR_SUB
		DB	GRR_SUB_AFA - GRR_SUB
		DB	GRR_SUB_BCA - GRR_SUB
		DB	GRR_SUB_DEA - GRR_SUB
		DB	GRR_SUB_HLA - GRR_SUB
		DB	0
		DB	0
		DB	0

PURR_TBL	DB	PURR_SUB_SP - GRR_SUB
		DB	PURR_SUB_AF - GRR_SUB
		DB	PURR_SUB_BC - GRR_SUB
		DB	PURR_SUB_DE - GRR_SUB
		DB	PURR_SUB_HL - GRR_SUB
		DB	PURR_SUB_PC - GRR_SUB
		DB	PURR_SUB_IX - GRR_SUB
		DB	PURR_SUB_IY - GRR_SUB
		DB	PURR_SUB_IR - GRR_SUB
		DB	PURR_SUB_AFA - GRR_SUB
		DB	PURR_SUB_BCA - GRR_SUB
		DB	PURR_SUB_DEA - GRR_SUB
		DB	PURR_SUB_HLA - GRR_SUB
		DB	0
		DB	0
		DB	0

		;Stack holds:
		;SP	RETURN TO ISR	(CALL PUT_REG)
		;SP+2	RETURN TO VECTOR DISPATCH (CALL VCALL_HL)
		;SP+4	AF
		;SP+6	HL
		;
		;PREVIOUS STACK SAVED AT SP_ISR_SAVE
		;	RETURN TO MAIN CODE (PC)
		;
		
GRR_SUB		LD	HL,(SP_ISR_SAVE)	;Get SP;True value of SP (prior to ISR)
		INC	HL
		INC	HL
		RET
GRR_SUB_AF	LD	HL,4		;Get AF
		ADD	HL,SP
		CALL	LD_HL_HL	;HL=(HL)
		RET
GRR_SUB_BC	PUSH	BC
		POP	HL
		RET
GRR_SUB_DE	PUSH	DE
		POP	HL
		RET
GRR_SUB_HL	LD	HL,6		;Get HL
		ADD	HL,SP
		CALL	LD_HL_HL	;HL=(HL)
		RET
GRR_SUB_PC	LD	HL,(SP_ISR_SAVE) ;Get PC
		CALL	LD_HL_HL	;HL=(HL)
		RET
GRR_SUB_IX	PUSH	IX
		POP	HL
		RET
GRR_SUB_IY	PUSH	IY
		POP	HL
		RET
GRR_SUB_IR	LD	A,I
		LD	H,A
		LD	A,R
		LD	L,A
		RET
GRR_SUB_AFA	EX	AF,AF'
		PUSH	AF
		EX	AF,AF'
		POP	HL
		RET
GRR_SUB_BCA	EXX
		PUSH	BC
		EXX
		POP	HL
		RET
GRR_SUB_DEA	EXX
		PUSH	DE
		EXX
		POP	HL
		RET
GRR_SUB_HLA	EXX
		PUSH	HL
		EXX
		POP	HL
		RET


		;SP	RETURN TO ISR
		;SP+2	DE
		;SP+4	RETURN TO VECTOR DISPATCH (CALL VCALL_HL)
		;SP+6	RRSTATE
		;SP+8	AF
		;SP+10	HL
		;PREVIOUS STACK SAVED AT SP_ISR_SAVE
		;	RETURN TO MAIN CODE (PC)

PURR_SUB_SP	RET		;Do we really want to change the SP during RUN mode??? Suicide!
PURR_SUB_AF	LD	HL,8		;Get DE
		ADD	HL,SP
		JP	PURRS_RET
PURR_SUB_BC	PUSH	DE
		POP	BC
		RET
PURR_SUB_DE	LD	HL,2		;10 Get DE
		ADD	HL,SP		;11
		JP	PURRS_RET	;10  st=31
PURR_SUB_HL	LD	HL,10		;Get HL
		ADD	HL,SP
		JP	PURRS_RET
PURR_SUB_PC	LD	HL,(SP_ISR_SAVE) ;Get PC
		JP	PURRS_RET
PURR_SUB_IX	PUSH	DE
		POP	IX
		RET
PURR_SUB_IY	PUSH	DE
		POP	IY
		RET
PURR_SUB_IR	LD	A,D
		LD	I,A
		LD	A,E
		LD	R,A
		RET
PURR_SUB_AFA	PUSH	DE
		EX	AF,AF'
		POP	AF
		EX	AF,AF'
		RET
PURR_SUB_BCA	PUSH	DE
		EXX
		POP	BC
		EXX
		RET
PURR_SUB_DEA	PUSH	DE
		EXX
		POP	DE
		EXX
		RET
PURR_SUB_HLA	PUSH	DE
		EXX
		POP	HL
		EXX
		RET


;=============================================================================
SPACE_GET_BYTE	CALL	PUT_SPACE

;=============================================================================
;GET_BYTE -- Get byte from console as hex
;
;in:	Nothing
;out:	A = Byte (if CY=0)  (last 2 hex characters)  Exit if Space Entered
;	A = non-hex char input (if CY=1)
;-----------------------------------------------------------------------------
GET_BYTE:	CALL	GET_HEX	;Get 1st HEX CHAR
		JR  NC,	GB_1
		CP	' '		;Exit if not HEX CHAR (ignoring SPACE)
		JR Z,	GET_BYTE	;Loop back if first char is a SPACE
		SCF			;Set Carry
		RET			;or EXIT with delimiting char
GB_1		PUSH	DE		;Process 1st HEX CHAR
		RLCA
		RLCA
		RLCA
		RLCA
		AND	0xF0
		LD	D,A
		CALL	GET_HEX
		JR  NC,	GB_2		;If 2nd char is HEX CHAR
		CP	' '
		JR Z,	GB_RET1
		SCF			;Set Carry
		POP	DE
		RET			;or EXIT with delimiting char
GB_2		OR	D
		POP	DE
		RET
GB_RET1		LD	A,D
		RRCA
		RRCA
		RRCA
		RRCA
GB_RET		OR	A
		POP	DE
		RET


;=============================================================================
SPACE_GET_WORD	CALL	PUT_SPACE

;=============================================================================
;GET_WORD -- Get word from console as hex (ignores initial spaces)
;
;in:	Nothing
;out:	c=1	A = non-hex char input (Typically Space, CR or ESC)
;		DE = Word
;out:	c=0	A = non-hex char input (No Word in DE)
;-----------------------------------------------------------------------------
GET_WORD:	LD	DE,0
		CALL	GET_HEX	;Get 1st HEX CHAR ;out:	A = Value of HEX Char when CY=0
							;	A = Received (non-hex) char when CY=1
		JR  NC,	GW_LP
		CP	' '		;Exit if not HEX CHAR (ignoring SPACE)
		JR Z,	GET_WORD	;Loop back if first char is a SPACE
		OR	A		;Clear Carry
		RET			;or EXIT with delimiting char
GW_LP		LD	E,A		;Save first/combined char in E
		CALL	GET_HEX	;Get next char
		RET C			;EXIT when a delimiting char is entered
		EX	DE,HL		;Else, shift new HEX Char Value into DE
		ADD	HL,HL		;Shift DE up 1 nibble
		ADD	HL,HL
		ADD	HL,HL
		ADD	HL,HL
		EX	DE,HL
		OR	E		;Combine new char with E
		JR	GW_LP



;===============================================
;Get HEX CHAR
;in:	Nothing
;out:	A = Value of HEX Char when CY=0
;	A = Received (non-hex) char when CY=1
;-----------------------------------------------
GET_HEX:	CALL	GET_CHAR
		
;in:	A = CHAR
;out:	A = Value of HEX Char when CY=0
;	A = Received (non-hex) char when CY=1
ASC2HEX		CP	'0'
		JP M,	GHC_NOT_RET
		CP	'9'+1
		JP M,	GHC_NRET
		CP	'A'
		JP M,	GHC_NOT_RET
		CP	'F'+1
		JP M,	GHC_ARET
		CP	'a'
		JP M,	GHC_NOT_RET
		CP	'f'+1
		JP M,	GHC_ARET
GHC_NOT_RET	SCF
		RET
GHC_ARET	SUB	07h
GHC_NRET	AND	0Fh
		RET
		

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;PRINT -- Print A null-terminated string @(HL)
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
PRINT:		PUSH	AF
PRINT_LP	LD	A, (HL)
		INC	HL
		OR	A
		JR Z,	PRINT_RET
		CALL	Put_Char
		JR	PRINT_LP
PRINT_RET	POP	AF
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;PRINT IMMEDIATE
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
PRINTI:		EX	(SP),HL	;HL = Top of Stack
		CALL	PRINT
		EX	(SP),HL	;Move updated return address back to stack
		RET

;===============================================
;PRINT B-LENGTH
;-----------------------------------------------
PRINTB:		LD	A, (HL)
		CALL	PUT_CHAR
		INC	HL
		DJNZ	PRINTB
		RET

;===============================================
;PUT_BC Prints BC Word
;-----------------------------------------------
PUT_BC:		LD	A, B
		CALL	PUT_BYTE
		LD	A, C
		CALL	PUT_BYTE
		RET

;===============================================
;PUT_DE Prints DE Word
;-----------------------------------------------
PUT_DE:		LD	A, D
		CALL	PUT_BYTE
		LD	A, E
		CALL	PUT_BYTE
		RET

;===============================================
;PUT_HL Prints HL Word
;-----------------------------------------------
PUT_HL:		LD	A, H
		CALL	PUT_BYTE
		LD	A, L
		CALL	PUT_BYTE
		RET


;===============================================
;SPACE_PUT_BYTE -- Output (SPACE) & byte to console as hex
;
;pre:	A register contains byte to be output
;post:	Destroys A
;-----------------------------------------------
SPACE_PUT_BYTE	CALL	PUT_SPACE
		
;===============================================
;PUT_BYTE -- Output byte to console as hex
;
;pre:	A register contains byte to be output
;-----------------------------------------------
PUT_BYTE:	PUSH	AF
		PUSH	AF
		RRCA
		RRCA
		RRCA
		RRCA
		AND	0x0F
		CALL	PUT_HEX
		POP	AF
		AND	0x0F
		CALL	PUT_HEX
		POP	AF
		RET

;===============================================
;PUT_HEX -- Convert nibble to ASCII char
;-----------------------------------------------
PUT_HEX:	CALL	HEX2ASC
		JP	Put_Char

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;HEX2ASC - Convert nibble to ASCII char
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
HEX2ASC:	AND	0xF
		ADD	A,0x30
		CP	0x3A
		RET C
		ADD	A,0x7
		RET


;===============================================
;PUT_SPACE -- Print a space to the console
;
;pre: none
;post: 0x20 printed to console
;-----------------------------------------------
PUT_SPACE:	CALL	PRINTI
		DB	' ',EOS
		RET

;===============================================
;PUT_NEW_LINE -- Start a new line on the console
;
;pre: none
;post: 
;-----------------------------------------------
PUT_NEW_LINE:	CALL	PRINTI
		DB	CR,LF,EOS
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Terminal Increment byte at (HL).  Do not pass 0xFF
TINC:		INC	(HL)
		RET	NZ
		DEC	(HL)
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
DELAY_100mS	LD	C,1
DELAY_C		PUSH	BC
		LD	B,0
DELAY_LP	PUSH	BC
		DJNZ	$		;13   * 256 / 4 = 832uSec
		POP	BC
		DJNZ	DELAY_LP	;~100mSEC
		DEC	C
		JR  NZ,	DELAY_LP	;*4 ~= 7mSec
		POP	BC
		RET

;============================================================================
;	Subroutine	Delay_A
;
;	Entry:	A = Millisecond count
;============================================================================
DELAY_A:	PUSH	HL			; Save count
		LD	HL,TicCounter
		ADD	A,(HL)			; A = cycle count
DlyLp		CP	(HL)			; Wait required TicCounter times
		JP	NZ,DlyLp		;  loop if not done
		POP	HL
		RET


;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
ADD_HL_A	ADD	A,L		;4
		LD	L,A		;4
		RET NC			;10
		INC	H
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
LD_HL_HL	LD      A,(HL)		;7
		INC     HL		;6
		LD      H,(HL)		;7
		LD      L,A		;4
		RET			;10




;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_6	Menu operations. ASCII HEXFILE TRANSFER
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;----------------------------------------------------------------------------------------------------; ASCII HEXFILE TRANSFER
GETHEXFILE	CALL	PRINTI
		DB	CR,LF,"WAITING FOR HEX TRANSFER",EOS

		LD	HL,READ_SERIAL	;Set Serial Port as the source for the hex file
		LD	(HEX_SOURCE),HL
		LD	HL,VIEW_FLAGS	;Clear BIT .0= No View HEX Load
		RES	0,(HL)
		CALL	READ_HEX_FILE	;CY=1 ERROR encountered,  Z=0 Time Out
		JR  NZ,	GHENDTO
		RET	C		;If Error, exit without displaying "Complete" message
		
		CALL	PRINTI
		DB	CR,LF,"HEX TRANSFER COMPLETE, LINES=",EOS
		LD	HL,(RHF_LINES)
		CALL	PUT_HL
		RET

GHENDTO		CALL	PRINTI
		DB	CR,LF,"HEX TRANSFER TIMEOUT",EOS
		RET

		;FILL LINE_BUFF WITH A SINGLE LINE FROM SERIAL PORT
		;RETURN Z=0 IF TIMED OUT (RET NZ)		
READ_SERIAL	CALL	CLEAR_LINE_BUFF

		LD	HL, LINE_BUFF	;Data desination @HL
RS_CLP		LD	B,2		;Enable ':' search for 1st char
		
RS_LP		LD	A,117		;20 Second Timeout for Get char
		CALL	TIMED_GETCHAR
		JR  C, 	RS_TIMEOUT
		CP	27
		JR  Z,	RS_TIMEOUT

		CP	':'
		JR  Z,	RS_COK
		DJNZ	RS_CLP
		
RS_COK		LD	B,1		;Disable ':' search
		LD	(HL),A
		CP	CR		;Test for CR
		RET	Z			
		CP	LF		;Test if LF
		RET	Z

		INC	HL		
		LD	A,L
		CP	LOW LINE_BUFFEND
		JR  NZ,	RS_LP
		
RS_TIMEOUT	XOR	A	;C=0,Z=1
		DEC	A	;Z=0
		RET





;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_7	Menu operations. XMODEM FILE TRANSFER
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;----------------------------------------------------------------------------------------------------; XMODEM ROUTINES

SOH	equ	1	;Start of Header
EOT	equ	4	;End of Transmission
ACK	equ	6
DLE	equ	16
DC1	equ	17	; (X-ON)
DC3	equ	19	; (X-OFF)
NAK	equ	21
;SYN	equ	22
CAN	equ	24	;(Cancel)

;---------------------------------------------------------------------------------
;XMODEM MENU
;ENTRY:	TOP OF LDCK HOLDS RETURN ADDRESS (EXIT MECHANDSM IF XMODEM IS CANCELLED)
;---------------------------------------------------------------------------------
XMODEM		CALL	PUT_SPACE
		CALL	GET_CHAR	;get char
		AND	0x5F		;to upper case
		CP	'D'
		JR Z,	XMDN		; D = DOWNLOAD (from memory to serial port)
		CP	'U'
		JR Z,	XMUP		; U = UPLOAD (to memory from serial port)
		CALL 	PRINTI
		DB	"?",EOS
		RET

;---------------------------------------------------------------------------------
;XMDN - XMODEM DOWNLOAD (send file from IMSAI to Terminal)
;INPUT STARTING ADDRESS AND COUNT OF BLOCKS (WORD)
;WAIT FOR 'C' OR NAK FROM HOST TO START CRC/CS TRANSFER
;---------------------------------------------------------------------------------
XMDN		CALL	SPACE_GET_WORD	;Input Address
		EX	DE,HL		;HL = Address to SAVE DATA
		CALL	SPACE_GET_WORD	;Input #Blocks to Send
					;DE = Count of Blocks

		LD	A,D
		OR	E
		RET Z			;Exit if Block Count = 0

	;HL = Address of data to send from the IMSAI 8080
	;DE = Count of Blocks to send.

		CALL	XMS_INIT	;Starts the Seq, Sets the CS/CRC format
					;Cancelled Transfers will cause a RET

XMDN_LP		CALL	XMS_SEND	;Sends the packet @HL, Resends if NAK
					;Cancelled Transfers will cause a RET
		DEC	DE
		LD	A,D
		OR	E
		JR  NZ,	XMDN_LP

		CALL	XMS_EOT		;Send End of Transmission
		JP	PURGE


;---------------------------------------------------------------------------------
;XMUP - XMODEM UPLOAD (receive file from Terminal to IMSAI 8080)
;INPUT STARTING ADDRESS
;SEND 'C' OR NAK TO HOST TO START CRC/CS TRANSFER
;---------------------------------------------------------------------------------
XMUP		CALL	SPACE_GET_WORD	;Input Address
		EX	DE,HL		;HL = Address to SAVE DATA

	;HL = Address of data to send from the IMSAI 8080

		CALL	XMR_INIT	;Starts the transfer & Receives first PACKET
					;Cancelled Transfers will cause a RET

XMUP_LP		CALL	XMR_RECV	;Receives the next packet @HL, Resends if NAK
					;Cancelled Transfers will cause a RET
		JR C,	XMUP_LP		;Jump until EOT Received
		JP	PURGE



;---------------------------------------------------------------------------------
;INIT FOR SENDING XMODEM PROTOCOL, GET NAK OR 'C', SAVE THE XMTYPE
;---------------------------------------------------------------------------------
XMS_INIT	LD	A,1		;First SEQ number
		LD	(XMSEQ),A

		LD	B,33		;33 retries for initiating the transfer
XMS_INIT_LP	LD	A,10		;GET CHAR, 5 SECONDS TIMEOUT (EXPECT C OR NAK)
		CALL	TIMED_GETCHAR
		JP C,	XMS_INIT_RT	;Cancel if Host Timed out

		CP	NAK		;If NAK, Start Checksum Download
		JR Z,	XMS_DO
		CP	'C'		;If C, Start CRC Download
		JR Z,	XMS_DO
XMS_INIT_RT	DJNZ	XMS_INIT_LP	;Count down Retries
		JP	XM_CANCEL	;Cancel XModem if all retries exhausted

XMS_DO		LD	(XMTYPE),A
		RET

;---------------------------------------------------------------------------------
;SEND A PACKET (RESEND UPON NAK)
;---------------------------------------------------------------------------------
XMS_RESEND	LD	BC,0xFF80
		ADD	HL,BC
XMS_SEND	PUSH	DE
		LD	A,SOH		;SEND THE HEADER FOR CRC OR CHECKSUM
		CALL	Put_Char
		LD	A,(XMSEQ)
		CALL	Put_Char
		CPL
		CALL	Put_Char
		LD	DE,0x0000	;Init DE=0000 (CRC Accumulator)
		LD	C,0		;Init C=00 (CS Accumulator)
		LD	B,128		;Count 128 bytes per block
XMS_BLP		CALL	GET_MEM		;Fetch bytes to send  -------------------\
		PUSH	AF
		CALL	Put_Char	;Send them
		CALL	CRC_UPDATE	;Update the CRC
		POP	AF
		ADD	A,C		;Update the CS
		LD	C,A
		INC	HL		;Advance to next byte in block
		DEC	B		;Count down bytes sent
		JR NZ,	XMS_BLP		;Loop back until 128 bytes are sent -----^
		LD	A,(XMTYPE)
		CP	NAK		;If NAK, send Checksum
		JR Z,	XMS_CS		;----------------------v
		LD	A,D		;else, Send the CRC next
		CALL	Put_Char
		LD	C,E
XMS_CS		LD	A,C		;----------------------/
		CALL	Put_Char
					;Packet Sent, get Ack/Nak Response
		LD	A,120		;GET CHAR, 60 SECONDS TIMEOUT (EXPECT C OR NAK)
		CALL	TIMED_GETCHAR
		POP	DE
		JR C,	XM_CANCEL	;Cancel download if no response within 45 seconds
		CP	NAK
		JR Z,	XMS_RESEND	;Loop back to resend packet
		CP	CAN
		JR Z,	XM_CANCEL
		CP	ACK
		JR NZ,	XM_CANCEL

		LD	A,(XMSEQ)
		INC	A		;NEXT SEQ
		LD	(XMSEQ),A
		RET


;---------------------------------------------------------------------------------
;XMDN - DOWNLOAD XMODEM PACKET
;---------------------------------------------------------------------------------
XMS_EOT		LD	A,EOT		;HANDLE THE END OF TRANSFER FOR CRC OR CHECKSUM
		CALL	Put_Char
		LD	A,120		;GET CHAR, 60 SECONDS TIMEOUT (EXPECT C OR NAK)
		CALL	TIMED_GETCHAR
		JR C,	XM_CANCEL
		CP	NAK
		JR Z,	XMS_EOT
		CP	ACK
		JR NZ,	XM_CANCEL

XM_DONE		CALL	PURGE
		CALL	PRINTI
		DB	CR,LF,"TRANSFER COMPLETE\r\n",EOS
		XOR	A		;CLEAR A, CY
		RET

;FINISHING CODE PRIOR TO LEAVING XMODEM
XM_CANCEL	LD	A,CAN
		CALL	Put_Char
		CALL	Put_Char
		CALL	PURGE
		CALL	PRINTI
		DB	"TRANSFER CANCELED\r\n",EOS
		POP	BC		;SCRAP CALLING ROUTINE AND HEAD TO PARENT
		RET






;---------------------------------------------------------------------------------
;START XMODEM RECEIVING and RECEIVE FIRST PACKET
;---------------------------------------------------------------------------------
XMR_INIT	LD	E,20		;20 ATTEMPTS TO INITIATE XMODEM CRC TRANSFER
		LD	A,1		;EXPECTED SEQ NUMBER starts at 1
		LD	(XMSEQ),A
XMR_CRC		CALL	PURGE
		LD	A,'C'		;Send C
		LD	(XMTYPE),A	;Save as XM Type (CRC or CS)
		CALL	Put_Char
		CALL	XMGET_HDR	;Await a packet
		JR NC,	XMR_TSEQ	;Jump if first packet received
		JR NZ,	XM_CANCEL	;Cancel if there was a response that was not a header
		DEC	E		;Otherwise, if no response, retry a few times
		JR NZ,	XMR_CRC

		LD	E,20		;20 ATTEMPTS TO INITIATE XMODEM CHECKSUM TRANSFER
XMR_CS		CALL	PURGE
		LD	A,NAK		;Send NAK
		LD	(XMTYPE),A	;Save as XM Type (CRC or CS)
		CALL	Put_Char
		CALL	XMGET_HDR	;Await a packet
		JR NC,	XMR_TSEQ	;Jump if first packet received
		JR NZ,	XM_CANCEL	;Cancel if there was a response that was not a header
		DEC	E		;Otherwise, if no response, retry a few times
		JR NZ,	XMR_CS
		JR	XM_CANCEL	;Abort


;--------------------- XMODEM RECEIVE
;Entry:	XMR_TSEQ in the middle of the routine
;Pre:	C=1 (expected first block as received when negogiating CRC or Checksum)
;	HL=Memory to dump the file to
;Uses:	B to count the 128 bytes per block
;	C to track Block Number expected
;	DE as CRC (Within Loop) (D is destroyed when Getting Header)
;------------------------------------
XMR_RECV	LD	A,ACK		;Send Ack to start Receiving next packet
		CALL	Put_Char
XMR_LP		CALL	XMGET_HDR
		JR NC,	XMR_TSEQ
		PUSH	HL
		JR Z,	XMR_NAK		;NACK IF TIMED OUT
		POP	HL
		CP	EOT
		JR NZ,	XM_CANCEL	;CANCEL IF CAN RECEIVED (OR JUST NOT EOT)
		LD	A,ACK
		CALL	Put_Char
		JP	XM_DONE

XMR_TSEQ	LD	C,A
		LD	A,(XMSEQ)
		CP	C		;CHECK IF THIS SEQ IS EXPECTED
		JR Z,	XMR_SEQ_OK	;Jump if CORRECT SEQ
		DEC	A		;Else test if Previous SEQ
		LD	(XMSEQ),A
		CP	C
		JP NZ,	XM_CANCEL	;CANCEL IF SEQUENCE ISN'T PREVIOUS BLOCK
		CALL	PURGE		;ELSE, PURGE AND SEND ACK (ASSUMING PREVIOUS ACK WAS NOT RECEIVED)
		JR	XMR_ACK

XMR_SEQ_OK	LD	B,128		;128 BYTES PER BLOCK
		LD	C,0		;Clear Checksum
		LD	DE,0x0000	;CLEAR CRC
		PUSH	HL		;Save HL where block is to go
XMR_BLK_LP	CALL	TIMED1_GETCHAR
		JR C,	XMR_NAK
		LD	(HL),A		;SAVE DATA BYTE
		CALL	CRC_UPDATE
		LD	A,(HL)		;Update checksum
		ADD	A,C
		LD	C,A
		INC	HL		;ADVANCE
		DEC	B
		JR NZ,	XMR_BLK_LP
					;After 128 byte packet, verify error checking byte(s)
		LD	A,(XMTYPE)	;Determine if we are using CRC or Checksum
		CP	NAK		;If NAK, then use Checksum
		JR Z,	XMR_CCS
		CALL	TIMED1_GETCHAR
		JR C,	XMR_NAK
		CP	D
		JR NZ,	XMR_NAK
		CALL	TIMED1_GETCHAR
		JR C,	XMR_NAK
		CP	E
		JR NZ,	XMR_NAK
		JR	XMR_ACK

XMR_CCS		CALL	TIMED1_GETCHAR
		JP C,	XMR_NAK
		CP	C
		JR NZ,	XMR_NAK

		;If we were transfering to a FILE, this is where we would write the
		;sector and reset HL to the same 128 byte sector buffer.
		;CALL	WRITE_SECTOR

XMR_ACK		;LD	A,ACK		;The sending of the Ack is done by
		;CALL	Put_Char	;the calling routine, to allow writes to disk
		LD	A,(XMSEQ)
		INC	A		;Advance to next SEQ BLOCK
		LD	(XMSEQ),A
		POP	BC
		SCF			;Carry set when NOT last packet
		RET

XMR_NAK		POP	HL		;Return HL to start of block
		CALL	PURGE
		LD	A,NAK
		CALL	Put_Char
		JP	XMR_LP


;--------------------- XMODEM - GET HEADER
;
;pre:	Nothing
;post:	Carry Set: A=0, (Zero set) if Timeout
;	Carry Set: A=CAN (Not Zero) if Cancel received
;	Carry Set: A=EOT (Not Zero) if End of Tranmission received
;	Carry Clear and A = B = Seq if Header found and is good
;------------------------------------------
XMGET_HDR	LD	A,6		;GET CHAR, 3 SECONDS TIMEOUT (EXPECT SOH)
		CALL	TIMED_GETCHAR
		RET C			;Return if Timed out
		CP	SOH		;TEST IF START OF HEADER
		JR Z,	GS_SEQ		;IF SOH RECEIVED, GET SEQ NEXT
		CP	EOT		;TEST IF END OF TRANSMISSION
		JR Z,	GS_ESC		;IF EOT RECEIVED, TERMINATE XMODEM
		CP	CAN		;TEST IF CANCEL
		JR NZ,	XMGET_HDR
GS_ESC		OR	A		;Clear Z flag (because A<>0)
		SCF
		RET
GS_SEQ		CALL	TIMED1_GETCHAR	;GET SEQ CHAR
		RET C			;Return if Timed out
		LD	B,A		;SAVE SEQ
		CALL	TIMED1_GETCHAR	;GET SEQ COMPLEMENT
		RET C			;Return if Timed out
		CPL
		CP	B		;TEST IF SEQ VALID
		JR NZ,	XMGET_HDR	;LOOP BACK AND TRY AGAIN IF HEADER INCORRECT (SYNC FRAME)
		RET

;------------------------------------------ CRC_UPDATE
;HANDLE THE CRC CALCULATION FOR UP/DOWNLOADING
;Total Time=775 cycles = 388uSec
;In:	A  = New char to roll into CRC accumulator
;	DE = 16bit CRC accumulator
;Out:	DE = 16bit CRC accumulator
;------------------------------------------
;CRC_UPDATE	XOR	D		;4
;		LD	D,A		;5
;		PUSH	BC		;11
;		LD	B,8		;7	PRELOOP=27
;CRCU_LP	OR	A		;4	CLEAR CARRY
;		LD	A,E		;5
;		RLA			;4
;		LD	E,A		;5
;		LD	A,D		;5
;		RLA			;4
;		LD	D,A		;5
;		JP NC,	CRCU_NX		;10
;		LD	A,D		;5
;		XOR	0x10		;7
;		LD	D,A		;5
;		LD	A,E		;5
;		XOR	0x21		;7
;		LD	E,A		;5
;CRCU_NX	DEC	B		;5
;		JP NZ,	CRCU_LP		;10	LOOP=91*8 (WORSE CASE)
;		POP	BC		;10	POSTLOOP=20
;		RET			;10


;------------------------------------------ CRC_UPDATE
;HANDLE THE CRC CALCULATION FOR UP/DOWNLOADING
;Total Time=604 cycles = 302uSec MAX
;In:	A  = New char to roll into CRC accumulator
;	DE = 16bit CRC accumulator
;Out:	DE = 16bit CRC accumulator
;------------------------------------------
CRC_UPDATE	EX	DE,HL			;4
		XOR	H		;4
		LD	H,A		;5
		ADD	HL,HL		;10	Shift HL Left 1
		CALL C,	CRC_UPC		;17 (10/61)
		ADD	HL,HL		;10	Shift HL Left 2
		CALL C,	CRC_UPC		;17
		ADD	HL,HL		;10	Shift HL Left 3
		CALL C,	CRC_UPC		;17
		ADD	HL,HL		;10	Shift HL Left 4
		CALL C,	CRC_UPC		;17
		ADD	HL,HL		;10	Shift HL Left 5
		CALL C,	CRC_UPC		;17
		ADD	HL,HL		;10	Shift HL Left 6
		CALL C,	CRC_UPC		;17
		ADD	HL,HL		;10	Shift HL Left 7
		CALL C,	CRC_UPC		;17
		ADD	HL,HL		;10	Shift HL Left 8
		CALL C,	CRC_UPC		;17
		EX	DE,HL			;4
		RET			;10

CRC_UPC		LD	A,H		;5
		XOR	0x10		;7
		LD	H,A		;5
		LD	A,L		;5
		XOR	0x21		;7
		LD	L,A		;5
		RET			;10


;XModem implementation on 8080 Monitor (CP/M-80)
;
;Terminal uploads to 8080 system:
;-Terminal user enters command "XU aaaa"
;-8080 "drives" the protocol since it's the receiver
;-8080 sends <Nak> every 10 seconds until the transmitter sends a packet
;-if transmitter does not begin within 10 trys (100 seconds), 8080 aborts XMODEM
;-a packet is:
; <SOH> [seq] [NOT seq] [128 bytes of data] [checksum or CRC]
;
;<SOH> = 1 (Start of Header)
;<EOT> = 4 (End of Transmission)
;<ACK> = 6
;<DLE> = 16
;<DC1> = 17 (X-ON)
;<DC3> = 19 (X-OFF)
;<NAK> = 21
;<SYN> = 22
;<CAN> = 24 (Cancel)
;
;Checksum is the ModuLOW 256 sum of all 128 data bytes
;
;                                     <<<<<          [NAK]
;       [SOH][001][255][...][csum]    >>>>>
;                                     <<<<<          [ACK]
;       [SOH][002][254][...][csum]    >>>>>
;                                     <<<<<          [ACK]
;       [SOH][003][253][...][csum]    >>>>>
;                                     <<<<<          [ACK]
;       [EOT]                         >>>>>
;                                     <<<<<          [ACK]
;
;-if we get <EOT> then ACK and terminate XModem
;-if we get <CAN> then terminate XModem
;-if checksum invalid, then NAK
;-if seq number not correct as per [NOT seq], then NAK
;-if seq number = previous number, then ACK (But ignore block)
;-if seq number not the expected number, then <CAN><CAN> and terminate XModem
;-if data not received after 10 seconds, then NAK (inc Timeout Retry)
;-if timeout retry>10 then <CAN><CAN> and terminate XModem
;
;-To keep synchronized,
;  -Look for <SOH>, qualify <SOH> by checking the [seq] / [NOT seq]
;  -if no <SOH> found after 135 chars, then NAK
;
;-False EOT condtion
;  -NAK the first EOT
;  -if the next char is EOT again, then ACK and leave XModem
;
;-False <CAN>, expect a 2nd <CAN> ?
;
;-Using CRC, send "C" instead of <NAK> for the first packet
;  -Send "C" every 3 seconds for 3 tries, then degrade to checksums by sending <NAK>
;
;
;
;* The character-receive subroutine should be called with a
;parameter specifying the number of seconds to wait.  The
;receiver should first call it with a time of 10, then <nak> and
;try again, 10 times.
;  After receiving the <soh>, the receiver should call the
;character receive subroutine with a 1-second timeout, for the
;remainder of the message and the <cksum>.  Since they are sent
;as a continuous stream, timing out of this implies a serious
;like glitch that caused, say, 127 characters to be seen instead
;of 128.
;
;* When the receiver wishes to <nak>, it should call a "PURGE"
;subroutine, to wait for the line to clear.  Recall the sender
;tosses any characters in its UART buffer immediately upon
;completing sending a block, to ensure no glitches were mis-
;interpreted.
;  The most common technique is for "PURGE" to call the
;character receive subroutine, specifying a 1-second timeout,
;and looping back to PURGE until a timeout occurs.  The <nak> is
;then sent, ensuring the other end will see it.
;
;* You may wish to add code recommended by Jonh Mahr to your
;character receive routine - to set an error flag if the UART
;shows framing error, or overrun.  This will help catch a few
;more glitches - the most common of which is a hit in the high
;bits of the byte in two consecutive bytes.  The <cksum> comes
;out OK since counting in 1-byte produces the same result of
;adding 80H + 80H as with adding 00H + 00H.



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_8	Menu operations. RAM TEST
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;----------------------------------------------------------------------------------------------------; RAM TEST
;B=START PAGE
;C=END PAGE
RAM_TEST:	CALL	SPACE_GET_BYTE
		LD	B, A
		CALL	SPACE_GET_BYTE
		LD	C, A

		LD	HL,GET_REG_RUN
		LD	(GET_REG),HL
		CALL	RT_GO
		LD	HL,GET_REG_MON
		LD	(GET_REG),HL
		RET

;Page March Test.  1 Sec/K
;
; FOR E = 00 TO FF STEP FF   'March 00 then March FF
;   FOR H = B TO C
;      PAGE(H) = E
;   NEXT H
;   FOR D = B TO C
;      PAGE(D) = NOT E
;      FOR H = B TO C
;         A = E
;         IF H = D THEN A = NOT E
;         IF PAGE(H) <> A THEN ERROR1
;      NEXT H
;   NEXT D
; NEXT E
;

RT_GO		CALL	PRINTI
		DB	CR,LF,"TESTING RAM",EOS
		LD	E,0xFF		;E selects the polarity of the test, ie March a page of 1'S or 0's

;Clear/Set all pages
RT1_LP0		LD	H,B		;HL = BASE RAM ADDRESS
		LD	L,0
RT1_LP1		LD	A,E		;CLEAR A
		CPL
RT1_LP2		LD	(HL),A		;WRITE PAGE
		INC	L
		JR NZ,	RT1_LP2		;LOOP TO QUICKLY WRITE 1 PAGE
		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT1_LP1		;LOOP UNTIL = END PAGE

;March 1 PAGE through RAM
		LD	D,B		;Begin with START PAGE

;Write FF to page D
RT1_LP3		LD	H,D		;HL = Marched Page ADDRESS
		;LD	L,0
		CALL	ABORT_CHECK

		LD	A,D
		CPL
;		OUT	FPLED
		;LD	A,E		;SET A
RT1_LP4		LD	(HL),E		;WRITE PAGE
		INC	L
		JR  NZ,	RT1_LP4		;LOOP TO QUICKLY WRITE 1 PAGE

;Test all pages for 0 (except page D = FF)
		LD	H,B		;HL = BASE RAM ADDRESS
		;LD	L,0

RT1_LP5		LD	A,H		;IF H = D
		CP	D
		LD	A,E		;THEN Value = FF
		JR Z,	RT1_LP6
		CPL			;ELSE Value = 00

RT1_LP6		CP	(HL)		;TEST RAM
		JP NZ,	RT_FAIL1
		INC	L
		JR NZ,	RT1_LP6		;LOOP TO QUICKLY TEST 1 PAGE
		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT1_LP5		;LOOP UNTIL = END PAGE

;Write 00 back to page D
		LD	H,D		;HL = Marched Page ADDRESS
		;LD	L,0
		LD	A,E
		CPL
RT1_LP7		LD	(HL),A		;WRITE PAGE
		INC	L
		JR NZ,	RT1_LP7		;LOOP TO QUICKLY WRITE 1 PAGE

		LD	A,D
		INC	D		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT1_LP3		;LOOP UNTIL = END PAGE

		INC	E
		JR Z,	RT1_LP0

		CALL	PRINTI
		DB	CR,LF,"RAM PAGE MARCH PASSED",EOS


;Byte March Test.  7 Sec/K
;
; FOR E = 00 TO FF STEP FF   'March 00 then March FF
;   FOR H = B TO C
;      PAGE(H) = E
;      FOR D = 00 TO FF
;         PAGE(H).D = NOT E
;         FOR L=0 TO FF
;            IF PAGE(H).L <> E THEN
;               IF PAGE(H).L <> NOT E THEN ERROR2
;               IF L<>D THEN ERROR2
;            ENDIF
;         NEXT L
;      NEXT D
;   NEXT H
; NEXT E

		LD	E,0xFF		;E selects the polarity of the test, ie March a page of 1'S or 0's

;Clear/Set all pages

RT2_LP0		LD	H,B		;HL = BASE RAM ADDRESS
RT2_LP1		LD	L,0
		CALL	ABORT_CHECK

		LD	A,H
		CPL
;		OUT	FPLED

		LD	A,E		;CLEAR A
		CPL
RT2_LP2		LD	(HL),A		;WRITE PAGE
		INC	L
		JR NZ,	RT2_LP2		;LOOP TO QUICKLY WRITE 1 PAGE


		LD	D,0		;Starting with BYTE 00 of page

RT2_LP3		LD	L,D		;Save at byte march ptr
		LD	A,E		;SET A
		LD	(HL),A

		;LD	A,E
		CPL			;CLEAR A
		LD	L,0

RT2_LP4		CP	(HL)		;TEST BYTE FOR CLEAR
		JR Z,	RT2_NX1
		CPL			;SET A
		CP	(HL)		;TEST BYTE FOR SET
		JP NZ,	RT_FAIL2	;IF NOT FULLY SET, THEN DEFINITELY FAIL
		LD	A,L		;ELSE CHECK WE ARE ON MARCHED BYTE
		CP	D
		JP NZ,	RT_FAIL2
		LD	A,E		;CLEAR A
		CPL
RT2_NX1		INC	L
		JR NZ,	RT2_LP4		;LOOP TO QUICKLY WRITE 1 PAGE

		LD	L,D		;Save at byte march ptr
		LD	A,E
		CPL			;CLEAR A
		LD	(HL),A

		INC	D
		JR NZ,	RT2_LP3

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT2_LP1		;LOOP UNTIL = END PAGE

		INC	E
		JR Z,	RT2_LP0

		CALL	PRINTI
		DB	CR,LF,"RAM BYTE MARCH 1 PASSED",EOS

;26 Sec/K

BYTEMARCH2
		LD	E,0xFF		;E selects the polarity of the test, ie March a page of 1'S or 0's

RT4_LP0		LD	D,0		;Starting with BYTE 00 of page

;CLEAR all pages

		LD	H,B		;HL = BASE RAM ADDRESS
		LD	L,0

RT4_LP1		LD	A,E		;CLEAR A
		CPL
RT4_LP2		LD	(HL),A		;WRITE PAGE
		INC	L
		JR NZ,	RT4_LP2		;LOOP TO QUICKLY WRITE 1 PAGE

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT4_LP1		;LOOP UNTIL = END PAGE


RT4_LP3		CALL	ABORT_CHECK
		LD	A,D
		CPL
;		OUT	FPLED

					;Write SET byte at "D" in every page
		LD	H,B		;HL = BASE RAM ADDRESS
		LD	L,D		;Save at byte march ptr
RT4_LP4		LD	(HL),E

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT4_LP4		;LOOP UNTIL = END PAGE


		LD	L,0

RT4_LP5		LD	H,B		;HL = BASE RAM ADDRESS
		LD	A,L
		CP	D
		JR Z,	RT4_LP7		;Test for marked byte in all pages

RT4_LP6		LD	A,E
		CPL			;CLEAR A
		CP	(HL)		;TEST BYTE FOR CLEAR
		JP NZ,	RT_FAIL2

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT4_LP6		;LOOP UNTIL = END PAGE
		JR	RT4_NX

RT4_LP7		LD	A,E
		CP	(HL)		;TEST BYTE FOR SET
		JP NZ,	RT_FAIL2

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT4_LP7		;LOOP UNTIL = END PAGE

RT4_NX		INC	L
		JR NZ,	RT4_LP5

					;Write CLEAR byte at "D" in every page
		LD	H,B		;HL = BASE RAM ADDRESS
		LD	L,D		;Save at byte march ptr
RT4_LP8		LD	A,E
		CPL
		LD	(HL),A

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT4_LP8		;LOOP UNTIL = END PAGE

		INC	D
		JR NZ,	RT4_LP3


		INC	E
		JR Z,	RT4_LP0

		CALL	PRINTI
		DB	CR,LF,"RAM BYTE MARCH 2 PASSED",EOS


BIT_MARCH
;Bit March Test.  0.1 Sec/K

		LD	E,01		;E selects the bit to march

;Clear/Set all pages

RT3_LP1		LD	H,B		;HL = BASE RAM ADDRESS
		LD	L,0

		CALL	ABORT_CHECK

		LD	A,E		;Display bit pattern on LED PORT
		CPL
;		OUT	FPLED

RT3_LP2		LD	A,E		;FETCH MARCHING BIT PATTERN
RT3_LP3		LD	(HL),A		;WRITE PAGE
		INC	L
		JR NZ,	RT3_LP3		;LOOP TO QUICKLY WRITE 1 PAGE

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT3_LP2		;LOOP UNTIL = END PAGE

		LD	H,B		;HL = BASE RAM ADDRESS
;		LD	L,0

RT3_LP4		LD	A,E		;FETCH MARCHING BIT PATTERN
RT3_LP5		CP	(HL)
		JP NZ,	RT_FAIL3
		INC	L
		JR NZ,	RT3_LP5		;LOOP TO QUICKLY WRITE 1 PAGE

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT3_LP4		;LOOP UNTIL = END PAGE


					;0000 0010
					;...
					;1000 0000

		LD	A,E
		RLA			;ROTATE THE 01 UNTIL 00
		LD	A,E
		RLCA
		LD	E,A
		CP	1
		JR NZ,	RT3_NX1
		CPL			;INVERT ALL BITS
		LD	E,A
		JR	RT3_LP1
RT3_NX1		CP	0xFE
		JR NZ,	RT3_LP1

		CALL	PRINTI
		DB	CR,LF,"RAM BIT MARCH PASSED",EOS


		LD	E,01		;E selects the start sequence

;Clear/Set all pages

RT5_LP1		CALL	ABORT_CHECK

		LD	A,E		;Display bit pattern on LED PORT
		CPL
;		OUT	FPLED

		LD	H,B		;HL = BASE RAM ADDRESS
		LD	L,0
		LD	D,E

RT5_LP2		INC	D
		JR NZ,	RT5_NX1
		INC	D
RT5_NX1		LD	(HL),D		;WRITE PAGE
		INC	L
		JR NZ,	RT5_LP2		;LOOP TO QUICKLY WRITE 1 PAGE

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT5_LP2		;LOOP UNTIL = END PAGE

		LD	H,B		;HL = BASE RAM ADDRESS
		;LD	L,0
		LD	D,E

RT5_LP3		INC	D
		JR NZ,	RT5_NX2
		INC	D
RT5_NX2		LD	A,D
		CP	(HL)		;TEST
		JP NZ,	RT_FAIL5
		INC	L
		JR NZ,	RT5_LP3		;LOOP TO QUICKLY WRITE 1 PAGE

		LD	A,H
		INC	H		;ADVANCE TO NEXT PAGE
		CP	C		;COMPARE WITH END PAGE
		JR NZ,	RT5_LP3		;LOOP UNTIL = END PAGE

		INC	E
		JR NZ,	RT5_LP1

		CALL	PRINTI
		DB	CR,LF,"RAM SEQUENCE TEST PASSED",EOS
		RET

RT_FAIL1	CALL	PRINTI
		DB	CR,LF,"RAM FAILED PAGE MARCH AT:",EOS
		CALL	PUT_HL
		RET

RT_FAIL2	CALL	PRINTI
		DB	CR,LF,"RAM FAILED BYTE MARCH AT:",EOS
		CALL	PUT_HL
		RET

RT_FAIL3	CALL	PRINTI
		DB	CR,LF,"RAM FAILED BIT MARCH AT:",EOS
		CALL	PUT_HL
		RET

RT_FAIL5	CALL	PRINTI
		DB	CR,LF,"RAM FAILED SEQUENCE TEST AT:",EOS
		CALL	PUT_HL
		RET

ABORT_CHECK	CALL	IN_CHAR
		RET C
		CP	27
		RET NZ
		POP	HL			;SCRAP RETURN ADDRESS AND GO TO PARENT ROUTINE
		CALL	PRINTI
		DB	CR,LF,"ABORTED",EOS
		RET


;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_10	BIOS.
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;SET_IO		Sets the IO pointed to by PUT_CHAR AND IN_CHAR
;GET_CHAR_NE	Uses IN_CHAR
;GET_CHAR	Uses IN_CHAR
;Put_Char	
;RX_COUNT	Returns count of bytes recieved
;IN_CHAR	Returns received char or CF=1 if no char
;PURGE		
;TIMED1_GETCHAR	
;TIMED_GETCHAR	
;
;RXC_BOTH	BOTH BIT AND ACE
;IC_BOTH		
;PC_BOTH		
;
;LED_HOME	LED
;IC_KEY		
;PC_LED		
;
;RXC_ACE	ACE	
;IC_ACE		
;PC_ACE		
;
;RXC_BIT	BITBANG	
;IC_BIT		
;PC_POS_UPDATE	
;PC_BIT		

;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;----------------------------------------------------------------------------------------------------; CONSOLE BIOS


				;HW_LIST: 00 NO FP, 01 FP only, 02 SIO only, 03 BOTH SIO & FP
				;Input C: 00=BIT, 01=BIT, 02=ACE, 03=BOTH, 04=LED
SET_IO		PUSH	HL
		PUSH	DE
		PUSH	BC		
		PUSH	AF
		LD	HL,HW_LIST	;Only set the new I/O if Hardware is present
		LD	A,C
		OR	A
		JR  Z,	SIO_ZERO	;Special case, No FP & No SIO
		AND	(HL)
		LD	C,A
		JR NZ,	SIO_OK
		LD	A,C		;Handle special LED case.
		RRCA			;Shift LED (04) to FP hardware (01)
		RRCA
		AND	(HL)
		LD	A,C
		JR NZ,	SIO_OK
		POP	AF
		SCF
		JR 	SIO_RET
		
SIO_ZERO	INC	C
		INC	A
SIO_OK		LD	(HW_SETIO),A	;Save set IO configuration
		DEC	C
		SLA	C		;x2 for word
		SLA	C		;x2 for 2 words
		LD	B,0
		LD	HL,IOD_TABLE
		ADD	HL,BC
		LD	DE,PUTCHAR_EXE
		LD	C,4
		LDIR
		POP	AF
		OR	A		;CY=0
SIO_RET		POP	BC
		POP	DE
		POP	HL
		RET

IOD_TABLE	DW	PC_BIT		;FP       (PUTCHAR_EXE)
		DW	IC_BIT		;FP       (INCHAR_EXE)
		DW	PC_ACE		;SIO      (PUTCHAR_EXE)
		DW	IC_ACE		;SIO 	  (INCHAR_EXE)
		DW	PC_BOTH		;FP & SIO (PUTCHAR_EXE)
		DW	IC_BOTH		;FP & SIO (INCHAR_EXE)
		DW	PC_LED		;LED      (PUTCHAR_EXE)
		DW	IC_KEY		;LED      (INCHAR_EXE)


				;HW_SETIO: 00 NO FP, 01 FP only, 02 SIO only, 03 BOTH SIO & FP
GET_POS		LD	A,(HW_SETIO)
		RRCA
		RRCA
		LD	A,(POS_ACE)
		RET	C	;Exit with POS_ACE if ACE Selected
		LD	A,(POS_BIT)
		RET


;===============================================
;GET_CHAR -- Get a char from the console NO ECHO
;-----------------------------------------------
;HW_LIST: 00 NO FP, 01 FP only, 02 SIO only, 03 BOTH SIO & FP
GET_CHAR_NE:	CALL	IN_CHAR
		JR C,	GET_CHAR_NE
		RET

;===============================================
;GET_CHAR -- Get a char from the console
;-----------------------------------------------
GET_CHAR:	LD	A,(ECHO_STATE)
		OR	A
		JR Z,	GET_CHAR_NE
GET_CHAR_LP	CALL	GET_CHAR_NE
		CP	' '	;Do not echo control chars
		RET M
		;RET		;ECHO THE CHAR
				;FALL INTO PUT_CHAR

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Send A byte to Bitbanged RS-232, ACE RS-232 or LED
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
PUT_CHAR:	PUSH	AF
		PUSH	BC		;Save registers
		PUSH	DE
		PUSH	HL
		LD	C,A		;Put character to send IN C for shifting

		LD	HL,(PUTCHAR_EXE)
		CALL	VCALL_HL	;JP	(HL)

		POP HL
		POP DE
		POP BC
		POP AF
		RET

;Update position of PUT_CHAR (Used later to create aligned columns)
PC_POS_UPDATE	CP	13
		JR NZ,	PC_NCR
		LD	(HL),0
		RET
PC_NCR		CP	' '
		RET M
		INC	(HL)
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;RS-232 RX Buffer Count
RX_COUNT	PUSH	HL
		CALL	RXC_DO	;Create a routine without stack management
		POP	HL
		RET

RXC_DO		LD	A,(INCHAR_EXE)	;Test which INCHAR routine is active
		CP	LOW IC_BIT
		JP Z,	RXC_BIT
		CP	LOW IC_ACE
		JP Z,	RXC_ACE
		CP	LOW IC_BOTH
		JR Z,	RXC_BOTH
		XOR	A
		RET


;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Check for A byte
;	Exit:	C=0, A=Byte from Buffer
;		C=1, Buffer Empty, no byte
;		w/call, tcy if no byte = ACE:141, BIT:181, BOTH:269
IN_CHAR:	PUSH	HL		;11
		LD	HL,(INCHAR_EXE)	;16
		CALL	VCALL_HL	;17 +4 +routine ACE=56, BIT=96, Both=32+ACE+BIT   JP (HL)
		POP HL			;10
		RET			;10


;===============================================
;PURGE - Clears all in coming bytes until the line is clear for a full 2 seconds
;-----------------------------------------------
PURGE		LD	A,1	;1 seconds for time out
		CALL	TIMED_GETCHAR
		JR NC,	PURGE
		RET

;===============================================
;DOT_GETCHAR
;in:	B=Count of Dots
;out: 	C=1, No Char (Time Out)
;	C=0, A = Char
;-----------------------------------------------
DOT_GETCHAR	LD	A,1
		CALL	TIMED_GETCHAR	;C=0, A=Byte from Buffer; C=1, no byte
		JR  C,	DGC_DOT
		CP	9
		JR   Z,	DGC_RET
		AND 	0x5F		;to upper case
		RET			;Return to check charcter
DGC_DOT		LD	A,'.'		;Put out some thinking dots
		CALL	PUT_CHAR
		DJNZ	DOT_GETCHAR
DGC_RET		SCF
		RET

;===============================================
;TIMED1_GETCHAR - Gets a character within 1 second
;-----------------------------------------------
TIMED1_GETCHAR	LD	A,2

;===============================================
;TIMED_GETCHAR - Gets a character within a time limit
;in:	A contains # of 1/2 seconds to wait before returning
;out: 	C=1, No Char (Time Out)
;	C=0, A = Char
;-----------------------------------------------
TIMED_GETCHAR	PUSH	DE
		PUSH	BC
		LD	D,A
TGC_LP1		LD	C,15		;B,C=Loop Count down until timeout
					;TEST FOR RX DATA
TGC_LP2		CALL	IN_CHAR	;ACE:141, BIT:181, BOTH:269	
		JP NC,	TGC_RET	;10
		DJNZ	TGC_LP2	;13/8	;110 Cycles inner Loop time. 164*256/4 ~= 10 mSec
		DEC	C	;5
		JR NZ,	TGC_LP2	;10
		DEC	D
		JR NZ,	TGC_LP1
;		SCF		;CARRY STILL SET TO INDICATE TIME OUT
TGC_RET		POP	BC
		POP	DE
		RET


;-----------------------   B O T H  I/O   -----------------------
;----------------------------------------------------------------
RXC_BOTH	CALL	RXC_BIT
		LD	L,A	
		CALL	RXC_ACE
		OR	L
		RET

IC_BOTH		CALL	IC_BIT		;17
		RET	NC		;11/5 RETURN IF CHAR
		JR	IC_ACE		;10

PC_BOTH		CALL	PC_ACE
		CALL	PC_BIT
		RET		


;-----------------------   L E D  I/O   -----------------------
;--------------------------------------------------------------
;
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Select Put_Char Output
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
LED_HOME	PUSH	HL
		LD	HL,LED_DISPLAY
		LD	(LED_CURSOR),HL
		POP	HL
		RET
		
LED_CLEAR	PUSH	HL
		PUSH	BC
		LD	A,0x0C
		CALL	PC_LED
		POP	BC
		POP	HL
		RET
		

;Put_Char to LED Display, Char in C
PC_LED		LD	HL,(LED_CURSOR)	;Point to LED Display Buffer
		LD	A,C
		CP	0x20		;Test for Control/unprintable characters
		JR  C,	PCL_CTRL

		LD	B,HIGH LED_FONT	;Set BC to point to LED FONT
		RES	7,C		;Ensure ASCII 0x20-0x7F only
		LD	A,(BC)
		SET	7,A		;Ensure TXbit is 1
		LD	(HL),A		;Save Character in LED_DISPLAY BUFFER
		INC	L
		JR	PCL_RET2

PCL_CTRL	CP	0x0C		;<NP>
		JR NZ,	PCLC_1
		LD	B,8		;<NP> Clears LED Line
		LD	A,0x80
		LD	HL,LED_DISPLAY
PCLC_LP		LD	(HL),A
		INC	L
		DJNZ	PCLC_LP
		JR	PCL_RETC

PCLC_1		CP	0x0D		;<CR>	Control characters:
		RET	NZ
PCL_RETC	LD	HL,LED_DISPLAY	;<CR> Returns cursor to start of LED Line
PCL_RET2	RES	3,L
		LD	(LED_CURSOR),HL
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Keyboard Get A byte
;All Keys are equal, but F works as a SHIFT on Press and F on release
;Output:	Z=1, No Key Pressed
;		Z=0, A=Key Pressed, bit 4 = Shift, ie, 0x97 = Shift-7
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
IC_KEY		PUSH	HL
		LD	HL,KEY_PRESSED
		LD	A,(HL)
		LD	(HL),0
		POP	HL
		OR	A
		RET


;-----------------------   A C E  I/O   -----------------------
;--------------------------------------------------------------
;HW_LIST: 00 NO FP, 01 FP only, 02 SIO only, 03 BOTH SIO & FP

RXC_ACE		LD	A,(HW_SETIO)
		AND	2
		RET  Z
		IN	A,(ACE_STATUS)
		AND	1		
		RET

IC_ACE		LD	A,(HW_SETIO)	;13
		AND	2		;4
		SCF			;4  C=1, Assume byte NOT available
		RET  Z			;11/5
		IN	A,(ACE_STATUS)	;11
		AND	1		;4
		SCF			;4  C=1, Assume byte NOT available
		RET Z			;11/5 Exit if byte not available C=1
		IN	A,(ACE_DATA)
		OR	A		;Exit with C=0
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Put Char ACE,  C=Char to send
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
PC_ACE		LD	A,(HW_SETIO)
		AND	2
		RET  Z
PCA_LP		IN	A,(ACE_STATUS)
		AND	0x20
		JR Z,	PCA_LP
		LD	A,C
		OUT	(ACE_DATA),A
		LD  	HL, POS_ACE
		CALL	PC_POS_UPDATE
		RET

ACE_SET_BAUD	LD	A,0x80		;Set baud rate
		OUT	(ACE_LCR),A
		LD	A,(ACE_BAUD)	;12=9600 baud
		OUT	(ACE_BAUD0),A
		XOR	A
		OUT	(ACE_BAUD1),A
		LD	A,3		;Set 8 data bits, no parity, 1 stop
		OUT	(ACE_LCR),A
		IN	A,(ACE_DATA)	;Clear any rxd flag
		RET


;-----------------------   B I T  I/O   -----------------------
;--------------------------------------------------------------
RXC_BIT		LD	A,(HW_SETIO)
		AND	1
		RET  Z		
		LD	A,(RXBHEAD)
		LD	HL,RXBTAIL
		SUB	(HL)		
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;RS-232 Get A byte
;	Exit:	C=0, A=Byte from Buffer
;		C=1, Buffer Empty, no byte
;		w/call, tcy=87 if no byte
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
IC_BIT		LD	A,(HW_SETIO)	;13
		AND	1		;4
		SCF			;4  C=1, Assume byte NOT available
		RET  Z			;11/5
		PUSH	BC		;11
		LD	A,(RXBHEAD)	;13 Test if TAIL=HEAD (=No bytes in buffer)
		LD	B,A		;4
		LD	A,(RXBTAIL)	;13
		XOR	B		;4 Check if byte(s) in receive buffer
		POP	BC		;10
		SCF			;4  C=1, Assume byte NOT available
		RET Z			;11 Exit if byte not available (ie TAIL=HEAD), C=1
		PUSH	HL
		DI
		LD	HL,(RXBTAIL)
		INC	L
		LD	(RXBTAIL),HL	;Tail = Tail + 1
		EI
		LD	A,(HL)		;A = Byte from buffer (@ TAIL)
		POP	HL
		OR	A		;Exit with C=0
		RET


;Put_Char to RS232 BIT banger
;The bit banged byte to be sent is done through the msb of the LED Display Output byte.
;To simplify AND expedite the sending of those Display bytes (with the RS-232 BIT), the transmitted
;byte will be scattered in a secondary buffer that is 10 bytes (1 start, 8 data, 1 stop)
;This secondary buffer will have the transmitted bits mixed IN with the LED Display Bytes
;The Interrupt is disabled only at crucial moments, but otherwise left on to accept any characters
;received from the RS-232 line
PC_BIT		LD  	HL, POS_BIT
		CALL	PC_POS_UPDATE
		LD  	HL,LED_DISPLAY_SB
				;Copy 10 bytes from the LED_DISPLAY buffer (MOD 8) to the secondary buffer
PC_REDO		LD  	DE,(SCAN_PTR)   ;SCAN_PTR holds the next LED BYTE @ OUTPUT.
		LD  	B,E		;Save SCAN_PTR for test if an Interrupt occurs

		LD	A,(SCAN_LED)
		LD  	(HL),A
		RES 	7,(HL)	;Configure Start BIT (msb) to be 0

		INC 	L
				;Shift next 9 bits IN this loop,
PC_LP0		LD	A,(HW_LIST)	;Skip other buffer bytes if FP not present
		OR	A
		LD	A,(SCAN_LED)	;Save for next interrupt
		JR  Z,	PC_NOFP

		INC 	E
		RES 	3,E		;Bound DE to the 8 bytes of LED_DISPLAY
		LD  	A,(DE)
		
PC_NOFP		RLA		;Bump OUT msb
		RRC 	C		;Fetch Data BIT (non destructive shifting incase of REDO)
		RR  	A		;Shift IN Data BIT
		LD  	(HL),A
		INC 	L
		JR  	NZ,PC_LP0

		DEC 	L
		SET 	7,(HL)	;Stop Bit

		LD  	L,LOW LED_DISPLAY_SB  ;Restart Pointer to Secondary Buffer

				;Test if SCAN_PTR Changed (due to ISR)
		LD  E,5		;Preload RX delay counter (incase of RX byte during TX)
		LD  D,0x80	;Preload RxD Register with A marker BIT (to count 8 data bits)

		DI		;STOP INTERRUPTS HERE to see if SCAN_PTR has changed (due to Timer Interrupt)
		LD  A,(SCAN_PTR) ;Adjust working scan pointer (counted to 10 mod 8, so subtract 2 to restore)
		XOR B
		JR  Z,PC_0
				;If SCAN_PTR changed, Redo the Secondary Buffer
		EI		;Allow Interrupts again while preparing Secondary Buffer
		RLC C		;ADJUST Transmitted bits due to 9 bits shifted (back up 1 BIT)
		JR  PC_REDO
;- - - - - - - - - - - - - - - - - - - - - Transmit the BYTE here....(BYTE encoded in temp 10 byte LED buffer)
;1 Bit time at 9600 = 416.6666 cycles

PC_0		LD  C,Port40

PC_1		LD  A,(HL)	;7	Send BIT
		OUT (Port40),A	;11
		LD  B,8		;7

PC_2		IN  A,(C)	;12	;While waiting, Poll for RX DATA Start bit
		JP  P,PC_5	;10 tc (Note 1.JP)
		LD  A,(0)	;13 tc NOP
PC_3		DJNZ PC_2	;13/8  ;48 IN loop (-5 on last itteration).  48 * 8 + 39 - 5 = 418 tc per BIT

		INC L		;4
		JP  NZ,PC_1	;10	;39 TC Overhead to send BIT
		JP  PC_RET

PC_4		SRL B		;4	If false start bit detected, Divide B by 2 and return to simple tx
		JP  NZ,PC_3	;10
		INC L		;4
		JP  Z,PC_RET	;10
		LD  A,(HL)	;7	Send BIT
		OUT (Port40),A	;11
		JP  PC_2	;10


				;Here an RX byte was detected while transmitting.
				;Delay IN detection could be as much as 60tc, we will assume 1/2 (=30tc)
				;We need to test Start Bit @ 208tc,
				;We are juggling TX & RX. TX will occur earlier than BIT time due to shorter loop delay
PC_5		INC  L		;4
		DEC  B		;4
		JP Z,PC_7	;10
		SLA  B		;8      Multiply B by 2 for 24 cycle loop
PC_6		DEC  E		;4	RxBit Timing
		JR   Z,PC_9	;7/12   ;Either before OR after sending A BIT, we will branch OUT of loop here to check for RX Start Bit
		DJNZ PC_6	;13/8 tc TxBit Timing
				;		24 tc Loop
;TxBit
PC_7	       	LD  B,13	;7
		XOR A		;4
		OR  L		;4
		JR  Z,PC_8	;7/12	;Stop sending if L=0
		LD  A,(HL)	;7	;39 to send next BIT
		OUT (Port40),A	;11
		INC L		;4
PC_8		JP  PC_6	;10 tc (Note 1.JP)

				;Test if Start Bit is good (at ~1/2 BIT time)
PC_9		LD  E,5		;7   E=5 incase we have a bad start bit and have to return to simple TX
		IN  A,(C)	;12  Re-TEST Start Bit at 1/2 bit time
		JP  M,PC_4	;10  If Start BIT not verified, then return to simple TXD (return at point where we are Decrementing B to minimize diff)
		LD  E,15	;7   Adjust initial sampling delay (as per timing observed)


				;At this point, we have good start BIT, 1 OR more TX bits left to go...  here's where timing is accurate again
				;We will go through each TXbit AND RXBit once during the full BIT time.  So the time of these routines are added
PC_10		DEC E		;4
		JR  Z,PC_14	;7/12
PC_11		DJNZ PC_10	;13/8 tc    24Loop= 6uSec

				; TX= S 0 1 2 3 4 5 6 7 S
				; RX=  S 0 1 2 3 4 5 6 7 S  <-It's possible to receive all 8 data bits before sending Stop Bit
;TxBit ;54tc to Send BIT
		XOR A		;4
		OR  L		;4
		JR  Z,PC_13	;7/12	;Stop sending if L=0
		LD  A,(HL)	;7
		OUT (Port40),A	;11
		INC L		;4
PC_12        	LD  B,13	;7     (417 - 54 - 51)/24 = 13 counts required to pace 1 BIT
		JP  PC_10	;10 tc (Note 1.JP)

PC_13		LD  B,13	;7     (7tc NOP)
		JP  PC_12	;10 tc (Note 1.JP)

;RxBit ;51tc to Receive BIT
PC_14		IN   A,(Port40)	;11	Fetch RXbit
		NOP		;4
		RLCA		;4	put IN CARRY
		RR    D		;8	shift into RxD
		LD    E,13	;7      (417 - 54 - 51)/24 = 13 counts required to pace 1 BIT
		JR C, PC_15	;7/12	;Test for marker BIT shifting OUT of D
		JP    PC_11	;10	RXBIT = 40tc

PC_15		NOP		;4
PC_16		DEC  E		;4
		JR Z,PC_19	;7/12
		DJNZ PC_16	;13/8 tc    24Loop= 6uSec

				; TX= S 0 1 2 3 4 5 6 7 S
				; RX=  S 0 1 2 3 4 5 6 7 S  <-It's possible to receive all 8 data bits before sending Stop Bit
;TxBit ;54tc to Send BIT
		XOR  A		;4
		OR   L		;4
		JR Z,PC_18	;7/12	;Stop sending if L=0
		LD   A,(HL)	;7
		OUT (Port40),A	;11
		INC  L		;4
PC_17        	LD   B,13	;7     (417 - 54 - 51)/24 = 13 counts required to pace 1 BIT
		JP   PC_16	;10 tc (Note 1.JP)

PC_18		LD   B,13	;7     (7tc NOP)
		JP   PC_17	;10 tc (Note 1.JP)



;RxBit ;51tc to Receive BIT
PC_19		IN  A,(Port40)	;11	Fetch Stop BIT
		RLCA		;4	put IN CARRY
		JP C,PC_20
		LD   HL,RX_ERR_STOP
		CALL TINC

PC_20		LD  A,D		;Fetch received byte to RX Buffer
		LD  HL,(RXBHEAD)
		INC L
		LD  (RXBHEAD),HL ;Head = Head + 1
		LD  (HL),A	;Stuff into RX BUFFER
		LD  A,(RXBTAIL)
		CP  L
		JR  NZ,PC_RET	;Jump if NOT Zero = No Over run error (Head <> Tail)
		INC A		;Else
		LD  (RXBTAIL),A	;Tail = Tail + 1
		LD   HL,RX_ERR_OVR ;Count Over Run Error
		CALL TINC

PC_RET		LD	A,(HW_LIST)	;Skip Resync if FP not present
		OR	A
		JR  Z,	PC_RET1

		IN	A,(Port40)	;Resync the SCAN_PTR
		INC	A
		AND	7
		OR  LOW LED_DISPLAY
		LD	L,A
		LD	(SCAN_PTR),A	;Save Scan Ptr @ Next Scan Output
		LD	H,HIGH SCAN_PTR
		LD	A,(HL)
		LD	(SCAN_LED),A	;Save for next interrupt		
PC_RET1		EI
		RET


;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_11	ISR.  RS-232 Receive, LED & Keyboard scanning, Timer tic counting
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;                       *********   *******    ********
;                       *********  *********   *********
;                          ***     **     **   **     **
;                          ***     **          **     **
;---------------------     ***     *******     ********   ---------------------
;---------------------     ***       *******   ********   ---------------------
;                          ***            **   **  **
;                          ***     **     **   **   **
;                       *********  *********   **    **
;                       *********   *******    **     **
;
;
;
;The ISR will service either the Timer interrupt or Serial Data (bit banger) interrupt.
;
;The ISR will re-enable interrupts during Timer functions for re-entry to catch Serial Data.
;
;
;The INT calls RST 38.  RST38 is the same in ROM or RAM.
;
;**RST38--------Switch to the Alternate Register set, jump to ISR_VEC (typcially ISR_DISPATCH)
;**ISR_DISPATCH-Direct Select ROM, Check input port, jump to ISR_RXD (RS-232) or ISR_TIMER
;**ISR_RXD------Bit Bang a byte, store in RXBUFFER, jump to ISR_RET
;**ISR_TIMER----Output LED (Clears TIMER_INT), enter Extended Timer or jump to ISR_RET (If
;		already executing the extended Timer)
;ISR_EXTIMER----RAM/ROM state, set to ROM. Use ISR Stack, Undo Alternate Registers, EI, PUSH HL, AF
;		Resync LED to scan, Do Halt Test (jump BREAK_RET), Run TicCounter (1mSec),
;		Do extra extended timer to complete keypad scans every 32mSec or exit EXTIMER
;ISR_EXXTIMER---Scan Keypad for events, do Keyevents as they occur to control LED output,
;		update LED output, <Ctrl>-C Test (jump BREAK_RET), Call User Interrupt Vector
;		Keypad Event F-E causes jump to BREAK_RET
;		Single Step causes jump to BREAK_RET
;ISR_EXTIMER_RET POP AF, HL. Restore Stack, Restore RAM/ROM state, Switch to Alternate Registers for RET
;**ISR_RET------Select RAM/ROM state as tracked in Mainline code, Undo Alternate Registers, EI. RETI
;BREAK_RET------Leaves the ISR to return to 0000 and restart the monitor.

;** = ISR RUNNING ON ALTERNATE REGSITERS (**IROAR**)

;
;
;	Normal timer interrupt takes 463 cycles  (??? outdated counts)
;	42  ISR Vectoring & Redirection
;	50  Timer Int Detection
;	28  LED Refresh (Re-enable Interrupts)
;	78  Resync & Prepare next LED Refresh value
;	111 Halt Test
;	38  TIC Counter
;	21  Keyboard/Display maintenance required check
;	41  User Interrupt Check
;	34  ISR Exit
;
;	Occuring 8 out of 32 ISR's:
;	82  Scanning non pressed keys
;
;	Occuring 1 out of 32 ISR's
;	165 Processing non pressed keys
;	97  Ctrl-C Checking
;	67  Beeper Timer
;	31  Cmd Expiration Timer
;	262 Display Memory contents
;	649 Display Register contents
;
;	24/32 ISR's = 463 cycles	= 11,112
;	7/32  ISR's = 545 cycles    	=  3,815
;	1/32  ISR's = 1,554 cycles	=  1,554 (Displaying Register)
;	Total over 32 ISR's		= 16,481
;	Average per ISR = 515 cycles
;			= 128.75uS used every 1024uS interrupt cycle
;			= 13% ISR Overhead (When Displaying Register)
;
;
;
;
;
;Note: A start bit can still hijack the ISR at Dispatch.
;
;A start bit happening anywhere in mainline code will take ~???tc to reach ISR_RXD
;A start bit happening upon entry to ISR_DISPATCH (when doing timer int) will take ??tc to reach ISR_RXD
;A start bit happening just past the ISR_DISPATCH window of opportunity will take 
;
;ISR_DISPATCH Sort out what is causing the interrupt
;


;- - - - - - - - - - - - - - RS-232 Receive BYTE
;
;1 Bit time at 9600 = 416.6666 cycles	;We get here ~25 to 200 tc (range = 175tc = 1/2 bit time)
;**IROAR**
ISR_RXD		LD   L,2	;7
IRXD_VS		IN   A,(Port40)	;11	;Re-sample Start BIT @+7, +35tc
		RLCA		;4	;(Actual sampling occurs 9 OR 10 tc later)
		JR   C,IRXD_INC	;7/12
		DEC  L		;4
		JR  NZ,IRXD_VS	;7/12
				;35tc per loop
				;@+68 when we come OUT of loop
				;
				;=~93tc to ~268tc  Middle = 180
				;
				;Must have a total delay of 1.5 bits 625 to reach middle of 1st data BIT
				;Total delay: 1.5 * 416 = 625tc
				;Next bit is at 417tc to 834tc
				;625-180 = Need another 445 Delay
		

				;My cycle counting is off by at least 30uSec = 120tc
				;Changed 26 to 20.  20*16-5=315 (411-315=96)

		
		LD   A,22	;7	;Delay loop after START Bit
		DEC  A		;4
		JR   NZ,$-1	;12/7	Delay loop = 26 * 16 - 5 = 411


;- - - - - - - - - - - - - - RS-232 Receive BYTE
		LD   L,8	;7
				;@624	;Loop through sampling 8 data bits
IRXD_NB		
		IN   A,(Port40)	;11	;Sample BIT
		RLCA		;4	;Get BIT
		RR   H		;8	;Shift IN
		DEC  L		;4	;Count down 8 bits
	;	JR   Z,IRXD_STP	;7/12	;Use this jump to test for STOP bit

		JR  Z,IRXD_SAVE	;7/12	Optional to finish receiving byte here AND ignore framing errors
				;	(Replace the previous condital jump with IRXD_SAVE destination).

IRXD_NI		LD  A,23	;7	;Delay loop between data bits
		DEC A		;4
		JR  NZ,$-1	;12/7	;Delay loop = 16 * 23 + 53 - 5 = 416
		JR  IRXD_NB	;12	;Total Overhead = 53
				;Time to get all data bits = 416 * 7 + 39 = 2951 (last BIT does not get full delay)
				
				;@3576  (we wish to sample stop BIT @3958) (need to delay another 382)				
IRXD_STP	LD  A,23	;7	;Delay loop before STOP BIT
		DEC A		;4
		JR  NZ,$-1	;12/7	;Delay loop =
		IN  A,(Port40)	;11	;NOP for 11tc
		IN  A,(Port40)	;11	;Sample Stop BIT @3957
		OR  A		;4	;(Actual sampling occurs 9 OR 10 tc later)
		JP  P,IRXD_BAD

IRXD_SAVE	LD  A,H		;4	;Fetch received byte
		LD  HL,(RXBHEAD) ;16	;Advance Head Ptr of RX Buffer, Head = Head + 1
		INC L		;4
		LD  (RXBHEAD),HL ;16
		LD  (HL),A	;7	;Save Received byte into RX Buffer
		
		

		
		LD  A,(RXBTAIL)	;13	;Test if buffer has over ran
		CP  L		;4	;If Tail = Head Then Tail = Tail + 1 & Flag overrun
		JR NZ,IRXD_RESET ;Return if NO overrun error
				
		INC A		;Lose A BYTE in buffer and count the overrun
		LD  (RXBTAIL),A

		LD   HL,RX_ERR_OVR
		JR 	IRXD_TINC
IRXD_INC	LD   HL,RX_ERR_LDRT ;10
		JR 	IRXD_TINC
IRXD_BAD	LD  HL,RX_ERR_STOP
IRXD_TINC	CALL TINC

IRXD_RESET	LD	A,(HW_LIST)	;Skip Resync if FP not present
		OR	A
		JR  Z,	IRXD_WS

		IN	A,(Port40)	;Resync the SCAN_PTR
		INC	A
		AND	7
		OR  LOW LED_DISPLAY	;LED_DISPLAY is at xxEO (it's ok to overlap in this order)
		LD	(SCAN_PTR),A	;Save Scan Ptr @ Next Scan Output

		LD	HL,(SCAN_PTR)	;Fetch next byte to output
		LD	A,(HL)
		OUT	(Port40),A	;Output ASAP to satisfy Interrupt Flag
		LD	(SCAN_LED),A	;Save for next interrupt

IRXD_WS		IN   	A,(Port40)	;11	;Sample BIT
		RLCA		;4	;Get BIT 
		JR  NC,	IRXD_WS		;Wait for STOP bit

		JP	ISR_RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - Timer Tics
;Refresh next LED Display
;**IROAR**
ISR_TIMER	LD	A,(SCAN_LED)	;13 ZMC-Display Refresh / Reset Int
		OUT	(Port40),A	;11 Output ASAP to satisfy Interrupt Flag

		LD	HL,ISR_FLAGS	;10
		BIT	0,(HL)		;12
		JP  NZ,	ISR_RET		;10

ISR_EXTIMER	;Extended Timer Int.
					;If ISR NEST level = 0 then save the stack and run on ISR Stack

		SET	0,(HL)		;ISR_FLAGS (Prevent addition entries to this routine)

		LD	(SP_ISR_SAVE),SP ;20 ;ALTERNATE TIMER STACK
		LD	SP,STACK_ISR1	;10  ;ALTERNATE TIMER STACK

		INC	L		;RRSTATE
		RLC	(HL)		;RRSTATE.0=0 Additional interrupts shall return to ROM
					;RRSTATE.1=RRSTATE.0 Save RAM/ROM selection for exit from this TIMER ISR
	
		EX	AF,AF'		;Revert to regular registers in the event of another interrupt.
		EI			;4  Allow RXD interrupts
		EXX

;Time critical functions are over.  		
;We've entered an interrupt and want to stay in for a while to update the front panel.
;But we want serial interrupts to be allowed to continue
;Alternate registers have been relinquished for other Interrupts.
;Must Push our own.

;On First entry here, we have a new stack, old stack is saved at SP_ISR_SAVE and return address is on the old stack

		PUSH	HL
		PUSH	AF

					;********************************* Tic counter - Advance
		LD	HL,(TicCounter)	;16 Advance Timer Counter
		INC	HL		;6
		LD	(TicCounter),HL	;16
					;st=38

		LD	A,(HW_LIST)	;Exit ISR if FP not present
		OR	A
		JR  Z,	ISR_EXTIMER_RET

					;********************************* SCAN RESYNC
		IN	A,(Port40)	;11 Resync the SCAN_PTR
		INC	A		;4  Advance to next column to match column after next OUT
		AND	7		;7
		OR  LOW LED_DISPLAY	;7  LED_DISPLAY is at xxEO (it's ok to overlap in this order)
		LD	(SCAN_PTR),A	;13 Save Scan Ptr @ Next Scan Output
		LD	HL,(SCAN_PTR)	;16 Fetch next byte to output
		LD	A,(HL)		;7
		LD	(SCAN_LED),A	;13 Save for next interrupt
					;st=78


					;********************************* HALT TEST
		LD	HL,(HALT_TEST)	;Ignore HALT Test in SD Card Mode
		JP	(HL)
DO_HALT_TEST
		LD	HL,(SP_ISR_SAVE);10 Get PC
		CALL	LD_HL_HL	;17+43
		DEC	HL		;6
		LD	A,(HL)		;7 Fetch Previous Instruction
		CP	0x76		;7 Is HALT?
		JP  Z,	ICMD_BREAK_RET	;10
					;st=111
SKIP_HALT_TEST


;Keyboard / Display Update / Keyboard Commands or Entry
					;********************************* KEYBOARD SCANNING
		LD	HL,XTIMER_TIC
		DEC	(HL)
		JP  M,	ISR_EXXTIMER
		
;*************************************************** EXIT THE EXTENDED (INTERRUPTABLE) ISR

ISR_EXTIMER_RET	POP	AF
		POP	HL

		DI
		LD	SP,(SP_ISR_SAVE) ;RESUME SAVED STACK

		EX	AF,AF'		;Swap Registers for Jump Back
		EXX

		LD	HL,ISR_FLAGS
		RES	0,(HL)		;Reset NEST level to 0

		INC	L		;RRSTATE
		RRC	(HL)		;RRSTATE.0=RRSTATE.1 Restore RAM/ROM selection
		JP	ISR_RET

;Somewhere in High RAM...
;
;ISR_RET	LD	A,(RRSTATE)	;Restore RAM/ROM selection
;		OUT	(RAMROM),A
;		EX	AF,AF'		;Restore swapped Registers
;		EXX
;		EI
;		RETI			;Return to Mainline code


ISR_EXXTIMER				;Extra Extended Timer (8 times every 32mSec)

		IN	A,(Port40)	;11 Read KEY down & ScanPtr
		LD	HL,KBPORTSAMPLE	;10
		LD	(HL),A
		AND	7
		LD	L,A		;HL->BIT_TABLE
		LD	A,(HL)
		LD	L,LOW KBCOLSAMPLED ;HL->KBCOLSAMPLED
		OR	(HL)
		LD	(HL),A		;Save bit map of columns sampled
				
		INC	HL		;HL->KBPORTSAMPLE
		LD	A,(HL)
		AND	7
		BIT 	3,(HL)		;8  Test ROW-0
		JR  NZ,	IKEY0_UP	;12 Jump if key UP
		OR	0x80		;Flag a Key is down
		INC	HL		;HL->KBHEXSAMPLE
		LD	(HL),A		;Save HEX key
		DEC	HL		;HL->KBPORTSAMPLE
		
					;Sample input again for Row-1 test
IKEY0_UP
		BIT 	4,(HL)		;8 Test ROW-1
		JR  NZ,	IKEY1_UP	;12 Jump if key UP
		OR	0x88		;Flag a key is down AND set bit 3 so key is between 8 and E
		INC	HL		;HL->KBHEXSAMPLE
		LD	(HL),A		;Save HEX key
		DEC	HL		;HL->KBPORTSAMPLE
		
IKEY1_UP		
					;   Test for all columns Scanned
		DEC	HL		;HL->KBCOLSAMPLED 
		LD	A,(HL)		;   *ALL keys scanned when KBCOLSAMPLED = 0xFF
		INC	A		;4
		JP  NZ,	IKEY_SCAN_END	;10
		
		LD	(HL),A		;KBCOLSAMPLED = 0x00 for next Scan

;- - - - - - - - - - - - - - - - - - - - - - - - 
;Keys and Display update on Column 7 Only
		
		INC	HL		;HL->KBPORTSAMPLE
		BIT 	5,(HL)		;8  Test F KEY
		JR  Z,	IKEYF_UP	;12 Jump if key UP
					;When F key is down, it can serve as either F or Shift.
					;
		LD	A,(KEYBFMODE)	;Check the F MODE (shift key or HEX key)
		INC	HL		;HL->KBHEXSAMPLE
		OR	(HL)		;8F=HEX INPUT, 90=Shiftable
					;so when F is pressed:
					;  If 8F it overrides all the other keys and KBHEXSAMPLE=8F
					;  If 90 it OR's with KBHEXSAMPLE to become 9X where X is previous key held down
		LD	(HL),A		;Save HEX key
		DEC	HL		;HL->KBPORTSAMPLE
		
IKEYF_UP


;-Keyboard Scanning, only after scanning Column 7 we are here with the following:
;	KeyPad	KBHEXSAMPLE
;	no-key	0000 0000
;	  0	1000 0000
;	  1	1000 0001
;	  2	1000 0010
;	  3	1000 0011
;	  4	1000 0100
;	  5	1000 0101
;	  6	1000 0110
;	  7	1000 0111
;	  8	1000 1000
;	  9	1000 1001
;	  A	1000 1010
;	  B	1000 1011
;	  C	1000 1100
;	  D	1000 1101
;	  E	1000 1110
;	  F	1000 1111
;	  ^F			1001 xxxx (xxxx=any key pressed during the scan from Column 0)

		INC	HL		;HL->KBHEXSAMPLE
		LD	A,(HL)		;7  Get new HEX sample		
		LD	(HL),0		;10 Zero KBHEXSAMPLE for next scan
		LD	HL,XTIMER_TIC
		LD	(HL),20		;Start new Scan in 20 Tics

IKEY_DEBOUNCE	;A=current key scan or 0x00 for no key.
		LD	HL,(KEYBSCANPV) ;16 Get previously saved scanned key and timer
		CP	L		;4
		JR  Z,	IKEYP_NCOS	;12 Jump if NO Change of State
		LD	H,3		;Timer = 3 (Controls how sensitive the keyboard is to Key Inputs)
		LD	L,A		;Previous scan=current scan

IKEYP_NCOS	DEC	H		;4  Timer = Timer - 1
		LD	(KEYBSCANPV),HL	;16 Save previous scan & timer
		JP  Z,	IKEYP_EVENT	;10 Jump when Timer = 0
		JP  P,	IK_NOKEY_EVENT	;10 Jump when Timer = 1 to 7F
					
					;When Timer underflows to FF, we can use the range from FF to 80
					;to perform time delayed auto repeat for the key.
					
		LD	A,0xD0		;Sets when to repeat (closer to FF, faster)
		CP	H
		JP  NZ, IK_NOKEY_EVENT
					;Timer then "lives" between D4 and D0 causing auto repeat of
					;present key pressed (even if it's no key).
		
		LD	A,0xD4		;Sets how fast to repeat (closer to "when to repeat" faster)
		LD	(KEYBSCANTIMER),A ;Save timer
		LD	A,L		;Fetch keyscan


IKEYP_EVENT	;A=current key scan or 0x00 for no key (either after debounce or as repeat)
		LD	HL,KEY_PRES_EV	;Point HL to previously saved/processed Key
		OR	A
		JR  Z,	IK_KEYUP_EVENT
		
IKEYP_EVENT_DN				;When A<>0, It's a KEY DOWN EVENT
		CP	0x90		;Is it Shift key down?
		JP  NZ,	IK_KEYDN_EVENT	;Jump to process key down if it's NOT a shift key
					;Special consideration given here for Shift Key down.

		BIT	4,(HL)		;Test bit 4 of KEY_PRES_EV (previously saved/processed key)
		LD	(HL),A		;Save the 0x90 to KEY_PRES_EV
					;Exit with just the Shift Key pressed (wait for the next shifted key to come in)
		JP  Z,	IKEY_DONE	;If previously saved key was not a shifted key, keep the 0x90
					;If shift key is held down long enough to repeat, it then becomes F key
		DEC	A		;Otherwise, reduce the shift key to a simple "F" key (0x90 - 1 = 0x8F)
		JP	IK_KEYDN_EVENT

IK_KEYUP_EVENT	;*************************************************** KEY UP EVENT
					;When A=0, It's a KEY UP EVENT
		INC	HL
		LD	(HL),0
		DEC	HL
		LD	A,(HL)		;Fetch the previous key down code
		CP	0x90
		JP  NZ,	IKEY_DONE	;Exit if not the shift key going up
					;Otherwise, if it was the Shift key going up....
		DEC	A  ;90->8F	;replace it with a simple "F" key
		;JP	IK_KEYDN_EVENT	;and execute the key down event.
		INC	HL
		LD	(HL),A
		DEC	HL


IK_KEYDN_EVENT	;*************************************************** KEY DOWN EVENT
		LD	(HL),A		;Save Last Key Down (for Shift Testing)
		INC	HL
		CP	(HL)
		JR   Z,	IK_RTN
		INC	(HL)
		DEC	(HL)
		JR  NZ,	IK_NOKEY_EVENT
		LD	(HL),A

IK_RTN		LD	HL,LED_ANBAR
		SET	0,(HL)		;ANBARLED 0 = BEEPER
		LD	HL,BEEP_TO
		SET	1,(HL)		;Time out beep in 2 counts

		LD	HL,(KEY_EVENT)
		JP	(HL)
;KEY_EVENT_DISPATCH
;Execute different routines based on Users actions.
;Possible choices within this firmware include:
;IMON_CMD	- Menu Command Input (default and initial setting).  Resets to this even upon IKC_RESET_CMD
;ICMD0_R	- HEX Input Mode to select a Register (valid input is 0-12 for the 13 registers). Value saved in RegPtr.
;ICMD_BYTE	- BYTE Input Mode. IK_HEXST tracks state machine of routine. IK_HEXH store the value.  Execute @HEX_READY when done.
;ICMD_WORD	- WORD Input Mode. IK_HEXST tracks state machine of routine. IK_HEXH & IK_HEXL store the value.  Execute @HEX_READY when done.
;ISET_PRESSED	- Saves Key Pressed for User, does not process any keys.

IK_NOKEY_EVENT	;*************************************************** NO KEY EVENT


					;********************************* <Ctrl>-C Checking
		LD	HL,(CTRL_C_CHK)	;16 <Ctrl>-C check +77
		JP	(HL)		;4
					;st=97
CTRL_C_RET

		LD	HL,IK_TIMER	;10
		LD	A,(HL)		;7 Time out any pending Monitor Input
		OR	A		;4
		JP Z,	IKEY_DONE	;10 st=31
		DEC	(HL)
		JP NZ,	IKEY_DONE

					;IK Timer Expired Event
IKC_RESET_CMD				;Upon time out, return monitor to CMD input
		LD	HL,(LDISPMODE)
		LD	(DISPMODE),HL
		LD	HL,IMON_CMD
		LD	(KEY_EVENT),HL
		LD	HL,KEYBFMODE	;Shiftable Keyboard
		LD	(HL),0x90

IKC_REFRESH	LD	A,(ANBAR_DEF)	;Refresh Display
		LD	(LED_ANBAR),A
IKR_QREFRESH	LD	A,-1
		LD	(IK_HEXST),A	;Zero HEX Input Sequencer
		LD	A,1		;Force Quick Refresh of Label
		LD	(DISPLABEL),A
		;JP	IKEY_DONE
IKEY_DONE


		;*************************************************** UP DATE LED DISPLAY
		LD	HL,(DISPMODE)	;16 +242 (for Display Memory)
		JP	(HL)		;4
IDISP_RET
		LD	HL,BEEP_TO	;10
		DEC	(HL)		;11
		JR Z,	IKEY_NO_BEEP	;10
		LD	HL,LED_ANBAR	;10  ANBARLED 0 = BEEPER
		SET	0,(HL)		;15	BEEP ON WHEN BEEP_TO > 1
		JR	IKEY_SCAN_END
		
IKEY_NO_BEEP	INC	(HL)		;11
		LD	HL,LED_ANBAR	;10
		RES	0,(HL)		;15  ANBARLED 0 = BEEPER
					;st=67
					
IKEY_SCAN_END	

		LD	HL,(UiVec)	;16  Do a User Interrupt Vector
		CALL	VCALL_HL
		JP	ISR_EXTIMER_RET

UiVec_RET	RET			;Default Return for UiVec

ICMD_BREAK	LD	A,0xFE
ICMD_BREAK_RET	LD	(SOFT_RST_FLAG),A	;FE=RESTART, D1=SINGLE STEP, F6=HALT, CC=<Ctrl>-C
		LD	A,(ANBAR_DEF)	;Soft Restart only allowed while in Run Mode
		AND	2		;Run mode LED
		JP  Z,	IKEY_DONE

		DI
		POP	AF
		POP	HL
		LD	SP,(SP_ISR_SAVE)
		JP	0
		
CTRL_C_TEST	LD	HL,(RXBHEAD)	;16
		LD	A,(HL)		;7
		LD	HL,CTRL_C_TIMER	;10
		CP	3		;7  Compare <Ctrl>-C
		JP  Z,	CTRL_C_IN_Q	;10
		LD	(HL),10		;10
CTRL_C_IN_Q	DEC	(HL)		;7
		JP  NZ,	CTRL_C_RET	;10  st=77
		LD	A,0xCC
		JP	ICMD_BREAK_RET

CTRL_C_CHK_ON	LD	HL,CTRL_C_TEST
		LD	(CTRL_C_CHK),HL
		RET
CTRL_C_CHK_OFF	LD	HL,CTRL_C_RET
		LD	(CTRL_C_CHK),HL
		RET



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Keyboard Monitor
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;============================================================================
; No Keyboard activity, Save Key for User.
ISET_PRESSED	LD	(KEY_PRESSED),A
		JP	IKEY_DONE


;============================================================================
;	IMON - Monitor Loop
;
; This is the main executive loop for the Front Panel Emulator, Dispatch the Command
;============================================================================
IMON_CMD	LD	HL,IMON_TBL
		AND	0x1F
		RLCA			;X2
		CALL	ADD_HL_A
		CALL	LD_HL_HL	; HL = (HL)
		JP	(HL)

ICMD4_EXEC	LD	HL,ISR_FLAGS
		RES	0,(HL)		;Reset NEST level to 0
		JP	GO_EXEC_T

IMON_TBL	DW	ICMD0		;0 = Display Register
		DW	IKEY_DONE	;ICMD1
		DW	IKEY_DONE	;ICMD2
		DW	IKEY_DONE	;ICMD3
		DW	ICMD4_EXEC	;4 = GO Execute
		DW	ICMD5		;5 = Input Port
		DW	ICMD6		;6 = Output Port
		DW	ICMD7		;7 = Single Step
		DW	IKEY_DONE	;ICMD8
		DW	IRAMROMBANK	;ICMD9
		DW	ICMDA		;A = Advance Element
		DW	ICMDB		;B = Backup Element
		DW	IKEY_DONE	;ICMDC
		DW	ICMDD		;D = Alter Element
		DW	ICMDE		;E = Display Memory
		DW	IKEY_DONE	;ICMDF
		DW	IKEY_DONE	;ICMD10 (Shift-0 Can't happen, you get hard reset)
		DW	IKEY_DONE	;ICMD11
		DW	IKEY_DONE	;ICMD12
		DW	IKEY_DONE	;ICMD13
		DW	IKEY_DONE	;ICMD14
		DW	IKEY_DONE	;ICMD15
		DW	IKEY_DONE	;ICMD16
		DW	IKEY_DONE	;ICMD17
		DW	IKEY_DONE	;ICMD18
		DW	IKEY_DONE	;ICMD19
		DW	IRAMROMBANK	;ICMD1A
		DW	IKEY_DONE	;ICMD1B
		DW	IKEY_DONE	;ICMD1C
		DW	IKEY_DONE	;ICMD1D
		DW	ICMD_BREAK	;ICMD1E
		DW	IKEY_DONE	;ICMD1F (Shift-F Can't happen)


ICMD0		CALL	WRITE_BLOCK	;0 = Display Register
		DW	LDISPMODE	;Where to write
		DW	7		;Bytes to write
		DW	IDISP_REG	;(LDISPMODE)
		DW	IDISP_REG	;(DISPMODE)
		DW	ICMD0_R		;(KEY_EVENT) Switch to HEX Input Mode
		DB	80		;(IK_TIMER)
		LD	HL,LED_ANBAR
		SET	6,(HL)		;ANBARLED 6 = Enter Register
		JP	IKEY_DONE

ICMDE		CALL	WRITE_BLOCK	;E = Display Memory
		DW	LDISPMODE	;Where to write
		DW	14		;Bytes to write
		DW	IDISP_MEM	;(LDISPMODE)
		DW	IDISP_MEM	;(DISPMODE)
		DW	ICMD_WORD	;(KEY_EVENT) Switch to HEX Input Mode
		DB	80		;(IK_TIMER)
		DB	0x8F		;(KEYBFMODE) HEX Keyboard Mode (F on press)
		DB	0		;(DISPLABEL)
		DB	-1		;(IK_HEXST)
		DW	LED_DISPLAY	;(HEX_CURSOR) @d1
		DW	HEX2ABUSS	;(HEX_READY)

		LD	HL,LED_ANBAR
		SET	5,(HL)		;ANBARLED 5 = Enter Mem Loc
		JP	IKEY_DONE


ICMD_WORD	LD	HL,(HEX_CURSOR)
		CALL	LED_PUT_HEX_HL
		LD	(HEX_CURSOR),HL
		LD	HL,IK_HEXST
		INC	(HL)
		JR NZ,	ICMD_WORDN1	;Do 1st digit

		LD	HL,(DISPMODE)
		LD	(LDISPMODE),HL
		LD	HL,IDISP_RET
		LD	(DISPMODE),HL	;No Display Update while HEX Input Mode

		LD	HL,(HEX_CURSOR)
		LD	A,0x81		;Underscore
		LD	(HL),A		;Display X _
		INC	L
		LD	(HL),A		;Display X _ _
		INC	L
		LD	(HL),A		;Display X _ _ _
		LD	HL,IK_HEXH	;HL=DIGITS 1&2
		JR	ICMD_WORD1

ICMD_WORDN1	LD	A,(HL)
		LD	HL,IK_HEXH	;HL=DIGITS 1&2
		DEC	A
		JR Z,	ICMD_WORD2	;Do 2nd digit
		LD	HL,IK_HEXL	;HL=DIGITS 3&4
		DEC	A
		JR NZ,	ICMD_WORD2

ICMD_WORD1	LD	A,(KEY_PRES_EV)	;1st & 3rd DIGIT
		RRD
		JR	ICMD_WORD_RET

ICMD_WORD2	RRD			;2nd & 4th DIGIT
		LD	A,(KEY_PRES_EV)
		RLD

ICMD_WORD_RET	LD	A,160
		LD	(IK_TIMER),A	;Set Time out on Register Selection
		LD	A,(IK_HEXST)	;Advance to next DspMod
		CP	3
		JP NZ,	IKEY_DONE
		LD	HL,(HEX_READY)
		JP	(HL)

HEX2ABUSS	LD	HL,(IK_HEXL)
		LD	(ABUSS),HL
		JP	IKC_RESET_CMD

HEX2REG		LD	A,(RegPtr)	;Select Register
		PUSH	DE
		LD	DE,(IK_HEXL)
		CALL	PUT_REGISTER
		POP	DE
		JP	IKC_RESET_CMD


ICMD_BYTE	LD	HL,(HEX_CURSOR)
		CALL	LED_PUT_HEX_HL
		LD	(HEX_CURSOR),HL		
		LD	HL,IK_HEXST
		INC	(HL)
		JR NZ,	ICMD_BYTE2	;Do 1st digit

		LD	HL,(DISPMODE)
		LD	(LDISPMODE),HL
		LD	HL,IDISP_RET
		LD	(DISPMODE),HL	;No Display Update while HEX Input Mode

		LD	HL,(HEX_CURSOR)
		LD	A,0x81		;Underscore
		LD	(HL),A		;Display X _

		LD	HL,IK_HEXH	;HL=DIGITS 1&2
		LD	A,(KEY_PRES_EV)	;1st DIGIT
		RRD
		LD	A,160
		LD	(IK_TIMER),A	;Set Time out on Register Selection
		JP 	IKEY_DONE

ICMD_BYTE2	LD	HL,IK_HEXH	;HL=DIGITS 1&2
		RRD			;2nd DIGIT
		LD	A,(KEY_PRES_EV)
		RLD
		LD	A,(HL)
		LD	HL,(HEX_READY)
		JP	(HL)

HEX2IN_Ptr	LD	(IoPtr),A	;Save Byte input to IoPtr
		JP	IKC_RESET_CMD

HEX2OUT_Ptr	LD	(IoPtr),A	;Save Byte input to IoPtr
		JP	ICMD_IO_OUT

HEX2MEM		LD	HL,(ABUSS)
		LD	(HL),A
		INC	HL
		LD	(ABUSS),HL
		JP	ICMD_AMEM

HEX2OUT_PORT	PUSH	BC
		LD	B,A
		LD	A,(IoPtr)
		LD	C,A
		OUT	(C),B
		POP	BC
		JP	ICMD_IO_OUT

ICMD1
ICMD2
ICMD3
ICMD4

ICMD5		CALL	WRITE_BLOCK
		DW	LDISPMODE	;Where to write
		DW	14		;Bytes to write
		DW	IDISP_IN	;(LDISPMODE)
		DW	IDISP_IN	;(DISPMODE)
		DW	ICMD_BYTE	;(KEY_EVENT) Switch to BYTE Input Mode
		DB	80		;(IK_TIMER)
		DB	0x8F		;(KEYBFMODE) HEX Keyboard Mode (F on press)
		DB	0		;(DISPLABEL)
		DB	-1		;(IK_HEXST)
		DW	LED_DISPLAY+2	;(HEX_CURSOR) @d3
		DW	HEX2IN_Ptr	;(HEX_READY)

		LD	HL,LED_ANBAR
		SET	5,(HL)		;ANBARLED 5 = Enter Mem Loc
		JP	IKEY_DONE

ICMD6		CALL	WRITE_BLOCK
		DW	LDISPMODE	;Where to write
		DW	14		;Bytes to write
		DW	IDISP_OUT	;(LDISPMODE)
		DW	IDISP_OUT	;(DISPMODE)
		DW	ICMD_BYTE	;(KEY_EVENT) Switch to BYTE Input Mode
		DB	80		;(IK_TIMER)
		DB	0x8F		;(KEYBFMODE) HEX Keyboard Mode (F on press)
		DB	0		;(DISPLABEL)
		DB	-1		;(IK_HEXST)
		DW	LED_DISPLAY+2	;(HEX_CURSOR) @d3
		DW	HEX2OUT_Ptr	;(HEX_READY)

		LD	HL,LED_DISPLAY+5
		LD	(HL),0x80	;Blank d6
		INC	L
		LD	(HL),0x80	;Blank d7
		LD	HL,LED_ANBAR
		SET	5,(HL)		;ANBARLED 5 = Enter Mem Loc
		JP	IKEY_DONE



IRAMROMBANK	CALL	WRITE_BLOCK
		DW	DISPMODE	;Where to write
		DW	5		;Bytes to write
		DW	IDISP_RET	;(DISPMODE)
		DW	IRAMROMBANK_CHG	;(KEY_EVENT) Switch to BYTE Input Mode
		DB	80		;(IK_TIMER)
		
		LD	A,(READ_RAMROM)
IDDR_DISP	RRA
		OUT	(ACE_OUT),A	;SET Bank
		JR  NC,	IDRR_ROM
		CALL	LED_HOME_PRINTI
		DB	'ram    ',EOS
		LD	HL,LED_DISPLAY+4
		CALL	LED_PUT_HEX_HL
		JP	IKEY_DONE
		
IDRR_ROM	CALL	LED_HOME_PRINTI
		DB	'rom    ',EOS
		JP	IKEY_DONE

;SET_BANK	
IRAMROMBANK_CHG	LD	A,80
		LD	(IK_TIMER),A
		LD	A,(READ_RAMROM)
		RRA
		JR  NC,	IDRRC_ROM
		INC	A
		BIT	4,A
		JR  NZ,	IDRRC_2ROM
		RLA
		JR	IDRRC_RET

IDRRC_ROM	LD	A,1		;Start at BANK 0 of RAM, bit0 set = RAM
		JR	IDRRC_RET
		
IDRRC_2ROM	XOR	A		;Return to ROM after 15 Banks
IDRRC_RET	LD	(READ_RAMROM),A
		JR	IDDR_DISP




;============================================================================
;	Single Step
;============================================================================
ICMD7		LD	A,(ANBAR_DEF)	;Single step only allowed while in Monitor Mode
		AND	4
		JP  Z,	IKEY_DONE
		
		;DEBUG SINGLE STEP ONLY ALLOWED WITH FP BOARD
		LD	A,(HW_LIST)
		AND	1
		JP  Z,	IKEY_DONE
		

GO_SINGLE	LD	A,(HW_LIST)	;TEST HARDWARE LIST here for Return to Main Menu when
		AND	1		;Single Step was an RS232 input command
		JR  NZ,	GS_OK

		CALL	PRINTI
		DB	CR,LF,"Single Step requires Front Panel",EOS		
		RET	
		
GS_OK		LD	HL,ISINGLE	;Redirect next Interrupt to Single Step
		LD	(INT_VEC),HL
		HALT			;Halt for next interrupt (Aligns TC with INT)

					;The following interrupt code happens		
		;RST	0x38		;13  (11 + 2 wait cycles)
		;EX	AF,AF'		;4
		;EXX			;4
		;LD	HL,(INT_VEC)	;16 Typical Calls are to ISR_DISPATCH
		;JP	(HL)		;4
					;st=41
					
					;On the next interrupt, handle it here
ISINGLE		EX	AF,AF'		;4
		EXX			;4
		LD	A,(SCAN_LED)	;13 ZMC-Display Refresh / Reset Int
		OUT	(Port40),A	;11 Output ASAP to satisfy Interrupt Flag

		IN	A,(Port40)	;11 Resync the SCAN_PTR
		INC	A		;4  Advance to next column to match column after next OUT
		AND	7		;7
		OR  LOW LED_DISPLAY	;7  LED_DISPLAY is at xxEO (it's ok to overlap in this order)
		LD	(SCAN_PTR),A	;13 Save Scan Ptr @ Next Scan Output
		LD	HL,(SCAN_PTR)	;16 Fetch next byte to output
		LD	A,(HL)		;7
		LD	(SCAN_LED),A	;13 Save for next interrupt
					;st=110

		LD	HL,ISINGLE_DONE	;10 Redirect next Interrupt to Single Step
		LD	(INT_VEC),HL	;16

		LD	A,212		;7
					;st=33
					;-----
					;184 (+completion), There are 4096 cycles between interrupts.
					;	 4096 cycles to waste
					;	 -184 cycles to get here
					;	 -525 cycles to execute
					;	=3387 cycles more to waste
					;
					;	Waste Loop = 16 * 212 -5 = 3387
					;
ISINGLE_LP	DEC	A		;4  Count down the cycles to time the next ISR to occur
		JR NZ,	ISINGLE_LP	;12/7 cycle after execution commences
		NOP		
		JP	GO_EXEC		;10 Go Execute the single instruction!
					;(515 T states until executing next instruction)

ISINGLE_DONE	EX	AF,AF'		;4
		EXX			;4
		LD	(SP_ISR_SAVE),SP
		PUSH	HL
		PUSH	AF

		LD	A,0xD1		;ISR being re-entered after the single step
		JP 	ICMD_BREAK_RET	;

;ICMD_BREAK_RET	....
;		DI
;		POP	AF
;		POP	HL
;		LD	SP,(SP_ISR_SAVE)
;		JP	0
		




;============================================================================
GET_DISPMODE	LD	A,(DISPMODE)
		CP  LOW IDISP_REG_DATA
		RET Z				;Z=1 : DISPMODE = REGISTER
		CP  LOW IDISP_MEM_DATA
		SCF
		RET NZ				;Z=0, C=1 : DISPMODE = I/O
		OR	A			;WARNING, If LOW IDISP_MEM_DATA=0 Then ERROR
		RET				;Z=0, C=0 : DISPMODE = MEM

;	if 0x00 = LOW IDISP_MEM_DATA
;	   error "Error, LOW IDISP_MEM_DATA must not be 0x00"
;	endif

;============================================================================
;	Increment Display Element
;============================================================================
ICMDA		CALL	GET_DISPMODE
		JP  Z,	ICMA_REG
		JP  C,	ICMA_IO

		LD	HL,(ABUSS)
		INC     HL
		LD	(ABUSS),HL
		JP	IKR_QREFRESH

ICMA_REG	LD      A,(RegPtr)
		INC	A
		JP	ICMD_SET_REG

ICMA_IO		LD	HL,IoPtr
		INC	(HL)
		JP	IKR_QREFRESH


;============================================================================
;	Decrement Display Element (Reg, I/O, Mem)
;============================================================================
ICMDB		CALL	GET_DISPMODE
		JP  Z,	ICMB_REG
		JP  C,	ICMB_IO

		LD	HL,(ABUSS)
		DEC     HL
		LD	(ABUSS),HL
		JP	IKR_QREFRESH

ICMB_REG	LD      A,(RegPtr)
ICMD0_R		DEC	A		;Adjust so Key 1 = 0 = SP
		AND	0xF
ICMD_SET_REG	CP	13
		JR  C,	ICMD_SR_OK
		XOR	A
ICMD_SR_OK	LD	(RegPtr),A
		JP	IKC_RESET_CMD


ICMB_IO		LD	HL,IoPtr
		DEC	(HL)
		JP	IKR_QREFRESH

;============================================================================
;	Alter Display Element (Reg, I/O, Mem)
;============================================================================
ICMDD		CALL	GET_DISPMODE
		JP  Z,	ICMD_REG
		JP  C,	ICMD_IO

ICMD_AMEM	CALL	WRITE_BLOCK
		DW	LDISPMODE	;Where to write
		DW	14		;Bytes to write
		DW	IDISP_MEM	;(LDISPMODE)
		DW	IDISP_MEM	;(DISPMODE)
		DW	ICMD_BYTE	;(KEY_EVENT) Switch to BYTE Input Mode
		DB	80		;(IK_TIMER)
		DB	0x8F		;(KEYBFMODE) HEX Keyboard Mode (F on press)
		DB	1		;(DISPLABEL)
		DB	-1		;(IK_HEXST)
		DW	LED_DISPLAY+5	;(HEX_CURSOR) @d6
		DW	HEX2MEM		;(HEX_READY)

		LD	HL,LED_ANBAR
		SET	5,(HL)		;ANBARLED 5 = Enter Mem Loc
		SET	4,(HL)		;ANBARLED 4 = Alter
		JP	IKEY_DONE



ICMD_REG	LD	HL,ICMD_WORD	;Switch to WORD Input Mode
		LD	(KEY_EVENT),HL
		LD	HL,HEX2REG
		LD	(HEX_READY),HL
		LD	HL,LED_DISPLAY+3 ;@d4
		LD	(HEX_CURSOR),HL
		;LD	A,0x8F
		;LD	(KEYBFMODE),A	;HEX Keyboard
		LD	HL,LED_ANBAR
		SET	5,(HL)		;ANBARLED 5 = Enter Mem Loc
		SET	4,(HL)		;ANBARLED 4 = Alter
		JP	IKEY_DONE

ICMD_IO		CP  LOW IDISP_IN_DATA
		JP  Z,	ICMD5

ICMD_IO_OUT	CALL	WRITE_BLOCK
		DW	LDISPMODE	;Where to write
		DW	14		;Bytes to write
		DW	IDISP_OUT	;(LDISPMODE)
		DW	IDISP_OUT	;(DISPMODE)
		DW	ICMD_BYTE	;(KEY_EVENT) Switch to HEX Input Mode
		DB	80		;(IK_TIMER)
		DB	0x8F		;(KEYBFMODE) HEX Keyboard Mode (F on press)
		DB	0		;(DISPLABEL)
		DB	-1		;(IK_HEXST)
		DW	LED_DISPLAY+5	;(HEX_CURSOR) @d6
		DW	HEX2OUT_PORT	;(HEX_READY)
		LD	HL,LED_ANBAR
		SET	3,(HL)		;ANBARLED 3 = Send Data to Output Port
		JP	IKEY_DONE


;============================================================================
;	LED Display Register
;============================================================================
IDISP_REG	CALL	LED_HOME
		LD	A,(RegPtr)
		CALL	GET_REGNAME
		CALL	LED_PRINT
		LD	HL,IDISP_REG_DATA
		LD	(DISPMODE),HL

IDISP_REG_DATA	LD	A,(RegPtr)	;13 Then Display Data
		CALL	GET_REGISTER	;17+169
		LD	A,L		;4
		PUSH	AF		;11
		LD	A,H		;4
		LD	HL,LED_DISPLAY+3 ;10
		CALL	LED_PUT_BYTE_HL	;17+165
		POP	AF		;10
		CALL	LED_PUT_BYTE_HL	;17+165
		LD	HL,DISPLABEL	;10
		DEC	(HL)		;7
		JP NZ,	IDISP_RET	;10   sp=629
		LD	HL,IDISP_REG
		LD	(DISPMODE),HL
		JP 	IDISP_RET

;============================================================================
;	LED Display Memory Location
;============================================================================
IDISP_MEM	LD	HL,LED_DISPLAY	;First, Display location
		LD	A,(ABUSS+1)
		CALL	LED_PUT_BYTE_HL
		LD	A,(ABUSS)
		CALL	LED_PUT_BYTE_HL
		LD	A,0x80		;Blank next char
		LD	(HL),A
		LD	HL,IDISP_MEM_DATA
		LD	(DISPMODE),HL
					;Then Display DATA
IDISP_MEM_DATA	LD	HL,(ABUSS)	;16
		CALL	GET_MEM
		LD	HL,LED_DISPLAY+5 ;10
		CALL	LED_PUT_BYTE_HL	;17+165
		LD	HL,DISPLABEL	;10 Repeat Display of Data several times before redisplaying Location
		DEC	(HL)		;7
		JP NZ,	IDISP_RET	;10  st=242
		LD	HL,IDISP_MEM
		LD	(DISPMODE),HL
		JP 	IDISP_RET


;============================================================================
;	LED Display Input Port
;============================================================================
IDISP_IN	CALL	LED_HOME_PRINTI
		DB	'in',EOS
		LD	A,(IoPtr)
		LD	HL,LED_DISPLAY+2
		CALL	LED_PUT_BYTE_HL
		LD	(HL),0x80	;Blank d5
		LD	HL,IDISP_IN_DATA
		LD	(DISPMODE),HL

IDISP_IN_DATA	PUSH	BC
		LD	A,(IoPtr)
		LD	C,A
		IN	A,(C)
		POP	BC
		LD	HL,LED_DISPLAY+5
		CALL	LED_PUT_BYTE_HL
		LD	HL,DISPLABEL
		DEC	(HL)
		JP NZ,	IDISP_RET
		LD	HL,IDISP_IN
		LD	(DISPMODE),HL
		JP 	IDISP_RET


;============================================================================
;	LED Display Output Port		cmd 6
;============================================================================
IDISP_OUT	CALL	LED_HOME_PRINTI
		DB	'ou',EOS
		LD	A,(IoPtr)
		LD	HL,LED_DISPLAY+2
		CALL	LED_PUT_BYTE_HL
		LD	(HL),0x80	;Blank d5
		LD	HL,IDISP_OUT_DATA
		LD	(DISPMODE),HL

IDISP_OUT_DATA	LD	HL,DISPLABEL
		DEC	(HL)
		JP NZ,	IDISP_RET
		LD	HL,IDISP_OUT
		LD	(DISPMODE),HL
		JP 	IDISP_RET

;============================================================================
;	LED Display OFF
;============================================================================
IDISP_OFF	LD	HL,LED_DISPLAY
		LD	A,0x80
		PUSH	BC
		LD	B,8
IDO_LP		LD	(HL),A
		INC	L
		DJNZ	IDO_LP
		POP	BC
		LD	HL,IDISP_RET
		LD	(DISPMODE),HL
		JP 	(HL)

;============================================================================
;	LED Delay	- After a delay for spash screen, display Registers
;============================================================================
IDISP_DELAY	LD	HL,DISPLABEL
		DEC	(HL)
		JP NZ,	IDISP_RET
		JP 	IKC_RESET_CMD

;============================================================================
;PUTS 2 HEX digits to LED Display
;Input:	A=BYTE to display
;	HL=Where to display
;Output: HL=Next LED Display location
LED_PUT_BYTE_HL	PUSH	AF		;11 Save Byte to display (for 2nd HEX digit)
		RRCA			;4
		RRCA			;4
		RRCA			;4
		RRCA			;4
		CALL	LED_PUT_HEX_HL
		POP	AF		;10
		;CALL	LED_PUT_HEX_HL
		;RET			;10  st=165

LED_PUT_HEX_HL	PUSH	HL
		AND	0xF
		LD	H,HIGH LED_HEX
		LD	L,A
		LD	A,(HL)		;Fetch LED Font for HEX digit
		POP	HL
		LD	(HL),A		;Display HEX digit
		INC	L
		RES	3,L
		RET
		
LED_PUT_BYTE	PUSH	HL
		LD	HL,(LED_CURSOR)
		CALL	LED_PUT_BYTE_HL
		LD	(LED_CURSOR),HL
		POP	HL
		RET
		
LED_PUT_HEX	PUSH	HL
		LD	HL,(LED_CURSOR)
		CALL	LED_PUT_HEX_HL
		LD	(LED_CURSOR),HL
		POP	HL
		RET
		
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;LED_PRINT -- Print A null-terminated string @(HL)
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
LED_PRINT:	PUSH	AF
		PUSH	BC
LED_PRINT_LP	LD	A, (HL)
		INC	HL
		OR	A
		JR Z,	LED_PRINT_RET
		LD	C,A
		PUSH	HL
		CALL	PC_LED
		POP	HL
		JR	LED_PRINT_LP
LED_PRINT_RET	POP	BC
		POP	AF
		RET



LED_HOME_PRINTI	CALL	LED_HOME
		
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;PRINT IMMEDIATE
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
LED_PRINTI:	EX	(SP),HL	;HL = Top of Stack
		CALL	LED_PRINT
		EX	(SP),HL	;Move updated return address back to stack
		RET



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;
;	Chapter_12	S D - C A R D
;
;   *******   *******                *******      ***     ********   *******  
;  *********  ********              *********    *****    *********  ******** 
;  **         **    ***             **     **   *** ***   **     **  **    ***
;  **         **     **             **         ***   ***  **     **  **     **
;   ******    **     **   ******    **         *********  ********   **     **
;    ******   **     **   ******    **         *********  ********   **     **
;         **  **     **             **         **     **  **  **     **     **
;         **  **    ***             **     **  **     **  **   **    **    ***
;  *********  ********              *********  **     **  **    **   ******** 
;   *******   *******                *******   **     **  **     **  *******  
;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;String equates

GO_SD_CARD	DI			; Disable Interrupts

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Init all System RAM, enable interrupts
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
GSC_INIT	LD	HL,0x0038
		LD	DE,0x0038
		LD	BC,RST38_LEN
		LDIR

		LD	HL,SDISKA	;Initialize New Variables for SD-CARD operation & full 64K RAM
		LD	(FCB_PTR),HL
		LD	HL,SKIP_HALT_TEST
		LD	(HALT_TEST),HL
	
		CALL	WRITE_BLOCK
		DW	ANBAR_DEF	;Where to write
		DW	16		;Bytes to write
		DB	0x82		;(ANBAR_DEF) = RUN MODE
		DW	GET_REG_RUN	;(GET_REG)
		DW	PUT_REG_RUN	;(PUT_REG)
		DW	CTRL_C_RET	;(CTRL_C_CHK)
		DW	IDISP_RET	;(LDISPMODE)
		DW	IDISP_RET	;(DISPMODE)
		DW	IMON_CMD	;(KEY_EVENT) Initialize to Command Mode
		DB	1		;(IK_TIMER)
		DB	0x90		;(KEYBFMODE) HEX Keyboard Mode (F on release)
		DB	0xFF		;(DISPLABEL)

		EI			;************** Interrupts ON!!!!

		CALL	LED_HOME_PRINTI
		DB	"Sd-CARD",EOS
		
		CALL	INIT_FAT
		RET	NZ		;RETURN IF FAILED

		CALL	PUT_NEW_LINE
		
		LD	A,0xC0
		LD	(VIEW_FLAGS),A	;View File open/not status & Size

					;Filename will be preloaded on auto run
		LD	A,(FILENAME)	;Check for any Auto Run
		OR	A
		JR   Z,	SDC_MENU
		
		CALL	SD_OPEN_FILENAME
		JR Z,	SDC_MENU	;Jump if file not found
		CALL	READ_HEX_EXEC	;Read & Execute HEX file, Return if Error

;----------------------------------------------------------------------------------------------------; SD MENU
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;----------------------------------------------------------------------------------------------------; SD MENU

SDC_MENU:	CALL	PRINTI		;Monitor Start, Display Welcome Message
		DB	CR,LF
		DB	"L              LIST FILES",CR,LF
		DB	"W              WITNESS LOAD",CR,LF
		DB	"? >",EOS

		CALL 	GET_CHAR	;get char
		CP	27
		RET	Z		;MAIN MENU
		AND 	0x5F		;to upper case
		CP	'L'
		JR Z,	DO_DIR		; L = List Files
		CP	'W'
		JR Z,	BOOT_SDVIEW	; W = Witness Load
		JR	SDC_MENU	

;=============================================================================
BOOT_SDVIEW	LD	HL,VIEW_FLAGS	;BIT .0=View HEX Load
		SET	0,(HL)
		CALL 	PRINTI
		DB	" -ON",EOS
		JR	SDC_MENU	
			
;=============================================================================
DO_DIR		CALL	PRINT_DIR
		CALL	INPUT_FILENAME
		RET	C		;Return to Menu if <Esc>
		CALL	PUT_NEW_LINE
		CALL	SD_OPEN_FILENAME
		JR Z,	DO_DIR		;Jump if file not found
		CALL	READ_HEX_EXEC	;Read & Execute HEX file, Return if Error
		JR	DO_DIR


;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_13	FILE operations
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;=============================================================================
;		Destroys A,B,C,IX
;-----------------------------------------------------------------------------
PRINT_DIR	CALL	PRINTI
		DB CR,LF,"\DIRECTORY:",CR,LF,EOS
		CALL	SD_LDIR1
SDLF_LP 	RET	Z			;End of list
		LD	A,(HL)
		CP	33
		JP M,	DD_NEXT
		CP	127
		JP P,	DD_NEXT
		PUSH	HL			;Test if starting cluster is 0, skip file
		POP	IX
		LD	A,(IX+1Ah)
		OR	(IX+1Bh)
		JP Z,	DD_NEXT

		CALL	PRINT_FILENAME
		
		CALL	GET_POS		;Get output position (counted characters after CR)
		CP	64
		CALL P,	PUT_NEW_LINE
DD_TAB_LP	CALL	GET_POS		;TAB OUT 16 CHARS
		AND	0Fh
		JP Z,	DD_NEXT
		CALL	PUT_SPACE
		JR	DD_TAB_LP

DD_NEXT		CALL	SD_LDIRN
		JR	SDLF_LP


;=============================================================================
;Open File	Enter with FILENAME set and FCB_PTR set to desired FCB
;		EXIT Z=1 If File Not Found
;-----------------------------------------------------------------------------
SD_OPEN_FILENAME LD	HL,(FCB_PTR)
		INC	HL		;+1 = FNAME
		EX	DE,HL		
		LD	HL,FILENAME	;Write FILENAME to FCB
		LD	BC,11
		LDIR

;=============================================================================
;Open File	Enter with FCB_PTR set to desired FCB
;		EXIT Z=1 If File Not Found
;-----------------------------------------------------------------------------
SD_OPEN		LD	HL,(FCB_PTR)
		LD	(HL),0		;FSTAT=0, Clear Open Status
		INC	HL		;+1 = FNAME
		LD	DE,FILENAME	;Write FCB Name to FILENAME
		LD	BC,11
		LDIR		
		CALL	SDV_FIND_FILE	;HL = Directory Entry  ;PRINT FILE NAME, FOUND OR NOT
		RET	Z		;Exit if file not found

;FAT-16 Directory Entry
; 46 44 49 53 4b 20 20 20 45 58 45 20 00 00 00 00  FDISK   .EXE
; 00 00 00 00 00 00 36 59 62 1b 02 00 17 73 00 00
;
;Bytes   Content
;0-10    File name (8 bytes) with extension (3 bytes)
;11      Attribute - a bitvector. Bit 0: read only. Bit 1: hidden.
;        Bit 2: system file. Bit 3: volume label. Bit 4: subdirectory.
;        Bit 5: archive. Bits 6-7: unused.
;12-21   Reserved (see below)
;22-23   Time (5/6/5 bits, for hour/minutes/doubleseconds)
;24-25   Date (7/4/5 bits, for year-since-1980/month/day)
;26-27   Starting cluster (0 for an empty file)
;28-31   Filesize in bytes
;
;Z80MC FCB
;
;SDFCB:
;FSTAT		EQU	0	;DS  1	;+0  Status of FCB, 00=File Not Open, 01=File Opened, 80=EOF (Line_Input)
;FNAME		EQU	1	;DS 11	;+1  File name
;AFClus0	EQU	12	;DS  2	;+12 First Cluster of File as given by the Directory Entry.
;CRFClus (FFFF)	EQU	14	;DS  2	;+14 Current Relative Cluster location in file, (0 to F for a system with 32 Sectors per Cluster)
;CAFClus (FFFF)	EQU	16	;DS  2	;+16 Current Absolute Cluster location in file, set to AFClus0, then updated with FAT
;RFSec	 (FFFF)	EQU	18	;DS  2	;+18 Relative Sector being addressed (0 to 500, based 26 sectors per track and 77 tracks Divide by 4)
;SSOC	 (FF's)	EQU	20	;DS  4	;+20 Starting Sector of Cluster, this is the first Sector for that Cluster
;ABS_SEC (FF's)	EQU	24	;DS  4	;+24 Absolute Sector of Current Relative Sector
;FSIZE		EQU	28	;DS  4	;+28 File Size of file (not used, just kept for completness)


SDO_DO		PUSH	HL		;HL Points to FAT Directory Entry
		LD	BC,1Ch		;File Size Offset (into Directory Entry)
		ADD	HL,BC
		EX	DE,HL
		LD	HL,(FCB_PTR)	;HL=FCB
		ADD	HL,BC		;1C is also the offset for FSIZE
		PUSH	HL
		EX	DE,HL
		CALL	MOV_32_HL	;Move (HL) to 32 bit register BCDE, Fetch File Size
		LD	HL,FILESIZE
		CALL	MOV_HL_32	;Save 32 bits to RAM at HL (FILESIZE)
		POP	HL		;HL = FCB.FSIZE
		CALL	MOV_HL_32	;Save 32 bits to RAM at HL (FCB)
		POP	HL

		LD	BC,1Ah		;H=(START CLUSTER)
		ADD	HL,BC
		CALL	LD_HL_HL	;Fetch Starting Custer
		EX	DE, HL		;DE=Starting Cluster
		LD	HL,(FCB_PTR)	;HL=FCB
		LD	(HL),1		;FSTAT=1
		LD	BC,AFClus0 	;offset to AFClus0
		ADD	HL,BC
		LD	(HL),E		;Save Starting Cluster
		INC	HL
		LD	(HL),D
		INC	HL
		LD	B,14
		LD	A,0FFH
		CALL	FILL_BLOCK	;Fill 14 bytes of FF (Nuke pointers to force new calculations)
		
		LD	A,(VIEW_FLAGS)	;BIT .0=View HEX Load, .6=FILE SIZE
		BIT	6,A
		JR  Z,	SDO_RET
		
		CALL	PRINTI
		DB 	" FILE SIZE=0x",EOS
		LD	HL, (FILESIZE+2)
		CALL	PUT_HL
		LD	HL, (FILESIZE)
		CALL	PUT_HL
					
SDO_RET		LD	HL,(FCB_PTR)	;HL=FCB
		OR	0FFh		;Clear Z
		RET
		

;=============================================================================
;LINE INPUT	Enter with FCB_PTR set to desired FCB
;		(Recently opened so pointers will be initialized)
;		if Relative Cluster = 0xFFFF (new open), pointer will init to start of file.
;		Output Z=1 if EOF, HL=FCB_PTR
;		Z=0, HL=Pointer to Line Buffer
;		BC, DE nuked
;		LOGICAL_SEC is used/nuked
;-----------------------------------------------------------------------------
LINE_INPUT	LD	HL,(FCB_PTR)	;Test FSTAT for EOF, marked in FCB Status
		BIT	7,(HL)
		RET	NZ		;Exit if EOF
					;Test for newly opened file
		LD	BC,14		; check Current Relative Cluster <> FFFF
		ADD	HL,BC
		LD	A,0xFF
		CP	(HL)
		JR NZ,	LI_TP		;Jump if file was read from
		INC	HL
		CP	(HL)
		JR NZ,	LI_TP		;Jump if file was read from
					;File has not yet been read...
		LD	BC, 28-15	;Advance to File Size
		ADD	HL,BC
		LD	DE,LI_FILESIZE	;Init LI_FILESIZE with max count of bytes to read
		LD	BC,4
		LDIR			;Copy File Size to Byte counter

		LD	HL,0		;Start at first Logical Sector
		LD	(LI_SDLOG_SEC),HL
		LD	(LOGICAL_SEC),HL
					;FCB_PTR = FILE TO READ
		CALL	DISK_READ	;HL=BUFF
		LD	(LI_SDBUFF_PTR),HL	;Save Disk Buffer pointer
		
LI_TP		CALL	CLEAR_LINE_BUFF

		LD	BC,(LI_FILESIZE) ;Low 16bits of Filesize counter (32bit)
		LD	DE, LINE_BUFF	;Data desination @DE
		LD	HL,(LI_SDBUFF_PTR) ;Data source (SD BUFFER)

LI_LP		CALL	LI_GETDATA	;Fetch byte from SD_RAMBUFFER @HL
					;Advances to read next logical sector if HL>BUFF_SIZE
		
LI_1		LDI			;COPY A BYTE.  (DE)=(HL), INC HL, INC DE, DEC BC
		JP PE,	LI_2		;Jump if BC>0, Not at End of File possible
		
					;BC=0
		PUSH	HL		
		LD	HL,(LI_FILESIZE+2) ;Check upper 32 bits for 0 or decrement size.
		SCF
		SBC	HL,BC		;HL=HL-0000-1 (HL=HL-1 with *Borrow*)
		JR NC,	LI_1B
		EX	DE,HL
		LD	HL,(FCB_PTR)
		SET	7,(HL)		;SET FSTAT EOF FLAG
		POP	HL
		JR	LI_EOL
		
		
LI_1B		LD	(LI_FILESIZE+2),HL
		POP	HL
	
LI_2		CP	CR		;Test for CR
		JR NZ,	LI_3
					;If CR, check if next char is LF
		CALL	LI_GETDATA	;Look ahead to next char
		CP	LF
		JR NZ,	LI_EOL		;NO LF? Consider the CR as EOL
		INC	A		;Nuke the LF
			
LI_3		CP	LF		;Test if LF
		JR Z,	LI_EOL		;Jump if LF char copied
LI_4		LD	A,LOW LINE_BUFFEND
		CP	E
		JR NZ,	LI_LP

LI_EOL		LD	(LI_SDBUFF_PTR),HL	;Save SD_READ_BUFFER POINTER for next line
		LD	(LI_FILESIZE),BC ;Save count down for next line
		LD	HL, LINE_BUFF	;Return with HL=LINE_BUFF
		XOR	A		;Z=1
		RET

LI_GETDATA	LD	A,H		;Check for SD buffer over bounds
		CP	HIGH (SD_RAM_BUFFER + 0x200)
		LD	A,(HL)
		RET	NZ
		LD	HL,(LI_SDLOG_SEC) ;Read next 512 sector from file
		INC	HL
		LD	(LI_SDLOG_SEC),HL
		LD	(LOGICAL_SEC),HL
		PUSH	BC
		PUSH	DE
		CALL	DISK_READ	;HL=BUFF
		POP	DE
		POP	BC
		LD	A,(HL)
		RET

CLEAR_LINE_BUFF	LD	HL,LINE_BUFF	;Clear LINE_BUFF
		LD	B, LOW (LINE_BUFFEND - LINE_BUFF) +2
		CALL	CLEAR_BLOCK	;LINE FULL OF EOS
		RET

;=============================================================================
;	Read a HEX file and execute it
;-----------------------------------------------------------------------------
READ_HEX_EXEC	LD	HL,LINE_INPUT
		LD	(HEX_SOURCE),HL

		CALL	READ_HEX_FILE
		RET	C		;CY=1 ERROR encountered

		LD	A, (VIEW_FLAGS)	;When veiwing load, prompt EXECUTE
		RRCA
		JP  NC,	GH_EXEC

		CALL	PRINTI		;End of File Reached
		DB CR,LF,"-EOF-"
		DB CR,LF,"Execute?",EOS
		
		CALL	GET_CHAR
		AND 	0x5F		;to upper case
		CP	'Y'
		JP Z,	GH_EXEC
		RET	



;=============================================================================
;	Read a HEX file
;Input:	HEX_SOURCE must point to the routine that will fill LINE_BUFF with the next line
;Output	CY=1 ERROR encountered,  Z=0 Time Out
;-----------------------------------------------------------------------------
READ_HEX_FILE	CALL	LED_HOME_PRINTI
		DB	'FL ',EOS
		LD	HL, 0		;Zero Line Counter
		LD	(RHF_LINES),HL
		

RHF_LOOP	LD	HL,(HEX_SOURCE)
		CALL	VCALL_HL	;JP	(HL)
		SCF
		CCF			;CY=0
		RET	NZ		;Exit if Z=1 (End of File or Serial Time Out)
				
		LD	HL, (RHF_LINES)	;Increment Line Counter
		INC	HL
		LD	(RHF_LINES),HL
		
		LD	A,H		;Display line on LED
		LD	B,L
		LD	HL,LED_DISPLAY+3
		CALL	LED_PUT_BYTE_HL
		LD	A,B
		CALL	LED_PUT_BYTE_HL
		
		LD	A, (VIEW_FLAGS)	;BIT .0=View HEX Load, .6=FILE SIZE
		RRCA
		JR  NC,	RHF_DO
		
		LD	HL, LINE_BUFF	;Reload HL
		CALL	PRINT		
				
RHF_DO		CALL	RHF_LINE	;-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_
		RET	C		;CY=1 ERROR encountered
		JR  NZ,	RHF_LOOP	;Z=1 when End Of File record encountered
		RET
		
;-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_
RHF_LINE	LD	HL, LINE_BUFF	;Reload HL
		LD	A,(HL)
		CP	':'		;Step 0, look for colon
		JR Z,	RHF_OK1

		OR	A		;If LINE BLANK then EOF
		RET	Z		;Z=1,CY=0 EOF

		CALL	PRINTI
		DB CR,LF,"COLON",EOS
		JR	RHF_ERR_ON_LINE

RHF_OK1		PUSH	HL
		POP	DE		;DE=HL=LINE_BUFF
		LD	C,0		;Convert remaining HEX characters to Binary values
RHF_BYTE_LP2	LD	B,2
RHF_BYTE_LP	INC	HL		;
		LD	A,(HL)
		CALL	ASC2HEX		;Process LSD char
		JR C,	RHF_NOT_HEX
		EX	DE,HL
		RLD			;Shift digit to LINE_BUFF @ DE
		EX	DE,HL
		DJNZ	RHF_BYTE_LP	;Repeat for 2 HEX digits
		LD	A,(DE)		;Update Checksum
		ADD	A,C
		LD	C,A
		INC	DE
		JR	RHF_BYTE_LP2	;Repeat for all HEX pairs
		
RHF_NOT_HEX	CALL	IS_CRLF		;Test if final char is CR or LF
		JR Z,	RHF_OK2
	
		CALL	PRINTI
		DB CR,LF,"INVALID CHAR",EOS
		JR	RHF_ERR_ON_LINE

RHF_OK2		DEC	B
		DEC	B
		JR Z,	RHF_OK3
		CALL	PRINTI		;If B<>2 then there was a PAIRING error of HEX Digits
		DB CR,LF,"HEX PAIRING",EOS
		;JR	RHF_ERR_ON_LINE
		
RHF_ERR_ON_LINE	CALL	PRINTI
		DB " ERROR ON LINE# 0x",EOS
		LD	HL, (RHF_LINES)
		CALL	PUT_HL
		SCF			;Return with ERROR
		RET			;Z=?,CY=1 ERROR


RHF_OK3		LD	A,(LINE_BUFF)	;Get Size of HEX line
		LD	A,E		;Get number of bytes processed from hex line
		SUB	LOW LINE_BUFF
		LD	HL,LINE_BUFF
		SUB	(HL)		;Subtract the number of bytes indicated in the HEX Line
		CP	5
		JR Z,	RHF_OK4
		CALL	PRINTI		;
		DB CR,LF,"LENGTH",EOS
		JR	RHF_ERR_ON_LINE
		
; LLAAAAFF000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1FXX
;:200000000000000000000000000000000000000000000000000000000000000000000000E0
;
;Length is 0x20 bytes, total line length is then 0x25 (len+add+field+data+chksum)  1+2+1+20+1

RHF_OK4		LD	A,C
		OR	A
		JR Z,	RHF_OK5
		CALL	PRINTI		;If B<>2 then there was a PAIRING error of HEX Digits
		DB CR,LF,"CHECKSUM",EOS
		JR	RHF_ERR_ON_LINE

RHF_OK5		LD	HL,LINE_BUFF+3	;FIELD TYPE
		LD	A,(HL)
		OR	A
		JR Z,	RHF_OK6
		DEC	A
		RET	Z		;Z=1,CY=0 EOF
		CALL	PRINTI		;If B<>2 then there was a PAIRING error of HEX Digits
		DB CR,LF,"UNKNOWN RECORD TYPE, IGNORED",EOS
		CALL	RHF_ERR_ON_LINE
		JR	RHF_DO_NEXTL
		
RHF_OK6		DEC	HL		;ADDRESS (BIG ENDIAN)
		LD	E,(HL)
		DEC	HL
		LD	D,(HL)
		
		DEC	HL		;Length
		LD	A,(HL)
		LD	C,A
		LD	B,0

		LD	HL, VIEW_FLAGS
		BIT	1,(HL)
		JP  NZ,	RHF_OK7
		SET	1,(HL)
		EX	DE,HL
		LD	(GH_START), HL
		EX	DE,HL

RHF_OK7		LD	HL,LINE_BUFF+4	;DATA
		LDIR
RHF_DO_NEXTL	XOR	A
		DEC	A
		RET			;Z=0,CY=0 IGNORE


IS_CRLF		CP	CR
		RET 	Z
		CP	LF
		RET

GH_EXEC		LD	A, (VIEW_FLAGS)
		BIT	1,A
		JR NZ,	GH_EXEC_GO
		CALL	PRINTI
		DB CR,LF,"!!! START ADDRESS NOT SET",EOS
		RET

GH_EXEC_GO	LD	HL, (GH_START)	;HL = JUMP ADDRESS
GH_EXEC_GO2	CALL	PRINTI
		DB CR,LF,"Execute at:",EOS
		
		CALL	PUT_HL
		CALL	PUT_NEW_LINE

		PUSH	HL
		LD	HL,HR_EXEC_GO_SUB	;Copy from
		LD	DE,HR_EXE_GO		;Copy to
		LD	BC,HR_EXEC_GS_LEN	;Length=11 bytes
		LDIR
		POP	HL
		JP	HR_EXE_GO

HR_EXEC_GO_SUB	LD	A,1
		LD	(RRSTATE),A
		OUT	(RAMROM),A
		JP	(HL)	;Execute the Get HEX

HR_EXEC_GS_LEN	EQU	$-HR_EXEC_GO_SUB



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_14	FILE Support Routines
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;=============================================================================
		;Find File w/ Verbose Output
		;Call with File Name set in FILENAME.EXT
		;Return Z=1 File Not Found
		;	Z=0 File Found, HL = Ptr to Directory Entry in SD_RAM_BUFFER
		;Destroys A,B,C
SDV_FIND_FILE	LD	A,(VIEW_FLAGS)	;BIT .0=View HEX Load, .6=Display FILE SIZE, .7=Display File name & Found or Not
		RLCA
		JR NC,	SD_FIND_FILE	;Call the Find File routine
		LD	HL,FILENAME
		CALL	PRINT_FILENAME
		CALL	SD_FIND_FILE	;Call the Find File routine
		PUSH	AF
		JP NZ,	SDV_FOUND	;Print Yah or Nah
		CALL 	PRINTI
		DB " -NOT FOUND",EOS
		POP	AF
		RET
SDV_FOUND	CALL 	PRINTI
		DB " -EXISTS",EOS
		POP	AF
		RET

;=============================================================================
		;Find File
		;Call with File Name set in RAM variable: FILENAME.EXT
		;Return Z=1 File Not Found
		;	Z=0 File Found, HL = Ptr to Directory Entry in SD_RAM_BUFFER
SD_FIND_FILE	CALL	SD_LDIR1
SDFF_LP 	RET Z			;End of list
		CALL	CMP_FILENAME	;Compares file name at (HL) with FILENAME.EXT in RAM
		RET NZ			;FILE FOUND
		CALL	SD_LDIRN
		JP	SDFF_LP


;=============================================================================
;Directory Routines.  1st Routine to start/init the search, 2nd routine to continue the search
;-----------------------------------------------------------------------------
;Call this routine to initialize and start the HL Pointer to the first Directory Entry
SD_LDIR1	LD	HL,DIR_SECTOR	;SEC_PTR = DIR_SECTOR
		CALL	MOV_32_HL
		LD	HL, (ROOTDIR_SIZE)	;ENT_COUNT = ROOTDIR_SIZE (to count down directory entries searched)
		LD	(ENT_COUNT), HL
		

SD_FETCH	CALL	SD_READ_SEC		;Fetch a ROOT DIRECTORY sector
		LD	HL,SD_RAM_BUFFER	;(Re)start H at start of Sector
SD_TEST		XOR	A			;EXIT Z=0 if there is a File at this entry
		CP	(HL)
		RET

;=============================================================================
;Call this routine to advance to the next Directory Entry (loads next sector and restarts HL as needed)
;-----------------------------------------------------------------------------
SD_LDIRN	LD	BC,20h		;Advance to next file entry
		ADD	HL,BC
		LD	A,H
		CP	HIGH (SD_RAM_BUFFER+200h)
		JP NZ,	SD_TEST		;Check if extended beyond this sector

		XOR	A		;Return Z=1 if no more files
		LD	HL, (ENT_COUNT)
		LD	BC,-16
		ADD	HL,BC
		LD	(ENT_COUNT), HL
		RET NC			;Out of Directory entries
		LD	A,H
		OR	L
		RET Z			;Out of Directory entries

		LD	HL,SEC_PTR	;Advance to next SECTOR
		CALL	MOV_32_HL
		CALL	INC_32
		JP	SD_FETCH


;=============================================================================
;	Prints Filename at HL (DESTROYS A)
;-----------------------------------------------------------------------------
PRINT_FILENAME	PUSH	HL
		PUSH	BC
		LD	B,8
		CALL	PRINT_BS	;Print up to 8 characters or until an encouter with Space or NULL
		LD	A,'.'
		CALL	PUT_CHAR
		LD	C,B		;Adjust HL to +8
		LD	B,0	
		ADD	HL,BC
		LD	B,3
		CALL	PRINT_BS	;Print up to 3 characters or until an encouter with Space or NULL
		POP	BC
		POP	HL
		RET

PRINT_BS	LD	A,(HL)		;PRINT B CHARS OR UP TO EITHER A NULL OR SPACE.
		OR	A
		RET Z
		CP	' '
		RET Z
		CALL	PUT_CHAR
		INC	HL
		DJNZ	PRINT_BS
		RET


;=============================================================================
;	OUTPUT: FILENAME ENTERED, C=0
;		<ESC> PRESSED C=1
INPUT_FILENAME	CALL 	PRINTI		;Display Menu Prompt
		DB CR,LF,"ENTER 8.3 FILE NAME> ",EOS
		LD	HL,FILENAME
		LD	B,11
		LD	A,' '
		CALL	FILL_BLOCK
		LD	C,'.'
		LD	B,8
		CALL	GET_STRING
		RET C
		CP	13
		RET	Z
		LD	HL,FILEEXT
		LD	B,3
		CALL	GET_STRING
		RET

;=============================================================================
CMP_FILENAME	PUSH	HL		;Save H pointer into  Directory
		LD	BC,8		;Compare 8 characters
		LD	DE,FILENAME
		CALL	CMP_STRING
		JR NZ,	CMPF_RETFAIL	;Exit if not equal
		;ADD	HL,BC		;Adjust HL to +8, HL should be +8
		LD	BC,3
		LD	DE,FILEEXT
		CALL	CMP_STRING
		JR NZ,	CMPF_RETFAIL	;Exit if not equal
		INC	C		;Z=0
		POP	HL
		RET
		
CMPF_RETFAIL	XOR	A		;Z=1
		POP	HL
		RET

CMP_STRING	LD	A, (DE)
		CPI			;CP (HL):INC HL:DEC BC
		RET NZ			;Exit if not equal
		RET PO			;Exit if end of string
		INC	DE
		JR	CMP_STRING


;=====================================================================================================
;Read of Logical Disk Sector.
;=====================================================================================================
	;Start-of-Directory = Size-of-Fat * Number-of-fats + 1 (boot sector)
	;Start-of-Data-Area = Start-of-Directory + #Entries/32/bytes_per_sector

	;Input:	LOGICAL_SEC Disk Sector required (0 to 2001) based on 26 sectors per track by 77 tracks, counting from 0
	;Disk FCB in HL

	;if AFClus0 = 0x0000 then attempt to open the file report Disk not loaded if fail
	;Relative file sector:
	;DISK_SEC is the input to this routine, it holds Virtual Disk Sector 0 to 2001
	;because every SD sector has 512 bytes, each SD sector holds 4 CP/M Virtual Disk sectors (that's 128 bytes)
	;RFSec is the Relative File Sector, it spans from 0 to 500 (this accomodates 501 SD Sectors or 256,512 bytes)
	;If RFSec = DISK_SEC / 4 Then...
	;If RFSec has not changed, then read that sector into RAM and be done.
	;That Relative File sector is located on the SD card at address set in the absolute sector (ABS_SEC)
	;
	;If RFSec has changed... then determine is the new RFSec is within the same cluster or not.
	;On a 1Gig SD card, the system uses 32 sectors per cluster.  This means, 32 sequential SD Memory Card sectors form 1 cluster.
	;If a sector within the same cluster is being accessed, then the cluster does not have to be found again.
	;If it's NOT in the same cluster, then find new cluster by looking through the FAT
	;If it is in the same cluster, then skip to the part were we can just offset the RFSec into the current cluster
	;
	;...else
	;RFSec = DISK_SEC / 4  'Set the new sector as the current one.
	;
	;Find the Relative File Cluster (RFClus).  This number will be from 0 to 15 on a 1Gig SD Card = 262,144 bytes (to hold a 256,256 file)
	;
	;Relative file cluster:
	;RFClus = RFSec / SEC_PER_CLUS
	;if RFClus has changed, then recalculate the File Cluster and then update the SSOC.
	;  if RFClus < CRFClus then CRFClus=0, CAFClus = AFClus0  'start FAT search from 0 if going backward
	;  RFClus = RFClus - CRFClus
	;  do while RFClus>0
	;      if CAFClus = 0xFFFF Then EOF reached, file too small to be a disk. (Report as disk error)
	;      CAFClus = FAT(CAFClus)
	;      CRFClus = CRFClus + 1
	;      RFClus = RFClus - 1
	;  loop
	;  SSOC = (CAFClus - 2) * SEC_PER_CLUS + Start-of-Data-Area
	;if RFClus has NOT changed, then ABS_SEC = SSOC + (RFSec MOD SEC_PER_CLUS), then read ABS_SEC into the buffer.
;Destroys: A, BC, DE, HL, IX
;=====================================================================================================
;Read of Logical Disk Sector.
;Input:	LOGICAL_SEC = 0=First Sector
DISK_READ	
		;CALL	PRINTI		;debug
		;DB  " R-",EOS		;debug
		;LD	HL,(LOGICAL_SEC);debug
		;CALL	PUT_HL		;debug

		LD	IX,(FCB_PTR)	;Get Current Disk FCB
		LD	A,(IX)		;Is file open?
		OR	A		;Test FSTAT
		JP NZ,	DR_1		;Jump YES
		
		;PUSH	IX		;debug
		;POP	HL		;debug
		;CALL	PRINTI		;debug
		;DB  " HL:",EOS		;debug
		;CALL	PUT_HL		;debug
		
		CALL	SD_OPEN		;ELSE, Attempt to open file
		LD	A, (IX)		;Is file open?
		OR	A		;Test FSTAT
		JP NZ,	DR_1		;Jump YES
		CALL	PRINTI
		DB " -Disk Not Loaded",EOS
		RET			;Exit if file could not open

DR_1		LD	L,(IX+RFSec)	;If file open, Check if Read is from same Data Sector
		LD	H,(IX+RFSec+1)	;D=RFSec

		;CALL	PRINTI		;debug
		;DB  " LS:",EOS		;debug
		;CALL	PUT_DE		;debug

		LD	DE, (LOGICAL_SEC) ;Fetch sector to be read
		CALL	CMP_DE_HL
		JP NZ,	DR_NEW_SEC	;Jump if Read is from a different Data Sector

					;LOGICAL SECTOR = LAST READ SECTOR, Fetch Absolute Sector and read it to RAM (if wasn't last read)
		LD	HL,(FCB_PTR)	;Get Current Disk FCB
		LD	A,ABS_SEC	;H=FCB(ABS_SEC)
		ADD	A,L
		LD	L,A

		CALL	MOV_32_HL
		JP	DR_READ_IT


	;RFClus = RFSec / SEC_PER_CLUS
	;if RFClus has changed, then recalculate the File Cluster and then update the SSOC.
	;  if RFClus < CRFClus then
	;     CRFClus=0, CAFClus = AFClus0  'start FAT search from 0 if going backward
	;  eles
	;     RFClus = RFClus - CRFClus	   'else, continue FAT search from point of
	;  endif
	;  do while RFClus>0
	;      if CAFClus = 0xFFFF Then EOF reached, file too small to be a disk. (Report as disk error)
	;      CAFClus = FAT(CAFClus)
	;      CRFClus = CRFClus + 1
	;      RFClus = RFClus - 1
	;  loop
	;  SSOC = (CAFClus - 2) * SEC_PER_CLUS + Start-of-Data-Area
	;if RFClus has NOT changed, then ABS_SEC = SSOC + (RFSec MOD SEC_PER_CLUS), then read ABS_SEC into the buffer.
	
DR_NEW_SEC				;We are to read a sector of the file that is different from the last READ.
					;This branch would also take place on the first time we read a file, since
					;the "Last Sector Read" was set to a dummy value of 0xFFFF
					;
					;Save the sector we are now reading as "Last Sector Read" = RFSec

		LD	(IX+RFSec  ),E	;D=LOGICAL_SEC = Relative File Sector (Update FCB with this new Rel-File-Sec
		LD	(IX+RFSec+1),D	;

					;Find in which relative or sequential Cluster this sector is in
					;by dividing by the "Sectors per Cluster"
					;eg. If there are 4 sectors per cluster, then Sectors 0 to 3 will be in Cluster 0
					;Cluster 0 is the first cluster of the file and the location of this cluster
					;(the Absolute Cluster) is given by the directory entry for this file.

		LD	HL, (DIVIDE_FUNC)	;DE = DE / Sectors-Per-Cluster (Divide Func hard coded with Sec/Clus)
		CALL	VCALL_HL

		;CALL	PRINTI		;debug
		;DB  " RC:",EOS		;debug
		;CALL	PUT_DE		;debug
		
		LD	HL,(FCB_PTR)
		LD	A,CRFClus	;H=FCB(CRFClus)
		ADD	A,L
		LD	L,A

		LD	C,(HL)
		INC	HL
		LD	B,(HL)		;BC = CRFClus
		DEC	HL

		;CALL	PRINTI		;debug
		;DB  " LRC:", EOS	;debug
		;CALL	PUT_BC		;debug


					;H->FCB(CRFClus)
					;TEST DE - BC  aka NewRFClus vs FCB-RFClus
					;Speed Optimize the above code
		LD	A,D
		CP	B
		JR NZ,	DR_DIFF_CLUS
		LD	A,E
		CP	C
		JP Z,	DR_SAME_CLUS	;IF they are the same, then the new sector is in the same cluster
DR_DIFF_CLUS	JR NC,	DR_BIGGER_CLUS

					;If going to a smaller cluster, restart the FAT search from the begining
		LD	BC,0		;CRFClus = 0
		DEC	HL
		DEC	HL
		JR	DR_SEEK_FAT	;HL will load with AFClus0

DR_BIGGER_CLUS	LD	A,E		;NewRFClus = NewRFClus - FCB-RFClus,  ie Set counter for number of new FAT hops.
		SUB	C
		LD	E,A
		LD	A,D
		SBC	A,B
		LD	D,A

		INC	HL
		INC	HL		;HL will load with CAFClus

	;  do while RFClus>0
	;      if CAFClus = 0xFFFF Then EOF reached, file too small to be a disk. (Report as disk error)
	;      CAFClus = FAT(CAFClus)
	;      CRFClus = CRFClus + 1
	;      RFClus = RFClus - 1
	;  loop

DR_SEEK_FAT
		CALL	LD_HL_HL	;HL = CAFClus or AFClus0

;		CALL	PRINTI		;debug
;		DB  CR,LF,"CAFClus=",EOS ;debug
;		CALL	PUT_HL		;debug
;		CALL	PRINTI		;debug
;		DB  CR,LF,"CRFClus=",EOS ;debug
;		CALL	PUT_BC		;debug
;		CALL	PRINTI		;debug
;		DB  CR,LF,"RFClus=",EOS	;debug
;		CALL	PUT_DE		;debug

					;BC = CRFClus
DR_SEEK_LP	LD	A,D		;DE = RFClus
		OR	E
		JR Z,	DR_SEEK_DONE

;		CALL	PRINTI		;debug
;		DB  CR,LF,"seek=",EOS	;debug
;		CALL	PUT_HL		;debug
				
		LD	A,H		;IF CAFClus = 0xFFFF... (No more clusters to fetch)
		AND	L
		CPL
		OR	A		
		JP NZ,	DR_SEEK_1
					;Error, File too small
		CALL	PRINTI
		DB " -ERROR, NO MORE ALLOCATED CLUSTERS!",EOS
		HALT
		JP	$-1
		
DR_SEEK_1	

	;Here comes the FAT Hopping FUN...
	;      CAFClus = FAT(CAFClus)
	;it's convenient that 1 Sector is 512 bytes, that's 256 words = 256 FAT Entries, therefore...
	;H = Sector of FAT
	;L = Word within that Sector of FAT

		PUSH	BC
		PUSH	DE
		PUSH	HL
		LD	E,H		;E=Sector of FAT
		LD	HL, (FAT1START)	;DE = E + FAT1START
		LD	A,L
		ADD	A,E
		LD	E,A
		LD	A,H
		ADC	A,0		;Carry it forward
		LD	D,A
		LD	HL, (FAT1START+2)
		JR NC,	DRS_0		;Test for Carry
		INC	HL		;Carry it forward
DRS_0		LD	B,H
		LD	C,L		;BCDE now have Sector of FAT desired
		CALL	SD_READ_SEC
		POP	DE		;Fetch DE, E=Word within that FAT sector
		LD	HL,SD_RAM_BUFFER
		OR	A		;Clear Carry
		LD	A,E		;Fetch offset into FAT sector read
		RLA
		LD	L,A		;
		LD	A,H
		ADC	A,0
		LD	H,A		;HL -> FAT Entry
		CALL	LD_HL_HL	;HL = FAT Entry
		POP	DE
		POP	BC

		INC	BC
		DEC	DE
		JR	DR_SEEK_LP

DR_SEEK_DONE	;Write Registers to FCB
		;BC = CRFClus  = The Relative Cluster (ie, the 5th cluster into the file)
		;DE = RFClus - Not required (it's a counter down to zero to find the correct cluster)
		;HL = CAFClus  = The Absolute or Actual Cluster (ie, Cluster 149 on the disk)

		LD	IX,(FCB_PTR)
		LD	(IX+CRFClus  ),C	;Save CRFClust to FCB
		LD	(IX+CRFClus+1),B
		LD	(IX+CAFClus  ),L	;Save CAFClus to FCB
		LD	(IX+CAFClus+1),H

	;Now, let's find the Data Sector to be loaded....
	;First, calculate the Starting Sector of Cluster (SSOC)
	;  SSOC = (CAFClus - 2) * SEC_PER_CLUS + Start-of-Data-Area

		DEC	HL		;HL = CAFClus - 2
		DEC	HL
		
		EX	DE,HL

;-------------------------------------	Multiply Routine.  16bit by 8 bit -> 24bit
		LD	C,0		;CDE = 16bit input (need 24 bits to shift)
		LD	B,8		;Go through 8 bits
		LD	IX, SEC_PER_CLUS	;Fetch Multiplier
		XOR	A
		LD	HL,0		;AHL = 24bit output
		
DRSS_LP		RRC	(IX)
		JR NC,	DRSS_SHIFT
		ADD	HL,DE		;DE=DE+HL
		ADC	A,C

DRSS_SHIFT	SLA	E
		RL	D
		RL	C
		DJNZ	DRSS_LP
	
		LD	C,A
		LD	B,0		;BCHL = 32bit Absolute sector
					;Add to BCHL, the DATASTART sector
		LD	DE, (DATASTART)	;32 Bit ADD DATASTART
		ADD	HL, DE
		EX	DE, HL		;DE=DE+START (LSB)
		LD	HL, (DATASTART+2)
		ADC	HL,BC
		LD	C,L
		LD	B,H		;BC=BC+START (MSB)
;-------
					;Save the result to RAM variable SSOC
		LD	HL,(FCB_PTR)
		LD	A,SSOC		;Set FCB(SSOC)
		ADD	A,L
		LD	L,A		
		CALL	MOV_HL_32	;Save the 32 bit register BCDE to (HL)
;-------

	;ABS_SEC = SSOC + (RFSec MOD SEC_PER_CLUS), then read ABS_SEC into the buffer.
DR_SAME_CLUS				;Fetch the RFSec
		LD	IX,(FCB_PTR)	;Set FCB(RFSec)
		LD	E,(IX+RFSec)	;DE=RFSec
		LD	D,(IX+RFSec+1)

		LD	HL,(MOD_FUNC)	;DE = DE % Sectors-Per-Cluster
		CALL	VCALL_HL	;A = RFSec MOD SEC_PER_CLUS

		LD	BC,0		;BCDE = (RFSec MOD SEC_PER_CLUS)
		LD	D,0
		LD	E,A

		LD	HL,(FCB_PTR)
		LD	A,SSOC		;Set FCB(SSOC)
		ADD	A,L
		LD	L,A		
		CALL	ADD_32_HL	;BCDE = SSOC + (RFSec MOD SEC_PER_CLUS)
					;(HL returns +4) to ABS_SEC

		CALL	MOV_HL_32	;Save the ABS_SEC

DR_READ_IT	CALL	SD_READ_SEC	;Fetch the Sector

		LD	HL,SD_RAM_BUFFER
		RET



;=====================================================================================================
;=====================================================================================================
;SD_CARD_TYPE	.BLOCK	1	;SD CARD TYPE
;SDC_STATUS	.BLOCK	1	;SD Status Code returned
;SD_PARAM	.BLOCK	4	;32 bit address parameter for SD Commands
;SD_PART_TYPE	.BLOCK	1	;SD PARTITION TYPE
;SD_PART_BASE	.BLOCK	4	;SD PARTITION STARTING RECORD
;SD_PART_SIZE	.BLOCK	4	;SD PARTITION SIZE (Must follow SD_PART_BASE)
;SEC_PER_CLUS	.BLOCK	1	;0x0D
;RESERVED_SEC	.BLOCK	2	;0x0E - 0x0F
;FAT_COPIES	.BLOCK	1	;0x10
;RT_DIR_ENTRIES	.BLOCK	2	;0x11 - 0x12
;TOT_FILESYS_SEC.BLOCK	4	;0x13 - 0x14 or 0x20 - 0x23
;HIDDEN_SECTORS	.BLOCK	4	;0x1C - 0x1F
;SEC_PER_FAT	.BLOCK	2	;0x16 - 0x17
;FAT1START	.BLOCK	4	;Calculated
;DIR_SECTOR	.BLOCK	4	;Calculated
;DATASTART	.BLOCK	4	;Calculated
;-----------------------------------------------------------------------------
INIT_FAT	LD	HL,FAT_CLEAR	;Clear RAM
		LD	B,FAT_CLR_LEN
		CALL	CLEAR_BLOCK

		CALL	INIT_SDCARD
		RET NZ			;RET NZ IF FAILED

		CALL 	PRINTI		;
		DB "MBR",EOS

		LD	BC,0		;BCDE = 0x00000000
		LD	DE,0
		CALL	SD_RS_FORCED	;READ MBR (FORCED READ)
		CALL	TEST_SIGNATURE
		RET NZ			;RET NZ IF FAILED

		CALL 	PRINTI		;
		DB " Type",EOS
		LD	A, (SD_RAM_BUFFER+01C2h)
		CALL	PUT_BYTE
		LD	(SD_PART_TYPE), A
		CP	4
		JR   Z,	INITFAT_PGOOD
		CP	6
		JR   Z,	INITFAT_PGOOD
		CP	86h
		JP  NZ,	INITFAT_FAIL

INITFAT_PGOOD	LD	HL,SD_RAM_BUFFER+1C6h
		LD	DE,SD_PART_BASE
		LD	BC,8
		LDIR		;Copy BASE & SIZE from BUFFER to RAM Variables
		CALL 	PRINTI		;
		DB " PBR",EOS

		LD	HL,SD_PART_BASE
		CALL	MOV_32_HL	;Copy BASE to SEC_PTR
		CALL	SD_READ_SEC	;READ BOOT RECORD OF PARTITION
		CALL	TEST_SIGNATURE
		RET NZ

		LD	HL,SD_RAM_BUFFER+0Bh
		LD	DE,BYTE_P_SEC
		LD	BC,10
		LDIR	;Copy Description Table to RAM Variables (Up to Total Filesys Sectors)
		EX	DE, HL	;Test TOTAL_FILESYS_SECTORS = 0
		DEC	HL
		DEC	HL
		LD	A,(HL)
		INC	HL
		OR	(HL)
		JR  NZ,	INITFAT_TFS_OK
		DEC	HL
		EX	DE, HL	
		LD	HL,SD_RAM_BUFFER+020h
		LD	BC,4
		LDIR
		JR	INITFAT_TFS_DONE

INITFAT_TFS_OK	XOR	A
		INC	HL
		LD	(HL),A
		INC	HL
		LD	(HL),A
		INC	HL
		EX	DE, HL	
INITFAT_TFS_DONE

		LD	HL,SD_RAM_BUFFER+01Ch
		LD	BC,4
		LDIR			;Copy HIDDEN_SECTORS to RAM Variables
		LD	HL,SD_RAM_BUFFER+016h
		LD	BC,2
		LDIR			;Copy SECTORS_PER_FAT to RAM Variables

;BS.fat1Start = MBR.part1Start + BS.reservedSectors;
		LD	HL, (RESERVED_SEC)	;H=Reserved Sectors
		EX	DE, HL	
		LD	HL, (SD_PART_BASE)	;FAT1START = SD_PART_BASE + RESERVED_SEC
		ADD	HL,DE
		LD	(FAT1START), HL
		LD	HL, (SD_PART_BASE+2)
		JR  NC,	INITFAT_C1_DONE
		INC	HL
INITFAT_C1_DONE	LD	(FAT1START+2), HL

;firstDirSector = BS.fat1Start + (BS.fatCopies * BS.sectorsPerFAT);
		LD	A, (FAT_COPIES)
		LD	B,A
		LD	HL, (SEC_PER_FAT)
		EX	DE, HL	
		LD	HL,0
INITFAT_C2_LP	ADD	HL,DE
		DJNZ	INITFAT_C2_LP	;H = FAT_COPIES * SEC_PER_FAT
		EX	DE, HL		;DE = FATS * SECperFAT
		LD	HL, (FAT1START)
		ADD	HL,DE		;DIR_SECTOR = FAT1START + FAT_COPIES * SEC_PER_FAT
		LD	(DIR_SECTOR), HL
		LD	HL, (FAT1START+2)
		JR  NC,	INITFAT_C2_DONE
		INC	HL
INITFAT_C2_DONE	LD	(DIR_SECTOR+2), HL

;DATASTART = DIR_SECTOR + LEN(Directory)
;          = DIR_SECTOR + ROOTDIR_SIZE * 32 / BYTE_P_SEC
		LD	B,16		;Maximum # of Reductions
		LD	HL, (BYTE_P_SEC)	;To fit math into 16 bits, let's reduce "ROOTDIR_SIZE / BYTE_P_SEC"
		EX	DE, HL	;Divide each by 2 while dividable
		LD	HL, (ROOTDIR_SIZE)	;H=ROOTDIR_SIZE, D=BYTE_P_SEC
INITFAT_C3_LP	LD	A,E
		RRA
		JR  C,	INITFAT_C3_0	;If lsb of D is 1, no more Reduction possible
		LD	A,L
		RRA
		JR  C,	INITFAT_C3_0	;If lsb of H is 1, no more Reduction possible
		LD	A,D
		RRA
		LD	D,A
		LD	A,E
		RRA
		LD	E,A
		LD	A,H
		RRA
		LD	H,A
		LD	A,L
		RRA
		LD	L,A
		DJNZ	INITFAT_C3_LP

INITFAT_C3_ERR	CALL 	PRINTI		;
		DB " Error DATASTART",EOS
		XOR	A
		DEC	A
		RET
		
INITFAT_C3_0	LD	B,5		;5 shifts = Multiply 32
INITFAT_C3_LP2	ADD	HL,HL		;Double H
		JP C,	INITFAT_C3_ERR
		DJNZ	INITFAT_C3_LP2

		LD	A,E		;2'S Complement BYTE_P_SEC
		CPL
		LD	C,A
		LD	A,D
		CPL
		LD	B,A
		INC	BC
		LD	DE,0FFFFh	;Start with -1
INITFAT_C3_LP3	ADD	HL,BC		;Divide by counting Subtractions
		INC	DE
		JR   C,	INITFAT_C3_LP3
		LD	HL, (DIR_SECTOR)	;Add the Dword at DIR_SECTOR
		ADD	HL,DE
		LD	(DATASTART), HL
		LD	HL, (DIR_SECTOR+2)
		JR  NC,	INITFAT_C3_1
		INC	HL
INITFAT_C3_1	LD	(DATASTART+2), HL

		LD	A, (SEC_PER_CLUS)	;Determine the best way to divide Sectors into cluster#
		DEC	A
		LD	(MODMASK), A
		INC	A
		JR   Z,	INITFAT_FAIL1
		LD	BC,0800h
INITFAT_C4_LP	RRA
		JR  NC,	INITFAT_C4_1
		LD	D,B		;Save location of "1" bit
		INC	C		;Count of 1 bits.
INITFAT_C4_1	DJNZ	INITFAT_C4_LP
		LD	A,1
		CP	C
		JR  NZ,	INITFAT_C4_2	;More than 1 "1" bit, cannot do divide by simple shift.
		LD	A,D		;Fetch position of the 1 bit.  8=lsb, 1=msb
		CPL
		ADD	A,10		;Re-adjust to make 1=lsb AND 8=msb  A=9-A
		LD	(DF_SHIFTCNT), A
		LD	HL,DIVBYSHIFT	;Use fast shift divider
		LD	DE,MODBYMASK
		JR	INITFAT_C4_3

INITFAT_C4_2	LD	HL,DIV16BY8SPC	;Use Full Divide function for Sectors Per Cluster
		PUSH	HL
		POP	DE
INITFAT_C4_3	LD	(DIVIDE_FUNC), HL
		EX	DE, HL	
		LD	(MOD_FUNC), HL

		CALL 	PRINTI		;
		DB " VOL=",EOS
		LD	HL,SD_RAM_BUFFER+002Bh
		LD	B,11
		CALL	PRINTB
		CALL 	PRINTI		;
		DB " SYS=",EOS
		LD	B,8
		CALL	PRINTB
		XOR	A
		RET

INITFAT_FAIL1	CALL 	PRINTI		;
		DB CR,LF,"Error=0 Sec/Clus",EOS
INITFAT_FAIL	CALL 	PRINTI		;
		DB CR,LF,"FAT Init FAILED",EOS
		XOR	A
		DEC	A
		RET

;-------------------------------------------------
TEST_SIGNATURE	CALL 	PRINTI		;
		DB " S",EOS
		DEC	HL
		LD	A,0AAh
		CP	(HL)
		JR  NZ,	INITFAT_FAIL
		DEC	HL
		LD	A,055h
		CP	(HL)
		JR  NZ,	INITFAT_FAIL
		RET



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_15	SD Memory Card Routines, Mid Level, Send/Recieve Data Sectors (Writes out Dirty Data)
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;-----------------------------------------------------------------------------------------------------
;Read the SD Card at Sector BCDE TO the SD_RAM_BUFFER
;-----------------------------------------------------------------------------------------------------
		;Sector in SEC_PTR
SD_READ_SEC	LD	HL,SEC_PTR	;READ SECTOR
		CALL	CMP_HL_32
		RET Z			;Return if no change to sector being read

		LD	A, (DIRTY_DATA)	;Test if flush required
		OR	A
		JP Z,	SD_RS_FORCED	;Jump if no change in SD RAM BUFFER
		XOR	A
		LD	(DIRTY_DATA), A	;Clear Write Flag

		PUSH	BC		;Save BCDE (Sector to Read)
		PUSH	DE
		PUSH	HL
		CALL	MOV_32_HL	;Fetch the last SEC_PTR

;		CALL	PRINTI		;debug
;		DB  " Write:",EOS	;debug
;		CALL	PUT_BC		;debug
;		CALL	PUT_DE		;debug

		CALL	SD_WRITE_SEC
		POP	HL
		POP	DE
		POP	BC
		
SD_RS_FORCED
	;CALL	PRINTI		;debug
	;DB  " Read:",EOS	;debug
	;CALL	PUT_BC		;debug
	;CALL	PUT_DE		;debug

		LD	HL,SEC_PTR
		CALL	MOV_HL_32	;Save Sector in SEC_PTR
		CALL	SET_PARAM	;READ SECTOR, HL=SD_RAM_BUFFER
		LD	B,5		;5 Retries to read
SD_RS_LP0	LD	A,17 		;Read Sector Command
		CALL	SD_CMD
		JR Z,	SD_RS_0
		DJNZ	SD_RS_LP0
					;Read failed
		DEC	B		;Clear Zero flag
		CALL	SD_DESELECT	;Deselect card
	;CALL	PRINTI	;debug
	;DB  " #1",EOS	;debug
		RET

SD_RS_0		LD	B,0		;256 Attempts to recieve the DATASTART
SD_RS_LP1	CALL	SPI_RX
		CP	0FEh		;IS DATASTART?
		JR Z,	SD_RS_1
		DJNZ	SD_RS_LP1
		CALL	SD_DESELECT	;Deselect card
	;CALL	PRINTI	;debug
	;DB  " #2",EOS	;debug
		RET

SD_RS_1		LD	BC,0200h
SD_RS_LP2	CALL	SPI_RX		;Fetch 512 Bytes to M(HL)
		LD	(HL),A
		INC	HL
		DEC	C
		JR NZ,	SD_RS_LP2
		DJNZ	SD_RS_LP2

		CALL	SPI_RX		;BURN 2 BYTES (CRC)
		CALL	SPI_RX		;
		CALL	SD_DESELECT	;Deselect card
	;CALL	PRINTI	;debug
	;DB  " #3",EOS	;debug
		XOR	A
		RET



;-----------------------------------------------------------------------------------------------------
;Write the SD_RAM_BUFFER to the SD Card at Sector 'SEC_PTR'
;-----------------------------------------------------------------------------------------------------
		;Sector in SEC_PTR, H=SD_RAM_BUFFER
SD_WRITE_SEC	XOR	A
		OUT	(GREEN_LED),A
		CALL	SET_PARAM
		LD	A,24 	;Write Sector Command
		CALL	SD_CMD
		LD	A,1 	;Error Code
		JP NZ,	SD_WR_FAIL

		LD	A,0FEh	;DATA START BLOCK
		CALL	SPI_TX
		LD	BC,0200h
SD_WR_LP	LD	A,(HL)
		INC	HL
		CALL	SPI_TX
		DEC	C
		JP NZ,	SD_WR_LP
		DJNZ	SD_WR_LP

		LD	A,0FFh
		CALL	SPI_TX
		NOP
		LD	A,0FFh
		CALL	SPI_TX

		CALL	SPI_RX
		AND	1Fh
		CP	5
		LD	A,2 	;Error Code
		JP NZ,	SD_WR_FAIL
		CALL	WAIT_NOT_BUSY
		LD	A,3 	;Error Code
		JP C,	SD_WR_FAIL
		CALL	SD_CLEAR_ARG
		LD	A,13		;SEND_STATUS
		CALL	SD_CMD
		LD	A,4 	;Error Code
		JP NZ,	SD_WR_FAIL
		CALL	SPI_RX
		OR	A
		LD	A,5 	;Error Code
		JP NZ,	SD_WR_FAIL

		XOR	A		;A should be zero
		LD	(DIRTY_DATA), A

		CALL	SD_DELAY

		CALL	SD_DESELECT	;Deselect card
		RET

SD_WR_FAIL	CALL	SD_DESELECT	;Deselect card
		CALL	PRINTI
		DB CR,LF,"-Write Failed:",EOS
		CALL	PUT_BYTE
		RET

;-----------------------------------------------------------------------------------------------------
;Input:	Sector in 32 bit register BCDE
SET_PARAM	LD	A, (SD_CARD_TYPE)	;IF CARD_TYPE <> 3 THEN SHIFT SECTOR << 9 Bits
		CP	3
		JP Z,	SP_RET

		LD	A,C
		EX	DE, HL	
		ADD	HL,HL
		RLA
		LD	B,A
		LD	C,H
		LD	D,L
		LD	E,0

SP_RET		LD	HL,SD_PARAM
		CALL	MOV_HL_32		;Save Parameter
		LD	HL,SD_RAM_BUFFER	;Set buffer space
		RET

;-----------------------------------------------------------------------------------------------------
SD_CLEAR_ARG	XOR	A
		LD	(SD_PARAM),A
		LD	(SD_PARAM+1),A
		LD	(SD_PARAM+2),A
		LD	(SD_PARAM+3),A
		RET


;=====================================================================================================
;SD Memory Car Routines, Low Level, INIT CARD, Send/Recieve Data, Send Commands
;=====================================================================================================
;-------------------------------- INIT SDCARD --------------------------------
INIT_SDCARD	CALL	SD_DESELECT	;Deselect and clock the card many cycles
		LD	B,080H
ISD_0		LD	A,0FFH
		LD	(SD_CARD_TYPE),A
		CALL	SPI_TX		;CLOCK many cycles
		DJNZ	ISD_0		;256 Clocks
		CALL	SD_SELECT

		CALL 	PRINTI		;
		DB	CR,LF,"Init SD",EOS

		CALL	SD_CLEAR_ARG	;Fetch the 01 response
		LD	B,0		;256 retries
ISD_LP1		LD	A,0		;CMD 0
		CALL	SD_CMD
		;CALL	PUT_BYTE
		CP	1		;Test 01 response
		JR  Z,	ISD_1
		DJNZ	ISD_LP1
		
INIT_FAIL	CALL 	PRINTI		;
		DB	"-FAILED",EOS
		CALL	SD_DESELECT
		XOR	A		;Return Zero Flag cleared = Failure
		DEC	A
		RET

ISD_1		CALL 	PRINTI		;
		DB	" Type#",EOS
		LD	HL,01AAh	;Deterimine Card Type
		LD	(SD_PARAM),HL
		LD	A,8		;CMD 8
		CALL	SD_CMD
		AND	4
		JR   Z,	ISD_2
		LD	A,1		;If CMD8 is Illegal Cmd, CARD_TYPE=1
		LD	(SD_CARD_TYPE),A
		JP	ISD_3

ISD_2		CALL	SPI_RX
		CALL	SPI_RX
		CALL	SPI_RX
		CALL	SPI_RX
		LD	(SDC_STATUS),A
		CP	0AAh
		LD	A,0AAh		;Error code
		JR  NZ,	INIT_FAIL
		LD	A,2
		LD	(SD_CARD_TYPE),A

ISD_3		CALL	PUT_HEX
		CALL 	PRINTI		;
		DB	" ACMD41",EOS
		CALL	SD_CLEAR_ARG

		LD	B,0
ISD_LP2		LD	A,55		;CMD 55 (ACMD)
		CALL	SD_CMD
		LD	A,41		;CMD 41
		CALL	SD_CMD
		CP	0
		JR Z,	ISD_4
		XOR	A		;256 ~= 2mSec Delay
		CALL	SD_DELAY
		DJNZ	ISD_LP2
		JR	INIT_FAIL

ISD_4		CALL 	PRINTI		;
		DB	"+",EOS
		LD	A,(SD_CARD_TYPE)
		CP	2
		JR  NZ,	ISD_6
		LD	A,58		;CMD 58
		CALL	SD_CMD
		CP	0
		JP  NZ,	INIT_FAIL
		CALL	SPI_RX
		AND	0C0h
		CP	0C0h
		JP NZ,	ISD_5
		LD	A,3
		LD	(SD_CARD_TYPE),A
		CALL 	PRINTI		;
		DB	" Type#3",EOS
ISD_5		CALL	SPI_RX
		CALL	SPI_RX
		CALL	SPI_RX

ISD_6		CALL	SD_DESELECT
		XOR	A		;Set Zero Flag = Success
		RET

;-----------------------------------------------------------------------------------------------------
SD_DESELECT	PUSH	AF
		LD	A,1
		OUT	(SDCS),A
		OUT	(SDCLK),A
		OUT	(GREEN_LED),A
		POP	AF
		RET
		
;-----------------------------------------------------------------------------------------------------
SD_SELECT	PUSH	AF
		XOR	A
		OUT	(SDCS),A
		POP	AF
		RET

;-----------------------------------------------------------------------------------------------------
SD_DELAY100	LD	A,13	 ;Small delay after selecting card
SD_DELAY	DEC	A	 ;5
		JP NZ,	SD_DELAY ;10    15*13 ~= 200 ~= 100uSec
		RET

;-----------------------------------------------------------------------------------------------------
;Send command to SD card
SD_CMD		PUSH	BC
		CALL	SD_SELECT
		CALL	WAIT_NOT_BUSY

		LD	B,0FFh	;Default CRC
		CP	0
		JP NZ,	SDC_1
		LD	B,095h	;If CMD=0 THEN CRC=95
SDC_1		CP	8
		JP NZ,	SDC_2
		LD	B,087h
SDC_2

		OR	040H	;All Commands start with 40h
		CALL	SPI_TX
		LD	A,(SD_PARAM+3)
		CALL	SPI_TX
		LD	A,(SD_PARAM+2)
		CALL	SPI_TX
		LD	A,(SD_PARAM+1)
		CALL	SPI_TX
		LD	A,(SD_PARAM)
		CALL	SPI_TX
		LD	A,B
		CALL	SPI_TX

		LD	B,0
SDC_LP		CALL	SPI_RX	;Read Respsonse?
		LD	(SDC_STATUS),A
		OR	A
		JP P,	SDC_RET	;If Positive Response, EXIT
		DJNZ	SDC_LP	;Else Read next Response
		OR	A
SDC_RET		POP	BC
		RET

;-----------------------------------------------------------------------------------------------------
;------------------------------- Receive a byte from SPI
SPI_RX		LD	A,0FFH	;Read Respsonse, send a byte to get a byte...

SPI_TX		PUSH	BC
		LD	C,A	;Save Byte to send in C
		LD	B,8	;8 BITS
		
SPI_TX_LP	IN	A,(ACE_MSR)	;Fetch RX bit
		RLA			;Save bit in CY
		RL	C		;Move bit into C (and bump next tx bit up)
		RLA			;Get bit to send in lsb
		OUT	(SDTX),A	;Send bit
		IN	A,(SDCLK)	;Clock the bit
		DJNZ	SPI_TX_LP		
		LD	A,C
		CPL			;Correct Inversion by ACE
		POP	BC
		RET

;-----------------------------------------------------------------------------------------------------
;------------------------------- Wait until FF's come back from Card (ie NOT BUSY)
WAIT_NOT_BUSY	PUSH	AF	;Do not destroy Acc
		PUSH	BC	;Fetch 1 consecutive FF's to be sure SD card NOT BUSY
		LD	B,0
WNB_LP		LD	C,1	;Set count for 1 trys
WNB_LP2		CALL	SPI_RX
		INC	A
		JP NZ,	WNB_0	;NOT FF RETURNED, JUMP TO COUNT DOWN TRYS
		DEC	C	;Count Down Consecutive FF's
		JP NZ,	WNB_LP2
		POP	BC
		POP	AF
		SCF		;Return NOT BUSY (Clear Carry)
		CCF
		RET

WNB_0		XOR	A
		CALL	SD_DELAY
		DJNZ	WNB_LP	;Count Down Trys
		POP	BC
		POP	AF
		SCF		;Return STILL BUSY (Set Carry)
		RET



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_16	General Support Routines, 32 Bit stuff and other math
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;-----------------------------------------------------------------------------------------------------
;Maximum number to divide is Logical Sector 2001/4 = 500
;If dividing by powers of 2, then we can shift the number for fast divide
DIV16BY8SPC	LD	A, (SEC_PER_CLUS)
;Input:	DE=Dividend, A=Divisor
;Out:	DE=Result, A=Remainder
DIV16BY8	EX	DE, HL	; HL = Dividend
		LD	E,00	; Quotient = 0
		LD	C, A	; Store        Divisor
		LD	B, 08	; Count = 8
DIV16BY8_LP	ADD	HL,HL	; Dividend = Dividend x 2
		RLC	E	; Quotient = Quotient x 2
		LD	A, H
		SUB	C	; Is most significant byte of Dividend > divisor
		JR C,	DIV16BY8_SK	; No, go to Next step
		LD	H, A	; Yes, subtract divisor
		INC	E	; and Quotient = Quotient + 1
DIV16BY8_SK	DJNZ 	DIV16BY8_LP ; Count = Count - 1
		LD	A, H
		LD	D,0	; Quotient in DE
		RET

;-----------------------------------------------------------------------------------------------------
DIVBYSHIFT	LD	A, (DF_SHIFTCNT)	; DE = Dividend
		LD	B,A
DBS_LP		DEC	B
		RET Z
		SRL	D
		RR	E
		JP	DBS_LP

;-----------------------------------------------------------------------------------------------------
MODBYMASK	LD	A, (MODMASK)
		AND	E
		RET

;------------------------- Move (HL) to 32 bit register BCDE
MOV_32_HL	LD	E,(HL)
		INC	HL
		LD	D,(HL)
		INC	HL
		LD	C,(HL)
		INC	HL
		LD	B,(HL)
		DEC	HL
		DEC	HL
		DEC	HL
		RET

;------------------------- Move 32 bit register BCDE to (HL)
MOV_HL_32	LD	(HL),E
		INC	HL
		LD	(HL),D
		INC	HL
		LD	(HL),C
		INC	HL
		LD	(HL),B
		DEC	HL
		DEC	HL
		DEC	HL
		RET

;------------------------- ADD (HL) to 32 bit register BCDE - (Must return with HL changed to last byte)
ADD_32_HL	LD	A,E
		ADD	A,(HL)
		LD	E,A
		INC	HL
		LD	A,D
		ADC	A,(HL)
		LD	D,A
		INC	HL
		LD	A,C
		ADC	A,(HL)
		LD	C,A
		INC	HL		
		LD	A,B
		ADC	A,(HL)
		LD	B,A
		INC	HL		;(Must return with HL advanced past 32 bits)
		RET

;-----------------------------------------------------------------------------------------------------
INC_32		INC	DE
		LD	A,D
		OR	E
		RET NZ
		INC	BC
		RET

;-----------------------------------------------------------------------------------------------------
DEC_32		LD	A,D
		OR	E
		JP NZ,	DEC_32NB
		DEC	BC
DEC_32NB	DEC	DE
		RET

;-----------------------------------------------------------------------------------------------------
TSTZ_32		LD	A,D
		OR	E
		OR	C
		OR	B
		RET

;-----------------------------------------------------------------------------------------------------
;Compare BCDE with 32bit word at HL
CMP_HL_32	INC	HL		;Point to MSB
		INC	HL
		INC	HL
		LD	A,B		;Compare with B
		CP	(HL)
		JR NZ,	CH3_R1
		DEC	HL
		LD	A,C
		CP	(HL)
		JR NZ,	CH3_R2
		DEC	HL
		LD	A,D
		CP	(HL)
		JR NZ,	CH3_R3
		DEC	HL
		LD	A,E
		CP	(HL)
		RET
CH3_R1		DEC	HL
CH3_R2		DEC	HL
CH3_R3		DEC	HL
		RET

;------------------------- COMPARE DE WITH HL
CMP_DE_HL	LD	A,D		;Compare the MSB first
		CP	H
		RET NZ
		LD	A,E
		CP	L
		RET



;=====================================================================================================
;General Support Routines, Strings
;=====================================================================================================
;	OUTPUT: STRING ENTERED @M(HL), C=0
;		<ESC> PRESSED C=1
;-----------------------------------------------------------------------------------------------------
GET_STRING:	CALL	GET_CHAR
		CP	27
		SCF			;Set Carry to indicate Abort
		RET Z
		CP	13		;Exit on <CR>
		RET Z
		CP	C		;Exit on Selectable Char (dot for file input)
		RET Z
		CALL	TO_UPPER
		CP	' '+1		;Test if ACC is Control or Space
		JR C,	GET_STRING	;Skip such characters
		DEC	B
		INC	B		;Exit if B characters are already inputed
		RET Z			;Exit if no more characters allowed
		LD	(HL),A
		INC	HL
		DEC	B
		JR	GET_STRING

;-----------------------------------------------------------------------------------------------------
TO_UPPER	CP	'a'
		RET C		;Return if ACC < 'a'
		CP	'z'+1
		RET NC		;Return if ACC > 'z'
		AND	5Fh	;Convert to upper case
		RET

;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
IS_LETTER	CP	'A'
		RET C
		CP	'Z'+1
		CCF
		RET

;-----------------------------------------------------------------------------------------------------
;ASCII TO BINARY
;INPUT:	HL Points to a 2 character String of ASCII
;	DE Points to a memory location to receive the Binary value
;OUTPUT: CY=0 if successful (both ASCII characters were a valid HEX digit (0-9,A-F,a-f)
;	 (DE) = Binary value of (HL) and (HL+1)
;	eg:
;	Before Call: HL=1000, DE=2000, M(1000)='A', M(1001)='B', M(2000)=xx, CY=?
;	After  Call: HL=1002, DE=2000, M(1000)='A', M(1001)='B', M(2000)=AB, CY=0
;	
;	CY=1 if invalid hex char encountered, a partial output may appear at (DE)
;	eg:
;	Before Call: HL=1000, DE=2000, M(1000)='J', M(1001)='B', M(2000)=12, CY=?
;	After  Call: HL=1000, DE=2000, M(1000)='J', M(1001)='B', M(2000)=12, CY=1
;	eg:
;	Before Call: HL=1000, DE=2000, M(1000)='A', M(1001)='J', M(2000)=12, CY=?
;	After  Call: HL=1001, DE=2000, M(1000)='A', M(1001)='J', M(2000)=2A, CY=1
;
;DESTROYS: AF
;
ASC2BIN		CALL	ABF1		;Convert 2 chars to HEX @(DE)
		RET	C		;Exit on error CY=1
ABF1		LD	A,(HL)
		CALL	ASC2HEX	;(non-hex) char when CY=1
		RET	C
ABF_GOODHEX	EX	DE,HL
		RLD
		EX	DE,HL
		INC	HL
		RET		

BEEP		LD	(BEEP_TO),A
		RET

SET_ECHO	LD	(ECHO_STATE),A	;TURN ON/OFF ECHO
		RET

LED_UPDATE	LD	(IK_TIMER),A	;0 = Cancel any monitor time outs
		LD	HL,ISET_PRESSED
		LD	(KEY_EVENT),HL
		LD	HL,IDISP_RET
		LD	(DISPMODE),HL
		RET

LED_GET_POS	LD	A,(LED_CURSOR)
		AND	7
		RET
		
LED_SET_POS	AND	7
		OR	LOW LED_DISPLAY
		LD	(LED_CURSOR),A
		RET
		
LED_RIGHT	PUSH	HL
		PUSH	BC
		LD	HL,LED_DISPLAY
		LD	(LED_CURSOR),HL
		LD	B,7
		LD	A,0x80				
LEDR_LP		LD	C,(HL)
		LD	(HL),A
		LD	A,C
		INC	HL
		DJNZ	LEDR_LP
		POP	BC
		POP	HL				
		RET

LED_LEFT	PUSH	HL
		PUSH	BC
		LD	HL,LED_DISPLAY+6
		LD	(LED_CURSOR),HL
		LD	B,7
		LD	A,0x80				
LEDL_LP		LD	C,(HL)
		LD	(HL),A
		LD	A,C
		DEC	HL
		DJNZ	LEDR_LP
		POP	BC
		POP	HL
		RET


;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Chapter_17	High RAM routines
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;These routines cause bank memory switches between RAM/ROM and must be in HIGH RAM to execute.
;They will be loaded into HIGH RAM

LOAD_HIGH_RAM	LD	HL,ROM_CODE	;Copy ISR Dispatch & Return to Upper RAM
		LD	DE,HRAM_CODE
		LD	BC,ROM_CODE_LEN
		LDIR
		RET

;FOLLOWING IS THE ISR DISPATCH ROUTINE  ISR_DISPATCH (IN HIGH RAM)
;
;ISR_DISPATCH:
ROM_CODE	XOR	A		;4  
		OUT	(RAMROM),A	;11 SELECT ROM
		IN	A,(Port40)	;11	
		RLCA			;4
		JP NC,	ISR_RXD		;10	Jump ASAP if RS-232 start bit coming in (2 Stack Words)
		RLCA			;4
		JP C,	ISR_TIMER	;10 (st=50) Jump if Timer interrupt   		(7 Stack Words)
		LD	A,0x80		;	Otherwise, unknown interrupt (RS-232 noise?)
		OUT	(Port40),A	;11	Just reset Timer interrupt, just incase?
		;JP	ISR_RET

ISR_RET_OFF	EQU	$-ROM_CODE
		LD	A,(RRSTATE)	;Restore RAM/ROM selection
		OUT	(RAMROM),A
		EX	AF,AF'		;Restore swapped Registers
		EXX
		EI
		RETI			;Return to Mainline code
		
GET_MEM_OFF	EQU	$-ROM_CODE
		LD	A,(READ_RAMROM)	;Bit 0 is RAM/ROM
		DI
		OUT	(RAMROM),A	;Select RAM/ROM
		LD	A,(HL)		;Fetch from RAM/ROM
		EX	AF,AF'
		XOR	A		;Return to ROM
		OUT	(RAMROM),A
		EX	AF,AF'
		EI		
		RET
		
;Alternative way to Get Mem and minimize the Interrupt mask black out
;HR_GET_MEM	PUSH	BC
;		LD	B,0
;		LD	C,RAMROM
;		LD	A,(READ_RAMROM)
;		DI
;		OUT	(C),A
;		LD	A,(HL)		;Fetch from RAM/ROM
;		OUT	(C),B
;		EI
;		POP	BC
;		RET
		
		
ROM_CODE_LEN	equ	$-ROM_CODE


;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Appendix_A	LED FONT
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;


; **         *********  *******        *********    *****    **     **  *********
; **         *********  ********       *********   *******   ***    **  *********
; **         **         **    ***      **         ***   ***  ****   **     ***
; **         **         **     **      **         **     **  *****  **     ***
; **         *********  **     **      *********  **     **  ** *** **     ***
; **         *********  **     **      *********  **     **  **  *****     ***
; **         **         **     **      **         **     **  **   ****     ***
; **         **         **    ***      **         ***   ***  **    ***     ***
; *********  *********  ********       **          *******   **     **     ***
; *********  *********  *******        **           *****    **     **     ***

;	0 = Segment D OR LED7       --4--
;	1 = Segment E OR LED6      2|   |3
;	2 = Segment F OR LED5       |   |
;	3 = Segment B OR LED4       --5--
;	4 = Segment A OR LED3      1|   |6
;	5 = Segment G OR LED2       |   |
;	6 = Segment C OR LED1       --0--


		ORG  ($ & 0xFF00) + 0x100
LED_HEX	DB	11011111b, 11001000b, 10111011b, 11111001b, 11101100b, 11110101b, 11110111b, 11011000b	;00-07 01234567
	DB	11111111b, 11111100b, 11111110b, 11100111b, 10010111b, 11101011b, 10110111b, 10110110b	;08-0F 89ABCDEF


		ORG  ($ & 0xFF00) + 0x20
;	**** 	; CGABFED,   CGABFED,   CGABFED,   CGABFED,   CGABFED,   CGABFED,   CGABFED,   CGABFED	;HEX	Character
LED_FONT DB	10000000b, 10000110b, 10001100b, 10111100b, 11010101b, 10101000b, 10101001b, 10000100b 	;20-27  !"#$%&'
	DB	10010111b, 11011001b, 10010100b, 10100110b, 11000001b, 10100000b, 10000001b, 10101010b	;28-2F ()*+,-./
	DB	11011111b, 11001000b, 10111011b, 11111001b, 11101100b, 11110101b, 11110111b, 11011000b	;30-37 01234567
	DB	11111111b, 11111100b, 10010001b, 11010001b, 10000011b, 10100001b, 11000001b, 10111010b	;38-3F 89:;<=>?
	DB	11111011b, 11111110b, 11100111b, 10010111b, 11101011b, 10110111b, 10110110b, 11010111b	;40-47 @ABCDEFG
	DB	11101110b, 11001000b, 11001011b, 10101110b, 10000111b, 11101010b, 11011110b, 11011111b	;48-4F HIJKLMNO
	DB	10111110b, 11111100b, 10100010b, 11110101b, 10010110b, 11001111b, 11001111b, 11001111b	;50-57 PQRSTUVW
	DB	11100000b, 11101101b, 10011011b, 10010111b, 11100100b, 11011001b, 10011100b, 10000001b	;58-5F XYZ[\]^_
	DB	10001000b, 11111011b, 11100111b, 10100011b, 11101011b, 10111111b, 10110110b, 11111101b	;60-67 `abcdefg
	DB	11100110b, 11000000b, 11001011b, 10101110b, 10000110b, 11101010b, 11100010b, 11100011b	;68-6F hijklmno
	DB	10111110b, 11111100b, 10100010b, 11110101b, 10100111b, 11000011b, 11000011b, 11000011b	;70-77 pqrstuvw
	DB	11100000b, 11101101b, 10011011b, 10010111b, 10000110b, 11011001b, 10010000b, 11101011b	;78-7F xyz{|}~

		ORG  ($ & 0xFF00) + 0x100
GO_BASIC
	INCLUDE	BASICZ80.ASM





;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Appendix_B	Future Use
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;


;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Appendix_C	RAM. System Ram allocation
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;


;                       ********      ***     **     **
;                       *********    *****    ***   ***
;                       **     **   *** ***   **** ****
;                       **     **  ***   ***  *********
;---------------------  ********   *********  ** *** **  ---------------------
;---------------------  ********   *********  ** *** **  ---------------------
;                       **  **     **     **  **     **
;                       **   **    **     **  **     **
;                       **    **   **     **  **     **
;                       **     **  **     **  **     **

RAM_LDRT	equ	0x8000


		ORG	0xFA00
LINE_BUFF	DS	128
LINE_BUFFEND	DS	2	;Room for CR,LF if needed
LI_FILESIZE	DS	4	;LINE INPUT FILE SIZE, counts down the bytes in the file as Lines are fetched
LI_SDBUFF_PTR	DS	2	;Pointer to Disk Buffer for next Line Read operation
LI_SDLOG_SEC	DS	2	;Logical Sector last read in Buffer


				;Read Hex File Performance counter
RHF_LINES	DS	2	;Line counter


;----------------------------------------------------------------------------------------------------; RAM SPACE
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;----------------------------------------------------------------------------------------------------; RAM SPACE
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;Reserve space from 0xFB00 to FB1F for Stack
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		ORG	0xFB00
StackTop	equ	$-2	; Stack = 0xFB00

STACKSPACE1	DS	40	;Stack space for ISR L1 calls  (Measured to only need 14 words, resv 20 words)
STACK_ISR1	EQU	$	;Points to top of stack
SP_ISR_SAVE	DS	2	;Space to save SP, Happens on first ISR call


HRAM_CODE	DS	ROM_CODE_LEN	;Code for Returning from ISR (may need to switch to RAM, so it must be here in High RAM)

ISR_DISPATCH	EQU	HRAM_CODE
ISR_RET		EQU	ISR_RET_OFF+HRAM_CODE ;Assign address in the HIGH RAM
GET_MEM		EQU	GET_MEM_OFF+HRAM_CODE ;Assign address in the HIGH RAM	


HR_EXE_GO	DS	HR_EXEC_GS_LEN	;Routine for Switching to RAM and Executing loaded program.

HERE1		EQU	$

	IF HERE1 > 0xFB80
	   ERROR RAM OVERLAP AT 0xFB80
	ENDIF
	
	;FREE RAM 25 BYTES

		ORG	0xFB80
;SDFCB:
FSTAT		EQU	0	;DS  1	;+0  Status of FCB, 00=File Not Open, 01=File Opened, 80=EOF (Line_Input)
FNAME		EQU	1	;DS 11	;+1  File name
AFClus0		EQU	12	;DS  2	;+12 First Cluster of File as given by the Directory Entry.
CRFClus		EQU	14	;DS  2	;+14 Current Relative Cluster location in file, (0 to F for a system with 32 Sectors per Cluster)
CAFClus		EQU	16	;DS  2	;+16 Current Absolute Cluster location in file, set to AFClus0, then updated with FAT
RFSec		EQU	18	;DS  2	;+18 Relative Sector being addressed (0 to 500, based 26 sectors per track and 77 tracks Divide by 4)
SSOC		EQU	20	;DS  4	;+20 Starting Sector of Cluster, this is the first Sector for that Cluster
ABS_SEC		EQU	24	;DS  4	;+24 Absolute Sector of Current Relative Sector
FSIZE		EQU	28	;DS  4	;+28 File Size of file (not used, just kept for completness)

;Warning: FCB's must never cross page boundaries.
SDISKA		DS	32	;File Control Block	;No FCB must cross a page boundary (simple 8 bit addition used for offseting into struct.)
SDISKB		DS	32	;File Control Block	;No FCB must cross a page boundary (simple 8 bit addition used for offseting into struct.)
SDISKC		DS	32	;File Control Block	;No FCB must cross a page boundary (simple 8 bit addition used for offseting into struct.)
SDISKD		DS	32	;File Control Block	;No FCB must cross a page boundary (simple 8 bit addition used for offseting into struct.)

;
;  ********   **     **  *********  *********  *********  ******** 
;  *********  **     **  *********  *********  *********  *********
;  **     **  **     **  **         **         **         **     **
;  **     **  **     **  **         **         **         **     **
;  ********   **     **  *******    *******    *******    ******** 
;  ********   **     **  *******    *******    *******    ******** 
;  **     **  **     **  **         **         **         **  **   
;  **     **  **     **  **         **         **         **   **  
;  *********  *********  **         **         *********  **    ** 
;  ********    *******   **         **         *********  **     **
;
		ORG	0xFC00
SD_RAM_BUFFER	DS	512	;FC00-FDFF 512 bytes of SD Sector Buffer space

RXBUFFER	DS	256	;FExx      256 bytes of RX Buffer space
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
;- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


		ORG	0xFF00
BIT_TABLE	DS	8

;Warning: FCB's must never cross page boundaries.
FCB_PTR		DS	2	;Pointer to Current FCB


FAT_CLEAR	EQU	$	;Clear bytes of RAM after this point on FAT_INIT
GH_START	DS	2	;HEX File, start address
VIEW_FLAGS	DS	1	;View File Load;  File Open View
				;BIT .0=View HEX Load, display file while reading HEX file
				;    .1=GH_START Address has been set
				;    .6=View FILE SIZE during File Open
				;    .7=View File Name & Found or Not status durin File Open

LOGICAL_SEC	DS	2	;Logical Sector for next Read Operation (Input paramater for the File Read)

DIRTY_DATA	DS	1	;Indicates when data Read has been altered, ie. Requires flushing back to SD Card
SD_CARD_TYPE	DS	1	;SD CARD TYPE
SDC_STATUS	DS	1	;SD Status Code returned
SD_PARAM	DS	4	;32 bit address parameter for SD Commands
SD_PART_TYPE	DS	1	;SD PARTITION TYPE
SD_PART_BASE	DS	4	;SD PARTITION STARTING RECORD
SD_PART_SIZE	DS	4	;SD PARTITION SIZE (Must follow SD_PART_BASE)
BYTE_P_SEC	DS	2	;0x0B Bytes per Sector (Almost always 512)
SEC_PER_CLUS	DS	1	;0x0D
RESERVED_SEC	DS	2	;0x0E - 0x0F
FAT_COPIES	DS	1	;0x10
ROOTDIR_SIZE	DS	2	;0x11 - 0x12
FILESYS_SEC	DS	4	;0x13 - 0x14 or 0x20 - 0x23
HIDDEN_SEC	DS	4	;0x1C - 0x1F
SEC_PER_FAT	DS	2	;0x16 - 0x17
FAT1START	DS	4	;Calculated Sector to FAT1
DIR_SECTOR	DS	4	;Calculated Sector to Root Directory
DATASTART	DS	4	;Calculated Sector to Data Area
SEC_PTR		DS	4	;Sector Pointer, general use variable that holds the last sector read
ENT_COUNT	DS	2	;Directory Entry Counter, Counts down maximum directory entries in Find File
FAT_CLR_LEN	EQU	$ - FAT_CLEAR 
FILENAME	DS	8	;File Name
FILEEXT		DS	3	;File Extension
FILESIZE	DS	4	;File Size


DIVIDE_FUNC	DS	2	;Pointer to the Divide Function
DF_SHIFTCNT	DS	1	;Count of shifts required for Fast Divide
MUL8		DS	1	;8 bit multiplier
MOD_FUNC	DS	2	;Pointer to the Mod Function
MODMASK		DS	1	;8 bit mask to get Relative Sector within a cluster from a Relative File sector

IK_HEXL		DS	1	;IMON HEX INPUT
IK_HEXH		DS	1	;IMON HEX INPUT
HEX_SOURCE	DS	2	;Pointer to the HEX INPUT Source (FILE or Serial Port)

	IF $ >= 0xFF78
	   ERROR RAM ALLOCATION ERROR
	ENDIF

;*** BEGIN COLD_BOOT_INIT (RAM that is to be initialized upon COLD BOOT) ***
RAMSIGNATURE	equ	0xFF78	;RAM signature
				;Following bytes are cleared on COLD BOOT
RC_TYPE		equ	0xFF80	;(1) Type of Reset (WARNING, Next 7 RC counters must end with lsb bits = 001,010,011,100,101,110,111)
RC_SOFT		equ	0xFF81	;(1) Count of Resets by SOFT F-E SWITCH
RC_STEP		equ	0xFF82	;(1) Count of Resets by SINGLE STEP
RC_CC		equ	0xFF83	;(1) Count of Resets by CTRL-C
RC_HALT		equ	0xFF84	;(1) Count of Resets by HALT INSTRUCTION
RC_F0		equ	0xFF85	;(1) Count of Resets by pressing F & 0 keys
RC_RST0		equ	0xFF86	;(1) Count of Resets by RST 0 INSTRUCTION
RC_HARD		equ	0xFF87	;(1) Count of Resets by UNKNOWN RESET LINE

ABUSS		equ	0xFF88	;(2) 
RegPtr		equ	0xFF8A	;(1) Ptr to Registers
IoPtr		equ	0xFF8B	;(1)  I/O Ptr
RX_ERR_LDRT	equ	0xFF8C	;(1) Counts False Start Bits (Noise Flag)
RX_ERR_STOP	equ	0xFF8D	;(1) Counts Missing Stop Bits (Framing Error)
RX_ERR_OVR	equ	0xFF8E	;(1) Counts Overrun Errors

CS_CLR_LEN	equ	0xFF8F-RC_TYPE

ACE_BAUD	equ	0xFF8F	;(1) Baudrate of ACE, 12=9600

				;PUTCHAR_EXE and INCHAR_EXE *must be consecutive in this order*
PUTCHAR_EXE	equ	0xFF90	;(2) PutChar Execution (Set for PC_LED, PC_BIT, PC_ACE or PC_BOTH)
INCHAR_EXE	equ	0xFF92	;(2) InChar Execution (Set for IN_KEY, IN_BIT, IN_ACE or IN_BOTH)
;*** END COLD_BOOT_INIT (RAM that is to be initialized upon COLD BOOT) ***

HW_SETIO	equ	0xFF94	;Serial IO selected, 01=Bit, 02=ACE, 03=BIT & ACE
HW_LIST		equ	0xFF95	;Hardware List, 00=NO Boards, 01=FP Only, 02=SIO only, 03=FP & SIO

				;Saved Registers
RSSP		equ	0xFF96	;Value of SP upon REGISTER SAVE
RSAF		equ	0xFF98	;Value of AF upon REGISTER SAVE
RSBC		equ	0xFF9A	;Value of BC upon REGISTER SAVE
RSDE		equ	0xFF9C	;Value of DE upon REGISTER SAVE
RSHL		equ	0xFF9E	;Value of HL upon REGISTER SAVE
RPC		equ	0xFFA0	;Value of PC upon REGISTER SAVE
RSIX		equ	0xFFA2	;Value of IX upon REGISTER SAVE
RSIY		equ	0xFFA4	;Value of IY upon REGISTER SAVE
RSIR		equ	0xFFA6	;Value of IR upon REGISTER SAVE
RSAF2		equ	0xFFA8	;Value of AF' upon REGISTER SAVE
RSBC2		equ	0xFFAA	;Value of BC' upon REGISTER SAVE
RSDE2		equ	0xFFAC	;Value of DE' upon REGISTER SAVE
RSHL2		equ	0xFFAE	;Value of HL' upon REGISTER SAVE

UiVec		equ	0xFFB0	;(2) User Interrupt Vector


;*** BEGIN WARM_BOOT_INIT (RAM that is to be initialized on every boot) ***
				;WARNING, Following 34 bytes must be consecutive in this order for Block Write
BEEP_TO		equ	0xFFB2	;(1) Count down the beep (beep duration)
ANBAR_DEF	equ	0xFFB3	;(1) Base setting for the Annunciator LED's (after current function times out)
GET_REG		equ	0xFFB4	;(2) Get Reg Routine (in monitor mode, registers fetched from RAM)
PUT_REG		equ	0xFFB6	;(2) Put Reg Routine
CTRL_C_CHK	equ	0xFFB8	;(2) Vector for CTRL-C Checking
LDISPMODE	equ	0xFFBA	;(2) Last Display Mode (Holds DISPMODE while in HEX Entry)
DISPMODE	equ	0xFFBC	;(2) Display Routine
KEY_EVENT	equ	0xFFBE	;(2) Routine to call upon Key Press (changes based on user actions, see KEY_EVENT_DISPATCH)
IK_TIMER	equ	0xFFC0	;(1) IMON TIMEOUT
KEYBFMODE	equ	0xFFC1	;(1) KEY INPUT MODE. 8F=HEX INPUT, 90=Shiftable
DISPLABEL	equ	0xFFC2	;(1) Display Label Refresh
IK_HEXST	equ	0xFFC3	;(1) IMON HEX Input State
HEX_CURSOR	equ	0xFFC4	;(2) HEX Input Cursor location
HEX_READY	equ	0xFFC6	;(2) HEX Input Ready
LED_CURSOR	equ	0xFFC8	;(2) Cursor location for LED Put_Char
RXBHEAD		equ	0xFFCA	;(2) RS-232 RX BUFFER HEAD
RXBTAIL		equ	0xFFCC	;(2) RS-232 RX BUFFER TAIL
INT_VEC		equ	0xFFCE	;(2) Vector to Interrupt ISR
SCAN_PTR	equ	0xFFD0	;(2) SCAN_PTR points to next LED_DISPLAY byte to output (will always be 1 more
				;    than the current hardware column because hardware automatically advances)
HALT_TEST	equ	0xFFD2	;(2) HALT_TEST
;*** END WARM_BOOT_INIT (RAM BLOCK that is to be initialized on every boot) ***


CLEARED_SPACE	equ	0xFFD4	;Bytes here and later are cleared upon init (some initialized seperately)
CLEARED_LEN	equ	0xFFFF - CLEARED_SPACE + 1

SDISPMODE	equ	0xFFD4	;(2)
POS_BIT		equ	0xFFD6	;(1) BIT Bang RS232 Character position
POS_ACE		equ	0xFFD7	;(1) ACE RS232 Character position
ISR_FLAGS	equ	0xFFD8	;(1) Indicates how many levels the ISR has nested
RRSTATE		equ	0xFFD9	;(1) RAM/ROM SELECT STATE (FOR RETURN FROM ISR)	
XTIMER_TIC	equ	0xFFDA	;(1) Counts Down tics until Extended Timer ISR_EXXTIMER
READ_RAMROM	equ	0xFFDB	;(1) Selected RAMROM for Monitor READ Operations.  Bit0 for RAM/ROM, Bits1-4 for Bank

NMI_VEC		equ	0xFFDC	;(2) NMI VEC.  Not used

CTRL_C_TIMER	equ	0xFFDE	;Count down the CTRL-C condition
SOFT_RST_FLAG	equ	0xFFDF	;Flag a Soft Reset (F-E Keys, Single Step)

				;Display/Serial Comms
LED_DISPLAY	equ	0xFFE0	;8 Bytes of LED Output bytes to Scan to hardware
;8 Bytes			;Warning, LED_DISPLAY must be nibble aligned at E0 (XXE0)
LED_ANBAR	equ	0xFFE7	;LED Annunciator Bar (Part of LED_DISPLAY Buffer)
				;.0 = x7 = BEEPER
				;.1 = x6 = Run Mode
				;.2 = x5 = Monitor Mode (Default Mode upon Power up)
				;.3 = x4 = Send Data to Output Port
				;.4 = x3 = Alter Memory/Register
				;.5 = x2 = Enter Memory Location
				;.6 = x1 = Enter Register

KBCOLSAMPLED	equ	0xFFE8	;Columns Sampled
KBPORTSAMPLE	equ	0xFFE9	;Input Port sampled only once on each scan, saved here
KBHEXSAMPLE	equ	0xFFEA	;KEY SAMPLER Input HEX format
KEYBSCANPV	equ	0xFFEB	;KEY Input HEX format
KEYBSCANTIMER	equ	0xFFEC	;KEY Input TIMER
KEY_PRES_EV	equ	0xFFED	;KEY INPUT LAST & Currently Processing
KEY_PRES_RTN	equ	0xFFEE	;KEY LAST, for Return To Normal between strokes
KEY_PRESSED	equ	0xFFEF	;(1) KEY PRESSED

TicCounter	equ	0xFFF0	;Tic Counter
;TicCounter	equ	0xFFF1	;
ECHO_STATE	equ	0xFFF2	;Echo characters
XMSEQ		equ	0xFFF3	;XMODEM SEQUENCE NUMBER
XMTYPE		equ	0xFFF4	;XMODEM BLOCK TYPE (CRC/CS)
SCAN_LED	equ	0xFFF5	;Holds the next LED output
LED_DISPLAY_SB	equ	0xFFF6	;10 Bytes FFF6=Start BIT, 7,8,9,A,B,C,D,E=Data bits, F=Stop BIT
;10 bytes	equ	0xFFFF	;Warning, LED_DISPLAY_TBL must be at this address (XXF6) Roll over to xx00 tested



;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Appendix_D	HOOK LOCATIONS
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

		org	0x0042
VMAIN_MENU	JP	MAIN_MENU	;MONITOR
VPUT_CHAR	JP	PUT_CHAR	;PUT_CHAR
VGET_POS	JP	GET_POS		;Return the Position of the cursor on the line
VGET_CHAR	JP	GET_CHAR
VRX_COUNT	JP	RX_COUNT	;Count of Characters waiting in the RX Buffer
VIN_CHAR	JP	IN_CHAR		;Returns a char or C=1 if none.
VTIMED_GETCHAR	JP	TIMED_GETCHAR
VPRINT		JP	PRINT
VBEEP		JP	BEEP

VSET_ECHO	JP	SET_ECHO
VDELAY_C	JP	DELAY_C		;Loop based on C, 100mSEC
VDELAY_A	JP	DELAY_A		;Milli seconds based on A, requires FP board

		org	0x006B
VPUT_VERSION	JP	PUT_VERSION
VPUT_HEX	JP	PUT_HEX
VPUT_BYTE	JP	PUT_BYTE
VPUT_BC		JP	PUT_BC
VPUT_DE		JP	PUT_DE
VPUT_HL		JP	PUT_HL
VPUT_SPACE	JP	PUT_SPACE
VPUT_NEW_LINE	JP	PUT_NEW_LINE

VGET_BYTE	JP	GET_BYTE
VGET_WORD	JP	GET_WORD	;DE
VGET_HEX	JP	GET_HEX

VCLEAR_BLOCK	JP	CLEAR_BLOCK	;CLEAR_BLOCK
VLD_HL_HL	JP	LD_HL_HL
VADD_HL_A	JP	ADD_HL_A
VMOV_32_HL	JP	MOV_32_HL	;Move (HL) to 32 bit register BCDE
VMOV_HL_32	JP	MOV_HL_32	;Move 32 bit register BCDE to (HL)
VADD_32_HL	JP	ADD_32_HL	;ADD (HL) to 32 bit register BCDE - (returns with HL changed to last byte)
VCMP_HL_32	JP	CMP_HL_32	;Compare BCDE with 32bit word at HL
VINC_32		JP	INC_32		;INC BCDE
VDEC_32		JP	DEC_32		;DEC BCDE
VTSTZ_32	JP	TSTZ_32		;TEST_ZERO BCDE

VASC2BIN	JP	ASC2BIN
VASC2HEX	JP	ASC2HEX
VHEX2ASC	JP	HEX2ASC		
VTO_UPPER	JP	TO_UPPER

VSET_IO		JP	SET_IO
VIC_KEY		JP	IC_KEY
VLED_UPDATE	JP	LED_UPDATE	;Turn ON/OFF LED UPDATE & KEY MON

VLED_GET_POS	JP	LED_GET_POS
VLED_SET_POS	JP	LED_SET_POS
VLED_HOME	JP	LED_HOME
VLED_PUT_CHAR	JP	PC_LED		;C = CHAR
VLED_PRINT	JP	LED_PRINT
VLED_PUT_BYTE	JP	LED_PUT_BYTE
VLED_PUT_HEX	JP	LED_PUT_HEX
VLED_CLEAR	JP	LED_CLEAR
VLED_RIGHT	JP	LED_RIGHT
VLED_LEFT	JP	LED_LEFT

VPRINT_FNAME	JP	PRINT_FILENAME
VINPUT_FNAME	JP	INPUT_FILENAME
VSD_OPEN	JP	SD_OPEN			;Copy FCB to filename then search & open
VSD_OPEN_FILENAME JP	SD_OPEN_FILENAME	;Copy Filename to FCB then search & open
VPRINT_DIR	JP	PRINT_DIR
VDISK_READ	JP	DISK_READ
VSD_READ_SEC	JP	SD_READ_SEC
VLINE_INPUT	JP	LINE_INPUT

		END	0

;                       *********   *******    *********
;                       *********  *********   *********
;                       **         **     **   **
;                       **         **     **   **
;---------------------  *******    **     **   *******    ---------------------
;---------------------  *******    **     **   *******    ---------------------
;                       **         **     **   **
;                       **         **     **   **
;                       *********  *********   **
;                       *********   *******    **

;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;
;	Appendix_E	Z80 Instruction Reference
;>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>;
;<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<;

;===================================================
;Mnemonic	Cyc	Opcodes		Bytes
;ADC A,(HL)	7	8E		1
;ADC A,(IX+o)	19	DD 8E oo	3
;ADC A,(IY+o)	19	FD 8E oo	3
;ADC A,n	7      	CE nn        	2
;ADC A,r	4	88+r		1
;ADC A,IXp	8	DD 88+P		2
;ADC A,IYp	8	FD 88+P		2
;ADC HL,BC	15	ED 4A		2
;ADC HL,DE	15	ED 5A		2
;ADC HL,HL	15	ED 6A		2
;ADC HL,sp	15	ED 7A		2
;ADD A,(HL)	7	86		1
;ADD A,(IX+o)	19	DD 86 oo	3
;ADD A,(IY+o)	19	FD 86 oo	3
;ADD A,n	7      	C6 nn		2
;ADD A,r	4	80+r		1
;ADD A,IXp      8      	DD 80+P		2
;ADD A,IYp      8      	FD 80+P		2
;ADD HL,BC	11	09		1
;ADD HL,DE	11	19		1
;ADD HL,HL	11	29		1
;ADD HL,sp	11	39		1
;ADD IX,BC	15	DD 09		2
;ADD IX,DE	15	DD 19		2
;ADD IX,IX	15	DD 29		2
;ADD IX,sp	15	DD 39		2
;ADD IY,BC	15	FD 09		2
;ADD IY,DE	15	FD 19		2
;ADD IY,IY	15	FD 29		2
;ADD IY,sp	15	FD 39		2
;AND (HL)	7	A6		1
;AND (IX+o)	19	DD A6 oo	3
;AND (IY+o)	19	FD A6 oo	3
;AND n       	7      	E6 nn		2
;AND r		4	A0+r		1
;AND IXp        8      	DD A0+P		2
;AND IYp        8      	FD A0+P		2
;BIT B,(HL)	12	CB 46+8*B	2	Test BIT B (AND the BIT, but do not save), Z=1 if BIT tested is 0
;BIT B,(IX+o)	20	DD CB oo 46+8*B	4	Test BIT B (AND the BIT, but do not save), Z=1 if BIT tested is 0
;BIT B,(IY+o)	20	FD CB oo 46+8*B	4	Test BIT B (AND the BIT, but do not save), Z=1 if BIT tested is 0
;BIT B,r	8	CB 40+8*B+r	2	Test BIT B (AND the BIT, but do not save), Z=1 if BIT tested is 0
;CALL nn	17	CD nn nn	3
;CALL C,nn	17/10	DC nn nn	3
;CALL M,nn	17/10	FC nn nn	3
;CALL NC,nn	17/10	D4 nn nn	3
;CALL NZ,nn	17/10	C4 nn nn	3
;CALL P,nn	17/10	F4 nn nn	3
;CALL PE,nn	17/10	EC nn nn	3
;CALL PO,nn	17/10	E4 nn nn	3
;CALL Z,nn	17/10	CALL C, nn nn	3
;CCF		4	3F		1
;CP (HL)	7	BE		1
;CP (IX+o)	19	DD BE oo	3
;CP (IY+o)	19	FD BE oo	3
;CP n        	7      	FE nn		2
;CP r		4	B8+r		1
;CP IXp        	8      	DD B8+P		2
;CP IYp        	8      	FD B8+P        	2
;CPD		16	ED A9		2
;CPDR		21/16	ED B9		2
;CP		16	ED A1		2
;CPIR		21/16	ED B1		2
;CPL		4	2F		1
;DAA		4	27		1
;DEC (HL)	11	35		1
;DEC (IX+o)	23	DD 35 oo	3
;DEC (IY+o)	23	FD 35 oo	3
;DEC A		4	3D		1
;DEC B		4	05		1
;DEC BC		6	0B		1
;DEC C		4	0D		1
;DEC D		4	15		1
;DEC DE		6	1B		1
;DEC E		4	1D		1
;DEC H		4	25		1
;DEC HL		6	2B		1
;DEC IX		10	DD 2B		2
;DEC IY		10	FD 2B		2
;DEC IXp        8      	DD 05+8*P	2
;DEC IYp        8      	FD 05+8*q      	2
;DEC L		4	2D		2
;DEC sp		6	3B		1
;DI		4	F3		1
;DJNZ o		13/8	10 oo		2
;EI		4	FB		1
;EX (sp),HL	19	E3		1
;EX (sp),IX	23	DD E3		2
;EX (sp),IY	23	FD E3		2
;EX AF,AF'	4	08		1
;EX	DE,HL	4	EB		1
;EXX		4	D9		1
;HALT		4	76		1
;IM 0		8	ED 46		2
;IM 1		8	ED 56		2
;IM 2		8	ED 5E		2
;IN A,(C)	12	ED 78		2
;IN A,(n)	11	db nn		2
;IN B,(C)	12	ED 40		2
;IN C,(C)	12	ED 48		2
;IN D,(C)	12	ED 50		2
;IN E,(C)	12	ED 58		2
;IN H,(C)	12	ED 60		2
;IN L,(C)	12	ED 68		2
;IN F,(C)	12	ED 70		3
;INC (HL)	11	34		1
;INC (IX+o)	23	DD 34 oo	3
;INC (IY+o)	23	FD 34 oo	3
;INC A		4	3C		1
;INC B		4	04		1
;INC BC		6	03		1
;INC C		4	0C		1
;INC D		4	14		1
;INC DE		6	13		1
;INC E		4	1C		1
;INC H		4	24		1
;INC HL		6	23		1
;INC IX		10	DD 23		2
;INC IY		10	FD 23		2
;INC IXp        8      	DD 04+8*P	2
;INC IYp       	8      	FD 04+8*q      	2
;INC L		4	2C		1
;INC sp		6	33		1
;IND		16	ED AA		2
;INDR		21/16	ED BA		2
;INI		16	ED A2		2
;INIR		21/16	ED B2		2
;JP nn		10	C3 nn nn	3	Jump Absolute
;JP (HL)	4	E9		1
;JP (IX)	8	DD E9		2
;JP (IY)	8	FD E9		2
;JP C,nn	10	DA nn nn	3
;JP M,nn	10	FA nn nn	3
;JP NC,nn	10	D2 nn nn	3
;JP NZ,nn	10	C2 nn nn	3
;JP P,nn	10	F2 nn nn	3
;JP PE,nn	10	EA nn nn	3
;JP PO,nn	10	E2 nn nn	3
;JP Z,nn	10	CA nn nn	3
;JR o		12	18 oo		2	Jump Relative
;JR C,o		12/7	38 oo		2
;JR NC,o	12/7	30 oo		2
;JR NZ,o	12/7	20 oo		2
;JR Z,o		12/7	28 oo		2
;LD (BC),A	7	02		1
;LD (DE),A	7	12		1
;LD (HL),n      10     	36 nn		2
;LD (HL),r	7	70+r		1
;LD (IX+o),n    19     	DD 36 oo nn	4
;LD (IX+o),r	19	DD 70+r oo	3
;LD (IY+o),n    19     	FD 36 oo nn	4
;LD (IY+o),r	19	FD 70+r oo	3
;LD (nn),A	13	32 nn nn	3
;LD (nn),BC	20	ED 43 nn nn	4
;LD (nn),DE	20	ED 53 nn nn	4
;LD (nn),HL	16	22 nn nn	3
;LD (nn),IX	20	DD 22 nn nn	4
;LD (nn),IY	20	FD 22 nn nn	4
;LD (nn),sp	20	ED 73 nn nn	4
;LD A,(BC)	7	0A		1
;LD A,(DE)	7	1A		1
;LD A,(HL)	7	7E		1
;LD A,(IX+o)	19	DD 7E oo	3
;LD A,(IY+o)	19	FD 7E oo	3
;LD A,(nn)	13	3A nn nn	3
;LD A,n        	7     	3E nn		2
;LD A,r		4	78+r		1
;LD A,IXp       8      	DD 78+P        	2
;LD A,IYp       8      	FD 78+P        	2
;LD A,I		9	ED 57		2
;LD A,R		9	ED 5F		2
;LD B,(HL)	7	46		1
;LD B,(IX+o)	19	DD 46 oo	3
;LD B,(IY+o)	19	FD 46 oo	3
;LD B,n        	7      	06 nn		2
;LD B,r		4	40+r		1
;LD B,IXp       8      	DD 40+P		2
;LD B,IYp       8     	FD 40+P        	2
;LD BC,(nn)	20	ED 4B nn nn	4
;LD BC,nn	10	01 nn nn	3
;LD C,(HL)	7	4E		1
;LD C,(IX+o)	19	DD 4E oo	3
;LD C,(IY+o)	19	FD 4E oo	3
;LD C,n        	7      	0E nn        	2
;LD C,r		4	48+r		1
;LD C,IXp       8      	DD 48+P        	2
;LD C,IYp       8      	FD 48+P		2
;LD D,(HL)	7	56		1
;LD D,(IX+o)	19	DD 56 oo	3
;LD D,(IY+o)	19	FD 56 oo	3
;LD D,n        	7      	16 nn		2
;LD D,r		4	50+r		1
;LD D,IXp       8      	DD 50+P        	2
;LD D,IYp       8      	FD 50+P        	2
;LD DE,(nn)	20	ED 5B nn nn	4
;LD DE,nn	10	11 nn nn	3
;LD E,(HL)	7	5E		1
;LD E,(IX+o)	19	DD 5E oo	3
;LD E,(IY+o)	19	FD 5E oo	3
;LD E,n        	7      	1E nn        	2
;LD E,r		4	58+r		1
;LD E,IXp       8      	DD 58+P        	2
;LD E,IYp       8      	FD 58+P        	2
;LD H,(HL)	7	66		1
;LD H,(IX+o)	19	DD 66 oo	3
;LD H,(IY+o)	19	FD 66 oo	3
;LD H,n        	7      	26 nn		2
;LD H,r		4	60+r		1
;LD HL,(nn)	16	2A nn nn	5
;LD HL,nn	10	21 nn nn	3
;LD I,A		9	ED 47		2
;LD IX,(nn)	20	DD 2A nn nn	4
;LD IX,nn	14	DD 21 nn nn	4
;LD IXh,n       11     	DD 26 nn 	2
;LD IXh,P       8     	DD 60+P		2
;LD IXl,n       11     	DD 2E nn 	2
;LD IXl,P       8     	DD 68+P		2
;LD IY,(nn)	20	FD 2A nn nn	4
;LD IY,nn	14	FD 21 nn nn	4
;LD IYh,n       11     	FD 26 nn 	2
;LD IYh,q       8     	FD 60+P		2
;LD IYl,n       11     	FD 2E nn 	2
;LD IYl,q       8     	FD 68+P		2
;LD L,(HL)	7	6E		1
;LD L,(IX+o)	19	DD 6E oo	3
;LD L,(IY+o)	19	FD 6E oo	3
;LD L,n       	7     	2E nn		2
;LD L,r		4	68+r		1
;LD R,A		9	ED 4F		2
;LD sp,(nn)	20	ED 7B nn nn	4
;LD sp,HL	6	F9		1
;LD sp,IX	10	DD F9		2
;LD sp,IY	10	FD F9		2
;LD sp,nn	10	31 nn nn	3
;LDD		16	ED A8		2
;LDDR		21/16	ED B8		2
;LDI		16	ED A0		2
;LDIR		21/16	ED B0		2
;MULUB A,r 		ED C1+8*r 	2
;MULUW HL,BC		ED C3 		2
;MULUW HL,sp		ED F3 		2
;NEG		8	ED 44		2
;NOP		4	00		1
;OR (HL)	7	B6		1
;OR (IX+o)	19	DD B6 oo	3
;OR (IY+o)	19	FD B6 oo	3
;OR n       	7     	F6 nn		2
;OR r		4	B0+r		1
;OR IXp       	8     	DD B0+P		2
;OR IYp       	8     	FD B0+P		2
;OTDR		21/16	ED BB		2
;OTIR		21/16	ED B3		2
;OUT (C),A	12	ED 79		2
;OUT (C),B	12	ED 41		2
;OUT (C),C	12	ED 49		2
;OUT (C),D	12	ED 51		2
;OUT (C),E	12	ED 59		2
;OUT (C),H	12	ED 61		2
;OUT (C),L	12	ED 69		2
;OUT (n),A	11	D3 nn		2
;OUTD		16	ED AB		2
;OUTI		16	ED A3		2
;POP AF		10	F1		1
;POP BC		10	C1		1
;POP DE		10	D1		1
;POP HL		10	E1		1
;POP IX		14	DD E1		2
;POP IY		14	FD E1		2
;PUSH AF	11	F5		1
;PUSH BC	11	C5		1
;PUSH DE	11	D5		1
;PUSH HL	11	E5		1
;PUSH IX	15	DD E5		2
;PUSH IY	15	FD E5		2
;RES B,(HL)	15	CB 86+8*B	2	Reset BIT B (clear BIT)
;RES B,(IX+o)	23	DD CB oo 86+8*B	4	Reset BIT B (clear BIT)
;RES B,(IY+o)	23	FD CB oo 86+8*B	4	Reset BIT B (clear BIT)
;RES B,r	8	CB 80+8*B+r	2	Reset BIT B (clear BIT)
;RET		10	C9		1
;RET C		11/5	D8		1
;RET M		11/5	F8		1
;RET NC		11/5	D0		1
;RET NZ		11/5	C0		1
;RET P		11/5	F0		1
;RET PE		11/5	E8		1
;RET PO		11/5	E0		1
;RET Z		11/5	C8		1
;RETI		14	ED 4D		2
;RETN		14	ED 45		2
;RL (HL)	15	CB 16		2  	9 BIT rotate left through Carry
;RL (IX+o)	23	DD CB oo 16	4	9 BIT rotate left through Carry
;RL (IY+o)	23	FD CB oo 16	4	9 BIT rotate left through Carry
;RL r       	8     	CB 10+r		2	9 BIT rotate left through Carry
;RLA		4	17		1	9 BIT rotate left through Carry
;RLC (HL)	15	CB 06		2	8 BIT rotate left, C=msb
;RLC (IX+o)	23	DD CB oo 06	4	8 BIT rotate left, C=msb
;RLC (IY+o)	23	FD CB oo 06	4	8 BIT rotate left, C=msb
;RLC r		8	CB 00+r		2	8 BIT rotate left, C=msb
;RLCA		4	07		1	8 BIT rotate left, C=msb
;RLD		18	ED 6F		2	3 nibble rotate, A3-0 to (HL)3-0, (HL)3-0 to (HL)7-4, (HL)7-4 to A3-0
;RR (HL)	15	CB 1E		2	9 BIT rotate right through Carry
;RR (IX+o)	23	DD CB oo 1E	4	9 BIT rotate right through Carry
;RR (IY+o)	23	FD CB oo 1E	4	9 BIT rotate right through Carry
;RR r       	8     	CB 18+r		2	9 BIT rotate right through Carry
;RRA		4	1F		1	9 BIT rotate right through Carry
;RRCA (HL)	15	CB 0E		2	8 BIT rotate right, C=lsb
;RRCA (IX+o)	23	DD CB oo 0E	4	8 BIT rotate right, C=lsb
;RRCA (IY+o)	23	FD CB oo 0E	4	8 BIT rotate right, C=lsb
;RRCA r		8	CB 08+r		2	8 BIT rotate right, C=lsb
;RRCAA		4	0F		1	8 BIT rotate right, C=lsb
;RRD		18	ED 67		2	3 nibble rotate, A3-0 to (HL)7-4, (HL)7-4 to (HL)3-0, (HL)3-0 to A3-0
;RST 0		11	C7		1
;RST 8H		11	CF		1
;RST 10H	11	D7		1
;RST 18H	11	DF		1
;RST 20H	11	E7		1
;RST 28H	11	EF		1
;RST 30H	11	F7		1
;RST 38H	11	FF		1
;SBC A,(HL)	7	9E		1
;SBC A,(IX+o)	19	DD 9E oo	3
;SBC A,(IY+o)	19	FD 9E oo	3
;SBC A,n	7	DE nn		2
;SBC A,r	4	98+r		1
;SBC A,IXp      8     	DD 98+P		2
;SBC A,IYp      8     	FD 98+P		2
;SBC HL,BC	15	ED 42		2
;SBC HL,DE	15	ED 52		2
;SBC HL,HL	15	ED 62		2
;SBC HL,sp	15	ED 72		2
;SCF		4	37		1	Set Carry
;SET B,(HL)	15	CB C6+8*B	2	Set BIT B (0-7)
;SET B,(IX+o)	23	DD CB oo C6+8*B	4	Set BIT B (0-7)
;SET B,(IY+o)	23	FD CB oo C6+8*B	4	Set BIT B (0-7)
;SET B,r	8	CB C0+8*B+r	2	Set BIT B (0-7)
;SLA (HL)	15	CB 26		2	9 BIT shift left, C=msb, lsb=0
;SLA (IX+o)	23	DD CB oo 26	4	9 BIT shift left, C=msb, lsb=0
;SLA (IY+o)	23	FD CB oo 26	4	9 BIT shift left, C=msb, lsb=0
;SLA r		8	CB 20+r		2	9 BIT shift left, C=msb, lsb=0
;SRA (HL)	15	CB 2E		2	8 BIT shift right, C=lsb, msb=msb (msb does not change)
;SRA (IX+o)	23	DD CB oo 2E	4	8 BIT shift right, C=lsb, msb=msb (msb does not change)
;SRA (IY+o)	23	FD CB oo 2E	4	8 BIT shift right, C=lsb, msb=msb (msb does not change)
;SRA r		8	CB 28+r		2	8 BIT shift right, C=lsb, msb=msb (msb does not change)
;SRL (HL)	15	CB 3E		2	8 BIT shift right, C=lsb, msb=0
;SRL (IX+o)	23	DD CB oo 3E	4	8 BIT shift right, C=lsb, msb=0
;SRL (IY+o)	23	FD CB oo 3E	4	8 BIT shift right, C=lsb, msb=0
;SRL r		8	CB 38+r		2	8 BIT shift right, C=lsb, msb=0
;SUB (HL)	7	96		1
;SUB (IX+o)	19	DD 96 oo	3
;SUB (IY+o)	19	FD 96 oo	3
;SUB n       	7     	D6 nn		2
;SUB r		4	90+r		1
;SUB IXp       	8     	DD 90+P		2
;SUB IYp       	8     	FD 90+P		2
;XOR (HL)	7	AE		1
;XOR (IX+o)	19	DD AE oo	3
;XOR (IY+o)	19	FD AE oo	3
;XOR n       	7     	EE nn		2
;XOR r       	4     	A8+r		1
;XOR IXp       	8     	DD A8+P		2
;XOR IYp       	8     	FD A8+P		2
;
;variables used:
;
; B = 3-BIT value
; n = 8-BIT value
; nn= 16-BIT value
; o = 8-BIT offset (2-complement)
; r = Register. This can be A, B, C, D, E, H, L OR (HL). Add to the last byte of the opcode:
;
;		Register	Register bits value
;		A		7
;		B		0
;		C		1
;		D		2
;		E		3
;		H		4
;		L		5
;		(HL)		6
;
; P = The high OR low part of the IX OR IY register: (IXh, IXl, IYh, IYl). Add to the last byte of the opcode:
;
;		Register	Register bits value
;		A		7
;		B		0
;		C		1
;		D		2
;		E		3
;		IXh (IYh)	4
;		IXl (IYl)	5

				
