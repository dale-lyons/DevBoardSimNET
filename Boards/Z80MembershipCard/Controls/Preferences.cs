using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z80MembershipCard.Controls
{
    public partial class Preferences : Form
    {
        public bool ResetSimulation { get; set; }

        private Z80MembershipConfig mZ80MembershipConfig;
        public Preferences(Z80MembershipConfig z80MembershipConfig)
        {
            mZ80MembershipConfig = z80MembershipConfig;
            InitializeComponent();

            propertyGrid1.SelectedObject = z80MembershipConfig;
            propertyGrid1.PropertyValueChanged += PropertyGrid1_PropertyValueChanged;

        }

        private void PropertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "ActiveBoard")
            {
                ResetSimulation = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}