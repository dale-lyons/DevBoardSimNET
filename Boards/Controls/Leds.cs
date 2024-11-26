using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boards.Contols
{
    public partial class Leds : UserControl
    {
        //private enum LEDTypes
        //{
        //    Power = 0,
        //    Rom = 1,
        //    User = 2
        //}

        //private int mSetSelectedBank;
        //public int SetSelectedBank { get { return mSetSelectedBank; } set { mSetSelectedBank = value; selectedBank.Text = String.Format("SelectedBank:{0}", value); } }

        private bool[] mLedStates;
        public Color LEDColor { get; set; }
        private int mNumberLeds;
        public int NumberLeds
        {
            get { return mNumberLeds; }
            set { mNumberLeds = value; mLedStates = new bool[value]; this.Invalidate(); }
        }

        public Leds()
        {
            InitializeComponent();
            LEDColor = Color.Red;
            NumberLeds = 3;
            this.Paint += panel_Paint;
        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {
            if (NumberLeds <= 0)
                return;

            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;
            int ledWidth = Math.Min((width / NumberLeds), height);

            for (int ii = 0; ii < NumberLeds; ii++)
            {
                var rect = new Rectangle(ii * ledWidth, 0, ledWidth, ledWidth);
                var brush = new SolidBrush(mLedStates[ii] ? LEDColor : Color.White);
                e.Graphics.FillEllipse(brush, rect);
            }
        }


        public void SetLED(int index, bool state)
        {
            mLedStates[index] = state;
            this.Invalidate();
        }

        //public void SetUserLED(bool state)
        //{
        //    mLedStates[(int)(LEDTypes.User)] = state;
        //    panel3.Invalidate();
        //}
        //public void SetPowerLED(bool state)
        //{
        //    mLedStates[(int)(LEDTypes.Power)] = state;
        //    panel3.Invalidate();
        //}

        //private void drawLED(Graphics g, int ii, int width, bool state)
        //{
        //    var rect = new Rectangle(ii * width, 0, width, width);
        //    var brush = new SolidBrush(state ? LEDColor : Color.White);
        //    g.FillEllipse(brush, rect);
        //}

    }
}