using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace Processors
{
    public class CodeLine
    {
        private static int[] mTabStops = new int[] { 25, 33, 41, 49, 57, 65 };
        public static IProcessor Processor
        {
            get;
            set;
        }
        public bool Breakpoint
        {
            get
            {
                return Processor.Breakpoints.Contains(Address);
            }
        }

        public CodeLineTypes CodeLineType { get; set; }

        public string Text { get; set; }
//        public static ISystemMemory SystemMemory { get; set; }
        public ISystemMemory SystemMemory
        {
            get
            {
                return Processor.SystemMemory;
            }
        }
        public uint Address { get; set; }
        public int Length { get; set; }
        public int Line { get; set; }

        public CodeLine()
        {
        }

        public CodeLine(string text)
        {
            Text = text;
        }

        public CodeLine(string text, uint address, int length)
        {
            Text = text;
            Address = address;
            Length = length;
            CodeLineType = CodeLineTypes.Comment;

            if (Text == null)
            {
                Text = "invalid text";
            }
        }
        public byte[] Bytes
        {
            get
            {
                var ret = new byte[Length];
                for (int ii = 0; ii < Length; ii++)
                    ret[ii] = (byte)SystemMemory.GetMemory((uint)(Address + ii), WordSize.OneByte, false);
                return ret;
            }
        }
        public override string ToString()
        {
            return Format(SystemMemory, Text, Address, Length);
        }

        private ushort swap(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            byte b = bytes[0];
            bytes[0] = bytes[1];
            bytes[1] = b;
            return BitConverter.ToUInt16(bytes, 0);
        }
        public string Format(ISystemMemory sm, string text, uint address, int length)
        {
            var prnLine = new PRNLine(1, mTabStops);
            if (CodeLineType == CodeLineTypes.Comment || CodeLineType == CodeLineTypes.EQU)
            {
                prnLine.Add('\t');
                prnLine.Add(text);
                return prnLine.ToString();
            }
            //else if (CodeLineType == CodeLineTypes.EQU)
            //{
            //    return text.Trim();
            //}

            if (SystemMemory.WordSize == WordSize.FourByte)
            {
                prnLine.Add(string.Format("{0:X4}:", address));
                //ushort w1 = (ushort)SystemMemory.GetMemory(address, WordSize.TwoByte, false);
                //ushort w2 = (ushort)SystemMemory.GetMemory(address + 2, WordSize.TwoByte, false);
                for (int yy = 4; yy >0; yy--)
                    prnLine.Add(string.Format("{0:X2}", SystemMemory.GetMemory((uint)(address + yy - 1), WordSize.OneByte, false)));
            }
            else if (SystemMemory.WordSize == WordSize.TwoByte)
            {
                prnLine.Add(string.Format("{0:X4}:", address));
                for (int yy = 0; yy < Math.Min(length, 5); yy++)
                    prnLine.Add(string.Format("{0:X2}", SystemMemory.GetMemory((uint)(address + yy), WordSize.OneByte, false)));
            }
            else if (SystemMemory.WordSize == WordSize.OneByte)
            {
                prnLine.Add(string.Format("{0:X2}:", address));
                for (int yy = 0; yy < Math.Min(length, 5); yy++)
                    prnLine.Add(string.Format("{0:X2}", SystemMemory.GetMemory((uint)(address + yy), WordSize.OneByte, false)));
            }
            prnLine.Add('\t');
            prnLine.Add(text);
            return prnLine.ToString();
        }

        private static ushort swapB(ushort w)
        {
            var bytes = BitConverter.GetBytes(w);
            byte b = bytes[0];
            bytes[0] = bytes[1];
            bytes[1] = b;
            return BitConverter.ToUInt16(bytes, 0);
        }

        private class PRNLine
        {
            private StringBuilder mSb;
            private int mTabStop;

            //            private int mLine;
            private int[] mTabStops;
            public PRNLine(int line, int[] tabStops)
            {
                //mLine = line;
                mSb = new StringBuilder();
                //if (useLinenumbers)
                //{
                //    mSb.Append(string.Format("{0,6}", line));
                //    mSb.Append("   ");
                //}

                mTabStops = new int[256];
                Array.Copy(tabStops, mTabStops, tabStops.Length);
                for (int ii = tabStops.Length; ii < mTabStops.Length; ii++)
                    mTabStops[ii] = mTabStops[ii - 1] + 8;
            }

            public void Add(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return;

                foreach (var c in str)
                    Add(c);
            }
            public void Add(char ch)
            {
                if (ch == '\t')
                {
                    int num = mTabStops[mTabStop++] - mSb.Length;
                    while (num <= 0)
                        num = mTabStops[mTabStop++] - mSb.Length;

                    for (int ii = 0; ii < num; ii++)
                        mSb.Append(' ');
                }
                else
                {
                    mSb.Append(ch);
                    if (mSb.Length > mTabStops[mTabStop])
                        mTabStop++;
                }
            }

            public override string ToString()
            {
                return mSb.ToString();
            }
        }

        public enum CodeLineTypes
        {
            Code,
            Comment,
            EQU,
            Error
        }
    }
}