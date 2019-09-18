using ADK;
using Planetarium.Calculators;
using Planetarium.Config;
using Planetarium.Objects;
using Planetarium.Types;
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
    public class StarsRenderer : BaseRenderer
    {
        private readonly IStarsProvider starsProvider;
        private readonly ISettings settings;

        private ICollection<Tuple<int, int>> ConLines = new List<Tuple<int, int>>();

        private Font fontStarNames;
        private Pen penConLine;
        private Brush brushStarNames;

        private const int limitAllNames = 40;
        private const int limitBayerNames = 40;
        private const int limitProperNames = 20;
        private const int limitFlamsteedNames = 10;
        private const int limitVarNames = 5;

        public StarsRenderer(IStarsProvider starsProvider, ISettings settings)
        {
            this.starsProvider = starsProvider;
            this.settings = settings;

            fontStarNames = new Font("Arial", 8);
            penConLine = new Pen(new SolidBrush(Color.FromArgb(64, 64, 64)));
            penConLine.DashStyle = DashStyle.Dot;
            brushStarNames = new SolidBrush(Color.FromArgb(64, 64, 64));
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allStars = starsProvider.Stars;
            bool isGround = settings.Get<bool>("Ground");
            double coeff = map.DiagonalCoefficient();

            if (settings.Get<bool>("ConstLines"))
            {
                PointF p1, p2;
                CrdsHorizontal h1, h2;
                foreach (var line in ConLines)
                {
                    h1 = allStars.ElementAt(line.Item1).Horizontal;
                    h2 = allStars.ElementAt(line.Item2).Horizontal;

                    if ((!isGround || h1.Altitude > 0 || h2.Altitude > 0) && 
                        Angle.Separation(map.Center, h1) < 90 * coeff &&
                        Angle.Separation(map.Center, h2) < 90 * coeff)
                    {
                        p1 = map.Project(h1);
                        p2 = map.Project(h2);

                        var points = map.SegmentScreenIntersection(p1, p2);
                        if (points.Length == 2)
                        {
                            g.DrawLine(penConLine, points[0], points[1]);
                        }
                    }
                }
            }

            if (settings.Get<bool>("Stars"))
            {
                var stars = allStars.Where(s => s != null && Angle.Separation(map.Center, s.Horizontal) < map.ViewAngle * coeff);
                if (isGround)
                {
                    stars = stars.Where(s => s.Horizontal.Altitude >= 0);
                }

                foreach (var star in stars)
                {
                    float diam = map.GetPointSize(star.Mag);
                    if ((int)diam > 0)
                    {
                        PointF p = map.Project(star.Horizontal);
                        if (!map.IsOutOfScreen(p))
                        {
                            g.FillEllipse(GetColor(star.Color), p.X - diam / 2, p.Y - diam / 2, diam, diam);                                
                            map.AddDrawnObject(star, p);
                        }
                    }
                }

                if (settings.Get<bool>("StarsLabels") && map.ViewAngle <= limitAllNames)
                {
                    foreach (var star in stars)
                    {
                        float diam = map.GetPointSize(star.Mag);
                        if ((int)diam > 0)
                        {
                            PointF p = map.Project(star.Horizontal);
                            if (!map.IsOutOfScreen(p))
                            {
                                DrawStarName(map, p, star, diam);
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
        private void DrawStarName(IMapContext map, PointF point, Star s, float diam)
        {
            // Star has proper name
            if (map.ViewAngle < limitProperNames && settings.Get<bool>("StarsProperNames") && s.ProperName != null)
            {
                map.DrawObjectCaption(fontStarNames, brushStarNames, s.ProperName, point, diam);
                return;
            }

            // Star has Bayer name (greek letter)
            if (map.ViewAngle < limitBayerNames)
            {
                string bayerName = s.BayerName;
                if (bayerName != null)
                {
                    map.DrawObjectCaption(fontStarNames, brushStarNames, bayerName, point, diam);
                    return;                    
                }
            }
            // Star has Flamsteed number
            if (map.ViewAngle < limitFlamsteedNames)
            {
                string flamsteedNumber = s.FlamsteedNumber;
                if (flamsteedNumber != null)
                {
                    map.DrawObjectCaption(fontStarNames, brushStarNames, flamsteedNumber, point, diam);
                    return;
                }
            }

            // Star has variable id
            if (map.ViewAngle < limitVarNames && s.VariableName != null)
            {
                string varName = s.VariableName.Split(' ')[0];
                if (!varName.All(char.IsDigit))
                {
                    map.DrawObjectCaption(fontStarNames, brushStarNames, varName, point, diam);
                    return;
                }
            }

            // Star doesn't have any names
            if (map.ViewAngle < 2)
            {
                map.DrawObjectCaption(fontStarNames, brushStarNames, $"HR {s.Number}", point, diam);
            }
        }

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/ConLines.dat");
            string[] chunks = null;
            int from, to;
            string line = "";

            using (var sr = new StreamReader(file, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    chunks = line.Split(',');
                    from = Convert.ToInt32(chunks[0]) - 1;
                    to = Convert.ToInt32(chunks[1]) - 1;
                    ConLines.Add(new Tuple<int, int>(from, to));
                }
            }
        }

        public override RendererOrder Order => RendererOrder.Stars;
    }
}
