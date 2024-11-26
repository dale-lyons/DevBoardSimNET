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

namespace GUI.Output
{
    public partial class OutputView : ToolWindows, IView
    {
        public OutputView(QueryViewFunc qview, IProcessor processor)
        {
            InitializeComponent();
        }

        void IView.RefreshView()
        {
        }

        void IView.ResetView()
        {
        }

        void IView.Start()
        {
        }

        void IView.Stop()
        {
        }

        public void WriteLine(string msg)
        {
            lb1.Items.Add(msg);
        }
    }
}