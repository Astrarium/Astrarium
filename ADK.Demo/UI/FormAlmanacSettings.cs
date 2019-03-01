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
    public partial class FormAlmanacSettings : Form
    {
        public FormAlmanacSettings(double jd, double utcOffset, ICollection<string> categories)
        {
            InitializeComponent();

            dtFrom.JulianDay = jd;
            dtFrom.UtcOffset = utcOffset;
            dtTo.JulianDay = jd + 30;
            dtTo.UtcOffset = utcOffset;

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

                lstCategories.Nodes.Add(node);
            }

            //lstCategories.Nodes.Add(root);
            lstCategories.ExpandAll();
        }

        private void lstCategories_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //bool check = !e.Node.Checked;
            //e.Node.Checked = check;
        }

        private void lstCategories_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void lstCategories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // The code only executes if the user caused the checked state to change.
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    CheckAllChildNodes(e.Node, e.Node.Checked);
                }
            }
        }

        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.Checked = nodeChecked;
                if (node.Nodes.Count > 0)
                {
                    CheckAllChildNodes(node, nodeChecked);
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

        }

        private IEnumerable<TreeNode> GetLeafs(TreeNode node)
        {
            if (node.Nodes.Count == 0)
            {
                yield return node;
            }
            else
            {
                foreach (TreeNode child in node.Nodes)
                {
                    foreach (var subchild in GetLeafs(child))
                    {
                        yield return subchild;
                    }
                }
            }
        }

        public double JulianDayFrom
        {
            get
            {
                return dtFrom.JulianDay;
            }
        }

        public double JulianDayTo
        {
            get
            {
                return dtTo.JulianDay;
            }
        }

        public ICollection<string> Categories
        {
            get
            {
                List<TreeNode> nodes = new List<TreeNode>();
                foreach (TreeNode node in lstCategories.Nodes)
                {
                    nodes.AddRange(GetLeafs(node));
                }

                return nodes.Where(n => n.Checked).Select(n => n.Name).ToArray();
            }
        }
    }
}
