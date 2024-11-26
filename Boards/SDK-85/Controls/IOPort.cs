using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDK_85.Controls
{
    public partial class IOPort : UserControl
    {
        private TextBox[] bitsT = new TextBox[8];

        public event GetIOPortDelegate GetIOPort;

        public IOPort()
        {
            InitializeComponent();
            for (int ii = 0; ii < 8; ii++)
            {
                bitsT[ii] = new TextBox();
                bitsT[ii].Text = ii.ToString();
                this.Controls.Add(bitsT[ii]);
            }
        }

        public string IOPortTitle
        {
            get
            {
                return Title.Text;
            }
            set
            {
                Title.Text = value;
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            byte val = 0;
            byte conf = 0;
            GetIOPort?.Invoke(out val, out conf);

            var blackBrush = new SolidBrush(Color.Black);
            var blackpen = new Pen(blackBrush, 2);
            var redBrush = new SolidBrush(Color.Red);
            var redpen = new Pen(redBrush);

            var size = pictureBox1.ClientSize;
            int bitDist = size.Width / 8;
            byte mask = 0x80;
            for(int ii=1; ii <= 8; ii++, mask >>= 1)
            {
                e.Graphics.DrawLine(blackpen, ii * bitDist, 0, ii * bitDist, size.Height);
                Point pt = new Point((ii - 1) * bitDist, 0);
                Rectangle rect = new Rectangle(pt, new Size(bitDist, size.Height));
                rect.Inflate(new Size(-10, -10));

                if ((conf & mask) == 0)
                {//NOT configured or is INPUT, Draw "X".
                    e.Graphics.DrawLine(blackpen, rect.Left, rect.Bottom, rect.Right, rect.Top);
                    e.Graphics.DrawLine(blackpen, rect.Left, rect.Top, rect.Right, rect.Bottom);
                }
                else
                {
                    bool On = ((val & mask) != 0);
                    e.Graphics.FillEllipse(On ? redBrush : blackBrush, rect);
                }
            }
        }

        private void IOPort_Resize(object sender, EventArgs e)
        {
            var size = this.ClientSize;
            pictureBox1.Location = new Point(0, size.Height / 3);

            int xpos = (size.Width - Title.Width) / 2;
            int ypos = ((size.Height / 3) - Title.Height) / 2;
            Title.Location = new Point(xpos, ypos);

            int bitDist = size.Width / 8;
            //Port bit numbers
            for (int ii = 7; ii >= 0; ii--)
            {
                xpos = ((7 - ii) * bitDist) + (bitDist / 2);
                ypos = ((size.Height * 4) / 5);
                bitsT[ii].Location = new Point(xpos, ypos);
                bitsT[ii].Width = 12;
            }
        }
    }
}