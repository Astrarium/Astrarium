using ADK.Demo.Objects;
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
    public partial class FormSearch : Form
    {
        private SearchDelegate SearchDelegate;

        public FormSearch(SearchDelegate searchDelegate)
        {
            InitializeComponent();
            SearchDelegate = searchDelegate;
        }

        public CelestialObject SelectedObject { get; private set; }

        private void txtSearchString_TextChanged(object sender, EventArgs e)
        {
            var results = SearchDelegate(txtSearchString.Text, 50);
            lstResults.BeginUpdate();
            lstResults.Items.Clear();
            foreach (var result in results)
            {
                lstResults.Items.Add(new ListViewItem(result.Name) { Tag = result.Body });
            }
            lstResults.EndUpdate();
        }

        private void lstResults_DoubleClick(object sender, EventArgs e)
        {
            if (lstResults.SelectedItems.Count > 0)
            {
                SelectedObject = lstResults.SelectedItems[0].Tag as CelestialObject;
                DialogResult = DialogResult.OK;
            }
        }

        private void FormSearch_ResizeEnd(object sender, EventArgs e)
        {
            colNames.Width = -2;
        }
    }
}
