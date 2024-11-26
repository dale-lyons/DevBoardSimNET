using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MicroBeast.Controls
{
    public partial class Character : UserControl
    {
        private static SolidBrush charBrush = new SolidBrush(Color.LightSlateGray);

        private static int H = 200;
        private static int W = 100;
        private static int w = 20;
        private static int W2 = W/2;
        private static int w2 = w/2;
        private static int H2 = H/2;

        private static Point[] segA = new Point[] { new Point(0, 0), new Point(w, w), new Point(W-w, w), new Point(W, 0) }; //0x0001
        private static Point[] segB = new Point[] { new Point(W, 0), new Point(W-w, w), new Point(W-w, W-w), new Point(W, W) }; //0x0002
        private static Point[] segC = new Point[] { new Point(W, W), new Point(W-w, W+w), new Point(W-w, H-w), new Point(W, H) }; //0x0004
        private static Point[] segD = new Point[] { new Point(0, H), new Point(w, H-w), new Point(W-w, H-w), new Point(W, H) }; //0x0008
        private static Point[] segE = new Point[] { new Point(0, W), new Point(w, W+w), new Point(w, H-w), new Point(0, H) }; //0x0010
        private static Point[] segF = new Point[] { new Point(0, 0), new Point(w, w), new Point(w, W-w), new Point(0, W) }; //0x0020
        private static Point[] segG1 = new Point[] { new Point(0, W), new Point(w, W-w2), new Point(W2-w, W-w2), new Point(W2, W), new Point(W2-w, W+w2), new Point(w, W+w2) };  //0x0040
        private static Point[] segG2 = new Point[] { new Point(W2, W), new Point(W2+w, W-w2), new Point(W-w, W-w2), new Point(100, 100), new Point(90, 105), new Point(W2+w, W+w2) };//0x0080
        //private static Point[] segH = new Point[] { new Point(15, 15), new Point(25, 15), new Point(40, 70), new Point(40, 80), new Point(30, 80), new Point(15, 25) };  //0x0100
        //private static Point[] segI = new Point[] { new Point(0, 0) };
        private static Point[] segJ = new Point[] { new Point(W - w - w2, w + 1), new Point(W - w - 1, w + 1), new Point(W - w - 1, w + w2), new Point(W2 + 2 * w2, H2 - w2 - 1), new Point(W2 + w2 + 1, H2 - w2 - 1), new Point(W2 + w2 + 1, H2 - 2 * w2) };//0x0200
        private static Point[] segK = new Point[] { new Point(W2 + w + 1, H2 + w + 1), new Point(W2 + 2 * w, H2 + w + 1), new Point(W - w2 - 1, H - 2 * w), new Point(W - w2 - 1, H - w), new Point(W - 2 * w, H - w), new Point(W2 + w + 1, H + w + 1) };  //0x0400
        //private static Point[] segL = new Point[] { new Point(55, 115), new Point(65, 115), new Point(80, 170), new Point(80, 180), new Point(70, 180), new Point(55, 125) };  //0x0800
        //private static Point[] segM = new Point[] { new Point(0, 0) };
        private static Point[] segDp = new Point[] { new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0) }; //0x4000

        private static Point[] segI = new Point[] {
                                      new Point(W2 - w2, w + 1),
                                      new Point(W2 + w2, w + 1),
                                      new Point(W2 + w2 / H2, - w2),
                                      new Point(W2, H2),
                                      new Point(W2 - w2, H2 - w2)};
        private static Point[] segM = new Point[] { 
                                      new Point(w+1, H-w-w2-1),
                                      new Point(w+1, H-w-1),
                                      new Point(w+w2, H-w-1),
                                      new Point(W2-w2-1, H2+3*w2+1),
                                      new Point(W2-w2-1, H2+2*w2+1),
                                      new Point(W2-2*w2-1, H2+2*w2+1) };
        private static Point[] segL = new Point[] {
                                      new Point(W2-w2, H-w-1),
                                      new Point(W2+w2, H-w-1),
                                      new Point(W2+w2, H2+w2+1),
                                      new Point(W2, H2),
                                      new Point(W2-w2, H2+w2) };  //0x1000
        private static Point[] segH = new Point[] {
                                      new Point(15, 15),
                                      new Point(25, 15),
                                      new Point(40, 70),
                                      new Point(40, 80),
                                      new Point(30, 80),
                                      new Point(15, 25) };  //0x0100

        static List<Point[]> mFont = new List<Point[]>
        {
            segA, segB, segC, segD, segE, segF, segG1, segG2, segH, segI, segJ, segK, segL, segM, segDp
        };

        private ushort mPattern = 0;
        public ushort Pattern
        {
            get { return mPattern; }
            set { mPattern = value; this.Invalidate(); }
        }
        public Character()
        {
            InitializeComponent();
        }

        private void Character_Paint(object sender, PaintEventArgs e)
        {
            ushort p = Pattern;
            if(p==0x0869)
            {

            }
            //debug
//            p = 0x00c0;
            ushort mask = 0x01;
            int pos = 0;
            while (pos < mFont.Count)
            {
                if ((p & mask) != 0)
                    e.Graphics.FillPolygon(charBrush, mFont[pos]);
                pos++;
                mask <<= 1;
            }//while
        }
    }
}