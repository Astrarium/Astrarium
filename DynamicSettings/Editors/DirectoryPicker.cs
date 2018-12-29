using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicSettings.Editors
{
    public partial class DirectoryPicker : UserControl
    {
        public DirectoryPicker()
        {
            InitializeComponent();
        }

        public event EventHandler SelectedValueChanged;

        public string Title
        {
            get { return lblTitle.Text; }
            set { lblTitle.Text = value; }
        }

        private string _SelectedValue = null;
        public string SelectedValue
        {
            get { return _SelectedValue; }
            set
            {
                _SelectedValue = value;
                txtDirectory.Text =_SelectedValue;
                SelectedValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void btnPicker_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                SelectedPath = SelectedValue
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SelectedValue = dialog.SelectedPath;
            }
        }
    }
}
