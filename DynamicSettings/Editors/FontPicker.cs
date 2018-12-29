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
    public partial class FontPicker : UserControl
    {
        public FontPicker()
        {
            InitializeComponent();
        }

        public event EventHandler SelectedValueChanged;

        public string Title
        {
            get { return lblTitle.Text; }
            set { lblTitle.Text = value; }
        }

        private Font _SelectedValue = SystemFonts.DefaultFont;
        public Font SelectedValue
        {
            get { return _SelectedValue; }
            set
            {
                _SelectedValue = value;
                txtFont.Text = $"{_SelectedValue.Name}, {_SelectedValue.Size}, {_SelectedValue.Style}";
                SelectedValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void btnPicker_Click(object sender, EventArgs e)
        {
            FontDialog dialog = new FontDialog()
            {
                Font = SelectedValue
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SelectedValue = dialog.Font;
            }
        }
    }
}
