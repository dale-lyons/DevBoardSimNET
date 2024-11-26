using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085
{
    public enum ValidDoubleRegisters
    {
        BD,             //ldax
        BDHSP,          //dcx
        BDHPSW          //pop,push
    }
}