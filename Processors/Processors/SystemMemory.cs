using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    /// <summary>
    /// This class represents the entire physical memory (ram and rom) of the simualted system
    /// </summary>
    public class SystemMemory : ISystemMemory
    {
        public delegate object InvokeCallback(Delegate method, params object[] args);
        public Endian Endian { get; set; } = Endian.Little;

        public const uint FIVE_TWELVEK = 1024 * 512;
        public static uint SIXTY_FOURK = 1024 * 64;
        public static uint THIRTYTWOK = (1024 * 32);
        public static uint SIXTEENK = (1024 * 16);
        public static uint EIGHTK = 1024 * 8;
        public static uint FOURK = 1024 * 4;

        private static byte[] mBuff2 = new byte[2];
        private static byte[] mBuff4 = new byte[4];

        private memoryAccessReadEventHandler[] MemoryAccessReadEventHandlers = new memoryAccessReadEventHandler[SIXTY_FOURK];
        private memoryAccessWriteEventHandler[] MemoryAccessWriteEventHandlers = new memoryAccessWriteEventHandler[SIXTY_FOURK];
        private IMemoryBlock[] mMemoryMap = new IMemoryBlock[SIXTY_FOURK];

        public IMemoryBlock[] MemoryMap { get { return mMemoryMap; } }

        public WordSize WordSize { get; set; }

        public event InvalidMemoryAccessDelegate OnInvalidMemoryAccess;

        public void Copy(uint addr, byte[] bytes)
        {
            foreach (byte b in bytes)
            {
                this[addr++] = b;
            }
        }
        public byte this[uint addr]
        {
            get { return getByte(addr, false); }
            set { putByte(addr, value, false); }
        }

        public string FormatAddress(uint addr)
        {
            switch (WordSize)
            {
                case WordSize.OneByte:
                    return ((byte)addr).ToString("X2");
                case WordSize.TwoByte:
                    return ((ushort)addr).ToString("X4");
                case WordSize.FourByte:
                    return addr.ToString("X8");
                default:
                    throw new Exception();
            }
        }

        public bool ValidDWordAddress(uint addr)
        {
            if (!ValidWordAddress(addr))
                return false;
            if (!ValidWordAddress(addr + 2))
                return false;
            return true;
        }

        public bool ValidWordAddress(uint addr)
        {
            if (!ValidByteAddress(addr))
                return false;
            if (!ValidByteAddress(addr + 1))
                return false;
            return true;
        }

        public bool ValidByteAddress(uint addr)
        {
            return mMemoryMap[addr] != null;
        }

        public void Default(uint size)
        {
            mMemoryMap = new IMemoryBlock[size];
            CreateMemoryBlock(0, null, 0, size, true);
        }

        public void Clear()
        {
            for (int ii = 0; ii < mMemoryMap.Length; ii++)
                mMemoryMap[ii] = null;
            for (int ii = 0; ii < MemoryAccessWriteEventHandlers.Length; ii++)
                MemoryAccessWriteEventHandlers[ii] = null;
            for (int ii = 0; ii < MemoryAccessReadEventHandlers.Length; ii++)
                MemoryAccessReadEventHandlers[ii] = null;
        }

        public uint Start
        {
            get
            {
                //find first non null memory location
                int index = 0;
                while (index < mMemoryMap.Length)
                    if (mMemoryMap[index++] != null)
                        return (uint)(index - 1);
                return 0;
            }
        }
        public uint End
        {
            get
            {
                int index = mMemoryMap.Length;
                while (index > 0)
                    if (mMemoryMap[--index] != null)
                        return (uint)index;
                return 0;
            }
        }

        public IMemoryBlock SnapShot(uint addr, uint length)
        {
            if (addr + length > SIXTY_FOURK)
                length = SIXTY_FOURK - addr;

            var ret = new MemoryBlock(addr, null, 0, length, false);
            //for (uint ii = addr; (ii < addr + length) && ii < this.End; ii++)
            //    ret[ii] = this.getByte(ii, false);

            return ret;
        }
        public IList<uint> Diff(IMemoryBlock original)
        {
            var ret = new List<uint>();
            //for (uint ii = original.StartAddress; (ii < original.StartAddress + original.Length) && ii < End; ii++)
            //{
            //    if (original[ii] != this.getByte(ii, false))
            //        ret.Add(ii);
            //}
            return ret;
        }

        public void SetReadWriteOverride(uint addr, bool allow)
        {
            var memoryBlock = mMemoryMap[addr];
            if (memoryBlock == null)
            {
                OnInvalidMemoryAccess?.Invoke(addr);
                return;
            }
            memoryBlock.AllowWrite = allow;
        }

        public IMemoryBlock CreateMemoryBlock(uint baseAddress, byte[] bytes, int offset, uint size, bool allowWrite)
        {
            var block = new MemoryBlock(baseAddress, bytes, 0, size, true);
            for (uint addr = block.StartAddress; addr <= block.EndAddress; addr++)
                mMemoryMap[addr] = block;
            block.AllowWrite = allowWrite;
            return block;
        }

        private void MGlobalMemoryBlock_OnInvalidMemoryAccess(uint address)
        {
            OnInvalidMemoryAccess?.Invoke(address);
        }

        public void AddMemoryOverride(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate)
        {
            for (uint addr = startAddress; addr <= endAddress; addr++)
            {
                MemoryAccessReadEventHandlers[addr] = readDelegate;
                MemoryAccessWriteEventHandlers[addr] = writeDelegate;
            }
        }

        private memoryAccessReadEventHandler hitTestRead(uint addr)
        {
            return MemoryAccessReadEventHandlers[addr];
        }

        private memoryAccessWriteEventHandler hitTestWrite(uint addr)
        {
            return MemoryAccessWriteEventHandlers[addr];
        }

        public uint GetMemory(uint address, WordSize ws, bool sideEffect = true)
        {
            ushort a = (ushort)(address & 0xffff);
            address = a;

            switch (ws)
            {
                case WordSize.OneByte:
                    return getByte(address, sideEffect);
                case WordSize.TwoByte:
                    mBuff2[0] = getByte(address, sideEffect);
                    mBuff2[1] = getByte(address + 1, sideEffect);
                    if (Endian == Endian.Big)
                        swap2(mBuff2);
                    return BitConverter.ToUInt16(mBuff2, 0);
                case WordSize.FourByte:
                    {
                        mBuff4[3] = getByte(address++, sideEffect);
                        mBuff4[2] = getByte(address++, sideEffect);
                        mBuff4[1] = getByte(address++, sideEffect);
                        mBuff4[0] = getByte(address, sideEffect);
                        if (Endian == Endian.Big)
                            swap4(mBuff4);
                        return BitConverter.ToUInt32(mBuff4, 0);
                    }
                default:
                    return 0;
            }
        }

        private static void swap4(byte[] buff)
        {
            byte b = buff[3];
            buff[3] = buff[0];
            buff[0] = b;

            b = buff[2];
            buff[2] = buff[1];
            buff[1] = b;
        }

        private static void swap2(byte[] buff)
        {
            byte b = buff[0];
            buff[0] = buff[1];
            buff[1] = b;
        }

        public void SetMemory(uint address, WordSize ws, uint data, bool sideEffect = true)
        {
            switch (ws)
            {
                case WordSize.OneByte:
                    putByte(address + 0, (byte)data, sideEffect);
                    break;
                case WordSize.TwoByte:
                    {
                        var bytes = BitConverter.GetBytes((ushort)data);
                        if (Endian == Endian.Big)
                            swap2(bytes);
                        putByte(address + 0, bytes[0], sideEffect);
                        putByte(address + 1, bytes[1], sideEffect);
                    }
                    break;
                case WordSize.FourByte:
                    {
                        var bytes = BitConverter.GetBytes((uint)data);
                        if (Endian == Endian.Big)
                            swap2(bytes);
                        putByte(address + 0, bytes[0], sideEffect);
                        putByte(address + 1, bytes[1], sideEffect);
                        putByte(address + 2, bytes[2], sideEffect);
                        putByte(address + 3, bytes[3], sideEffect);
                    }
                    break;
            }
        }

        private void putByte(uint addr, byte data, bool sideEffects)
        {
            if (sideEffects)
            {
                var writeDelegate = hitTestWrite(addr);
                if (writeDelegate != null)
                {
                    writeDelegate(this, new MemoryAccessWriteEventArgs(addr, WordSize.OneByte, data));
                    return;
                }
            }
            var memoryBlock = mMemoryMap[addr];
            if (memoryBlock == null)
            {
                OnInvalidMemoryAccess?.Invoke(addr);
                return;
            }
            //if (!memoryBlock.AllowWrite)
                //throw new Exception();
            memoryBlock[addr] = data;
        }

        private byte getByte(uint addr, bool sideEffects)
        {
            if (sideEffects)
            {
                var readDelegate = hitTestRead(addr);
                if (readDelegate != null)
                    return (byte)readDelegate(this, new MemoryAccessReadEventArgs(addr, WordSize.OneByte));
            }
            var memoryBlock = mMemoryMap[addr];
            if (memoryBlock == null)
            {
                OnInvalidMemoryAccess?.Invoke(addr);
                return 0;
            }
            return memoryBlock[addr];
        }

        //public byte this[uint addr]
        //{
        //    get
        //    {
        //        var readDelegate = hitTestRead(addr);
        //        if (readDelegate != null)
        //        {
        //            var ret = readDelegate(this, new MemoryAccessReadEventArgs(addr, WordSize.OneByte));
        //            return Convert.ToByte(ret);
        //        }

        //        var memoryBlock = mMemoryMap[addr];
        //        if (memoryBlock == null)
        //            throw new MemoryException("Address not found in system memory", addr);

        //        return memoryBlock[addr];
        //    }
        //    set
        //    {
        //        var writeDelegate = hitTestWrite(addr);
        //        if (writeDelegate != null)
        //        {
        //            writeDelegate(this, new MemoryAccessWriteEventArgs(addr, WordSize.OneByte, value));
        //            return;
        //        }

        //        var memoryBlock = mMemoryMap[addr];
        //        if (memoryBlock == null)
        //            throw new MemoryException("Address not found in system memory", addr);

        //        memoryBlock[addr] = value;
        //    }
        //}// byte this[]
    }// class SystemMemory
}