using System;

namespace Processors
{
    public class MemoryBlock : IMemoryBlock
    {
        public uint StartAddress { get; set; }
        public byte[] Bytes { get; set; }
        public uint Length { get; set; }
        public bool AllowWrite { get; set; }
        public uint EndAddress { get { return (uint)(StartAddress + Length - 1); } }

        public event InvalidMemoryAccessDelegate OnInvalidMemoryAccess;

        private MemoryBlock()
        {

        }

        public MemoryBlock(uint baseAddress, byte[] bytes, int offset, uint length, bool allowWrite)
        {
            StartAddress = baseAddress;
            Bytes = new byte[length];
            if (bytes != null)
                Array.Copy(bytes, offset, Bytes, 0, Math.Min(length, bytes.Length));
            else
            {
                for (uint ii = StartAddress; ii < Bytes.Length; ii++)
                    Bytes[ii] = 0xff;
            }

            Length = (uint)Bytes.Length;
            AllowWrite = allowWrite;
        }

        public object Clone()
        {
            return new MemoryBlock { StartAddress = this.StartAddress, Bytes = this.Bytes, Length = this.Length, AllowWrite = this.AllowWrite };
        }

        public bool Contains(uint addr)
        {
            return (addr >= StartAddress && addr < (StartAddress + Bytes.Length));
        }

        public byte this[uint addr]
        {
            get
            {
                uint offset = addr - StartAddress;
                if (!Contains(addr))
                {
                    OnInvalidMemoryAccess?.Invoke(addr);
                    return 0;
                }
                else
                {
                    return Bytes[addr - StartAddress];
                }
            }
            set
            {
                uint offset = addr - StartAddress;
                if (!Contains(addr))
                {
                    OnInvalidMemoryAccess?.Invoke(addr);
                    return;
                }
                else
                {
                    Bytes[addr - StartAddress] = value;
                }
            }
        }
    }//class MemoryBlock
}