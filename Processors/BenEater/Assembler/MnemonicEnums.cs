using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors.BenEater.Assembler
{
    public enum MnemonicEnums : byte
    {
        nop = 0x00,
        lda = 0x10,
        add = 0x20,
        sub = 0x30,
        sta = 0x40,
        ldi = 0x50,
        jmp = 0x60,
        jc = 0x70,
        outA = 0xe0,
        hlt = 0xf0,
    }
}