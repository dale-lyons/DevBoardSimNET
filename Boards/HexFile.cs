using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace Boards
{
    public class HexFile
    {
        public ushort StartAddress { get; private set; }
        public ushort EndAddress { get; private set; }
        public byte[] Bytes { get; private set; }
        public int Length {  get { return EndAddress - StartAddress; } }

        public static void Create(string hexFilename, ISystemMemory sm, OrgSections sections)
        {
            using (var file = new StreamWriter(hexFilename))
            {
                var hexLine = new HexLine((ushort)sections.StartAddress, file);
                for (ushort ii = (ushort)sections.StartAddress; ii < sections.EndAddress; ii++)
                    hexLine.AddByte((byte)sm.GetMemory(ii, WordSize.OneByte, false));

                hexLine.Final();
                file.WriteLine(":00000001FF");
            }
        }

        public static HexFile Load(string filename)
        {
            var ret = new HexFile();
            ret.StartAddress = ushort.MaxValue;

            if (!File.Exists(filename))
                return null;

            ret.Bytes = new byte[64 * 1024];
            for (int ii = 0; ii < ret.Bytes.Length; ii++)
                ret.Bytes[ii] = 0xff;

            var lines = File.ReadAllLines(filename);
            int index = 0;
            while (index < lines.Length)
            {
                string str = lines[index++];

                int count = int.Parse(str.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, null);
                if (count <= 0)
                    continue;

                ushort addr = ushort.Parse(str.Substring(3, 4), System.Globalization.NumberStyles.HexNumber, null);

                if (index == 1)
                    ret.StartAddress = addr;

                for (int ii = 0; ii < count; ii++)
                {
                    byte data = byte.Parse(str.Substring((ii * 2) + 9, 2), System.Globalization.NumberStyles.HexNumber, null);
                    ret.Bytes[addr++] = data;
                }
                ret.EndAddress = Math.Max(ret.EndAddress, addr);
            }
            return ret;
        }

        public IList<Tuple<ushort, byte, byte>> CompareTo(string fileName)
        {
            var ret = new List<Tuple<ushort, byte, byte>>();

            var comBytes = File.ReadAllBytes(fileName);
            for(int ii=0; ii<=this.Length; ii++)
            {
                if(comBytes[ii] !=this.Bytes[ii+0x100])
                {
                    ret.Add(new Tuple<ushort, byte, byte>((ushort)ii, this.Bytes[ii + 0x100], comBytes[ii]));
                }
            }
            return ret;
        }
        public IList<Tuple<ushort, byte, byte>> CompareToROM(string filename)
        {
            int addr = 0x3c00;
            var ret = new List<Tuple<ushort, byte, byte>>();
            var bytes = File.ReadAllBytes(filename);
            while(addr < 0x00008000L)
            {
                byte b1 = this.Bytes[addr+0x8000];
                byte b2 = bytes[addr];

                if(b1 != b2)
                {
                    ret.Add(new Tuple<ushort, byte, byte>((ushort)addr, this.Bytes[addr + 0x8000], Bytes[addr]));
                }
                addr++;
            }

            return ret;
        }

        public IList<Tuple<ushort, byte, byte>> CompareTo(HexFile hexFile)
        {
            var ret = new List<Tuple<ushort, byte, byte>>();

            if (this.StartAddress != hexFile.StartAddress)
                return null;

            for (int ii = 0; ii < this.Bytes.Length; ii++)
                if (this.Bytes[ii] != hexFile.Bytes[ii])
                    ret.Add(new Tuple<ushort, byte, byte>((ushort)ii, this.Bytes[ii], hexFile.Bytes[ii]));

            return ret;
        }

        ////public void Save(string filename)
        ////{
        ////    var path = Path.ChangeExtension(filename, ".hex");
        ////    List<string> lines = new List<string>();
        ////    var outPutBuffer = new List<byte>();

        ////    ushort mPtr = StartAddress;
        ////    ushort addr = mPtr;
        ////    while (mPtr < EndAddress)
        ////    {
        ////        if (outPutBuffer.Count >= 16)
        ////        {
        ////            dumpBuffer(outPutBuffer, lines, addr);
        ////            addr = mPtr;
        ////        }
        ////        outPutBuffer.Add(Bytes[mPtr++]);
        ////    }
        ////    File.WriteAllLines(path, lines.ToArray());
        ////}//Create
        private class HexLine
        {
            public ushort Address { get; private set; }

            private List<byte> mBuffer = new List<byte>();

            private StreamWriter mStreamWriter;
            public HexLine(ushort start, StreamWriter streamWriter)
            {
                Address = start;
                mStreamWriter = streamWriter;
            }

            public void Final()
            {
                dumpBuffer();
            }

            public void NewOrg(ushort address)
            {
                dumpBuffer();
                Address = address;
            }

            public void AddByte(byte b)
            {
                if (mBuffer.Count >= 16)
                    dumpBuffer();
                mBuffer.Add(b);
            }

            private void dumpBuffer()
            {
                if (mBuffer.Count > 0)
                {
                    var sb = new StringBuilder(":");
                    ushort checksum = 0;
                    checksum += outputByte(sb, (byte)mBuffer.Count);
                    checksum += outputWord(sb, Address);
                    checksum += outputByte(sb, (byte)0);
                    for (int ii = 0; ii < mBuffer.Count; ii++)
                        checksum += outputByte(sb, mBuffer[ii]);

                    byte cs = (byte)(checksum & 0x00ff);
                    cs = (byte)-cs;
                    outputByte(sb, cs);
                    mStreamWriter.WriteLine(sb.ToString());

                    Address += (ushort)mBuffer.Count;
                }
                mBuffer.Clear();
            }

            private ushort outputWord(StringBuilder sb, int data)
            {
                ushort checksum = 0;
                byte b = (byte)((data & 0xff00) >> 8);
                checksum += b;
                sb.Append(BitConverter.ToString(new byte[] { b }));
                b = (byte)(data & 0x00ff);
                checksum += b;
                sb.Append(BitConverter.ToString(new byte[] { b }));
                return checksum;
            }

            private ushort outputByte(StringBuilder sb, byte data)
            {
                sb.Append(BitConverter.ToString(new byte[] { data }));
                return (ushort)data;
            }
        }
    }//class HexFile
}