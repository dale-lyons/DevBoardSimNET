using System;
using System.Windows.Forms;
using GUI.Output;
using Processors;
using WeifenLuo.WinFormsUI.Docking;

namespace GUI.Stack
{
    public partial class StackView : ToolWindows, IView
    {
        private QueryViewFunc mQueryViewFunc;
        private StackConfig mConfig;
        public IProcessor Processor { get; private set; }

        public StackView(QueryViewFunc queryViewFunc, IProcessor processor, StackConfig config)
        {
            mQueryViewFunc = queryViewFunc;
            Processor = processor;
            mConfig = config;
            InitializeComponent();
        }

        public void ResetView()
        {
        }

        public void Start()
        {
        }

        public void Stop() { }

        private void StackView_Load(object sender, EventArgs e)
        {
            cb1.Items.AddRange(Processor.DoubleRegisters());
            cb1.SelectedItem = mConfig.StackPointer;
        }

        private void cb1_SelectedValueChanged(object sender, EventArgs e)
        {
            mConfig.StackPointer = cb1.SelectedItem as string;
            var ov = mQueryViewFunc(ViewEnums.OutputT) as OutputView;
            ov.WriteLine("Switched StackView to sp=" + mConfig.StackPointer);
            RefreshView();
        }

        public void RefreshView()
        {
            listView1.Items.Clear();
            string sp = mConfig.StackPointer;
            if (string.IsNullOrEmpty(sp))
                return;

            uint baseaddr = 0;
            try
            {
                baseaddr = Processor.Registers.GetSingleRegister(sp);
            }
            catch
            {
                (mQueryViewFunc(ViewEnums.OutputT) as OutputView).WriteLine("Error parsing stack pointer register:" + sp);
                baseaddr = 0;
            }

            int centreIndex = 0; ;
            int index = 0;
            for (int offset = -40; offset <=40; offset +=2, index++)
            {
                if (offset == 0)
                    centreIndex = index;

                uint addr = (uint)(baseaddr + offset);
                ushort val = (ushort)Processor.SystemMemory.GetMemory(addr, WordSize.TwoByte, false);
                var lvi = new ListViewItem(Processor.SystemMemory.FormatAddress(addr)+"("+offset.ToString("+0;-#") +")");
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, val.ToString("X4")));
                listView1.Items.Add(lvi);
            }
            listView1.EnsureVisible(centreIndex);
        }
    }
}