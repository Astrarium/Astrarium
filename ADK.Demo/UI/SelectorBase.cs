using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
#if DEBUG
    public partial class SelectorBase : UserControl
#else
    public abstract partial class SelectorBase : UserControl
#endif
    {
        public event EventHandler SelectorClicked;

        public SelectorBase()
        {
            InitializeComponent();
        }

        private void Selector_Resize(object sender, EventArgs e)
        {
            btnButton.Left = 0;
            btnButton.Top = 0;
            btnButton.Width = this.Width;
            this.Height = lblText.Height = 22;
            lblText.Width = this.Width;

            btnButton.Width = btnButton.Height = lblText.Height - 2;
            btnButton.Top = 1;
            btnButton.Left = lblText.Width - btnButton.Width - 1;
        }

        private void btnButton_Enter(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void btnButton_Click(object sender, EventArgs e)
        {
            if (SelectorClicked != null)
            {
                SelectorClicked.Invoke(this, new EventArgs());
            }
        }

        private void lblText_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (SelectorClicked != null)
            {
                SelectorClicked.Invoke(this, new EventArgs());
            }
        }

    }
}
