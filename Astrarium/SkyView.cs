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

        /// <inheritdoc />
        protected override bool RenderOnMainThread => true;

        /// <inheritdoc />
        protected override bool UseSpecificOpenGLVersion => true;

        /// <inheritdoc />
        protected override uint MajorOpenGLVersion => 3;

        /// <inheritdoc />
        protected override uint MinorOpenGLVersion => 0;
    }
}
