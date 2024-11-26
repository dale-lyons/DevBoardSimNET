using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Preferences.SubSwitch;

namespace Preferences
{
    public partial class DipSwitch : UserControl
    {
        public event OnSwitchChangedDelegate OnSwitchChanged;

        public int NumberSwitches { get; set; } = 4;

        private byte mSwitchValue;
        public byte SwitchValue
        {
            get
            {
                byte ret = 0;
                byte mask = 0x80;
                foreach (var sub in this.Controls)
                {
                    if (sub is SubSwitch)
                    {
                        if ((sub as SubSwitch).On)
                            ret |= mask;
                        mask >>= 1;
                    }
                }
                return ret;
            }
            set { mSwitchValue = value; }
        }

        public DipSwitch()
        {
            InitializeComponent();
        }

        private void resize()
        {
            var rect = this.ClientRectangle;
            int yHeight = rect.Height / 3;
            int xspace = (rect.Width / NumberSwitches) - 5;
            int ypos = (rect.Height / 3) - (yHeight / 10);

            int ii = 0;
            foreach (var sub in this.Controls)
            {
                if (sub is SubSwitch)
                {
                    int xpos = (ii * xspace) + (xspace / 2);
                    (sub as SubSwitch).Location = new Point(xpos, ypos);
                    (sub as SubSwitch).Size = new Size(xspace / 2, yHeight);
                    ii++;
                }
            }
        }

        private void DipSwitch_Resize(object sender, EventArgs e)
        {
            resize();
        }

        private void DipSwitch_SizeChanged(object sender, EventArgs e)
        {
            resize();
        }

        private void DipSwitch_Load(object sender, EventArgs e)
        {
            var setting = mSwitchValue;
            byte mask = (byte)(0x01 << NumberSwitches - 1);
            for (int ii = 0; ii < NumberSwitches; ii++)
            {
                var on = ((setting & mask) != 0);
                var sub = new SubSwitch(on);
                sub.OnSwitchChanged += Sub_OnSwitchChanged;
                this.Controls.Add(sub);
                mask >>= 1;
            }
            resize();
        }

        private void Sub_OnSwitchChanged()
        {
            OnSwitchChanged?.Invoke();
        }

        private void DipSwitch_Paint(object sender, PaintEventArgs e)
        {
            var rect = this.ClientRectangle;
            int yHeight = rect.Height / 3;
            int xspace = (rect.Width / NumberSwitches) - 5;
//            int ypos = (rect.Height / 3) - (yHeight / 10);

            e.Graphics.DrawString("ON", this.Font, Brushes.White, new Point(xspace/2, yHeight/3));
            e.Graphics.DrawString("DIP", this.Font, Brushes.White, new Point((NumberSwitches-2)*xspace, yHeight / 3));

                for (int ii = 0; ii < NumberSwitches; ii++)
                {
                    int xpos = ii * xspace + (xspace/2);
                    int ypos = rect.Height - this.Font.Height - 20;
                    e.Graphics.DrawString((ii+1).ToString(), this.Font, Brushes.White, new Point(xpos, ypos));
                }
        }
    }
}