using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    public class DeepSkyRenderer : BaseSkyRenderer
    {
        private const double maxSeparation = 90 * 1.2;

        public DeepSkyRenderer(Sky sky, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            
        }

        public override void Render(Graphics g)
        {
            var allDeepSkies = Sky.Get<ICollection<DeepSky>>("DeepSky");

            var deepSkies = allDeepSkies.Where(ds => Angle.Separation(Map.Center, ds.Horizontal) < Map.ViewAngle * 1.2);
            foreach (var ds in deepSkies)
            {
                switch (ds.Status)
                {
                    case DeepSkyStatus.Galaxy:
                        DrawGalaxy(g, ds);
                        break;

                    // TODO: other deep sky objects types

                    default:
                        break;
                }
            }
        }

        private void DrawGalaxy(Graphics g, DeepSky ds)
        {
            PointF p = Map.Projection.Project(ds.Horizontal);
            float sizeA = GetDiameter(ds.SizeA);
            float sizeB = GetDiameter(ds.SizeB);

            // elliptic object wuth known size
            if (sizeB > 0)
            {
                float diamA = GetDiameter(ds.SizeA);
                if (diamA > 10)
                {
                    float diamB = GetDiameter(ds.SizeB);

                    float rotation = GetRotationTowardsNorth(ds.Equatorial) + 90 - ds.PA;
                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(rotation);
                    g.DrawEllipse(Pens.Gray, -diamA / 2, -diamB / 2, diamA, diamB);
                    g.ResetTransform();
                    Map.VisibleObjects.Add(ds);
                }
            }
            // round object
            else if (sizeA > 0)
            {
                float diamA = GetDiameter(ds.SizeA);
                if (diamA > 10)
                {
                    g.TranslateTransform(p.X, p.Y);
                    g.DrawEllipse(Pens.Gray, -diamA / 2, -diamA / 2, diamA, diamA);
                    g.ResetTransform();
                    Map.VisibleObjects.Add(ds);
                }
            }
            // point object
            else
            {
                g.TranslateTransform(p.X, p.Y);
                g.DrawEllipse(Pens.Gray, -5, -5, 10f, 10f);
                Map.VisibleObjects.Add(ds);
            }
        }

        private float GetDiameter(double diam)
        {
            return (float)(diam / 60 / Map.ViewAngle * Map.Width / 2);
        }
    }
}
