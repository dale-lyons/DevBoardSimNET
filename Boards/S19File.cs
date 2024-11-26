using Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boards
{
    public class S19File
    {
        public ushort StartAddress { get; private set; }
        public ushort EndAddress { get; private set; }
        private byte[] bytes;
        public byte[] Bytes
        {
            get
            {
                int len = EndAddress - StartAddress;
                byte[] ret = new byte[len];
                Array.Copy(bytes, StartAddress, ret, 0, len);
                return ret;
            }
        }
        public int Length { get { return EndAddress - StartAddress; } }

        //S123E000CE100A1F0001037EB6008693B710398600B710248600B71035CEB600DF98CEB731
        //S9030000FC
        public static void Create(string hexFilename, ISystemMemory sm, OrgSections sections)
        {
            using (var file = new StreamWriter(hexFilename))
            {
                var s19Line = new S19Line((ushort)sections.StartAddress, file);
                for (ushort ii = (ushort)sections.StartAddress; ii < sections.EndAddress; ii++)
                    s19Line.AddByte((byte)sm.GetMemory(ii, WordSize.OneByte, false));
                s19Line.Final();
                file.WriteLine("S9030000FC");
            }
        }

        public bool Load(string[] lines)
        {
            StartAddress = ushort.MaxValue;
            EndAddress = ushort.MinValue;

            bytes = new byte[64 * 1024];
            for (int ii = 0; ii < bytes.Length; ii++)
                bytes[ii] = 0xff;

            int index = 0;
            while (index < lines.Length)
            {
                string str = lines[index++];
                int count = int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null) - 3;
                ushort addr = ushort.Parse(str.Substring(4, 4), System.Globalization.NumberStyles.HexNumber, null);

                if (index == 1)
                    StartAddress = addr;

                for (int ii = 0; ii < count; ii++)
                {
                    byte data = byte.Parse(str.Substring((ii * 2) + 8, 2), System.Globalization.NumberStyles.HexNumber, null);
                    bytes[addr++] = data;
                }
                //if(index == lines.Length-1)
                if (count > 0)
                    EndAddress = Math.Max(EndAddress, addr);
            }
            return true;
        }

        public bool Load(string filename)
        {
            var lines = File.ReadAllLines(filename);
            return Load(lines);
        }

        public IList<Tuple<ushort, byte, byte>> CompareTo(string fileName)
        {
            var ret = new List<Tuple<ushort, byte, byte>>();

            var comBytes = File.ReadAllBytes(fileName);
            for (int ii = 0; ii <= this.Length; ii++)
            {
                if (comBytes[ii] != this.bytes[ii + 0x100])
                {
                    ret.Add(new Tuple<ushort, byte, byte>((ushort)ii, this.bytes[ii + 0x100], comBytes[ii]));
                }
            }
            return ret;
        }
        public IList<Tuple<ushort, byte, byte>> CompareToROM(string filename)
        {
            int addr = 0x3c00;
            var ret = new List<Tuple<ushort, byte, byte>>();
            var bytes = File.ReadAllBytes(filename);
            while (addr < 0x00008000L)
            {
                byte b1 = this.bytes[addr + 0x8000];
                byte b2 = bytes[addr];

                if (b1 != b2)
                {
                    ret.Add(new Tuple<ushort, byte, byte>((ushort)addr, this.bytes[addr + 0x8000], bytes[addr]));
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

            for (int ii = 0; ii < this.bytes.Length; ii++)
                if (this.bytes[ii] != hexFile.Bytes[ii])
                    ret.Add(new Tuple<ushort, byte, byte>((ushort)ii, this.bytes[ii], hexFile.Bytes[ii]));

            return ret;
        }

        public void SaveBinary(string filename)
        {
            var path = Path.ChangeExtension(filename, ".bin");
            byte[] bytes = new byte[EndAddress - StartAddress];
            Array.Copy(this.bytes, StartAddress, bytes, 0, EndAddress - StartAddress);
            File.WriteAllBytes(path, bytes);
        }

        private class S19Line
        {
            public ushort Address { get; private set; }

            private List<byte> mBuffer = new List<byte>();

            private StreamWriter mStreamWriter;
            public S19Line(ushort start, StreamWriter streamWriter)
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
    }//class S19Line
}