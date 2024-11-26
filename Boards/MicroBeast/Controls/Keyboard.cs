using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Terminal.TerminalControl;

namespace MicroBeast.Controls
{
    public delegate void OnKeypressDelegate(ushort code);
    public partial class Keyboard : UserControl
    {
        public event OnKeypressDelegate OnKeypress;
        public Keyboard()
        {
            InitializeComponent();
            foreach (Button btn in Controls)
            {
                btn.MouseDown += Btn_MouseDown;
                btn.MouseUp += Btn_MouseUp;
            }
        }

        private ushort? findPressedKeycode(Button btn)
        {
            string key = btn.Tag as string;
            if (byte.TryParse(key.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte rowCode))
            {
                if (byte.TryParse(key.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, null, out byte colCode))
                {
                    byte[] bytes = new byte[] { colCode, rowCode };
                    ushort code = BitConverter.ToUInt16(bytes, 0);
                    return code;
                }
            }
            return null;
        }

        private void Btn_MouseDown(object sender, MouseEventArgs e)
        {
            ushort? code = findPressedKeycode(sender as Button);
            if (code.HasValue)
            {
                Debug.WriteLine("Key Down!");
                OnKeypress?.Invoke((ushort)code);
            }
        }
        private void Btn_MouseUp(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Key Up!");
            OnKeypress?.Invoke(0x003f);
            //ushort? code = findPressedKeycode(sender as Button);
            //if (code.HasValue)
            //{
            //    Debug.WriteLine("Key Up!");
            //    code |= 0x01;
            //    //OnKeypress?.Invoke((ushort)code);
            //}
        }
    }
}