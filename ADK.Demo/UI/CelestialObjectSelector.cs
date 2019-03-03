using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADK.Demo.Objects;

namespace ADK.Demo.UI
{
    public partial class CelestialObjectSelector : SelectorBase
    {
        public CelestialObjectSelector()
        {
            InitializeComponent();
            SelectorClicked += CelestialObjectSelector_SelectorClicked;
        }

        private void CelestialObjectSelector_SelectorClicked(object sender, EventArgs e)
        {
            FormSearch frmSearch = new FormSearch(Searcher);
            if (frmSearch.ShowDialog() == DialogResult.OK)
            {
                SelectedObject = frmSearch.SelectedObject;
            }
        }

        [Browsable(false)]
        public ISearcher Searcher { get; set; }

        private CelestialObject _SelectedObject = null;
        [Browsable(false)]
        public CelestialObject SelectedObject
        {
            get { return _SelectedObject; }
            set
            {
                _SelectedObject = value;
                lblText.Text = Text;
                ValueChanged?.Invoke();
            }
        }

        [Browsable(false)]
        public override string Text
        {
            get
            {
                return SelectedObject != null ? Searcher.GetObjectName(SelectedObject) : "(Not selected)";
            }
        }

        public event Action ValueChanged;
    }
}
