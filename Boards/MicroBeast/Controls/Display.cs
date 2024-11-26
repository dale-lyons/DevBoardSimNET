using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroBeast.Controls
{
    public partial class Display : UserControl
    {
        private int mCharacterMargin = 5;
        private Character[] charactersL = new Character[12];
        private Character[] charactersR = new Character[12];

        public Display()
        {
            InitializeComponent();
            int xpos = mCharacterMargin;
            int ypos = mCharacterMargin;
            for (int ii = 0; ii < charactersL.Length; ii++)
            {
                charactersL[ii] = new Character();
                charactersL[ii].Location = new Point(xpos, ypos);
                this.Controls.Add(charactersL[ii]);
                xpos = xpos + mCharacterMargin + charactersL[ii].Width;
            }

            xpos = mCharacterMargin;
            ypos = ypos + charactersL[0].Height + mCharacterMargin;
            for (int ii = 0; ii < charactersR.Length; ii++)
            {
                charactersR[ii] = new Character();
                charactersR[ii].Location = new Point(xpos, ypos);
                this.Controls.Add(charactersR[ii]);
                xpos = xpos + mCharacterMargin + charactersR[ii].Width;
            }
        }

        public void SetCharacterDisplayL(byte col, ushort pattern)
        {
            byte column = (byte)(col >> 1);
            if (column >= charactersL.Length)
                return;
            //Debug.Assert(false);
            charactersL[column].Pattern = pattern;
        }

        public void SetCharacterDisplayR(byte col, ushort pattern)
        {
            byte column = (byte)(col >> 1);
            if (column >= charactersR.Length)
                return;
                //Debug.Assert(false);
            charactersR[column].Pattern = pattern;
        }
    }
}