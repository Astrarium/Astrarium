using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Renderers
{
    /// <summary>
    /// Base class for all renderer classes which implement drawing logic of sky map.
    /// </summary>
    public abstract class BaseRenderer
    {
        public abstract void Render(IMapContext map);
        public virtual void Initialize() { }
        public virtual int ZOrder => 0;
    }
}
