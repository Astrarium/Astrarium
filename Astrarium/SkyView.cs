using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Astrarium
{
    [DesignerCategory("code")]
    public class SkyView : GLControl
    {
        public SkyView() : base()
        {
            Cursor = Cursors.Cross;
        }

        protected override bool RenderOnMainThread => true;
    }
}
