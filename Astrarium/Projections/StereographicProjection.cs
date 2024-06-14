using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Projections
{
    public class StereographicProjection : Projection
    {
        public StereographicProjection(SkyContext context) : base(context) { }

        public override double MaxFov => 210;

        protected override Vec2 Project(Vec3 v, Mat4 mat)
        {
            Vec3 vec = mat * v;
            double r = vec.Length;
            double h = 0.5 * (r - vec[2]);

            if (h < 1e-3)
                return null;

            double f = ScreenScalingFactor / h;

            return new Vec2(
                ScreenWidth / 2 + (FlipHorizontal ? -1 : 1) * vec[0] * f,
                ScreenHeight / 2 + (FlipVertical ? -1 : 1) * vec[1] * f);
        }

        protected override Vec3 Unproject(Vec2 s, Mat4 m)
        {
            double x = (FlipHorizontal ? -1 : 1) * (s[0] - ScreenWidth / 2) / (ScreenScalingFactor * 2);
            double y = (FlipVertical ? -1 : 1) * (s[1] - ScreenHeight / 2) / (ScreenScalingFactor * 2);
            double lq = x * x + y * y;

            Vec3 v = new Vec3(2 * x, 2 * y, lq - 1);
            v *= 1.0 / (lq + 1.0);

            var vv = m * v;
            v[0] = vv[0];
            v[1] = vv[1];
            v[2] = vv[2];

            return v;
        }
    }
}
