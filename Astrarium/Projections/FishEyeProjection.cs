using Astrarium.Types;
using System;

namespace Astrarium.Projections
{
    public class FishEyeProjection : Projection
    {
        public override double MaxFov => 120;

        public FishEyeProjection(SkyContext context) : base(context) { }

        protected override Vec2 Project(Vec3 vec, Mat4 mat)
        {
            Vec3 v = mat * vec;
            double oneoverh = 1.0 / Math.Sqrt(v[0] * v[0] + v[1] * v[1]);
            double a = Math.PI / 2 + Math.Atan(v[2] * oneoverh);
            double f = (a * ScreenScalingFactor) * oneoverh;
            v[0] = ScreenWidth / 2 + (FlipHorizontal ? -1 : 1) * v[0] * f;
            v[1] = ScreenHeight / 2 + (FlipVertical ? -1 : 1) * v[1] * f;
            if (a < 0.9 * Math.PI)
            {
                return new Vec2(v[0], v[1]);
            }
            else
            {
                return null;
            }
        }

        protected override Vec3 Unproject(Vec2 s, Mat4 m)
        {
            Vec3 v = new Vec3(
                (FlipHorizontal ? -1 : 1) * (s[0] - ScreenWidth / 2),
                (FlipVertical ? -1 : 1) * (s[1] - ScreenHeight / 2),
                0
            );

            double d = Math.Min(ScreenWidth, ScreenHeight) / 2;
            double length = v.Length;

            double angCenter = length / d * Fov / 2 * Math.PI / 180;
            double r = Math.Sin(angCenter);
            if (length != 0)
            {
                v.Normalize();
                v *= r;
            }
            else
            {
                v.Set(0, 0, 0);
            }

            v[2] = Math.Sqrt(1.0 - (v[0] * v[0] + v[1] * v[1]));
            if (angCenter < Math.PI / 2) 
            { 
                v[2] = -v[2]; 
            }

            return m * new Vec3(v[0], v[1], v[2]);
        }
    }
}
