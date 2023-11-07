using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Projections
{
    public class CylinderProjection : Projection
    {
        public override double MaxFov => 180;

        public CylinderProjection(SkyContext context) : base(context) { }

        public override Vec2 Project(Vec3 vec, Mat4 mat)
        {
            Vec3 v = mat * vec;

            double r = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            bool rval = (-r < v[1] && v[1] < r);

            if (v[2] > 0.5) return null;
            if (!rval) return null;

            double alpha = Math.Atan2(v[0], -v[2]);
            double delta = Math.Asin(v[1] / r);
            v[0] = alpha;
            v[1] = delta;
            v[2] = r;
   
            // common part
            return new Vec2(
                ScreenWidth / 2 + (FlipHorizontal ? -1: 1) * v[0] * (ScreenScalingFactor * 2),
                ScreenHeight / 2 - (FlipVertical ? -1 : 1) * v[1] * (ScreenScalingFactor * 2)
                );
        }

        public override Vec3 Unproject(Vec2 s, Mat4 m)
        {
            Vec2 v = new Vec2(
                (FlipHorizontal ? -1 : 1) * (s[0] - ScreenWidth / 2) / (ScreenScalingFactor * 2),
                -(FlipVertical ? -1 : 1) * (s[1] - ScreenHeight / 2) / (ScreenScalingFactor * 2)
            );

            if (!(v[1] < Math.PI / 2 && v[1] > -Math.PI / 2 && v[0] > -Math.PI && v[0] < Math.PI)) return null;
            
            double cd = Math.Cos(v[1]);

            Vec3 vv = new Vec3();

            vv[2] = -cd * Math.Cos(v[0]);
            vv[0] = cd * Math.Sin(v[0]);
            vv[1] = Math.Sin(v[1]);

            double x = vv[0] - m[12];
            double y = vv[1] - m[13];
            double z = vv[2] - m[14];

            vv = m * new Vec3(x, y, z);

            return vv;

        }
    }
}
