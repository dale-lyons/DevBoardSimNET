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
using WeifenLuo.WinFormsUI.Docking;

namespace GUI.Board
{
    public partial class BoardView : ToolWindows, IView
    {
        public IProcessor Processor { get; private set; }
        public ISystemMemory SystemMemory { get; set; }

        public BoardView(QueryViewFunc qview, IProcessor processor)
        {
            Processor = processor;
            InitializeComponent();
        }

        public Panel RequestPanel(string title)
        {
            TabPage tabPage = new TabPage(title);
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(panel);
            tabControl1.TabPages.Add(tabPage);
            return panel;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
        public void ResetView() { }
        public void RefreshView() { }



    }
}
