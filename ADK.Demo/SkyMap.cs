using ADK.Demo.Projections;
using ADK.Demo.Renderers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class SkyMap : ISkyMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double ViewAngle { get; set; } = 90;
        public CrdsHorizontal Center { get; set; } = new CrdsHorizontal(0, 0);
        public bool Antialias { get; set; } = true;
        public ICollection<BaseSkyRenderer> Renderers { get; private set; } = new List<BaseSkyRenderer>();
        public IProjection Projection { get; set; } = null;

        public SkyMap()
        {
            Projection = new ArcProjection(this);
        }

        public void Render(Graphics g)
        {
            g.PageUnit = GraphicsUnit.Display;
            g.SmoothingMode = Antialias ? SmoothingMode.HighQuality : SmoothingMode.HighSpeed;

            foreach (var renderer in Renderers)
            {
                renderer.Render(g);
            }

            g.DrawString(Center.ToString(), SystemFonts.DefaultFont, Brushes.Red, 10, 10);
        }

        public void Initialize()
        {
            foreach (var renderer in Renderers)
            {
                renderer.Initialize();
            }
        }
    }

    
}
