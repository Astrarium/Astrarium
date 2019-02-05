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
    [DesignerCategory("code")]
    public class CustomListView : ListView
    {
        [Description("Gets or sets text to be displayed when the list has no items.")]
        public string EmptyText { get; set; }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0xF)
            {
                if (Items.Count == 0)
                {
                    using (var g = Graphics.FromHwnd(Handle))
                    {
                        g.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
                        TextRenderer.DrawText(g, EmptyText, Font, ClientRectangle, ForeColor);
                    }
                }
            }
        }
    }
}
