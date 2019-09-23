using ADK;
using Planetarium.Renderers;
using Planetarium.Types;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.Tycho2
{
    public class Tycho2Renderer : BaseRenderer
    {
        private Font fontNames;
        private Brush brushNames;
        private readonly ITycho2Catalog tycho2;
        private readonly ISettings settings;

        public Tycho2Renderer(ITycho2Catalog tycho2, ISettings settings)
        {
            this.tycho2 = tycho2;
            this.settings = settings;

            fontNames = new Font("Arial", 6);
            brushNames = new SolidBrush(Color.FromArgb(64, 64, 64));
        }

        private bool MagFilter(IMapContext map, float mag)
        {
            if (mag > map.MagLimit)
                return false;

            if ((int)map.GetPointSize(mag) == 0)
                return false;

            return true;
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;            
            bool isGround = settings.Get<bool>("Ground");
            double coeff = map.DiagonalCoefficient();

            if (map.MagLimit > 8 && settings.Get<bool>("Stars") && settings.Get<bool>("Tycho2"))
            {
                PrecessionalElements pe = Precession.ElementsFK5(map.JulianDay, Date.EPOCH_J2000);

                var eq0 = map.Center.ToEquatorial(map.GeoLocation, map.SiderealTime);

                CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, pe);

                SkyContext context = new SkyContext(map.JulianDay, map.GeoLocation);

                tycho2.LockedStar = map.LockedObject as Tycho2Star;

                var stars = tycho2.GetStars(context, eq, map.ViewAngle * coeff, m => MagFilter(map, m));

                foreach (var star in stars)
                {
                    if (!isGround || star.Horizontal.Altitude > 0)
                    {
                        PointF p = map.Project(star.Horizontal);
                        if (!map.IsOutOfScreen(p))
                        {
                            float size = map.GetPointSize(star.Magnitude);

                            g.FillEllipse(Brushes.White, p.X - size / 2, p.Y - size / 2, size, size);

                            if (map.ViewAngle < 1 && size > 3)
                            {
                                map.DrawObjectCaption(fontNames, brushNames, star.ToString(), p, size);
                            }
                            map.AddDrawnObject(star, p);
                        }
                    }
                }
            }
        }

        public override RendererOrder Order => RendererOrder.Stars;

        public override string Name => "Tycho 2 Star Catalogue";
    }
}
