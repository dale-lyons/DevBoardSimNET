using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boards.Controls
{
    public partial class DipSwitch : UserControl
    {
        private bool[] mSwitchPositions = new bool[8];
        private Brush mBlackBrush = new SolidBrush(Color.Black);
        private int mSwitchWidth;
        private string mTitle;

        public enum SwitchDirection
        {
            Vertical,
            Horizontal
        }

        public delegate void SwitchClicked(object sender, SwitchClickedArgs args);

        [CategoryAttribute("ABC"), DescriptionAttribute("Event to indicate a switch has changed value.")]
        public event SwitchClicked OnSwitchClicked;

        public DipSwitch()
        {
            InitializeComponent();
        }

        public void SetSwitches(byte b)
        {
            for (int ii = 0; ii < this.NumberSwitches; ii++)
            {
                mSwitchPositions[ii] = (b & 0x01) != 0;
                b >>= 1;
            }
        }

        public string Title
        {
            get { return mTitle; }
            set { mTitle = value; label1.Text = value; }
        }

        private int mNumberSwitches=8;
        [Description("Gets/Sets the number of toggle switches for the Control."), Category("ABC")]
        [Browsable(true)]
        public int NumberSwitches
        {
            get { return mNumberSwitches; }
            set { mNumberSwitches = value; mSwitchPositions = new bool[value]; panel1.Invalidate(); }
        }

        private Color mSwitchColor = Color.White;
        [CategoryAttribute("ABC"), DescriptionAttribute("Gets/Sets the Color of the Swtiches.")]
        public Color SwitchColor
        {
            get { return mSwitchColor; }
            set { mSwitchColor = value; panel1.Invalidate(); }
        }

        private Color mCaseColor = Color.Aqua;
        [CategoryAttribute("ABC"), DescriptionAttribute("Gets/Sets the Color switch Case.")]
        public Color CaseColor
        {
            get { return mCaseColor; }
            set { mCaseColor = value; this.BackColor = value; panel1.Invalidate(); }
        }

        private SwitchDirection mSwitchDirection = SwitchDirection.Horizontal;
        [CategoryAttribute("ABC"), DescriptionAttribute("Gets/Sets orientation of the Control.")]
        public SwitchDirection Direction
        {
            get { return mSwitchDirection; }
            set { mSwitchDirection = value; panel1.Invalidate(); }
        }


        public class SwitchClickedArgs : EventArgs
        {
            public int Id { get; set; }
            public bool NewValue { get; set; }
            public SwitchClickedArgs(int id, bool newValue)
            {
                Id = id;
                NewValue = newValue;
            }
        }//class SwitchClickedArgs

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (NumberSwitches <= 0)
                return;
            int width = panel1.Width;
            mSwitchWidth = (width / (NumberSwitches + 1)) - 1;

            drawLeftLabel(e.Graphics, 0, mSwitchWidth, panel1.Height - 1);
            for (int ii = 0; ii < NumberSwitches; ii++)
            {
                drawSwitch(e.Graphics, (ii + 1) * mSwitchWidth, mSwitchWidth, panel1.Height - 1, ii);
            }
        }
        private void drawLeftLabel(Graphics g, int xpos, int width, int height)
        {
            var sz = g.MeasureString("O", this.Font);
            int midXpos = xpos + (width - (int)sz.Width) / 2;
            int ypos = height / 4;
            g.DrawString("O", this.Font, mBlackBrush, midXpos, ypos);
            ypos += (int)sz.Height;
            g.DrawString("N", this.Font, mBlackBrush, midXpos, ypos);

            ypos += (int)sz.Height;
            Pen pen = new Pen(Color.Black, 8);
            pen.StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            g.DrawLine(pen, xpos + (width / 2), ypos, xpos + (width / 2), ypos + 40);
        }

        private void drawSwitch(Graphics g, int xpos, int width, int height, int id)
        {
            int midXpos = xpos + (width / 4);
            var rect = new Rectangle(xpos, 0, width, height);
            g.DrawRectangle(new Pen(Color.Black), rect);
            g.DrawString((id + 1).ToString(), this.Font, mBlackBrush, midXpos, 3);

            int ypos = mSwitchPositions[id] ? 20 : height - 20;
            rect = new Rectangle(xpos + 1, ypos, width - 2, 20);
            g.FillRectangle(new SolidBrush(SwitchColor), rect);
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            var pt = (e as MouseEventArgs).Location;
            bool newPosition = false;
            int id = (pt.X / mSwitchWidth) - 1;
            if (id < 0)
                return;

            if (pt.Y < (this.Height / 2))
                newPosition = true;
            else if (pt.Y > (this.Height / 2))
                newPosition = false;
            else
                return;

            if (mSwitchPositions[id] != newPosition)
            {
                mSwitchPositions[id] = newPosition;
                OnSwitchClicked?.Invoke(this, new SwitchClickedArgs(id, newPosition));
                panel1.Invalidate();
            }
        }

    }
}