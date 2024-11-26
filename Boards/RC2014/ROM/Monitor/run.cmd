del *.lst
del *.hex
del *.bin
C:\Projects\DevBoardSimNET\Processors\Intel8085\A85\x64\Debug\a85.exe -I C:\Projects\DevBoardSimNET\Boards\GW8085SBC\ROM\Monitor -L ddt.lst -O ddt.hex ddt.asm
rem C:\Projects\DevBoardSimNET\Processors\ZilogZ80\zmac\zmac.exe -I C:\Projects\DevBoardSimNET\Boards\GW8085SBC\ROM\Monitor --od . --zmac --fcal -L ddt.asm