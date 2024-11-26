using Processors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI.Watch
{
    public partial class AddWatchItem : Form
    {
        private Dictionary<string, bool> mRegisters;
        private IProcessor mProcessor;

        public WatchItem WatchItem
        {
            get
            {
                var ret =  new WatchItem
                {
                    Expression = textBox1.Text.Trim().ToLower(),
                    Name = textBox2.Text.Trim(),
                    WordSize = (WordSize)comboBox1.SelectedItem,
                    Registers = mRegisters,
                    Processor = mProcessor
                };
                ret.Registers = mRegisters;
                return ret;
            }
        }

        public AddWatchItem(Dictionary<string, bool> registers, IProcessor processor)
        {
            mRegisters = registers;
            mProcessor = processor;
            InitializeComponent();
        }

        private void AddWatchItem_Load(object sender, EventArgs e)
        {
            foreach (var ws in Enum.GetValues(typeof(WordSize)))
                comboBox1.Items.Add(ws);
            comboBox1.SelectedIndex = 0;
        }
    }
}