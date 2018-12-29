using ADK.Demo.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    public partial class FormSettings : Form
    {
        public Settings Settings { get; private set; }

        [DesignOnly(true)]
        public FormSettings()
        {
            InitializeComponent();
        }

        public FormSettings(Settings settings)
        {
            InitializeComponent();
            Settings = settings;            
            settingsControl.Settings = settings;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Settings.Save();            
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Settings.Load();
            DialogResult = DialogResult.Cancel;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Do you really want to reset settings to default values?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                Settings.Reset();
            }
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Settings.IsChanged)
            {
                var result = MessageBox.Show("You have unsaved changes in program options. Do you want to apply them?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Yes:
                        btnSave_Click(sender, e);
                        break;
                    case DialogResult.No:
                        btnCancel_Click(sender, e);
                        break;
                    default:
                        e.Cancel = true;
                        break;                    
                }
            }
        }


    }
}
