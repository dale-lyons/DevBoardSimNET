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
    public partial class Keypad : UserControl
    { 
        public delegate void KeypressDelegate(byte c);
        public event KeypressDelegate OnKeypress;
        public delegate void ResetDelegate();
        public event ResetDelegate onReset;

        public Keypad()
        {
            InitializeComponent();
        }

        private void button_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, button1.ClientRectangle,
            SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset,
            SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset,
            SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset,
            SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset);
        }

        private void button_Click(object sender, EventArgs e)
        {
            var str = (sender as Button).Tag as string;
            byte keyVal = byte.Parse(str, System.Globalization.NumberStyles.HexNumber);
            OnKeypress?.Invoke(keyVal);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            onReset?.Invoke();
        }
    }
}