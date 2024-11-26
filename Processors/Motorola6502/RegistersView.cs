using Processors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Motorola6502
{
    public partial class RegistersView : UserControl, IRegistersView
    {
        private bool[] mChangedRegisters;
        private Brush mNormalBrush = new SolidBrush(Color.Black);
        private Brush mHightlightBrush = new SolidBrush(Color.Red);

        private Motorola6502 mMotorola6502;
        private Registers6502 mRegisters6502;
        public RegistersView(Motorola6502 motorola6502, Registers6502 registers6502)
        {
            InitializeComponent();
            mMotorola6502 = motorola6502;
            mRegisters6502 = registers6502;
        }

        public Panel UIPanel { get { return panel1; } }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Panel panel = sender as Panel;
            var registers6502 = mRegisters6502;

            if (mChangedRegisters == null)
                mChangedRegisters = new bool[19];

            // A Flags
            drawLine(g, panel, 1, 0, string.Format("A:{0:X2}", registers6502.A), mChangedRegisters[7]);
            drawLine(g, panel, 1, 1, string.Format("FLAGS:{0:X2}", registers6502.GetStatusFlags(false, false)), mChangedRegisters[11]);

            // B C
            drawLine(g, panel, 2, 0, string.Format("X:{0:X2}", registers6502.X), mChangedRegisters[0]);
            drawLine(g, panel, 2, 1, string.Format("Y:{0:X2}", registers6502.Y), mChangedRegisters[1]);

            // SP
            drawLine(g, panel, 5, 0, string.Format("SP:{0:X2}", registers6502.SP), mChangedRegisters[9]);
            //drawLine(g, panel, 5, 3, string.Format("(SP)={0:X2}", SystemMemory.GetMemory(registers8085.SP, WordSize.TwoByte, false)));

            // PC
            drawLine(g, panel, 6, 0, string.Format("PC:{0:X4}", registers6502.PC), mChangedRegisters[10]);

            //private static byte NegativeFlag = 0x80;
            //private static byte OverflowFlag = 0x40;
            //private static byte DecimalFlag = 0x08;
            //private static byte InterruptDisableFlag = 0x04;
            //private static byte ZeroFlag = 0x20;
            //private static byte CarryFlag = 0x01;

            //flags
            drawFlag(g, panel, 8, "0x80:Negative", registers6502.Negative);
            drawFlag(g, panel, 9, "0x40:Overflow", registers6502.Overflow);
            drawFlag(g, panel, 10, "0x20:Zero", registers6502.Zero);
            drawFlag(g, panel, 11, "0x08:Decimal", registers6502.Decimal);
            drawFlag(g, panel, 12, "0x04:InterruptDisable", registers6502.InterruptDisable);
            drawFlag(g, panel, 13, "0x01:Carry", registers6502.Carry);
        }

        private void drawFlag(Graphics g, Panel panel, int line, string str, bool highlight = false)
        {
            int ypos = (int)(line * panel.Font.Height) + 20;
            g.DrawString(str, panel.Font, highlight ? mHightlightBrush : mNormalBrush, 0, ypos);
        }

        private void drawLine(Graphics g, Panel panel, int line, int column, string str, bool highlight = false)
        {
            int ypos = (int)(line * panel.Font.Height) + 20;
            g.DrawString(str, panel.Font, highlight ? mHightlightBrush : mNormalBrush, column * panel.Width / 5, ypos);
        }
    }
}