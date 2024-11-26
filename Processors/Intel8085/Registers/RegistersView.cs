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

namespace Intel8085
{
    public partial class RegistersView : UserControl, IRegistersView
    {
        private bool[] mChangedRegisters;
        private Registers8085 mRegisters8085;
        private IProcessor mProcessor;

        private Registers8085 mSRegisters8085SnapShot;

        private ISystemMemory SystemMemory { get { return mProcessor.SystemMemory; } }

        private Brush mNormalBrush = new SolidBrush(Color.Black);
        private Brush mHightlightBrush = new SolidBrush(Color.Red);

        public RegistersView(IProcessor processor, Registers8085 registers8085)
        {
            InitializeComponent();
            mRegisters8085 = registers8085;
            mProcessor = processor;
        }

        public Panel UIPanel { get { return panel1; } }

        public void Start()
        {
            mSRegisters8085SnapShot = mRegisters8085.Clone() as Registers8085;
        }
        public void Stop()
        {
            mChangedRegisters = mSRegisters8085SnapShot.Difference(mRegisters8085);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Panel panel = sender as Panel;
            var registers8085 = mRegisters8085;

            if (mChangedRegisters == null)
                mChangedRegisters = new bool[19];

            // A Flags
            drawLine(g, panel, 1, 0, string.Format("A:{0:X2}", registers8085.A), mChangedRegisters[7]);
            drawLine(g, panel, 1, 1, string.Format("FLAGS:{0:X2}", registers8085.Flags), mChangedRegisters[11]);

            // B C
            drawLine(g, panel, 2, 0, string.Format("B:{0:X2}", registers8085.B), mChangedRegisters[0]);
            drawLine(g, panel, 2, 1, string.Format("C:{0:X2}", registers8085.C), mChangedRegisters[1]);
            drawLine(g, panel, 2, 2, string.Format("BC:{0:X4}", registers8085.BC), mChangedRegisters[0] || mChangedRegisters[1]);
            //drawLine(g, panel, 2, 3, string.Format("(BC)={0:X2}", SystemMemory.GetMemory(registers8085.BC, WordSize.TwoByte, false)));

            // D E
            drawLine(g, panel, 3, 0, string.Format("D:{0:X2}", registers8085.D), mChangedRegisters[2]);
            drawLine(g, panel, 3, 1, string.Format("E:{0:X2}", registers8085.E), mChangedRegisters[3]);
            drawLine(g, panel, 3, 2, string.Format("DE:{0:X4}", registers8085.DE), mChangedRegisters[2] || mChangedRegisters[3]);
            drawLine(g, panel, 3, 3, string.Format("(DE)={0:X2}", SystemMemory.GetMemory(registers8085.DE, WordSize.TwoByte, false)));

            // H L
            drawLine(g, panel, 4, 0, string.Format("H:{0:X2}", registers8085.H), mChangedRegisters[4]);
            drawLine(g, panel, 4, 1, string.Format("L:{0:X2}", registers8085.L), mChangedRegisters[5]);
            drawLine(g, panel, 4, 2, string.Format("HL:{0:X4}", registers8085.HL), mChangedRegisters[4] || mChangedRegisters[5]);
            //            drawLine(g, panel, 4, 3, string.Format("(HL)={0:X2}", SystemMemory.GetMemory(registers8085.HL, WordSize.TwoByte, false)));

            // SP
            drawLine(g, panel, 5, 0, string.Format("SP:{0:X4}", registers8085.SP), mChangedRegisters[9]);
            drawLine(g, panel, 5, 3, string.Format("(SP)={0:X2}", SystemMemory.GetMemory(registers8085.SP, WordSize.TwoByte, false)));

            // PC
            string dale = string.Format("PC:{0:X4}", registers8085.PC);
            drawLine(g, panel, 6, 0, string.Format("PC:{0:X4}", registers8085.PC), mChangedRegisters[10]);
            //drawLine(g, panel, 6, 3, string.Format("(PC)={0:X2}", SystemMemory.GetMemory(registers8085.PC, WordSize.TwoByte, false)));

            //Flags
            drawLine(g, panel, 8, 0, "Flags");
            drawLine(g, panel, 9, 0, "-----");
            drawLine(g, panel, 10, 0, "Zero:    " + (registers8085.Zero ? "1" : "0"), mChangedRegisters[8]);
            drawLine(g, panel, 11, 0, "Sign:    " + (registers8085.Sign ? "1" : "0"), mChangedRegisters[9]);
            drawLine(g, panel, 12, 0, "Aux      " + (registers8085.AuxCarry ? "1" : "0"), mChangedRegisters[10]);
            drawLine(g, panel, 13, 0, "Parity:  " + (registers8085.Parity ? "1" : "0"), mChangedRegisters[11]);
            drawLine(g, panel, 14, 0, "Carry:   " + (registers8085.Carry ? "1" : "0"), mChangedRegisters[12]);
            //drawLine(g, panel, 15, 0, "K:       " + (registers8085.K ? "1" : "0"), mChangedRegisters[13]);
            mChangedRegisters = null;

        }

        private void drawLine(Graphics g, Panel panel, int line, int column, string str, bool highlight = false)
        {
            int ypos = (int)(line * panel.Font.Height) + 20;
            g.DrawString(str, panel.Font, highlight ? mHightlightBrush : mNormalBrush, column * panel.Width / 5, ypos);
        }
    }
}