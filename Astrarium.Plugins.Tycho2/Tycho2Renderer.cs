using Astrarium.Algorithms;
using Astrarium.Renderers;
using Astrarium.Types;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tycho2
{
    public class Tycho2Renderer : BaseRenderer
    {
        private Font fontNames;
        private readonly ITycho2Catalog tycho2;
        private readonly ISettings settings;

        public Tycho2Renderer(ITycho2Catalog tycho2, ISettings settings)
        {
            this.tycho2 = tycho2;
            this.settings = settings;

            fontNames = new Font("Arial", 6);
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
            Brush brushNames = new SolidBrush(map.GetColor("ColorStarsLabels"));

            if (map.MagLimit > 8 && settings.Get<bool>("Stars") && settings.Get<bool>("Tycho2"))
            {
                Brush brushStar = GetColor(map);

                PrecessionalElements pe = Precession.ElementsFK5(map.JulianDay, Date.EPOCH_J2000);

                var eq0 = map.Center.ToEquatorial(map.GeoLocation, map.SiderealTime);

                CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, pe);

                SkyContext context = new SkyContext(map.JulianDay, map.GeoLocation);

                tycho2.LockedStar = map.LockedObject as Tycho2Star;
                tycho2.SelectedStar = map.SelectedObject as Tycho2Star;

                var stars = tycho2.GetStars(context, eq, map.ViewAngle * coeff, m => MagFilter(map, m));

                foreach (var star in stars)
                {
                    if (!isGround || star.Horizontal.Altitude > 0)
                    {
                        PointF p = map.Project(star.Horizontal);
                        if (!map.IsOutOfScreen(p))
                        {
                            float size = map.GetPointSize(star.Magnitude);

                            if (map.Schema == ColorSchema.White)
                            {
                                g.FillEllipse(Brushes.White, p.X - size / 2 - 1, p.Y - size / 2 - 1, size + 2, size + 2);
                            }
                            g.FillEllipse(brushStar, p.X - size / 2, p.Y - size / 2, size, size);

                            if (map.ViewAngle < 1 && size > 3)
                            {
                                map.DrawObjectCaption(fontNames, brushNames, star.ToString(), p, size);
                            }
                            map.AddDrawnObject(star);
                        }
                    }
                }
            }
        }

        public override RendererOrder Order => RendererOrder.Stars;

        private Brush GetColor(IMapContext map)
        {
            switch (map.Schema)
            {
                default:
                case ColorSchema.Night:
                    return Brushes.White;
                case ColorSchema.Red:
                    return Brushes.DarkRed;
                case ColorSchema.White:
                    return Brushes.Black;
            }            
        }
    }
}
