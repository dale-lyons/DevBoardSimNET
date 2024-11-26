using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085
{
    public enum SingleRegisterEnums : byte
    {
        b = 0x00,
        c = 0x01,
        d = 0x02,
        e = 0x03,
        h = 0x04,
        l = 0x05,
        m = 0x06,
        a = 0x07,
        None = 0xff
    }
}