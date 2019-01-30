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

        private Font fontStarNames;
        private Pen penConLine;
        private const double maxSeparation = 90 * 1.2;

        public StarsRenderer(Sky sky, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            fontStarNames = new Font("Arial", 8);
            penConLine = new Pen(new SolidBrush(Color.FromArgb(64, 64, 64)));
            penConLine.DashStyle = DashStyle.Dot;
        }

        public override void Render(Graphics g)
        {
            var allStars = Sky.Get<ICollection<Star>>("Stars");

            if (Settings.Get<bool>("ConstLines"))
            {
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
            }
            
            var stars = allStars.Where(s => s != null && Angle.Separation(Map.Center, s.Horizontal) < Map.ViewAngle * 1.2);
            foreach (var star in stars)
            {
                float diam = GetPointSize(star.Mag);
                if ((int)diam > 0)
                {
                    PointF p = Map.Projection.Project(star.Horizontal);
                    g.FillEllipse(GetColor(star.Color), p.X - diam / 2, p.Y - diam / 2, diam, diam);
                    Map.VisibleObjects.Add(star);
                    DrawStarName(g, p, star, diam);
                }
            }
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

        /// <summary>
        /// Draws star name
        /// </summary>
        private void DrawStarName(Graphics g, PointF point, Star s, float diam)
        {           
            int limitProperNames = 45; 
            int limitBayerNames = 45;
            int limitFlamsteed = 10;
            int limitVarNames = 10;

            Brush brush = Brushes.Gray;

            //// If star has proper name:
            //if (Map.ViewAngle < limitProperNames && Sky.StarNames.ContainsKey(s.HRIndex))
            //{
            //    DrawLabel(g, Sky.StarNames[s.HRIndex], font, brush, point);
            //    //g.DrawString(Sky.StarNames[s.HRIndex], font, brush, point.X + (float)(diam / 2 * 0.7), point.Y + (float)(diam / 2 * 0.7));
            //    return;
            //}

            // If star has Bayer name (greek letter)
            if (Map.ViewAngle < limitBayerNames)
            {
                string bayerName = s.BayerName;
                if (bayerName != null)
                {
                    DrawObjectCaption(g, fontStarNames, bayerName, point, diam);
                    return;                    
                }
            }
            // If star has Flamsteed number
            if (Map.ViewAngle < limitFlamsteed)
            {
                string flamsteedNumber = s.FlamsteedNumber;
                if (flamsteedNumber != null)
                {
                    DrawObjectCaption(g, fontStarNames, flamsteedNumber, point, diam);
                    return;
                }
            }

            // If star has variable id
            //if (Map.ViewAngle < limitVarNames && s.NameVar != "")
            //{
            //    //g.DrawString(s.NameVar.Substring(0, s.NameVar.Length - 4), font, brush, point.X + (float)(diam / 2 * 0.7), point.Y + (float)(diam / 2 * 0.7));
            //    DrawLabel(g, s.NameVar.Substring(0, s.NameVar.Length - 4), font, brush, point);
            //    return;
            //}

            // If star doesn't have any names
            if (Map.ViewAngle < 2)
            {
                DrawObjectCaption(g, fontStarNames, $"HR {s.Number}", point, diam);
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
