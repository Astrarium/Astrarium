using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    /// <summary>
    /// Extends the default <see cref="TreeView"/> control with <see cref="CheckOnDoubleClick"/> property.
    /// Also fixes the WinForms contol issue described here: https://stackoverflow.com/questions/3174412/winforms-treeview-recursively-check-child-nodes-problem.
    /// </summary>
    [DesignerCategory("code")]
    public class TreeViewEx : TreeView
    {
        /// <summary>
        /// Creates new instance of the TreeViewEx.
        /// </summary>
        public TreeViewEx()
        {
            BeforeCollapse += OnBeforeCollapse;
        }

        private Point mouseOverLocation;

        protected override void WndProc(ref Message m)
        {
            // Filter WM_LBUTTONDBLCLK
            if (m.Msg != 0x203)
            {
                base.WndProc(ref m);
            }
            else
            {
                var hitTestInfo = HitTest(mouseOverLocation);
                TreeNode node = hitTestInfo.Node;
                if (node != null)
                {
                    if (CheckOnDoubleClick)
                    {
                        node.Checked = !node.Checked;
                    }

                    OnNodeMouseDoubleClick(new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 2, mouseOverLocation.X, mouseOverLocation.Y));
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mouseOverLocation = e.Location;
        }

        protected override void OnAfterCheck(TreeViewEventArgs e)
        {
            base.OnAfterCheck(e);
            if (AutoCheckChildNodes)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    CheckAllChildNodes(e.Node, e.Node.Checked);
                }
            }
        }

        private void OnBeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (!AllowCollapse)
            {
                e.Cancel = true;
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

        private IEnumerable<TreeNode> GetLeafNodes(TreeNode node)
        {
            if (node.Nodes.Count == 0)
            {
                yield return node;
            }
            else
            {
                foreach (TreeNode child in node.Nodes)
                {
                    foreach (var subchild in GetLeafNodes(child))
                    {
                        yield return subchild;
                    }
                }
            }
        }

        /// <summary>
        /// Gets leaf nodes, i.e. nodes without subnodes.
        /// </summary>
        [Browsable(false)]        
        public IEnumerable<TreeNode> LeafNodes
        {
            get
            {
                List<TreeNode> leafs = new List<TreeNode>();
                foreach (TreeNode node in Nodes)
                {
                    leafs.AddRange(GetLeafNodes(node));
                }
                return leafs;
            }
        }

        /// <summary>
        /// If true, node will be checked/unchecked automatically on double click.
        /// </summary>
        [Description("If true, node will be checked/unchecked automatically on double click.")]
        [DefaultValue(true)]
        public bool CheckOnDoubleClick { get; set; } = true;

        /// <summary>
        /// If false, treeview can not be collapsed via user action.
        /// </summary>
        [Description("If false, treeview can not be collapsed via user action.")]
        [DefaultValue(true)]
        public bool AllowCollapse { get; set; } = true;

        /// <summary>
        /// If true, check state of child nodes will be changed automatically on changing state of parent node.
        /// </summary>
        [Description("If true, check state of child nodes will be changed automatically on changing state of parent node.")]
        [DefaultValue(true)]
        public bool AutoCheckChildNodes { get; set; } = true;
    }
}
