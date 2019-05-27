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
    public class AsteroidsRenderer : BaseRenderer
    {
        private Font fontNames;
        private readonly IAsteroidsProvider asteroidsProvider;
        private readonly ISettings settings;

        public AsteroidsRenderer(IAsteroidsProvider asteroidsProvider, ISettings settings)
        {
            this.asteroidsProvider = asteroidsProvider;
            this.settings = settings;

            fontNames = new Font("Arial", 8);
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allAsteroids = asteroidsProvider.Asteroids;
            bool isGround = settings.Get<bool>("Ground");
            double coeff = map.DiagonalCoefficient();

            if (settings.Get<bool>("Stars"))
            {
                var asteroids = allAsteroids.Where(a => Angle.Separation(map.Center, a.Horizontal) < map.ViewAngle * coeff);
                if (isGround)
                {
                    asteroids = asteroids.Where(a => a.Horizontal.Altitude >= 0);
                }

                foreach (var a in asteroids)
                {
                    float diam = map.GetPointSize(a.Magnitude);
                    if ((int)diam > 0)
                    {
                        PointF p = map.Project(a.Horizontal);
                        if (!map.IsOutOfScreen(p))
                        {
                            g.FillEllipse(Brushes.White, p.X - diam / 2, p.Y - diam / 2, diam, diam);
                            map.DrawObjectCaption(fontNames, Brushes.Gray, a.Name, p, diam);
                            map.AddDrawnObject(a, p);
                        }
                    }
                }

                
            }
        }
       
        public override int ZOrder => 699;
    }
}
