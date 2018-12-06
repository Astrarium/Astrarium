using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    public partial class FormSettings : Form
    {
        public FormSettings(ISettings settings)
        {
            InitializeComponent();
            settingsControl.Settings = settings;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            settingsControl.Settings.Save();
            DialogResult = DialogResult.OK;
        }
    }
}
