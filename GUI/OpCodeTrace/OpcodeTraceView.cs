using GUI.Code;
using Processors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace GUI.OpCodeTrace
{
    public partial class OpcodeTraceView : ToolWindows, IView
    {
        private CodeView mCodeView;
        public OpcodeTraceView(QueryViewFunc qview, IProcessor processor)
        {
            mCodeView = qview(ViewEnums.CodeT) as CodeView;
            InitializeComponent();
        }

        public void RefreshView()
        {
        }

        public void ResetView()
        {
        }

        public void Start()
        {
            mCodeView.Processor.OpcodeTrace.Clear();
        }

        public void Stop()
        {
            var dissasm = mCodeView.Processor.CreateDisassembler(0);
            var opcodeTrace = mCodeView.Processor.OpcodeTrace;
            listBox1.Items.Clear();
            while (opcodeTrace.Count > 0)
            {
                var addr = opcodeTrace.Dequeue();
                var cl = dissasm.ProcessInstruction(addr, mCodeView.Processor.SystemMemory, out uint newAddr);
                listBox1.Items.Add(cl.ToString());
            }
//            listBox1.Invalidate();
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            e.DrawBackground();
            Brush myBrush = Brushes.Black;

            uint addr = (uint)listBox1.Items[e.Index];
            e.Graphics.DrawString(addr.ToString("X4"), e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }
    }
}