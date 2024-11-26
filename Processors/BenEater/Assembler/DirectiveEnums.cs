using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors.BenEater.Assembler
{
    public enum DirectiveEnums
    {
        set,
        equ,
        org,
        db,
        dw,
        ds,
        end,
        ifA,
        endif,
        sym,
        include,
        page,
        title,
        link,
        macro,
        endm
    }
}