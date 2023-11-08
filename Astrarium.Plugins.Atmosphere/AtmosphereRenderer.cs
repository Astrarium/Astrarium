using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Atmosphere
{
    public class AtmosphereRenderer : BaseRenderer
    {
        public override RendererOrder Order => RendererOrder.Atmosphere;

        private readonly AtmosphereCalculator calc;
        private readonly ISettings settings;

        public AtmosphereRenderer(AtmosphereCalculator calc, ISettings settings)
        {
            this.calc = calc;
            this.settings = settings;
        }

        public override void Render(IMapContext map)
        {
            throw new NotImplementedException();
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Ground")) return;
            if (!settings.Get("Atmosphere")) return;

            var prj = map.Projection;

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcColor);

            // if only one of the flipping enabled
            if (prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.CullFace(CullFaceMode.Front);
            }

            double stepAlt = 10;
            double stepAzi = 10;

            for (double alt = 0; alt <= 90; alt += stepAlt)
            {
                GL.Begin(PrimitiveType.QuadStrip);

                for (double azi = 0; azi <= 360; azi += stepAzi)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var hor = new CrdsHorizontal(azi, alt - (k * stepAlt));

                        var p = prj.Project(hor);

                        if (p != null)
                        {
                            hor.Altitude = Math.Abs(hor.Altitude);
                            GL.Color3(calc.GetColor(hor));
                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(PrimitiveType.QuadStrip);
                            break;
                        }
                    }
                }

                GL.End();
            }
        }
    }
}
