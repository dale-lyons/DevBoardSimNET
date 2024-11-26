using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Drawing;

namespace Preferences
{
    public class IOSwitchEditor : UITypeEditor
    {
        //private string orgActiveBoard;
        public static IList<string> mBoards;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Indicates that this editor can display a control-based 
            // drop-down interface.
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            // Attempts to obtain an IWindowsFormsEditorService.
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc == null)
                return value;

            var dialog = new DipSwitchEditor(value);
            if (edSvc.ShowDialog(dialog) == DialogResult.OK)
                value = dialog.FinalDipValue;

            return value;
        }
    }

    //public class DipSwitchEditor : Form
    //{
    //    private Image image = Image.FromFile(@"C:\Projects\DevBoardSimNET\Preferences\dip-switch-9494489.jpg");

    //}
}