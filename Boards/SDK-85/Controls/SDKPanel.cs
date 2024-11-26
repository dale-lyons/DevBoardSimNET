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
    public partial class SDKPanel : UserControl
    {
        public delegate void KeypressDelegate(byte c);
        public event KeypressDelegate OnKeypress;
        public delegate void ResetDelegate();
        public event ResetDelegate onReset;
        public event GetCSRDelegate GetCSR1;
        public event GetCSRDelegate GetCSR2;

        public IOPort PortA { get { return ioPort1; } }
        public IOPort PortB { get { return ioPort2; } }
        public IOPort PortC { get { return ioPort3; } }

        private int mIndex;

        public SDKPanel()
        {
            InitializeComponent();
        }

        private void keypad1_OnKeypress(byte c)
        {
            OnKeypress?.Invoke(c);
        }

        public void SetIndex(int index)
        {
            mIndex = index;
            if (index > 0)
                mIndex++;
        }

        public void DisplayWriteSegment(byte c)
        {
            c = (byte)~c;
            switch (mIndex)
                {
                case 0: eightSegmentDisplay1.Code = c; break;
                case 1: eightSegmentDisplay2.Code = c; break;
                case 2: eightSegmentDisplay3.Code = c; break;
                case 3: eightSegmentDisplay4.Code = c; break;
                case 4: eightSegmentDisplay5.Code = c; break;
                case 5: eightSegmentDisplay6.Code = c; break;
                case 6: eightSegmentDisplay7.Code = c; break;
            }
            mIndex++;
            if (mIndex >= 7)
                mIndex = 0;
        }

        private void keypad1_onReset()
        {
            onReset?.Invoke();
        }
    }
}