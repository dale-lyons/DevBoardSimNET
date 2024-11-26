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
    public partial class EightSegmentDisplay : UserControl
    {
        [Flags]
        private enum SegFlags : byte
        {
            SEG_A = 0x80,
            SEG_B = 0x40,
            SEG_C = 0x20,
            SEG_P = 0x10,
            SEG_D = 0x08,
            SEG_E = 0x04,
            SEG_F = 0x02,
            SEG_G = 0x01
        }
        private byte mCode;
        private Segment[] mSegments;

        public EightSegmentDisplay()
        {
            InitializeComponent();
            mSegments = new Segment[8];
            //mSegments[0] = new Segment(false, 9, 5);//A
            //mSegments[1] = new Segment(true, 33, 29);//B
            //mSegments[2] = new Segment(true, 33, 54);//C
            //mSegments[3] = null;//P
            //mSegments[4] = new Segment(false, 9, 55);//D
            //mSegments[5] = new Segment(true, 8, 54);//E
            //mSegments[6] = new Segment(false, 9, 30);//F
            //mSegments[7] = new Segment(true, 8, 29);//G

            mSegments[7] = new Segment(true, 8, 54);//E
            mSegments[1] = new Segment(true, 33, 54);//C
            mSegments[2] = new Segment(true, 33, 29);//B
            mSegments[3] = new Segment(false, 9, 5);//A
            mSegments[4] = null;//P
            mSegments[5] = new Segment(false, 9, 30);//F
            mSegments[6] = new Segment(true, 8, 29);//G
            mSegments[0] = new Segment(false, 9, 55);//D
        }

        /// Get/Set the Eight segment display to the given pattern. Each of the 8 bits maps to a specific
        /// segment on the display. Where a bit is a "1" the segment is illuminated.
        /// </summary>
        public byte Code
        {
            get { return mCode; }
            set
            {
                //only set the pattern if it has changed. We dont want to overload the system
                //with paint events unnessecarily.
                if (mCode != value)
                {
                    mCode = value;
                    pictureBox1.Invalidate();
                }
            }//set
        }//property Code

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int mask = 0x80;
            for (int ii = 0; ii < mSegments.Length; ii++)
            {
                if ((mask & mCode) != 0)
                {
                    if (mSegments[ii] != null)
                    {
                        g.FillPolygon(new SolidBrush(Color.LightGreen), mSegments[ii].Points);
                    }
                }
                mask >>= 1;
            }//for ii
        }
    }

    public class Segment
    {
        private int _xpos;
        private int _ypos;
        private bool _vertical;
        private Point[] _points;
        public Segment(bool vertical, int xpos, int ypos)
        {
            _xpos = xpos;
            _ypos = ypos;
            _vertical = vertical;

            _points = new Point[8];
            if (_vertical)
            {
                int xoffset = _xpos - 33;
                int yoffset = _ypos - 54;
                _points[0] = new Point(xoffset + 33, yoffset + 54);
                _points[1] = new Point(xoffset + 35, yoffset + 52);
                _points[2] = new Point(xoffset + 35, yoffset + 34);
                _points[3] = new Point(xoffset + 33, yoffset + 30);
                _points[4] = new Point(xoffset + 32, yoffset + 30);
                _points[5] = new Point(xoffset + 30, yoffset + 34);
                _points[6] = new Point(xoffset + 30, yoffset + 52);
                _points[7] = new Point(xoffset + 32, yoffset + 54);
            }
            else
            {
                int xoffset = _xpos - 9;
                int yoffset = _ypos - 55;
                _points[0] = new Point(xoffset + 9, yoffset + 55);
                _points[1] = new Point(xoffset + 11, yoffset + 53);
                _points[2] = new Point(xoffset + 29, yoffset + 53);
                _points[3] = new Point(xoffset + 31, yoffset + 55);
                _points[4] = new Point(xoffset + 31, yoffset + 56);
                _points[5] = new Point(xoffset + 29, yoffset + 57);
                _points[6] = new Point(xoffset + 11, yoffset + 57);
                _points[7] = new Point(xoffset + 9, yoffset + 56);
            }
        }
        public Point[] Points { get { return _points; } }
    }

}
