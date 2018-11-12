using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class StarsRenderer : BaseSkyRenderer
    {
        public StarsRenderer(Sky sky, ISkyMap skyMap) : base(sky, skyMap)
        {

        }

        public override void Render(Graphics g)
        {
            var stars = Sky.Objects.OfType<Star>().Where(s => Angle.Separation(Map.Center, s.Horizontal) < Map.ViewAngle * 1.2);

            foreach (var star in stars)
            {
                float diam = GetDrawingSize(star.Mag);
                if ((int)diam > 0)
                {
                    PointF p = Map.Projection.Project(star.Horizontal);
                    g.FillEllipse(Brushes.White, p.X - diam / 2, p.Y - diam / 2, diam, diam);
                }
            }
        }


        // TODO: refactor this
        private float GetDrawingSize(float mag)
        {
            double maxMag = 0;
            double minMag = 5.0;

            
            minMag = 5.0;
            if (Map.ViewAngle <= 90) minMag = 5.0;
            if (Map.ViewAngle <= 70) minMag = 5.5;
            if (Map.ViewAngle < 50) minMag = 6.0;
            if (Map.ViewAngle < 30) minMag = 6.5;
            if (Map.ViewAngle < 20) minMag = 7.0;
            if (Map.ViewAngle < 15) minMag = 7.5;
            if (Map.ViewAngle < 10) minMag = 8.0;
            if (Map.ViewAngle < 5) minMag = 9.0;
            if (Map.ViewAngle < 2) minMag = 10.0;


            double d = 1;
            if (Map.ViewAngle < 2 && mag > minMag) return 1;
            if (mag > minMag) return 0;
            if (mag <= maxMag) return (float)(d * minMag - maxMag);

            float diam = (float)(d * minMag - mag);

            if (Map.ViewAngle < 2 && diam < 1) return 1;

            return diam;
        }
    }
}
