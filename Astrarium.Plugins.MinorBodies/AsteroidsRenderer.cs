﻿using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
{
    public class AsteroidsRenderer : BaseRenderer
    {
        private readonly AsteroidsCalc asteroidsCalc;
        private readonly ISettings settings;

        private readonly Color colorAsteroid = Color.FromArgb(100, 100, 100);

        public AsteroidsRenderer(AsteroidsCalc asteroidsCalc, ISettings settings)
        {
            this.asteroidsCalc = asteroidsCalc;
            this.settings = settings;
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allAsteroids = asteroidsCalc.Asteroids;
            bool isGround = settings.Get<bool>("Ground");
            bool drawAsteroids = settings.Get<bool>("Asteroids");
            bool drawLabels = settings.Get<bool>("AsteroidsLabels");
            bool drawAll = settings.Get<bool>("AsteroidsDrawAll");
            decimal drawAllMagLimit = settings.Get<decimal>("AsteroidsDrawAllMagLimit");
            bool drawLabelMag = settings.Get<bool>("AsteroidsLabelsMag");
            Brush brushNames = new SolidBrush(map.GetColor("ColorAsteroidsLabels"));
            var font = settings.Get<Font>("AsteroidsLabelsFont");

            if (drawAsteroids)
            {
                var asteroids = allAsteroids.Where(a => Angle.Separation(map.Center, a.Horizontal) < map.ViewAngle);

                foreach (var a in asteroids)
                {
                    double ad = Angle.Separation(a.Horizontal, map.Center);

                    if ((!isGround || a.Horizontal.Altitude + a.Semidiameter / 3600 > 0) &&
                        ad < map.ViewAngle + a.Semidiameter / 3600)
                    {
                        float diam = map.GetDiskSize(a.Semidiameter);
                        float size = map.GetPointSize(a.Magnitude);

                        // if "draw all" setting is enabled, draw asteroids brighter than limit
                        if (drawAll && size < 1 && a.Magnitude <= (float)drawAllMagLimit)
                        {
                            size = 1;
                        }

                        string label = drawLabelMag ? $"{a.Name} {Formatters.Magnitude.Format(a.Magnitude)}" : a.Name;
                        
                        // asteroid should be rendered as disk
                        if ((int)diam > 0 && diam > size)
                        {
                            PointF p = map.Project(a.Horizontal);
                            map.Graphics.FillEllipse(new SolidBrush(map.GetColor(colorAsteroid)), p.X - diam / 2, p.Y - diam / 2, diam, diam);
                            DrawVolume(map, diam, 0);

                            if (drawLabels)
                            {
                                map.DrawObjectCaption(font, brushNames, label, p, diam);
                            }

                            map.AddDrawnObject(a);
                            continue;
                        }
                        // asteroid should be rendered as point
                        else if (size > 0)
                        {
                            if ((int)size == 0) size = 1;

                            PointF p = map.Project(a.Horizontal);

                            if (!map.IsOutOfScreen(p))
                            {
                                g.FillEllipse(new SolidBrush(map.GetColor(Color.White)), p.X - size / 2, p.Y - size / 2, size, size);

                                if (drawLabels)
                                {
                                    map.DrawObjectCaption(font, brushNames, label, p, size);
                                }

                                map.AddDrawnObject(a);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private void DrawVolume(IMapContext map, float diam, float flattening)
        {
            if (map.Schema == ColorSchema.White) return;

            Graphics g = map.Graphics;
            float diamEquat = diam * 1.01f;
            float diamPolar = (1 - flattening) * diam * 1.01f;

            using (GraphicsPath gpVolume = new GraphicsPath())
            {
                gpVolume.AddEllipse(-diamEquat, -diamPolar, 2 * diamEquat, 2 * diamPolar);
                using (PathGradientBrush brushVolume = new PathGradientBrush(gpVolume))
                {
                    brushVolume.CenterPoint = new PointF(0, 0);
                    brushVolume.CenterColor = map.GetSkyColor();
                    brushVolume.SetSigmaBellShape(0.3f, 1);
                    brushVolume.SurroundColors = new Color[] { Color.Transparent };
                    g.FillEllipse(brushVolume, -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
                }
            }
        }

        public override RendererOrder Order => RendererOrder.SolarSystem;
    }
}
