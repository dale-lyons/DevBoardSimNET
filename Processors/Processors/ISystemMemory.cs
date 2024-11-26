using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public delegate void InvalidMemoryAccessDelegate(uint address);
    public interface ISystemMemory
    {
        byte this[uint addr]
        {
            get;
            set;
        }

        WordSize WordSize { get; set; }
        Endian Endian { get; set; }

        uint Start { get; }
        uint End { get; }

        IMemoryBlock[] MemoryMap { get; }

        void AddMemoryOverride(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate);
        void Copy(uint addr, byte[] bytes);
        string FormatAddress(uint addr);

        IMemoryBlock SnapShot(uint address, uint numBytes);

        void SetReadWriteOverride(uint addr, bool allow);


        IList<uint> Diff(IMemoryBlock block);

        void SetMemory(uint address, WordSize ws, uint data, bool sideEffect = true);
        uint GetMemory(uint address, WordSize ws, bool sideEffect = true);

        IMemoryBlock CreateMemoryBlock(uint baseAddress, byte[] bytes, int offset, uint size, bool allowWrite);
        bool ValidWordAddress(uint addr);
        bool ValidDWordAddress(uint addr);
        bool ValidByteAddress(uint addr);

        void Clear();
        void Default(uint size);

        event InvalidMemoryAccessDelegate OnInvalidMemoryAccess;

    }
}