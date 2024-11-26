using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM7.Disassembler
{
    public class AsmException : Exception
    {
        public AsmException() { }

        public AsmException(String msg, params Object[] args)
            :
            base("Internal error while processing Arm code:\n\t" +
                String.Format(msg, args))
        { }
    }
}
