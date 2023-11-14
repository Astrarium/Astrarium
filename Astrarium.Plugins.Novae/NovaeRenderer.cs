﻿using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Novae
{
    public class NovaeRenderer : BaseRenderer
    {
        public override RendererOrder Order => RendererOrder.Stars;

        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));
        private NovaeCalculator calc;
        private ISettings settings;
        private ISky sky;

        private const int limitAllNames = 40;
       
        public NovaeRenderer(NovaeCalculator calc, ISky sky, ISettings settings)
        {
            this.calc = calc;
            this.sky = sky;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Stars") || !settings.Get<bool>("Novae")) return;
            if (map.DaylightFactor == 1) return;

            var prj = map.Projection;
            var schema = settings.Get<ColorSchema>("Schema");
            bool drawLabels = settings.Get<bool>("StarsLabels") && settings.Get<bool>("NovaeLabels") && prj.Fov <= limitAllNames;
            Color labelColor = settings.Get<SkyColor>("ColorStarsLabels").Night.Tint(schema);
            Brush brushLabel = new SolidBrush(labelColor);
            var fontStarNames = settings.Get<Font>("StarsLabelsFont");

            // J2000 equatorial coordinates of screen center
            CrdsEquatorial eq0 = Precession.GetEquatorialCoordinates(prj.CenterEquatorial, calc.PrecessionalElements0);

            // matrix for projection, with respect of precession
            var mat = prj.MatEquatorialToVision * calc.MatPrecession;

            // real circular FOV with respect of screen borders
            double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight);

            // filter novae by magnitude and FOV
            var novae = calc.Novae.Where(n => n.Magnitude < prj.MagLimit && Angle.Separation(eq0, n.Equatorial0) < fov);

            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            foreach (var star in novae)
            {
                float size = prj.GetPointSize(star.Magnitude);
                if (size > 0)
                {
                    if ((int)size == 0) size = 1;

                    // cartesian coordinates of a nova for J2000 epoch
                    Vec3 v = Projection.SphericalToCartesian(Angle.ToRadians(star.Equatorial0.Alpha), Angle.ToRadians(star.Equatorial0.Delta));

                    // screen coordinates, for current epoch
                    Vec2 p = prj.Project(v, mat);

                    if (prj.IsInsideScreen(p))
                    {
                        GL.PointSize(size);
                        GL.Begin(PrimitiveType.Points);
                        GL.Color3(Color.White.Tint(schema));
                        GL.Vertex2(p.X, p.Y);
                        GL.End();

                        if (drawLabels)
                        {
                            map.DrawObjectLabel(textRenderer.Value, star.ProperName, fontStarNames, brushLabel, p, size);
                        }

                        map.AddDrawnObject(p, star, size);
                    }
                }
            }
        }
    }
}
