using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Processors;
using WeifenLuo.WinFormsUI.Docking;
using GUI.Arithmetic;

namespace GUI.Memory
{
    public partial class MemoryView : ToolWindows, IView
    {
        private static string bs2 = "XX";
        private static string bs4 = "XXXX";
        private static string bs8 = "XXXXXXXX";

        private IList<uint> mChangedBytesList;
        private IMemoryBlock mSnapShot;
        private uint mAddress;
        private Point mStartPoint;
        private int mNumLines;
        private Brush mBackgroundBrush;
        private Brush mChangedBrush;
        private Brush mTextBrush;
        private Font mFont;
        private SizeF mCharSize;
        private int mCellsPerLine;
        private QueryViewFunc mQueryViewFunc;
        public IProcessor Processor { get; private set; }
        private WordSize mMemorySizeType = WordSize.OneByte;
        public MemoryConfig Config { get; set; }
        public MemoryView(QueryViewFunc queryViewFunc, IProcessor processor, MemoryConfig config)
        {
            mQueryViewFunc = queryViewFunc;
            Processor = processor;
            Config = config;
            InitializeComponent();
        }

        public void RefreshView()
        {
            panel2.Invalidate();
        }

        public void ResetView()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            mChangedBytesList = null;
            mSnapShot = Processor.SystemMemory.SnapShot((ushort)mAddress, (uint)(mNumLines * mCellsPerLine));
        }

        public void Stop()
        {
            if (mSnapShot == null)
                return;
            mChangedBytesList = Processor.SystemMemory.Diff(mSnapShot);
            panel2.Invalidate();
        }

        private bool tryParseAddress(string text, out uint addr)
        {
            addr = 0;
            if (string.IsNullOrEmpty(text))
                return false;

            addr = AddressExpression.eval(Processor, text);
            Config.LastAddr = text;
            return true;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            uint addr;
            if (tryParseAddress(tbAddress.Text, out addr))
            {
                mAddress = addr;
            }
            if (mMemorySizeType == WordSize.OneByte)
                dumpMemoryByte(mAddress, e);
            else if (mMemorySizeType == WordSize.TwoByte)
                dumpMemoryWord((mAddress & 0xfffe), e);
            else
                dumpMemoryDWord((mAddress & 0xfffe), e);
        }
        private void dumpMemoryByte(uint addr, PaintEventArgs e)
        {
            addr &= 0xfff0;
            int ypos = mStartPoint.Y;
            var sbCells = new StringBuilder();
            var sbChars = new StringBuilder();
            var g = e.Graphics;

            uint memEnd = Processor.SystemMemory.End;
            for (int ii = 0; ii < mNumLines && addr < memEnd; ii++)
            {
                sbCells.Clear(); sbChars.Clear();
                sbCells.Append(addr.ToString("X4"));

                for (int cell = 0; (cell < mCellsPerLine) && addr <= memEnd; cell++, addr++)
                {
                    bool valid = Processor.SystemMemory.ValidByteAddress(addr);
                    if (valid)
                    {
                        byte b = (byte)Processor.SystemMemory.GetMemory(addr, WordSize.OneByte, false);
                        sbCells.Append(string.Format(" {0}", b.ToString("X2")));
                        char c = '.';
                        char ch = (char)b;

                        bool dig = char.IsLetterOrDigit(ch);
                        bool pun = char.IsPunctuation(ch);
                        bool wsp = char.IsWhiteSpace(ch);

                        if (char.IsLetterOrDigit(ch) || char.IsPunctuation(ch) || ch == ' ')
                            c = ch;
                        sbChars.Append(c);
                    }
                    else
                    {
                        sbCells.Append(string.Format(" {0}", bs2));
                    }
                }

                string str = string.Format("{0} {1}", sbCells.ToString(), sbChars.ToString());
                g.DrawString(str, mFont, mTextBrush, 0, ypos);
                ypos += (int)mCharSize.Height;
            } //for ii

            if (mChangedBytesList == null)
                return;

            foreach (var changedAddress in mChangedBytesList)
            {
                string dataChars = Processor.SystemMemory.GetMemory(changedAddress, WordSize.TwoByte, false).ToString("X2");
                int diffAddress = (int)(changedAddress - mAddress);
                int line = diffAddress / mCellsPerLine;
                int cellNo = diffAddress % mCellsPerLine;
                int xpos = mStartPoint.X + (int)(((float)cellNo * (mCharSize.Width * 3.0)));
                xpos = xpos - (cellNo * 1);
                ypos = mStartPoint.Y + (line * (int)mCharSize.Height);

                var rect = new Rectangle(xpos - 1, ypos, (int)(mCharSize.Width * 2) + 5, (int)mCharSize.Height);
                g.FillRectangle(mBackgroundBrush, rect);

                g.DrawString(dataChars, mFont, mChangedBrush, xpos, ypos);
            }
        }

        private void dumpMemoryDWord(uint addr, PaintEventArgs e)
        {
            int ypos = mStartPoint.Y;
            var sbCells = new StringBuilder();
            var sbChars = new StringBuilder();
            var g = e.Graphics;
            for (int ii = 0; ii < mNumLines; ii++)
            {
                sbCells.Clear(); sbChars.Clear();
                sbCells.Append(addr.ToString("X8"));

                for (int cell = 0; cell < (mCellsPerLine / 4); cell++, addr += 4)
                {
                    if (addr < Processor.SystemMemory.End)
                    {
                        if (Processor.SystemMemory.ValidDWordAddress(addr))
                        {
                            uint w = Processor.SystemMemory.GetMemory(addr, WordSize.FourByte, false);
                            sbCells.Append(string.Format(" {0}", w.ToString("X8")));
                        }
                        else
                            sbCells.Append(string.Format(" {0}", bs8));
                    }
                }

                string str = string.Format("{0} {1}", sbCells.ToString(), sbChars.ToString());
                g.DrawString(str, mFont, mTextBrush, 0, ypos);
                ypos += (int)mCharSize.Height;
            } //for ii

            if (mChangedBytesList == null)
                return;

            foreach (var changedAddress in mChangedBytesList)
            {
                string dataChars = Processor.SystemMemory.GetMemory(changedAddress, WordSize.OneByte, false).ToString("X2");
                int diffAddress = (int)(changedAddress - mAddress);
                int line = diffAddress / mCellsPerLine;
                int cellNo = diffAddress % mCellsPerLine;
                int xpos = mStartPoint.X + (int)(((float)cellNo * (mCharSize.Width * 3.0)));
                xpos = xpos - (cellNo * 1);
                ypos = mStartPoint.Y + (line * (int)mCharSize.Height);

                var rect = new Rectangle(xpos - 1, ypos, (int)(mCharSize.Width * 2) + 5, (int)mCharSize.Height);
                g.FillRectangle(mBackgroundBrush, rect);

                g.DrawString(dataChars, mFont, mChangedBrush, xpos, ypos);
            }
        }

        private void dumpMemoryWord(uint addr, PaintEventArgs e)
        {
            int ypos = mStartPoint.Y;
            var sbCells = new StringBuilder();
            var sbChars = new StringBuilder();
            var g = e.Graphics;
            for (int ii = 0; ii < mNumLines; ii++)
            {
                sbCells.Clear(); sbChars.Clear();
                sbCells.Append(addr.ToString("X4"));

                for (int cell = 0; cell < (mCellsPerLine / 2); cell++, addr += 2)
                {
                    if (addr < Processor.SystemMemory.End)
                    {
                        if (Processor.SystemMemory.ValidWordAddress(addr))
                        {
                            ushort w = (ushort)Processor.SystemMemory.GetMemory(addr, WordSize.TwoByte, false);
                            sbCells.Append(string.Format(" {0}", w.ToString("X4")));
                        }
                        else
                            sbCells.Append(string.Format(" {0}", bs4));
                    }
                }

                string str = string.Format("{0} {1}", sbCells.ToString(), sbChars.ToString());
                g.DrawString(str, mFont, mTextBrush, 0, ypos);
                ypos += (int)mCharSize.Height;
            } //for ii

            if (mChangedBytesList == null)
                return;

            foreach (var changedAddress in mChangedBytesList)
            {
                string dataChars = Processor.SystemMemory.GetMemory(addr, WordSize.OneByte, true).ToString("X2");
                int diffAddress = (int)(changedAddress - mAddress);
                int line = diffAddress / mCellsPerLine;
                int cellNo = diffAddress % mCellsPerLine;
                int xpos = mStartPoint.X + (int)(((float)cellNo * (mCharSize.Width * 3.0)));
                xpos = xpos - (cellNo * 1);
                ypos = mStartPoint.Y + (line * (int)mCharSize.Height);

                var rect = new Rectangle(xpos - 1, ypos, (int)(mCharSize.Width * 2) + 5, (int)mCharSize.Height);
                g.FillRectangle(mBackgroundBrush, rect);

                g.DrawString(dataChars, mFont, mChangedBrush, xpos, ypos);
            }
        }

        private void panel2_Resize(object sender, EventArgs e)
        {
            var clientSize = this.ClientSize;
            //            int panelHeight = clientSize.Height - groupBox1.Height;
            //            panel1.Height = panelHeight;

            var gbLoc = new Point();
            gbLoc.X = clientSize.Width - groupBox1.Width;
            gbLoc.Y = 3;
            groupBox1.Location = gbLoc;

            mFont = panel2.Font;
            var g = panel2.CreateGraphics();
            SizeF fontSize = g.MeasureString("ABCDEFGHIJKLMNOP", mFont);

            mCharSize = new SizeF(fontSize.Width / 16, fontSize.Height);
            mStartPoint = new Point((int)mCharSize.Width * 5, 0);
            mCellsPerLine = 32;
            mNumLines = panel2.Height / (int)mCharSize.Height;
            panel2.Invalidate();
        }

        private void MemoryView_Load(object sender, EventArgs e)
        {
            tbAddress.Text = Config.LastAddr;
            mFont = panel1.Font;
            var g = panel1.CreateGraphics();
            SizeF fontSize = g.MeasureString("ABCDEFGHIJKLMNOP", mFont);

            mCharSize = new SizeF(fontSize.Width / 16, fontSize.Height);
            mStartPoint = new Point((int)mCharSize.Width * 5, 0);
            mCellsPerLine = 32;
            mNumLines = panel1.Height / (int)mCharSize.Height;

            mBackgroundBrush = new SolidBrush(panel1.BackColor);
            mChangedBrush = new SolidBrush(Color.Red);
            mTextBrush = new SolidBrush(Color.Black);

            vScrollBar1.Minimum = (int)Processor.SystemMemory.Start;
            vScrollBar1.Maximum = (int)Processor.SystemMemory.End;
        }

        //Column numbers
        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            var clientSize = panel3.ClientSize;
            int panelHeight = clientSize.Height;
            int ypos = (int)((panelHeight - mCharSize.Height) / 2);

            if (mMemorySizeType == WordSize.OneByte)
                printByteColumnHeaders(e.Graphics);
            else if (mMemorySizeType == WordSize.TwoByte)
                printWordColumnHeaders(e.Graphics);
            else
                printDWordColumnHeaders(e.Graphics);
        }

        private void printByteColumnHeaders(Graphics g)
        {
            var clientSize = panel3.ClientSize;
            int panelHeight = clientSize.Height;
            int ypos = (int)((panelHeight - mCharSize.Height) / 2);

            var sbChars = new StringBuilder();
            int xpos = (int)((mCharSize.Width * 3.0)) + 10;
            int offset = 0;
            for (int cell = 0; (cell < mCellsPerLine); cell++, offset++)
                sbChars.Append(" " + offset.ToString("X2"));
            string str = sbChars.ToString();
            g.DrawString(str, panel3.Font, mTextBrush, xpos, ypos);
        }
        private void printWordColumnHeaders(Graphics g)
        {
            var clientSize = panel3.ClientSize;
            int panelHeight = clientSize.Height;
            int ypos = (int)((panelHeight - mCharSize.Height) / 2);

            var sbChars = new StringBuilder();
            int xpos = (int)(mCharSize.Width * 3.0);
            int offset = 0;
            for (int cell = 0; (cell < mCellsPerLine); cell += 2, offset += 2)
                sbChars.Append("   " + offset.ToString("X2"));
            string str = sbChars.ToString();
            g.DrawString(str, panel3.Font, mTextBrush, xpos, ypos);
        }
        private void printDWordColumnHeaders(Graphics g)
        {
            var clientSize = panel3.ClientSize;
            int panelHeight = clientSize.Height;
            int ypos = (int)((panelHeight - mCharSize.Height) / 2);

            var sbChars = new StringBuilder();
            int xpos = (int)((mCharSize.Width * 3.0) + 22);
            int offset = 0;
            for (int cell = 0; (cell < mCellsPerLine); cell += 4, offset += 4)
                sbChars.Append("       " + offset.ToString("X2"));
            string str = sbChars.ToString();
            g.DrawString(str, panel3.Font, mTextBrush, xpos, ypos);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mMemorySizeType = WordSize.OneByte;
            panel2.Invalidate();
            panel3.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mMemorySizeType = WordSize.TwoByte;
            panel2.Invalidate();
            panel3.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            mMemorySizeType = WordSize.FourByte;
            panel2.Invalidate();
            panel3.Invalidate();
        }

        private void tbAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0x0d)
            {
                e.Handled = true;
                panel1.Invalidate();
                panel2.Invalidate();
            }
        }
    }
}