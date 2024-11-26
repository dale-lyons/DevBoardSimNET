using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GUI.Output;
using Processors;
using WeifenLuo.WinFormsUI.Docking;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace GUI.Watch
{
    public partial class Watch : ToolWindows, IView
    {
        private int mMouseX;
        private int mMouseY;
        private Dictionary<string, bool> mRegisters = new Dictionary<string, bool>();

        private QueryViewFunc queryview;
        private IProcessor mProcessor;
        public WatchConfig mWatchConfig;
        public Watch(QueryViewFunc qview, IProcessor processor, WatchConfig watchConfig)
        {
            queryview = qview;
            mProcessor = processor;
            mWatchConfig = watchConfig;
            InitializeComponent();

            if(mWatchConfig.WatchList != null)
            {
                string[] dreg = mProcessor.DoubleRegisters();
                if (dreg != null)
                {
                    foreach (var reg in dreg)
                        mRegisters[reg] = true;
                }

                foreach (WatchItem watch in mWatchConfig.WatchList)
                {
                    watch.Processor = mProcessor;
                    watch.Registers = mRegisters;
                    ListViewItem lvi = new ListViewItem(watch.Expression);
                    lvi.SubItems.Add(watch.WordSize.ToString());
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add(watch.Name);
                    lvi.Tag = watch;
                    listView1.Items.Add(lvi);
                }
            }
        }

        private void refreshValues()
        {
            foreach(ListViewItem lvi in listView1.Items)
            {
                var watch = lvi.Tag as WatchItem;
                string val = string.Empty;
                try
                {
                    val = watch.EvaluateExpression();
                } catch
                {
                    (queryview(ViewEnums.OutputT) as OutputView).WriteLine("Error evaluating expression:" + watch.Expression);
                    continue;
                }
                lvi.SubItems[2].Text = val;
            }
        }

        public void RefreshView()
        {
            //refreshValues();
        }

        public void ResetView()
        {
        }

        public void Start()
        {
        }

        public void Stop() { }

        private static ListViewItem clickedItem(ListView lv, int x, int y)
        {
            foreach (ListViewItem item in lv.Items)
            {
                if (item.Bounds.Contains(x, y))
                    return item;
            }
            return null;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip mcs = sender as ContextMenuStrip;
            ListViewItem lvi = clickedItem(listView1, mMouseX, mMouseY);
            mcs.Items[1].Enabled = (lvi != null);
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            mMouseX = e.X;
            mMouseY = e.Y;
        }

        private void updateConfig()
        {
            if (mWatchConfig.WatchList == null)
                mWatchConfig.WatchList = new List<WatchItem>();
            mWatchConfig.WatchList.Clear();
            foreach(ListViewItem item in listView1.Items)
                mWatchConfig.WatchList.Add(item.Tag as WatchItem);
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var item = e.ClickedItem;
            if(item.Text == "Add")
            {
                var dialog = new AddWatchItem(mRegisters, mProcessor);
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    var watch = dialog.WatchItem;
                    watch.Processor = mProcessor;
                    watch.Registers = mRegisters;

                    ListViewItem lvi = new ListViewItem(watch.Expression);
                    lvi.SubItems.Add(watch.WordSize.ToString());
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add(watch.Name);
                    lvi.Tag = watch;
                    listView1.Items.Add(lvi);
                    updateConfig();
                }
            }
            else if (item.Text == "Delete")
            {
                ListViewItem lvi = clickedItem(listView1, mMouseX, mMouseY);
                if (lvi == null)
                    return;
                if(MessageBox.Show("Delete watch item?", "Delete!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    listView1.Items.Remove(lvi);
                    updateConfig();
                }
            }
        }
    }
}