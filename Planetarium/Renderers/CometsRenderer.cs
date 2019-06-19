using ADK;
using Planetarium.Calculators;
using Planetarium.Config;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Renderers
{
    public class CometsRenderer : BaseRenderer
    {
        private Font fontNames;
        private Brush brushNames;
        private readonly ICometsProvider cometsProvider;
        private readonly ISettings settings;

        public CometsRenderer(ICometsProvider cometsProvider, ISettings settings)
        {
            this.cometsProvider = cometsProvider;
            this.settings = settings;

            fontNames = new Font("Arial", 8);
            brushNames = new SolidBrush(Color.FromArgb(10, 44, 37));
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allComets = cometsProvider.Comets;
            bool isGround = settings.Get<bool>("Ground");
            bool useTextures = settings.Get<bool>("UseTextures");
            double coeff = map.DiagonalCoefficient();

            //if (settings.Get<bool>("Comets"))
            {
                var comets = allComets.Where(a => Angle.Separation(map.Center, a.Horizontal) < map.ViewAngle * coeff);

                foreach (var c in comets)
                {
                    double ad = Angle.Separation(c.Horizontal, map.Center);

                    if ((!isGround || c.Horizontal.Altitude + c.Semidiameter / 3600 > 0) &&
                        ad < coeff * map.ViewAngle + c.Semidiameter / 3600)
                    {
                        float diam = map.GetDiskSize(c.Semidiameter);

                        if (diam > 5)
                        {
                            PointF p = map.Project(c.Horizontal);

                            g.FillEllipse(Brushes.Azure, p.X - diam / 2, p.Y - diam / 2, diam, diam);
                        }
                        else
                        {
                            // comet should be rendered as point
                            float size = map.GetPointSize(c.Magnitude, maxDrawingSize: 3);
                            if ((int)size > 0)
                            {
                                PointF p = map.Project(c.Horizontal);

                                if (!map.IsOutOfScreen(p))
                                {
                                    g.FillEllipse(Brushes.White, p.X - size / 2, p.Y - size / 2, size, size);
                                    map.DrawObjectCaption(fontNames, brushNames, c.Name, p, size);
                                    map.AddDrawnObject(c, p);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }
       
        public override int ZOrder => 699;
    }
}
