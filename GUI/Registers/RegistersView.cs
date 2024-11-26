using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;
using Processors;

namespace GUI.Registers
{
    public partial class RegistersView : ToolWindows, IView
    {
        private IProcessor mProcessor;
        public Panel panel { get { return panel1; } }

        public RegistersView(QueryViewFunc qview, IProcessor processor)
        {
            InitializeComponent();
            mProcessor = processor;
        }

        public void RefreshView()
        {
            panel1.Invalidate(true);
        }

        public void Start()
        {
            mProcessor.RegistersView.Start();
        }

        public void Stop()
        {
            mProcessor.RegistersView.Stop();
        }

        public void ResetView()
        {
        }

    }
}
