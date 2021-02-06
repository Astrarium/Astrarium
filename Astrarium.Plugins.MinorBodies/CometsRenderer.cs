using Astrarium.Algorithms;
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

namespace Astrarium.Plugins.MinorBodies
{
    public class CometsRenderer : BaseRenderer
    {
        private readonly CometsCalc cometsCalc;
        private readonly ISettings settings;

        private readonly Color colorComet = Color.FromArgb(100, 191, 209, 255);

        public CometsRenderer(CometsCalc cometsCalc, ISettings settings)
        {
            this.cometsCalc = cometsCalc;
            this.settings = settings;
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allComets = cometsCalc.Comets;
            bool isGround = settings.Get("Ground");
            bool useTextures = settings.Get("PlanetsTextures");
            bool drawComets = settings.Get("Comets");
            bool drawLabels = settings.Get("AsteroidsLabels");
            bool drawAll = settings.Get<bool>("CometsDrawAll");
            decimal drawAllMagLimit = settings.Get<decimal>("CometsDrawAllMagLimit");
            bool drawLabelMag = settings.Get<bool>("CometsLabelsMag");
            Brush brushNames = new SolidBrush(map.GetColor("ColorCometsLabels"));
            var font = settings.Get<Font>("CometsLabelsFont");

            if (drawComets)
            {
                var comets = allComets.Where(a => Angle.Separation(map.Center, a.Horizontal) < map.ViewAngle);

                foreach (var c in comets)
                {
                    float diam = map.GetDiskSize(c.Semidiameter);
                    float size = map.GetPointSize(c.Magnitude);

                    // if "draw all" setting is enabled, draw comets brighter than limit
                    if (drawAll && size < 1 && c.Magnitude <= (float)drawAllMagLimit)
                    {
                        size = 1;
                    }

                    string label = drawLabelMag ? $"{c.Name} {Formatters.Magnitude.Format(c.Magnitude)}" : c.Name;

                    if (diam > 5)
                    {
                        double ad = Angle.Separation(c.Horizontal, map.Center);

                        if ((!isGround || c.Horizontal.Altitude + c.Semidiameter / 3600 > 0) &&
                            ad < map.ViewAngle + c.Semidiameter / 3600)
                        {
                            PointF p = map.Project(c.Horizontal);
                            PointF t = map.Project(c.TailHorizontal);

                            double tail = map.DistanceBetweenPoints(p, t);

                            if (diam > 5 || tail > 10)
                            {
                                using (var gpComet = new GraphicsPath())
                                {
                                    double rotation = Math.Atan2(t.Y - p.Y, t.X - p.X) + Math.PI / 2;
                                    gpComet.StartFigure();
                                    
                                    // tail is long enough
                                    if (tail > diam)
                                    {
                                        gpComet.AddArc(p.X - diam / 2, p.Y - diam / 2, diam, diam, (float)Angle.ToDegrees(rotation), 180);
                                        gpComet.AddLines(new PointF[] { gpComet.PathPoints[gpComet.PathPoints.Length - 1], t, gpComet.PathPoints[0] });
                                    }
                                    // draw coma only
                                    else
                                    {
                                        gpComet.AddEllipse(p.X - diam / 2, p.Y - diam / 2, diam, diam);
                                    }

                                    gpComet.CloseAllFigures();
                                    using (var brushComet = new PathGradientBrush(gpComet))
                                    {
                                        brushComet.CenterPoint = p;
                                        int alpha = 100;
                                        if (c.Magnitude >= map.MagLimit)
                                        {
                                            alpha -= (int)(100 * (c.Magnitude - map.MagLimit) / c.Magnitude);
                                        }
                                        brushComet.CenterColor = map.GetColor(Color.FromArgb(alpha, colorComet));
                                        brushComet.SurroundColors = gpComet.PathPoints.Select(pp => Color.Transparent).ToArray();
                                        g.FillPath(brushComet, gpComet);
                                    }
                                }

                                if (drawLabels)
                                {
                                    map.DrawObjectCaption(font, brushNames, label, p, diam);
                                }
                                map.AddDrawnObject(c);
                            }
                        }
                    }
                    else if ((int)size > 0)
                    {
                        PointF p = map.Project(c.Horizontal);

                        if (!map.IsOutOfScreen(p))
                        {
                            g.FillEllipse(new SolidBrush(map.GetColor(Color.White)), p.X - size / 2, p.Y - size / 2, size, size);

                            if (drawLabels)
                            {
                                map.DrawObjectCaption(font, brushNames, label, p, size);
                            }

                            map.AddDrawnObject(c);
                            continue;
                        }
                    }
                }
            }
        }

        public override RendererOrder Order => RendererOrder.SolarSystem;
    }
}
