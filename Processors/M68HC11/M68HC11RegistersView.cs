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

namespace M68HC11
{
    public partial class M68HC11RegistersView : UserControl, IRegistersView
    {
        private RegistersM68HC11 mRegistersM68HC11;
        private IProcessor mProcessor;
        private Brush mNormalBrush = new SolidBrush(Color.Black);
        private Brush mHightlightBrush = new SolidBrush(Color.Red);

        public M68HC11RegistersView(IProcessor processor, RegistersM68HC11 registersM68HC11)
        {
            mRegistersM68HC11 = registersM68HC11;
            mProcessor = processor;
            InitializeComponent();
            panel1.Paint += panel1_Paint;
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
            var registersM68HC11 = mRegistersM68HC11;

            //            if (mChangedRegisters == null)
            //                mChangedRegisters = new bool[19];

            // A
            //            drawLine(g, panel, 1, 1, string.Format("FLAGS:{0:X2}", registers8085.Flags), mChangedRegisters[11]);
            drawLine(g, panel, 1, 0, string.Format("A:{0:X2}", registersM68HC11.A), false);
            // B
            drawLine(g, panel, 1, 1, string.Format("B:{0:X2}", registersM68HC11.B), false);
            //D
            drawLine(g, panel, 2, 0, string.Format("D:{0:X4}", registersM68HC11.D), false);
            //X
            drawLine(g, panel, 3, 0, string.Format("X:{0:X4}", registersM68HC11.X), false);
            // B
            drawLine(g, panel, 3, 1, string.Format("Y:{0:X4}", registersM68HC11.Y), false);
            // PC
            drawLine(g, panel, 4, 0, string.Format("PC:{0:X4}", registersM68HC11.PC), false);
            //SP
            drawLine(g, panel, 5, 0, string.Format("SP:{0:X4}", registersM68HC11.SP), false);

            //CCR
            drawLine(g, panel, 7, 0, string.Format("CCR:{0:X2}", registersM68HC11.CCR), false);
            drawLine(g, panel, 8, 0, string.Format("Carry:{0}", registersM68HC11.Carry ? "1" : "0"), false);
            drawLine(g, panel, 9, 0, string.Format("Overflow:{0}", registersM68HC11.Overflow ? "1" : "0"), false);
            drawLine(g, panel, 10, 0, string.Format("Zero:{0}", registersM68HC11.Zero ? "1" : "0"), false);
            drawLine(g, panel, 11, 0, string.Format("Negative:{0}", registersM68HC11.Negative ? "1" : "0"), false);
            drawLine(g, panel, 12, 0, string.Format("IMask:{0}", registersM68HC11.IMask ? "1" : "0"), false);
            drawLine(g, panel, 13, 0, string.Format("HalfC:{0}", registersM68HC11.HalfC ? "1" : "0"), false);
            drawLine(g, panel, 14, 0, string.Format("XMask:{0}", registersM68HC11.XMask ? "1" : "0"), false);
            drawLine(g, panel, 15, 0, string.Format("StopDisable:{0}", registersM68HC11.StopDisable ? "1" : "0"), false);

        }
        private void drawLine(Graphics g, Panel panel, int line, int column, string str, bool highlight = false)
        {
            int ypos = (int)(line * panel.Font.Height) + 20;
            g.DrawString(str, panel.Font, highlight ? mHightlightBrush : mNormalBrush, column * panel.Width / 5, ypos);
        }

    }
}