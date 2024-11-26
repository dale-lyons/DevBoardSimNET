using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Preferences
{
    public partial class SubSwitch : UserControl
    {
        public delegate void OnSwitchChangedDelegate();
        public event OnSwitchChangedDelegate OnSwitchChanged;

        private bool mOn;
        public bool On
        {
            get { return mOn; }
            set { mOn = value; this.Invalidate(); }
        }
        public SubSwitch(bool on)
        {
            On = on;
            InitializeComponent();
        }

        private void SubSwitch_Paint(object sender, PaintEventArgs e)
        {
            var rect = this.ClientRectangle;
            int side = rect.Height / 3;
            int yoffset = mOn ? 0 : (rect.Height * 2) / 3;

            Rectangle rect2 = new Rectangle(new Point(0, yoffset), new Size(rect.Width, side));
            e.Graphics.FillRectangle(Brushes.White, rect2);
        }

        private void SubSwitch_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var rect = this.ClientRectangle;
            if (e.Location.Y < (rect.Height / 3))
                mOn = true;
            else if (e.Location.Y > ((rect.Height*2) / 3))
                mOn = false;

            OnSwitchChanged?.Invoke();
            this.Invalidate();
        }
    }
}