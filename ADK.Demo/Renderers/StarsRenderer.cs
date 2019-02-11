using ADK.Demo.Calculators;
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
        private IStarsProvider starsProvider;

        private ICollection<Tuple<int, int>> ConLines = new List<Tuple<int, int>>();

        private Font fontStarNames;
        private Pen penConLine;
        private Brush brushStarNames;

        private const double maxSeparation = 90 * 1.2;

        private const int limitAllNames = 40;
        private const int limitBayerNames = 40;
        private const int limitProperNames = 20;
        private const int limitFlamsteedNames = 10;
        private const int limitVarNames = 5;

        public StarsRenderer(Sky sky, IStarsProvider starsProvider, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            this.starsProvider = starsProvider;

            fontStarNames = new Font("Arial", 8);
            penConLine = new Pen(new SolidBrush(Color.FromArgb(64, 64, 64)));
            penConLine.DashStyle = DashStyle.Dot;
            brushStarNames = new SolidBrush(Color.FromArgb(64, 64, 64));
        }

        public override void Render(Graphics g)
        {
            var allStars = starsProvider.Stars;
            bool isGround = Settings.Get<bool>("Ground");

            if (Settings.Get<bool>("ConstLines"))
            {
                PointF p1, p2;
                CrdsHorizontal h1, h2;
                foreach (var line in ConLines)
                {
                    h1 = allStars.ElementAt(line.Item1).Horizontal;
                    h2 = allStars.ElementAt(line.Item2).Horizontal;

                    if ((!isGround || h1.Altitude > 0 || h2.Altitude > 0) && 
                        Angle.Separation(Map.Center, h1) < maxSeparation &&
                        Angle.Separation(Map.Center, h2) < maxSeparation)
                    {
                        p1 = Map.Projection.Project(h1);
                        p2 = Map.Projection.Project(h2);

                        var points = SegmentScreenIntersection(p1, p2);
                        if (points.Length == 2)
                        {
                            g.DrawLine(penConLine, points[0], points[1]);
                        }
                    }
                }
            }

            if (Settings.Get<bool>("Stars"))
            {
                var stars = allStars.Where(s => s != null && Angle.Separation(Map.Center, s.Horizontal) < Map.ViewAngle * 1.2);
                if (isGround)
                {
                    stars = stars.Where(s => s.Horizontal.Altitude >= 0);
                }

                foreach (var star in stars)
                {
                    float diam = GetPointSize(star.Mag);
                    if ((int)diam > 0)
                    {
                        PointF p = Map.Projection.Project(star.Horizontal);
                        if (!IsOutOfScreen(p))
                        {
                            g.FillEllipse(GetColor(star.Color), p.X - diam / 2, p.Y - diam / 2, diam, diam);                                
                            Map.AddDrawnObject(star, p);
                        }
                    }
                }

                if (Settings.Get<bool>("StarsLabels") && Map.ViewAngle <= limitAllNames)
                {
                    foreach (var star in stars)
                    {
                        float diam = GetPointSize(star.Mag);
                        if ((int)diam > 0)
                        {
                            PointF p = Map.Projection.Project(star.Horizontal);
                            if (!IsOutOfScreen(p))
                            {
                                DrawStarName(g, p, star, diam);
                            }
                        }
                    }
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
            // Star has proper name
            if (Map.ViewAngle < limitProperNames && Settings.Get<bool>("StarsProperNames") && s.ProperName != null)
            {
                DrawObjectCaption(g, fontStarNames, brushStarNames, s.ProperName, point, diam);
                return;
            }

            // Star has Bayer name (greek letter)
            if (Map.ViewAngle < limitBayerNames)
            {
                string bayerName = s.BayerName;
                if (bayerName != null)
                {
                    DrawObjectCaption(g, fontStarNames, brushStarNames, bayerName, point, diam);
                    return;                    
                }
            }
            // Star has Flamsteed number
            if (Map.ViewAngle < limitFlamsteedNames)
            {
                string flamsteedNumber = s.FlamsteedNumber;
                if (flamsteedNumber != null)
                {
                    DrawObjectCaption(g, fontStarNames, brushStarNames, flamsteedNumber, point, diam);
                    return;
                }
            }

            // Star has variable id
            if (Map.ViewAngle < limitVarNames && s.VariableName != null)
            {
                string varName = s.VariableName.Split(' ')[0];
                if (!varName.All(char.IsDigit))
                {
                    DrawObjectCaption(g, fontStarNames, brushStarNames, varName, point, diam);
                    return;
                }
            }

            // Star doesn't have any names
            if (Map.ViewAngle < 2)
            {
                DrawObjectCaption(g, fontStarNames, brushStarNames, $"HR {s.Number}", point, diam);
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
