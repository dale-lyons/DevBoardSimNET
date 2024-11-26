using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M68HC11
{
    public enum AM
    {// Define addressing modes
        ILL, //illegal opcode
        INH, //inherent (no operand, direct execution
        REL, //relative (branches)
        INX, //indexed relative to X, 8-bit value
        INY, //indexed relative to Y, 8-bit value
        EXT, //extended (two bytes absolute address), read 8 bits of data
        IM1, //immediate, one byte
        IM2, //immediate, two byte(s)
        DIR //direct (one byte abs address), read 8 bits of data
    }




        //ILL, //illegal opcode
        //INH, //inherent (no operand, direct execution
        //IM1, //immediate, one byte
        //IM2, //immediate, two bytes
        //DIR, //direct (one byte abs address), read 8 bits of data
        //DI2, //direct (one byte abs address), read 16 bits of data (X,Y,D)
        //DIS, //direct (one byte abs address), no read of data (store only)
        //REL, //relative (branches)
        //EXT, //extended (two bytes absolute address), read 8 bits of data
        //EX2, //extended (two bytes absolute address), read 16 bits of data (X,Y,D)
        //EXS, //extended (two bytes absolute address), no read of data (store only)
        //INX, //indexed relative to X, 8-bit value
        //IX2, //indexed relative to X, 16-bit value
        //IXS, //indexed relative to X, no value read
        //INY, //indexed relative to Y, 8-bit value
        //IY2, //indexed relative to Y, 16-bit value
        //IYS //indexed relative to Y, no value read
}