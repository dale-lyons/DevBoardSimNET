using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085
{
    public enum DoubleRegisterEnums : byte
    {
        b = 0x00,
        d = 0x01,
        h = 0x02,
        sp = 0x03,
        psw = 0x04,
        ix = 0x05,
        iy = 0x06,
        None = 0xff
    }
}