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
    public partial class FormEphemerisSettings : Form
    {
        public FormEphemerisSettings(Sky sky, CelestialObject body)
        {
            InitializeComponent();

            dtFrom.JulianDay = sky.Context.JulianDay;
            dtFrom.UtcOffset = sky.Context.GeoLocation.UtcOffset;
            dtTo.JulianDay = sky.Context.JulianDay + 30;
            dtTo.UtcOffset = sky.Context.GeoLocation.UtcOffset;
            selCelestialBody.Searcher = sky;
            selCelestialBody.ValueChanged += () => BuildCategoriesTree(sky.GetEphemerisCategories(selCelestialBody.SelectedObject));
            selCelestialBody.SelectedObject = body;
        }

        private void BuildCategoriesTree(ICollection<string> categories)
        {
            lstCategories.Nodes.Clear();

            var groups = categories.GroupBy(cat => cat.Split('.').First());

            TreeNode root = new TreeNode("All");

            foreach (var group in groups)
            {
                TreeNode node = new TreeNode(group.Key) { Name = group.Key };

                if (group.Count() > 1)
                {
                    foreach (var item in group)
                    {
                        node.Nodes.Add(key: item, text: item);
                    }
                }

                root.Nodes.Add(node);
            }

            lstCategories.Nodes.Add(root);
            lstCategories.ExpandAll();

            root.Checked = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public CelestialObject SelectedObject
        {
            get
            {
                return selCelestialBody.SelectedObject;
            }
        }

        /// <summary>
        /// Gets starting Julian Day for generating ephemeris
        /// </summary>
        public double JulianDayFrom
        {
            get
            {
                return dtFrom.JulianDay;
            }
        }

        /// <summary>
        /// Gets finishing Julian Day for generating ephemeris
        /// </summary>
        public double JulianDayTo
        {
            get
            {
                return dtTo.JulianDay;
            }
        }

        /// <summary>
        /// Gets step, in days, for generating ephemeris
        /// </summary>
        public double Step
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets collection of phenomena categories to be included in almanac
        /// </summary>
        public ICollection<string> Categories
        {
            get
            {
                return lstCategories.LeafNodes.Where(n => n.Checked).Select(n => n.Name).ToArray();
            }
        }

        private void lstCategories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            btnOK.Enabled = Categories.Any();
        }
    }
}
