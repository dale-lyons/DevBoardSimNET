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

namespace ARM7.Controls
{
    public partial class RegistersView : UserControl, IRegistersView
    {
        private enum RegisterDisplayBase
        {
            Hexadecimal,
            Unsigned,
            Signed
        };
        private enum FloatingPointType
        {
            Single,
            Double
        };

        private bool[] _gpRegistersChanged;
        private bool[] _cpsrChanged;


        private ARM7 mProcessor;
        public Panel MainPanel { get { return mainPanel; } }
        public RegistersView(IProcessor processor)
        {
            mProcessor = processor as ARM7;
            InitializeComponent();
            _gpRegistersChanged = new bool[16];
            _cpsrChanged = new bool[7];
        }

        private FloatingPointType CurrentFloatingPointType
        {
            get
            {
                if (btnSingle.Checked)
                    return FloatingPointType.Single;
                else
                    return FloatingPointType.Double;
            }
        }//CurrentFloatingPointType

        private RegisterDisplayBase CurrentDisplayBase
        {
            get
            {
                if (btnHex.Checked)
                    return RegisterDisplayBase.Hexadecimal;
                else if (btnUsigned.Checked)
                    return RegisterDisplayBase.Unsigned;
                else
                    return RegisterDisplayBase.Signed;
            }
        }

        //Floating Point Registers
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            System.Drawing.Drawing2D.Matrix mx = new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, panel2.AutoScrollPosition.X, panel2.AutoScrollPosition.Y);
            g.Transform = mx;

            SolidBrush backBrush = new SolidBrush(panel2.BackColor);
            SolidBrush textBrush = new SolidBrush(panel2.ForeColor);
            SolidBrush highlightBrush = new SolidBrush(Color.Red);

            int line = 0;
            switch (this.CurrentFloatingPointType)
            {
                case FloatingPointType.Single:
                    for (uint ii = 0; ii < 32; ii++, line++)
                    {
                        int ypos = (int)(ii * panel2.Font.Height);
                        Rectangle bounds = new Rectangle(0, ypos, panel2.ClientRectangle.Width, panel2.Font.Height);

                        string str = "s" + ii.ToString() + ((ii < 10) ? " " : "") + ":";
                        float regValue = mProcessor.FPP.FPR.ReadS(ii);
                        str += VFP.FloatingPointProcessor.FloatToString(regValue);
                        //str += float.IsNaN(regValue) ? "NaN" : regValue.ToString("0.###E+0");

                        g.FillRectangle(backBrush, bounds);
//                        Brush brush = _fpRegistersChanged[ii] ? highlightBrush : textBrush;
                        Brush brush = textBrush;
                        e.Graphics.DrawString(str, panel2.Font, brush, bounds, StringFormat.GenericDefault);

                    }
                    break;
                case FloatingPointType.Double:
                    for (uint ii = 0; ii < 16; ii++, line++)
                    {
                        int ypos = (int)(ii * panel2.Font.Height);
                        Rectangle bounds = new Rectangle(0, ypos, panel2.ClientRectangle.Width, panel1.Font.Height);

                        string str = "d" + ii.ToString() + ((ii < 10) ? " " : "") + ":";
                        double regValue = mProcessor.FPP.FPR.ReadD(ii);
                        str += VFP.FloatingPointProcessor.DoubleToString(regValue);
                        //str += double.IsNaN(regValue) ? "NaN" : regValue.ToString("0.###E+0");

                        g.FillRectangle(backBrush, bounds);
//                        Brush brush = _fpRegistersChanged[ii] ? highlightBrush : textBrush;
                        Brush brush = textBrush;
                        e.Graphics.DrawString(str, panel1.Font, brush, bounds, StringFormat.GenericDefault);
                    }
                    break;
            }//switch
            DrawLine(g, panel2, line++, textBrush, "------------------");
            DrawLine(g, panel2, line++, textBrush, "FCPSR Register");
            DrawLine(g, panel2, line++, textBrush, "Negative(N):" + (mProcessor.FPP.FPSCR.nf ? "1" : "0"));
            DrawLine(g, panel2, line++, textBrush, "Zero(Z)    :" + (mProcessor.FPP.FPSCR.zf ? "1" : "0"));
            DrawLine(g, panel2, line++, textBrush, "Carry(C)   :" + (mProcessor.FPP.FPSCR.cf ? "1" : "0"));
            DrawLine(g, panel2, line++, textBrush, "Overflow(V):" + (mProcessor.FPP.FPSCR.vf ? "1" : "0"));
            DrawLine(g, panel2, line++, textBrush, "Stride     :" + (mProcessor.FPP.FPSCR.Stride.ToString()));
            DrawLine(g, panel2, line++, textBrush, "Length     :" + (mProcessor.FPP.FPSCR.Length.ToString()));
            DrawLine(g, panel2, line++, textBrush, "-------------");
            DrawLine(g, panel2, line++, textBrush, "0x" + mProcessor.FPP.FPSCR.Flags.ToString("x8"));
        }

        private static void DrawLine(Graphics g, Panel panel, int line, Brush textBrush, string str)
        {
            int ypos = (int)(line * panel.Font.Height);
            Rectangle bounds = new Rectangle(0, ypos, panel.ClientRectangle.Width, panel.Font.Height);
            SolidBrush backBrush = new SolidBrush(panel.BackColor);
            g.FillRectangle(backBrush, bounds);
            g.DrawString(str, panel.Font, textBrush, bounds, StringFormat.GenericDefault);
        }

        //General Purpose Registers
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            System.Drawing.Drawing2D.Matrix mx = new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, panel1.AutoScrollPosition.X, panel1.AutoScrollPosition.Y);
            g.Transform = mx;

            SolidBrush backBrush = new SolidBrush(panel1.BackColor);
            SolidBrush textBrush = new SolidBrush(panel1.ForeColor);
            SolidBrush highlightBrush = new SolidBrush(Color.Red);

            for (uint ii = 0; ii < 16; ii++)
            {
                int ypos = (int)(ii * panel1.Font.Height);
                Rectangle bounds = new Rectangle(0, ypos, panel1.ClientRectangle.Width, panel1.Font.Height);

                string str = GeneralPurposeRegisters.registerToString(ii);

                uint regValue = mProcessor.GPR[ii];
                switch (this.CurrentDisplayBase)
                {
                    case RegisterDisplayBase.Hexadecimal:
                        str += regValue.ToString("x8"); break;
                    case RegisterDisplayBase.Signed:
                        str += ((int)regValue).ToString(); break;
                    case RegisterDisplayBase.Unsigned:
                        str += regValue.ToString(); break;
                }
                g.FillRectangle(backBrush, bounds);
                Brush brush = _gpRegistersChanged[ii] ? highlightBrush : textBrush;

                // Draw the current item text based on the current Font and the custom brush settings.
                e.Graphics.DrawString(str, panel1.Font, brush, bounds, StringFormat.GenericDefault);

            }//for ii

            DrawLine(g, panel1, 16, textBrush, "------------------");
            DrawLine(g, panel1, 17, textBrush, "CPSR Register");
            DrawLine(g, panel1, 18, _cpsrChanged[0] ? highlightBrush : textBrush, "Negative(N):" + (mProcessor.CPSR.nf ? "1" : "0"));
            DrawLine(g, panel1, 19, _cpsrChanged[1] ? highlightBrush : textBrush, "Zero(Z)    :" + (mProcessor.CPSR.zf ? "1" : "0"));
            DrawLine(g, panel1, 20, _cpsrChanged[2] ? highlightBrush : textBrush, "Carry(C)   :" + (mProcessor.CPSR.cf ? "1" : "0"));
            DrawLine(g, panel1, 21, _cpsrChanged[3] ? highlightBrush : textBrush, "Overflow(V):" + (mProcessor.CPSR.vf ? "1" : "0"));
            DrawLine(g, panel1, 22, textBrush, "IRQ Disable:" + (mProcessor.CPSR.IRQDisable ? "1" : "0"));
            DrawLine(g, panel1, 23, textBrush, "FIQ Disable:" + (mProcessor.CPSR.FIQDisable ? "1" : "0"));
            DrawLine(g, panel1, 24, textBrush, "Thumb(T)   :" + (mProcessor.CPSR.tf ? "1" : "0"));
            DrawLine(g, panel1, 25, textBrush, "CPU Mode   :" + System.Enum.GetName(typeof(CPSR.CPUModeEnum), mProcessor.CPSR.Mode));
            DrawLine(g, panel1, 26, textBrush, "------------------");
            DrawLine(g, panel1, 27, textBrush, "0x" + mProcessor.CPSR.Flags.ToString("x8"));
        }

        private void btn_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (!rb.Checked)
                return;

            this.updateView();
        }

        private void updateView()
        {
            int requiredHeight = (panel1.Font.Height * 28) + 1;
            //this.vScrollBar1.Visible = panel1.Height < requiredHeight;
            panel1.AutoScrollMinSize = new Size(panel1.Width / 2, requiredHeight);

            int numRegisters = this.CurrentFloatingPointType == FloatingPointType.Single ? 32 : 16;
            requiredHeight = (panel2.Font.Height * (numRegisters + 10)) + 1;
            panel2.AutoScrollMinSize = new Size(panel2.Width / 2, requiredHeight);

            panel1.Invalidate();
            panel2.Invalidate();
        }

        private void tabPage1_Resize(object sender, EventArgs e)
        {
            int formHeight = tabPage1.Height;
            int buttonHeight = btnHex.Height + btnUsigned.Height + btnSigned.Height;
            panel1.Height = formHeight - buttonHeight - 5;
        }

        private void tabPage2_Resize(object sender, EventArgs e)
        {
            int formHeight = tabPage2.Height;
            int buttonHeight = btnSingle.Height + btnDouble.Height;
            panel2.Height = formHeight - buttonHeight - 5;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public Panel UIPanel { get { return MainPanel; } }

    }
}