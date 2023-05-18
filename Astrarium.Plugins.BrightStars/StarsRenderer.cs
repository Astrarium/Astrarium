using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
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
        private TextRenderer textRenderer;

        private const int limitAllNames = 20;
        private const int limitBayerNames = 20;
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

        public override void Render(ISkyMap map)
        {
            var prj = map.SkyProjection;

            if (textRenderer == null)
            {
                textRenderer = new TextRenderer(128, 32);
            }

            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            // Color of const. lines
            GL.Color3(settings.Get<SkyColor>("ColorConstLines").GetColor(ColorSchema.Night));

            var allStars = starsCalc.Stars;

            double maxFov = Angle.ToRadians(prj.Fov + 30);

            // fov in radians
            double fov = Angle.ToRadians(prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight));

            // matrix for projection, with respect of precession
            var mat = prj.MatEquatorialToVision * starsCalc.MatPrecession;

            // equatorial vision vector in J2000 coords
            var eqVision0 = starsCalc.MatPrecession0 * prj.VecEquatorialVision;

            // years since 2000.0
            double t = prj.Context.Get(starsCalc.YearsSince2000);

            if (settings.Get("ConstLines"))
            {
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(1, 0xAAAA);

                foreach (var line in sky.ConstellationLines)
                {
                    var s1 = allStars.ElementAt(line.Item1);
                    var s2 = allStars.ElementAt(line.Item2);

                    // cartesian coordinates of stars with respect of proper motion,
                    // but for initial catalogue epoch
                    var c1 = CartesianWithProperMotion(s1, t);
                    var c2 = CartesianWithProperMotion(s2, t);

                    if (eqVision0.Angle(c1) < maxFov && eqVision0.Angle(c2) < maxFov)
                    {
                        var p1 = prj.Project(c1, mat);
                        var p2 = prj.Project(c2, mat);

                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex2(p1.X, p1.Y);
                        GL.Vertex2(p2.X, p2.Y);
                        GL.End();
                    }
                }

                GL.Disable(EnableCap.LineStipple);
            }

            if (settings.Get("Stars"))
            {
                float daylightFactor = map.DaylightFactor;

                // no stars if the Sun above horizon
                if (daylightFactor == 1) return;

                float starDimming = 1 - daylightFactor;

                float minStarSize = daylightFactor * 3; // empiric

                var labelBrush = new SolidBrush(Color.DimGray);

                var stars = allStars.Where(s => s != null && eqVision0.Angle(CartesianWithProperMotion(s, t)) < fov);

                foreach (var star in stars)
                {
                    float size = prj.GetPointSize(star.Magnitude) * starDimming;
                    if (size > minStarSize)
                    {
                        var c = CartesianWithProperMotion(star, t);

                        var p = prj.Project(c, mat);

                        if (prj.IsInsideScreen(p))
                        {
                            GL.PointSize(size);
                            GL.Color3(GetColor(star.Color));

                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, star);

                            DrawStarName(prj, p, star, size);
                        }
                    }
                }
            }

            GL.Disable(EnableCap.PointSmooth);
            GL.Disable(EnableCap.Blend);
        }

        private Vec3 CartesianWithProperMotion(Star s, double t)
        {
            double alpha = s.PmAlpha * t / 3600.0;
            double delta = s.PmDelta * t / 3600.0;

            alpha = s.Alpha0 + Angle.ToRadians(alpha);
            delta = s.Delta0 + Angle.ToRadians(delta);

            return Projection.SphericalToCartesian(alpha, delta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">Star</param>
        /// <param name="t">Number of years since initial epoch</param>
        /// <returns></returns>
        /// <remarks>
        /// See Abalakin, page 125. Signs of rotation angles are inverted.
        /// </remarks>
        private Mat4 GetProperMotion(Star s, double t)
        {
            return
                Mat4.ZRotation(s.Alpha0) *
                Mat4.YRotation(-s.Delta0 + Math.PI / 2) *
                Mat4.ZRotation(-s.PmPhi0) *
                Mat4.YRotation(-s.PmMu * t) *
                Mat4.ZRotation(s.PmPhi0) *
                Mat4.YRotation(-Math.PI / 2 + s.Delta0) *
                Mat4.ZRotation(-s.Alpha0);
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

                            g.FillEllipse(new SolidBrush(GetColor(star.Color)), p.X - size / 2, p.Y - size / 2, size, size);

                            map.AddDrawnObject(star);
                        }
                    }
                }

                if (settings.Get<bool>("StarsLabels") && map.ViewAngle <= limitAllNames)
                {
                    var color = settings.Get<SkyColor>("ColorStarsLabels").GetColor(ColorSchema.Night);
                    brushStarNames = new SolidBrush(color);

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

        private Color GetColor(char spClass)
        {
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

            return starColor;
        }

        /// <summary>
        /// Draws star name
        /// </summary>
        private void DrawStarName(Projection prj, PointF point, Star s, float diam)
        {
            var fontStarNames = settings.Get<Font>("StarsLabelsFont");

            var color = settings.Get<SkyColor>("ColorStarsLabels").GetColor(ColorSchema.Night);
            brushStarNames = new SolidBrush(color);

            // Star has proper name
            if (prj.Fov < limitProperNames && settings.Get<bool>("StarsProperNames") && s.ProperName != null)
            {
                textRenderer.DrawString(s.ProperName, fontStarNames, brushStarNames, new PointF(point.X + diam / 2, point.Y - diam / 2));
                return;
            }

            // Star has Bayer name (greek letter)
            if (prj.Fov < limitBayerNames)
            {
                string bayerName = s.BayerName;
                if (bayerName != null)
                {
                    textRenderer.DrawString(bayerName, fontStarNames, brushStarNames, new PointF(point.X + diam / 2, point.Y - diam / 2));
                    return;
                }
            }
            /*
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
            */
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
