using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;

using System.IO.Ports;

namespace Preferences
{
    public class ComPortEditor : UITypeEditor
    {
        private string orgComPort;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Indicates that this editor can display a control-based 
            // drop-down interface.
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            // Attempts to obtain an IWindowsFormsEditorService.
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc == null)
                return value;

            ListBox bx = new ListBox();
            bx.Tag = edSvc;
            bx.SelectedValueChanged += Bx_SelectedValueChanged;
            //foreach (var board in Boards.Boards.AvailableBoards)
            //    bx.Items.Add(board.FullName);
            var ports = SerialPort.GetPortNames();
            bx.Items.Add("<None>");
            foreach (var port in ports)
                bx.Items.Add(port);

            bx.SelectedItem = value as string;
            orgComPort = value as string;

            // Displays a drop-down control.
            edSvc.DropDownControl(bx);
            return bx.SelectedItem;
        }

        private void Bx_SelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(orgComPort))
                return;

            var bx = sender as ListBox;
            var serv = bx.Tag as IWindowsFormsEditorService;
            if (bx.SelectedItem as string != orgComPort)
                serv.CloseDropDown();
        }

    }
}