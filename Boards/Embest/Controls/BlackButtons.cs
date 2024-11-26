﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ARMSim.Plugins.UIControls
{
    public partial class BlackButtons : UserControl
    {
        [Flags]
        public enum BlackButtonEnum
        {
            Left = 0x01,
            Right= 0x02
        }

        bool mLeftPressed;
        bool mRightPressed;

        public event BlackButtonPressNotify Notify;

        public BlackButtons()
        {
            InitializeComponent();
        }

        public uint CheckPressed()
        {
            uint ret = 0;
            ret |= (uint)(mLeftPressed ? (int)BlackButtonEnum.Left : 0x00);
            ret |= (uint)(mRightPressed ? (int)BlackButtonEnum.Right : 0x00);

            mLeftPressed = mRightPressed = false;

            return ret;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (sender == leftButton)
            {
                mLeftPressed = true;
                if (Notify != null)
                    Notify(this, new BlackButtonEventArgs(BlackButtonEnum.Left));
            }
            else
            {
                mRightPressed = true;
                if (Notify != null)
                    Notify(this, new BlackButtonEventArgs(BlackButtonEnum.Right));
            }
        }

    }

    public class BlackButtonEventArgs : EventArgs
    {
        private readonly BlackButtons.BlackButtonEnum mButton;
        public BlackButtonEventArgs(BlackButtons.BlackButtonEnum button)
        {
            mButton = button;
        }
        public BlackButtons.BlackButtonEnum Button { get { return mButton; } }
    }
    public delegate void BlackButtonPressNotify(object sender, BlackButtonEventArgs args);

}
