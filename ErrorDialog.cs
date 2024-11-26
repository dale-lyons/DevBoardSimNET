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

namespace DevBoardSim.NET
{
    public partial class ErrorDialog : Form
    {
        private IList<ASMError> mErrors;
        public ErrorDialog(IList<ASMError> errors)
        {
            mErrors = errors;
            InitializeComponent();
        }

        private void ErrorDialog_Load(object sender, EventArgs e)
        {
            foreach(var error in mErrors)
            {
                listView1.Items.Add(new ListViewItem(error.Line.ToString())).SubItems.Add(error.Text);
            }
        }
    }
}
