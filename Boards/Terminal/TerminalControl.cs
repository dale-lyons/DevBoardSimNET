using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Terminal
{
    public partial class TerminalControl : UserControl
    {
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern int BitBlt(
IntPtr hdcDest,     // handle to destination DC (device context)
int nXDest,         // x-coord of destination upper-left corner
int nYDest,         // y-coord of destination upper-left corner
int nWidth,         // width of destination rectangle
int nHeight,        // height of destination rectangle
IntPtr hdcSrc,      // handle to source DC
int nXSrc,          // x-coordinate of source upper-left corner
int nYSrc,          // y-coordinate of source upper-left corner
System.Int32 dwRop  // raster operation code
);
        private static int SRCCOPY = 0xcc0020; // we want to copy an in memory image

        public delegate void KeypressDelegate(char ch);
        public event KeypressDelegate OnKeypress;
        public event KeypressDelegate OnSendByte;

        public int Rows { get; set; }
        public int Cols { get; set; }
        public int CharHeight { get; set; }
        public int CharWidth { get; set; }
        //private int TopLine { get; set; }

        private char[,] mScreen;
        private Brush mBackgroundBrush;
        private Brush mTextBrush;
        private Point mCursor;
        private FileStream mLogFileStream;
        //private Thread mCursorThread;

        public TerminalControl()
        {
            InitializeComponent();
            mBackgroundBrush = new SolidBrush(BackColor);
            mTextBrush = new SolidBrush(Color.White);
            this.KeyPress += terminalControl1_KeyPress;
        }

        private void terminalControl1_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeypress?.Invoke(e.KeyChar);
        }

        private void TerminalControl_Load(object sender, EventArgs e)
        {
            //mCursorThread = new Thread(cursorControl);
            //mCursorThread.Start();
        }

        private void TerminalControl_Paint(object sender, PaintEventArgs e)
        {
            if (mScreen == null)
                return;
            lock (mScreen)
            {
                Graphics g = e.Graphics;
                for (int row = 0; row < Rows; row++)
                    for (int col = 0; col < Cols; col++)
                        Draw(g, row, col);
            }
        }

        private void Draw(Graphics g, int row, int col)
        {
            int xpos = CharWidth * col;
            int ypos = CharHeight * row;
            g.FillRectangle(mBackgroundBrush, new Rectangle(xpos, ypos, CharWidth, CharHeight));
            g.DrawString(new string(mScreen[row, col], 1), Font, mTextBrush, xpos, ypos);
        }

        private void TerminalControl_Resize(object sender, EventArgs e)
        {
            using (Graphics g = this.CreateGraphics())
            {
                var size = (g.MeasureString("0123456789abcdef", Font)).ToSize();

                CharHeight = (int)size.Height + 1;
                CharWidth = (int)(size.Width / 16) + 1;
                Rows = this.ClientSize.Height / CharHeight;
                Cols = this.ClientSize.Width / CharWidth;
            }
            mScreen = new char[Rows, Cols];
            for (int row = 0; row < Rows; row++)
                for (int col = 0; col < Cols; col++)
                    mScreen[row, col] = ' ';

            vScrollBar1.Maximum = Rows;
            mCursor = new Point(0, 0);
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void SafeFeed(char ch)
        {
            if ((byte)ch >= 128)
                return;
            if (mLogFileStream != null)
                mLogFileStream.WriteByte((byte)ch);

            switch (ch)
            {
                case (char)0x1b:
                    return;
                case '\b':
                    lock (mScreen)
                        mScreen[mCursor.X, mCursor.Y] = ' ';
                    CursorLeftOne();
                    return;
                case '\r':
                    CursorXHome();
                    return;
                case '\n':
                    CursorDownOne();
                    CursorXHome();
                    vScrollBar1.Maximum = mCursor.Y;
                    return;
                case '\t':
                    {
                        int count = 5 - (mCursor.X % 5);
                        for (int ii = 0; ii < count; ii++)
                        {
                            SafeFeed(' ');
                        }
                    }
                    break;
                default:
                    if (ch < ' ')
                        break;
                    lock (mScreen)
                    {
                        using (Graphics g = this.CreateGraphics())
                        {
                            mScreen[mCursor.Y, mCursor.X] = ch;
                            Draw(g, mCursor.Y, mCursor.X);
                        }
                    }
                    CursorRightOne();
                    break;
            }//switch
        }

        public void Feed(char ch)
        {
            if (InvokeRequired)
                Invoke(new Action(() => { SafeFeed(ch); }));
            else
                SafeFeed(ch);
        }//Feed

        public void ClearFromCursor()
        {
            lock (mScreen)
            {
//                CursorDownOne();
//                CursorXHome();

                //blank out cursor to end of line
                for (int col = mCursor.X; col < Cols; col++)
                    mScreen[mCursor.Y, col] = ' ';
                SafeFeed('\n');
                this.Invalidate();

                //using (Graphics g = this.CreateGraphics())
                //{
                //    var xpos = mCursor.X * CharWidth;
                //    var ypos = (mCursor.Y - 1) * CharHeight;
                //    var r = new Rectangle(xpos, ypos, Cols * CharWidth, CharHeight);
                //    g.FillRectangle(mBackgroundBrush, r);
                //}
            }//lock
        }
        private void CursorLeftOne()
        {
            if (mCursor.X > 0)
                mCursor.X--;
        }
        private void CursorRightOne()
        {
            mCursor.X = Math.Min(mCursor.X + 1, Cols - 1);
        }
        private void CursorDownOne()
        {
            mCursor.Y++;
            if (mCursor.Y >= Rows)
            {
                scrollUpOne();
                mCursor.Y--;
            }
        }
        public void CursorXHome()
        {
            mCursor.X = 0;
        }

        /// <summary>
        /// Scroll the windows up onw line.
        /// </summary>
        private void scrollUpOne()
        {
            lock (mScreen)
            {
                //copy contents of screen buffer up one line
                for (int row = 0; row < Rows - 1; row++)
                {
                    for (int col = 0; col < Cols; col++)
                        mScreen[row, col] = mScreen[row + 1, col];
                }

                //blank out last line
                for (int col = 0; col < Cols; col++)
                    mScreen[Rows - 1, col] = ' ';
            }//lock

            Point p = this.Location;
            using (Graphics g = this.CreateGraphics())
            {
                Size s = new Size(CharWidth * Cols, CharHeight * (Rows - 1));
                IntPtr dc = g.GetHdc();
                BitBlt(dc, 0, 0, s.Width, s.Height, dc, 0, CharHeight, SRCCOPY); // copy this in memory graphic ( g )
                g.ReleaseHdc(dc);
                blankLine(g, (Rows - 1));
            }
        }

        private void blankLine(Graphics g, int row)
        {
            var r = new Rectangle(0, ((Rows - 1) * CharHeight), Cols * CharWidth, CharHeight);
            g.FillRectangle(mBackgroundBrush, r);
        }

        private void startLoggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mLogFileStream != null)
                return;
            string filename = Path.Combine(Application.CommonAppDataPath, "logFile.log");
            mLogFileStream = File.Create(filename);

            //contextMenuStrip1.Items[0].Enabled = false;
            //contextMenuStrip1.Items[1].Enabled = true;
        }

        private void stopLoggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mLogFileStream == null)
                return;

            mLogFileStream.Close();
            mLogFileStream = null;

            //contextMenuStrip1.Items[0].Enabled = true;
            //contextMenuStrip1.Items[1].Enabled = false;
        }


        private void loadHexFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            string[] lines = File.ReadAllLines(dialog.FileName);
            //ActiveBoard().LoadHex(bytes);
            foreach (var line in lines)
            {
                string line2 = line.Trim();
                if (string.IsNullOrEmpty(line2))
                    continue;

                foreach (var c in line2)
                {
                    OnSendByte?.Invoke(c);
                    Application.DoEvents();
                    Thread.Sleep(1);
                }
            }
        }
    }
}