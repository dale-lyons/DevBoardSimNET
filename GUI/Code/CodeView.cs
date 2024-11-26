using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Processors;
using Parsers;

namespace GUI.Code
{
    public partial class CodeView : ToolWindows, IView
    {
        private IList<CodeLine> mListLines = new List<CodeLine>();
        private ProgramImage mProgramImage;
        private uint mLinesPerPage;
        private int mLineHeight;

        private uint ViewTopLine { get; set; }

        public IProcessor Processor { get; private set; }

        public CodeView(QueryViewFunc qview, IProcessor processor)
        {
            Processor = processor;
            InitializeComponent();
            mProgramImage = new ProgramImage(Processor);
        }

        private void CodeView_Load(object sender, EventArgs e)
        {
            panel1.MouseWheel += Panel1_MouseWheel;
            vScrollBar1.Minimum = 0;
            vScrollBar1.Maximum = 64 * 1024;
            panel1.Resize += Panel1_Resize;
        }

        private void Panel1_Resize(object sender, EventArgs e)
        {
            mLineHeight = (int)panel1.Font.GetHeight();
            mLinesPerPage = (uint)(ClientSize.Height / mLineHeight);
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            long pos = ViewTopLine;
            pos = (pos - (e.Delta / 10));
            pos = Math.Max(pos, 0);
            ViewTopLine = (uint)pos;
            mListLines = mProgramImage.GetLinesByLineNo((int)ViewTopLine, Processor.SystemMemory, mLinesPerPage);
            panel1.Invalidate();
        }

        public void RefreshView()
        {
            if (isAddressVisible(Processor.Registers.PC))
            {
                panel1.Invalidate();
                return;
            }

            if (mProgramImage.ContainsKey(Processor.Registers.PC))
            {
                ViewTopLine = mProgramImage.LineAtAddress(Processor.Registers.PC);
                mListLines = mProgramImage.GetLinesByLineNo((int)ViewTopLine, Processor.SystemMemory, mLinesPerPage);
            }
            else
            {
                ViewTopLine = 0;
                mListLines = mProgramImage.GetLinesByAddr(Processor.Registers.PC, Processor.SystemMemory, mLinesPerPage);
            }
            panel1.Invalidate();
        }

        public void Start()
        {
        }

        public void Stop()
        {
            RefreshView();
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            for (int line = 0; line < mLinesPerPage && line < mListLines.Count; line++)
                drawLine(e.Graphics, panel, line, mListLines[line]);
        }

        private void drawLine(Graphics g, Panel panel, int line, CodeLine cline)
        {
            if (line >= mLinesPerPage)
                return;
            var size = panel.ClientSize;
            int ypos = line * mLineHeight; line++;
            var rect = new Rectangle(mLineHeight + 5, ypos, size.Width, size.Height);
            Color background = Color.Gray;
            if (cline.CodeLineType == CodeLine.CodeLineTypes.Error)
                background = Color.Red;
            else if (cline.Address == Processor.Registers.PC && cline.CodeLineType == CodeLine.CodeLineTypes.Code)
                background = Color.LightBlue;
            g.FillRectangle(new SolidBrush(background), rect);

            if (cline.CodeLineType != CodeLine.CodeLineTypes.Comment)
            {
                if (cline.Breakpoint)
                    g.FillEllipse(Brushes.Red, new Rectangle(0, ypos, mLineHeight, mLineHeight));
            }
            g.DrawString(cline.ToString(), panel.Font, Brushes.Black, rect, StringFormat.GenericDefault);
        }

        public void SetPC(uint addr)
        {
            Processor.Registers.PC = addr;
            RefreshView();
        }

        private bool isAddressVisible(uint addr)
        {
            foreach (var line in mListLines)
            {
                if (line.CodeLineType == CodeLine.CodeLineTypes.Code && line.Address == addr)
                    return true;
            }
            return false;
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int line = 0;
            switch (e.Type)
            {
                case ScrollEventType.EndScroll:
                    line = e.NewValue;
                    break;
                default:
                    return;
            }
            ViewTopLine = (uint)line;
            mListLines = mProgramImage.GetLinesByLineNo((int)ViewTopLine, Processor.SystemMemory, mLinesPerPage);
            panel1.Invalidate();
        }

        public void AddCompiledCode(IParser parser, bool loadBinary)
        {
            mProgramImage.AddCompiledCode(parser, loadBinary);
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            var location = (e as MouseEventArgs).Location;
            //int pos = (int)((location.Y / mLineHeight) + ViewTopLine);
            int pos = (int)(location.Y / mLineHeight);
            var line = mListLines[pos];

            if ((line.CodeLineType != CodeLine.CodeLineTypes.Code))
                return;

            if (line.Breakpoint)
                Processor.Breakpoints.Remove(line.Address);
            else
                Processor.Breakpoints.Add(line.Address, false);
            panel1.Invalidate();
        }

        public void ResetView()
        {
            panel1.Invalidate();
        }

        //public void AddBinaryCode(byte[] bytes, uint start, uint end)
        //{
        //    for (uint ii = start; ii < end; ii++)
        //        Processor.SystemMemory.SetMemory(ii, WordSize.OneByte, bytes[ii], false);
        //    CodeLine.Processor = Processor;
        //    Processor.Reset();
        //    RefreshView();
        //}

    }
}