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
    public partial class DipSwitchEditor : Form
    {
        private byte mDipValue;
        public DipSwitchEditor(object value)
        {
            mDipValue = Convert.ToByte(value);
            InitializeComponent();
            dipSwitch1.SwitchValue = mDipValue;
            textBox1.Text = "0x" + mDipValue.ToString("X2");
        }

        public byte FinalDipValue
        {
            get
            {
                return dipSwitch1.SwitchValue;
            }
        }

        private void dipSwitch1_OnSwitchChanged()
        {
            textBox1.Text = "0x" + dipSwitch1.SwitchValue.ToString("X2");
        }
    }
}
