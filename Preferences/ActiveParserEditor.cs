using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Drawing.Design;
using System.ComponentModel;

namespace Preferences
{
    public class ActiveParserEditor : UITypeEditor
    {
        private string orgActiveParser;
        public static List<string> mParsers;

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
            foreach (var board in mParsers)
                bx.Items.Add(board);

            bx.SelectedItem = value as string;
            orgActiveParser = value as string;

            // Displays a drop-down control.
            edSvc.DropDownControl(bx);
            return bx.SelectedItem;
        }

        private void Bx_SelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(orgActiveParser))
                return;
            var bx = sender as ListBox;
            var serv = bx.Tag as IWindowsFormsEditorService;
            if (bx.SelectedItem as string != orgActiveParser)
                serv.CloseDropDown();
        }
    }
}