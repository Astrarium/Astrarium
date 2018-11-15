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
    public class StarsRenderer : BaseSkyRenderer
    {
        private ICollection<Tuple<int, int>> ConLines = new List<Tuple<int, int>>();

        private Pen penConLine;
        private float magLimit = 100;
        private const double maxSeparation = 90 * 1.2;

        public StarsRenderer(Sky sky, ISkyMap skyMap) : base(sky, skyMap)
        {
            penConLine = new Pen(new SolidBrush(Color.FromArgb(64, 64, 64)));
            penConLine.DashStyle = DashStyle.Dot;
        }

        public override void Render(Graphics g)
        {
            var allStars = Sky.Get<ICollection<Star>>("Stars");

            magLimit = allStars.Select(s => s.Mag).Max();

            PointF p1, p2;
            CrdsHorizontal h1, h2;

            foreach (var line in ConLines)
            {
                h1 = allStars.ElementAt(line.Item1).Horizontal;
                h2 = allStars.ElementAt(line.Item2).Horizontal;

                if (Angle.Separation(Map.Center, h1) < maxSeparation &&
                    Angle.Separation(Map.Center, h2) < maxSeparation)
                {
                    p1 = Map.Projection.Project(h1);
                    p2 = Map.Projection.Project(h2);

                    var points = Geometry.SegmentRectangleIntersection(p1, p2, Map.Width, Map.Height);
                    if (points.Length == 2)
                    {
                        g.DrawLine(penConLine, points[0], points[1]);
                    }
                }
            }

            var stars = allStars.Where(s => Angle.Separation(Map.Center, s.Horizontal) < Map.ViewAngle * 1.2);
            foreach (var star in stars)
            {
                float diam = GetDrawingSize(star.Mag);
                if ((int)diam > 0)
                {
                    PointF p = Map.Projection.Project(star.Horizontal);
                    g.FillEllipse(GetColor(star.Color), p.X - diam / 2, p.Y - diam / 2, diam, diam);
                }
            }
        }

        private float GetDrawingSize(float mag)
        {
            float maxMag = 0;
            float MAG_LIMIT_NARROW_ANGLE = magLimit;
            const float MAG_LIMIT_WIDE_ANGLE = 5.5f;

            const float NARROW_ANGLE = 2;
            const float WIDE_ANGLE = 90;

            float K = (MAG_LIMIT_NARROW_ANGLE - MAG_LIMIT_WIDE_ANGLE) / (NARROW_ANGLE - WIDE_ANGLE);
            float B = MAG_LIMIT_WIDE_ANGLE - K * WIDE_ANGLE;

            float minMag = K * (float)Map.ViewAngle + B;

            if (Map.ViewAngle < 2 && mag > minMag)
                return 1;

            if (mag > minMag)
                return 0;

            if (mag <= maxMag)
                mag = maxMag;

            float range = minMag - maxMag;

            return (range - mag + 1);
        }

        private Brush GetColor(char spClass)
        {
            switch (spClass)
            {
                case 'O':
                case 'W':
                    return Brushes.LightBlue;
                case 'B':
                    return Brushes.LightCyan;
                case 'A':
                    return Brushes.White;
                case 'F':
                    return Brushes.LightYellow;
                case 'G':
                    return Brushes.Yellow;
                case 'K':
                    return Brushes.Orange;
                case 'M':
                    return Brushes.OrangeRed;
                default:
                    return Brushes.White;
            }
        }

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/ConLines.dat");
            string[] parsed_line = new string[2];
            int from, to;
            string line = "";

            using (var sr = new StreamReader(file, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    parsed_line = line.Split(',');
                    from = Convert.ToInt32(parsed_line[0]) - 1;
                    to = Convert.ToInt32(parsed_line[1]) - 1;
                    ConLines.Add(new Tuple<int, int>(from, to));
                }
            }
        }
    }
}
