using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Projections
{
    public class PerspectiveProjection : Projection
    {
        public PerspectiveProjection(SkyContext context) : base(context) { }

        public override double MaxFov => 100;

        protected override void UpdateProjectionMatrix()
        {
            double f = 1.0 / Math.Tan(Angle.ToRadians(Fov / 2));
            double ratio = (double)ScreenHeight / ScreenWidth;

            const double zNear = 0.1;
            const double zFar = 10000;

            MatProjection.Set((FlipHorizontal ? -1 : 1) * f * ratio, 0, 0, 0,
                            0, -(FlipVertical ? -1 : 1) * f, 0, 0,
                            0, 0, (zFar + zNear) / (zNear - zFar), -1,
                            0, 0, 2 * zFar * zNear / (zNear - zFar), 0);
        }

        public override Vec2 Project(Vec3 v, Mat4 mat)
        {
            Vec3 win = new Vec3();
            if (!gluProject(v, mat, MatProjection, ref win))
                return null;
            else
                return win[2] < 1 ? new Vec2(win[0], win[1]) : null;
        }

        public override Vec3 Unproject(Vec2 s, Mat4 m)
        {
            return m * new Vec3(
                s[0] * 2.0 / ScreenWidth - 1.0,
                s[1] * 2.0 / ScreenHeight - 1.0,
                1
            );
        }

        // https://www.cyberforum.ru/opengl/thread1347461.html
        // https://doxygen.reactos.org/d2/d0d/project_8c_source.html p.234
        protected bool gluProject(Vec3 obj, Mat4 modelview, Mat4 projection, ref Vec3 windowCoordinate)
        {
            Vec4 vec = projection * (modelview * new Vec4(obj[0], obj[1], obj[2], 1));

            if (vec[3] == 0.0) return false;

            vec[0] /= vec[3];
            vec[1] /= vec[3];
            vec[2] /= vec[3];

            // Map x, y and z to range 0...1
            vec[0] = vec[0] * 0.5 + 0.5;
            vec[1] = vec[1] * 0.5 + 0.5;
            vec[2] = vec[2] * 0.5 + 0.5;

            // Map x,y to screen coordinates
            vec[0] = vec[0] * ScreenWidth;
            vec[1] = vec[1] * ScreenHeight;

            windowCoordinate[0] = vec[0];
            windowCoordinate[1] = vec[1];
            windowCoordinate[2] = vec[2];
            return true;
        }
    }
}
