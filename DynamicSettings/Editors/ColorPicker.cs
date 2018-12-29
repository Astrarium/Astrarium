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
    public partial class ColorPicker : UserControl
    {
        private ColorDialog colorDialog = new ColorDialog();

        public ColorPicker()
        {
            InitializeComponent();
        }

        public override string Text
        {
            get { return lblCaption.Text; }
            set { lblCaption.Text = value; }
        }

        public Color SelectedValue
        {
            get { return btnPicker.BackColor; }
            set { btnPicker.BackColor = value; }
        }

        public event EventHandler SelectedValueChanged;

        private void btnPicker_Click(object sender, EventArgs e)
        {
            colorDialog.Color = SelectedValue;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                SelectedValue = colorDialog.Color;
                SelectedValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
