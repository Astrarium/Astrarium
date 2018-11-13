using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    /// <summary>
    /// Base class for all renderer classes which implement drawing logic of sky map.
    /// </summary>
    public abstract class BaseSkyRenderer
    {
        protected Sky Sky { get; private set; }
        protected ISkyMap Map { get; private set; }

        public BaseSkyRenderer(Sky sky, ISkyMap skyMap)
        {
            Sky = sky;
            Map = skyMap;
        }

        public abstract void Render(Graphics g);

        public virtual void Initialize() { }
    }
}
