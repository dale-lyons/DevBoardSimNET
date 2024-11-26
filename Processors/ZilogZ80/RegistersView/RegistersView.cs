using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Processors;

namespace ZilogZ80
{
    public partial class RegistersView : UserControl, IRegistersView
    {
        private bool[] mChangedRegisters = new bool[19];
        private RegistersZ80 mRegistersZ80;
        private RegistersZ80 mSRegistersZ80SnapShot;
        private ISystemMemory SystemMemory { get { return mProcessor.SystemMemory; } }

        private Brush mNormalBrush = new SolidBrush(Color.Black);
        private Brush mHightlightBrush = new SolidBrush(Color.Red);
        private IProcessor mProcessor;

        public RegistersView(IProcessor processor, RegistersZ80 registersZ80)
        {
            InitializeComponent();
            mProcessor = processor;
            mRegistersZ80 = registersZ80;
        }

        public Panel UIPanel { get { return panel1; } }

        public void Start()
        {
            mSRegistersZ80SnapShot = mRegistersZ80.Clone() as RegistersZ80;
        }
        public void Stop()
        {
            if(mSRegistersZ80SnapShot != null)
                mChangedRegisters = mSRegistersZ80SnapShot.Difference(mRegistersZ80);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (mChangedRegisters == null)
                return;

            Graphics g = e.Graphics;
            Panel panel = sender as Panel;

            // A Flags
            drawLine(g, panel, 1, 0, string.Format("A:{0:X2}", mRegistersZ80.A), mChangedRegisters[7]);
            drawLine(g, panel, 1, 1, string.Format("FLAGS:{0:X2}", mRegistersZ80.Flags), mChangedRegisters[8]);

            // BC
            drawLine(g, panel, 2, 0, string.Format("B:{0:X2}", mRegistersZ80.B), mChangedRegisters[0]);
            drawLine(g, panel, 2, 1, string.Format("C:{0:X2}", mRegistersZ80.C), mChangedRegisters[1]);
            drawLine(g, panel, 2, 2, string.Format("BC:{0:X4}", mRegistersZ80.BC), mChangedRegisters[0] || mChangedRegisters[1]);

            // DE
            drawLine(g, panel, 3, 0, string.Format("D:{0:X2}", mRegistersZ80.D), mChangedRegisters[2]);
            drawLine(g, panel, 3, 1, string.Format("E:{0:X2}", mRegistersZ80.E), mChangedRegisters[3]);
            drawLine(g, panel, 3, 2, string.Format("DE:{0:X4}", mRegistersZ80.DE), mChangedRegisters[2] || mChangedRegisters[3]);
            //drawLine(g, panel, 3, 3, string.Format("(DE)={0:X2}", SystemMemory.GetMemory(mRegistersZ80.DE, WordSize.TwoByte, false)));

            // HL
            drawLine(g, panel, 4, 0, string.Format("H:{0:X2}", mRegistersZ80.H), mChangedRegisters[4]);
            drawLine(g, panel, 4, 1, string.Format("L:{0:X2}", mRegistersZ80.L), mChangedRegisters[5]);
            drawLine(g, panel, 4, 2, string.Format("HL:{0:X4}", mRegistersZ80.HL), mChangedRegisters[4] || mChangedRegisters[5]);

            // SP
            drawLine(g, panel, 5, 0, string.Format("SP:{0:X4}", mRegistersZ80.SP), mChangedRegisters[9]);
            //drawLine(g, panel, 5, 3, string.Format("(SP)={0:X2}", SystemMemory.GetMemory(mRegistersZ80.SP, WordSize.TwoByte, false)));

            // PC
            drawLine(g, panel, 6, 0, string.Format("PC:{0:X4}", mRegistersZ80.PC), mChangedRegisters[10]);
            //drawLine(g, panel, 6, 3, string.Format("(PC)={0:X2}", SystemMemory.GetMemory(registers8085.PC, WordSize.TwoByte, false)));

            //IX,IY
            drawLine(g, panel, 7, 0, string.Format("IX:{0:X4}", mRegistersZ80.IX), mChangedRegisters[11]);
            drawLine(g, panel, 8, 0, string.Format("IY:{0:X4}", mRegistersZ80.IY), mChangedRegisters[12]);

            //Flags
            drawLine(g, panel, 10, 0, "Flags");
            drawLine(g, panel, 11, 0, "-----");
            drawLine(g, panel, 12, 0, "Sign:    " + (mRegistersZ80.Sign ? "1" : "0"), mChangedRegisters[13]);
            drawLine(g, panel, 13, 0, "Zero:    " + (mRegistersZ80.Zero ? "1" : "0"), mChangedRegisters[18]);
            drawLine(g, panel, 14, 0, "Half     " + (mRegistersZ80.AuxCarry ? "1" : "0"), mChangedRegisters[17]);
            drawLine(g, panel, 15, 0, "Parity:  " + (mRegistersZ80.Parity ? "1" : "0"), mChangedRegisters[16]);
            drawLine(g, panel, 16, 0, "Negative:" + (mRegistersZ80.Negative ? "1" : "0"), mChangedRegisters[15]);
            drawLine(g, panel, 17, 0, "Carry:   " + (mRegistersZ80.Carry ? "1" : "0"), mChangedRegisters[14]);

        }

        private void drawLine(Graphics g, Panel panel, int line, int column, string str, bool highlight = false)
        {
            int ypos = (int)(line * panel.Font.Height) + 20;
            g.DrawString(str, panel.Font, highlight ? mHightlightBrush : mNormalBrush, column * panel.Width / 5, ypos);
        }
    }
}