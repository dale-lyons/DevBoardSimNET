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

namespace BenEater
{
    public partial class BenEaterRegistersView : UserControl, IRegistersView
    {
        private RegistersBenEater mRegistersBenEater;
        private IProcessor mProcessor;
        private Brush mNormalBrush = new SolidBrush(Color.Black);
        private Brush mHightlightBrush = new SolidBrush(Color.Red);

        public BenEaterRegistersView(IProcessor processor, RegistersBenEater registersBenEater)
        {
            mRegistersBenEater = registersBenEater;
            mProcessor = processor;
            InitializeComponent();
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
            var registersBenEater = mRegistersBenEater;

            //            if (mChangedRegisters == null)
            //                mChangedRegisters = new bool[19];

            // A
//            drawLine(g, panel, 1, 1, string.Format("FLAGS:{0:X2}", registers8085.Flags), mChangedRegisters[11]);
            drawLine(g, panel, 1, 0, string.Format("A:{0:X2}", registersBenEater.A), false);
            // B
            drawLine(g, panel, 2, 0, string.Format("B:{0:X2}", registersBenEater.B), false);
            // Out
            drawLine(g, panel, 3, 0, string.Format("OUT:{0:X2}", registersBenEater.OUT), false);
            // PC
            drawLine(g, panel, 4, 0, string.Format("PC:{0:X4}", registersBenEater.PC), false);
            drawLine(g, panel, 6, 0, string.Format("Carry:{0}", registersBenEater.Carry ? "1" : "0"), false);
        }
        private void drawLine(Graphics g, Panel panel, int line, int column, string str, bool highlight = false)
        {
            int ypos = (int)(line * panel.Font.Height) + 20;
            g.DrawString(str, panel.Font, highlight ? mHightlightBrush : mNormalBrush, column * panel.Width / 5, ypos);
        }

    }
}