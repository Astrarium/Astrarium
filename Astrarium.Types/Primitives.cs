using Astrarium.Algorithms;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public class Primitives
    {
        public static void DrawLine(Vec2 p1, Vec2 p2, Pen pen)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            if (pen.DashStyle != DashStyle.Solid)
            {
                GL.Enable(EnableCap.LineStipple);
                switch (pen.DashStyle)
                {
                    case DashStyle.Dash:
                        GL.LineStipple(3, 0xAAAA);
                        break;
                    case DashStyle.Dot:
                        GL.LineStipple(1, 0xAAAA);
                        break;
                    default:
                        break;
                }
            }

            GL.LineWidth(pen.Width);
            GL.Color3(pen.Color);

            GL.Begin(PrimitiveType.Lines);

            GL.Vertex2(p1.X, p1.Y);
            GL.Vertex2(p2.X, p2.Y);

            GL.End();
        }

        public static void DrawEllipse(Vec2 center, Pen pen, double radius)
        {
            DrawEllipse(center, pen, radius, radius, 0);
        }

        public static void DrawEllipse(Vec2 center, Pen pen, double rx, double ry, double rotationDegrees)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            if (pen.DashStyle != DashStyle.Solid)
            {
                GL.Enable(EnableCap.LineStipple);
                switch (pen.DashStyle)
                {
                    case DashStyle.Dash:
                        GL.LineStipple(3, 0xAAAA);
                        break;
                    case DashStyle.Dot:
                        GL.LineStipple(1, 0xAAAA);
                        break;
                    default:
                        break;
                }
            }

            GL.Begin(PrimitiveType.LineStrip);

            GL.LineWidth(pen.Width);
            GL.Color3(pen.Color);

            int count = 64;

            double rot = Angle.ToRadians(rotationDegrees);
            double cosRot = Math.Cos(rot);
            double sinRot = Math.Sin(rot);

            for (int i = 0; i <= count; i++)
            {
                double t = i / (double)count * (2 * Math.PI);
                double cost = Math.Cos(t);
                double sint = Math.Sin(t);

                double x = center.X + rx * cost * cosRot - ry * sint * sinRot;
                double y = center.Y + ry * sint * cosRot + rx * cost * sinRot;
                GL.Vertex2(x, y);
            }

            GL.End();
        }
    }
}
