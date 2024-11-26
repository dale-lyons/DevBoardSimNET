using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public interface IMemoryBlock : ICloneable
    {
        byte this[uint addr]
        {
            get;
            set;
        }

        uint StartAddress { get; set; }
        byte[] Bytes { get; set; }
        uint Length { get; set; }
        bool AllowWrite { get; set; }
        uint EndAddress { get; }

        event InvalidMemoryAccessDelegate OnInvalidMemoryAccess;

    }
}