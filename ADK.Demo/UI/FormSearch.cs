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
        private ISearcher Searcher;
        private Func<CelestialObject, bool> Filter;

        public FormSearch(ISearcher searcher, Func<CelestialObject, bool> filter)
        {
            InitializeComponent();
            Searcher = searcher;
            Filter = filter;
        }

        /// <summary>
        /// Gets selected celestial object
        /// </summary>
        public CelestialObject SelectedObject { get; private set; }

        private async void txtSearchString_TextChanged(object sender, EventArgs e)
        {
            var results = await Task.Run(() => Searcher.Search(txtSearchString.Text, Filter));
            lstResults.BeginUpdate();
            lstResults.Items.Clear();
            foreach (var result in results)
            {
                lstResults.Items.Add(new ListViewItem(result.Name) { Tag = result.Body });
            }

            if (lstResults.Items.Count > 0)
            {
                lstResults.Items[0].Selected = true;
            }

            lstResults_SelectedIndexChanged(sender, e);

            lstResults.EndUpdate();
        }

        private void OnObjectSelected(object sender, EventArgs e)
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

        private void txtSearchString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OnObjectSelected(sender, e);
            }  
            else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = true;

                if (lstResults.Items.Count > 1)
                {
                    ListViewItem item = null;
                    if (e.KeyCode == Keys.Up && lstResults.SelectedIndices[0] > 0 )
                    {
                        item = lstResults.Items[lstResults.SelectedIndices[0] - 1];
                    }
                    else if (e.KeyCode == Keys.Down && lstResults.SelectedIndices[0] < lstResults.Items.Count - 1)
                    {
                        item = lstResults.Items[lstResults.SelectedIndices[0] + 1];
                    }

                    if (item != null)
                    {
                        item.Selected = true;
                        item.EnsureVisible();
                    }
                }
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            OnObjectSelected(sender, EventArgs.Empty);
        }

        private void lstResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSelect.Enabled = lstResults.Items.Count > 0 && lstResults.SelectedItems.Count > 0;
        }
    }
}
