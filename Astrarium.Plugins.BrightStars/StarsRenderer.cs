﻿using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.BrightStars
{
    public class StarsRenderer : BaseRenderer
    {
        private readonly ISky sky;
        private readonly StarsCalc starsCalc;
        private readonly ISettings settings;

        private Pen penConLine;
        private Color starColor;
        private Brush brushStarNames;

        private const int limitAllNames = 40;
        private const int limitBayerNames = 40;
        private const int limitProperNames = 20;
        private const int limitFlamsteedNames = 10;
        private const int limitVarNames = 5;

        public StarsRenderer(ISky sky, StarsCalc starsCalc, ISettings settings)
        {
            this.sky = sky;
            this.starsCalc = starsCalc;
            this.settings = settings;

            penConLine = new Pen(new SolidBrush(Color.Transparent));
            penConLine.DashStyle = DashStyle.Custom;
            penConLine.DashPattern = new float[] { 2, 2 };
            starColor = Color.White;
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allStars = starsCalc.Stars;
            bool isGround = settings.Get<bool>("Ground");

            if (settings.Get<bool>("ConstLines"))
            {
                PointF p1, p2;
                CrdsHorizontal h1, h2;
                penConLine.Brush = new SolidBrush(map.GetColor("ColorConstLines"));

                foreach (var line in sky.ConstellationLines)
                {
                    h1 = allStars.ElementAt(line.Item1).Horizontal;
                    h2 = allStars.ElementAt(line.Item2).Horizontal;

                    if ((!isGround || h1.Altitude > 0 || h2.Altitude > 0) &&
                        Angle.Separation(map.Center, h1) < 90 &&
                        Angle.Separation(map.Center, h2) < 90)
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

            if (settings.Get<bool>("Stars") && !(map.Schema == ColorSchema.Day && map.DayLightFactor == 1))
            {
                var stars = allStars.Where(s => s != null && Angle.Separation(map.Center, s.Horizontal) < map.ViewAngle);
                if (isGround)
                {
                    stars = stars.Where(s => s.Horizontal.Altitude >= 0);
                }

                foreach (var star in stars)
                {
                    float size = map.GetPointSize(star.Magnitude);
                    if (size > 0)
                    {
                        PointF p = map.Project(star.Horizontal);
                        if (!map.IsOutOfScreen(p))
                        {
                            if (map.Schema == ColorSchema.White)
                            {
                                g.FillEllipse(Brushes.White, p.X - size / 2 - 1, p.Y - size / 2 - 1, size + 2, size + 2);
                            }

                            g.FillEllipse(new SolidBrush(GetColor(map, star.Color)), p.X - size / 2, p.Y - size / 2, size, size);

                            map.AddDrawnObject(star);
                        }
                    }
                }

                if (settings.Get<bool>("StarsLabels") && map.ViewAngle <= limitAllNames)
                {
                    brushStarNames = new SolidBrush(map.GetColor("ColorStarsLabels"));

                    foreach (var star in stars)
                    {
                        float size = map.GetPointSize(star.Magnitude);
                        if (size > 0)
                        {
                            PointF p = map.Project(star.Horizontal);
                            if (!map.IsOutOfScreen(p))
                            {
                                DrawStarName(map, p, star, size);
                            }
                        }
                    }
                }
            }
        }

        private Color GetColor(IMapContext map, char spClass)
        {
            if (map.Schema == ColorSchema.White)
            {
                return Color.Black;
            }

            if (settings.Get("StarsColors"))
            {
                switch (spClass)
                {
                    case 'O':
                    case 'W':
                        starColor = Color.LightBlue;
                        break;
                    case 'B':
                        starColor = Color.LightCyan;
                        break;
                    case 'A':
                        starColor = Color.White;
                        break;
                    case 'F':
                        starColor = Color.LightYellow;
                        break;
                    case 'G':
                        starColor = Color.Yellow;
                        break;
                    case 'K':
                        starColor = Color.Orange;
                        break;
                    case 'M':
                        starColor = Color.OrangeRed;
                        break;
                    default:
                        starColor = Color.White;
                        break;
                }
            }
            else
            {
                starColor = Color.White;
            }

            return map.GetColor(starColor, Color.Transparent);
        }

        /// <summary>
        /// Draws star name
        /// </summary>
        private void DrawStarName(IMapContext map, PointF point, Star s, float diam)
        {
            var fontStarNames = settings.Get<Font>("StarsLabelsFont");

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

        public override RendererOrder Order => RendererOrder.Stars;
    }
}
