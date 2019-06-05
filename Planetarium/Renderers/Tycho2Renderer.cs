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
            if (map.ViewAngle < 3)
                return true;

            if (mag > map.MagLimit())
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

            if (map.ViewAngle <= 15 && settings.Get<bool>("Stars"))
            {
                PrecessionalElements pe = Precession.ElementsFK5(map.JulianDay, Date.EPOCH_J2000);

                var eqCenter0 = map.Center.ToEquatorial(map.GeoLocation, map.SiderealTime);

                CrdsEquatorial eqCenter = Precession.GetEquatorialCoordinates(eqCenter0, pe);

                SkyContext context = new SkyContext(map.JulianDay, map.GeoLocation);

                var stars = tycho2.GetStars(context, eqCenter, map.ViewAngle * coeff, m => MagFilter(map, m));
             
                foreach (var star in stars)
                {
                    if (!isGround || star.Horizontal.Altitude > 0)
                    {
                        PointF p = map.Project(star.Horizontal);
                        if (!map.IsOutOfScreen(p))
                        {
                            float size = map.GetPointSize(star.Magnitude);

                            g.FillEllipse(Brushes.White, p.X - size / 2, p.Y - size / 2, size, size);

                            if (map.ViewAngle <= 1)
                            {
                                map.DrawObjectCaption(fontNames, brushNames, star.ToString(), p, size);
                            }
                            map.AddDrawnObject(star, p);
                        }
                    }
                }
            }
        }
       
        public override int ZOrder => 599;
    }
}
