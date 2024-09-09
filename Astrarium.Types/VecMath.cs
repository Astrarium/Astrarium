using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public class Mat4
    {
        private readonly double[] r = new double[16];

        public Mat4() { }

        public double[] Values => r;

        public Mat4(double a, double b, double c, double d, double e, double f, double g, double h, double i, double j, double k, double l, double m, double n, double o, double p)
        {
            Set(a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p);
        }

        public void Set(double a, double b, double c, double d, double e, double f, double g, double h, double i, double j, double k, double l, double m, double n, double o, double p)
        {
            r[0] = a; r[1] = b; r[2] = c; r[3] = d; r[4] = e; r[5] = f; r[6] = g; r[7] = h;
            r[8] = i; r[9] = j; r[10] = k; r[11] = l; r[12] = m; r[13] = n; r[14] = o; r[15] = p;
        }

        private static double MATMUL(Mat4 _, Mat4 a, int R, int C)
        {
            return _.r[R] * a.r[C] + _.r[R + 4] * a.r[C + 1] + _.r[R + 8] * a.r[C + 2] + _.r[R + 12] * a.r[C + 3];
        }

        public static Mat4 operator *(Mat4 a, Mat4 b)
        {
            return new Mat4(
                MATMUL(a, b, 0, 0), MATMUL(a, b, 1, 0), MATMUL(a, b, 2, 0), MATMUL(a, b, 3, 0),
                MATMUL(a, b, 0, 4), MATMUL(a, b, 1, 4), MATMUL(a, b, 2, 4), MATMUL(a, b, 3, 4),
                MATMUL(a, b, 0, 8), MATMUL(a, b, 1, 8), MATMUL(a, b, 2, 8), MATMUL(a, b, 3, 8),
                MATMUL(a, b, 0, 12), MATMUL(a, b, 1, 12), MATMUL(a, b, 2, 12), MATMUL(a, b, 3, 12)
            );
        }

        public static Vec4 operator *(Mat4 m, Vec4 a)
        {
            return new Vec4(
                m.r[0] * a[0] + m.r[4] * a[1] + m.r[8] * a[2] + m.r[12] * a[3],
                m.r[1] * a[0] + m.r[5] * a[1] + m.r[9] * a[2] + m.r[13] * a[3],
                m.r[2] * a[0] + m.r[6] * a[1] + m.r[10] * a[2] + m.r[14] * a[3],
                m.r[3] * a[0] + m.r[7] * a[1] + m.r[11] * a[2] + m.r[15] * a[3]);
        }

        public static Vec3 operator *(Mat4 m, Vec3 a)
        {
            return new Vec3(
                m.r[0] * a[0] + m.r[4] * a[1] + m.r[8] * a[2] + m.r[12],
                m.r[1] * a[0] + m.r[5] * a[1] + m.r[9] * a[2] + m.r[13],
                m.r[2] * a[0] + m.r[6] * a[1] + m.r[10] * a[2] + m.r[14]);
        }

        public static Vec2 operator *(Mat4 m, Vec2 a)
        {
            return new Vec2(
                m.r[0] * a[0] + m.r[4] * a[1] + m.r[8] + m.r[12],
                m.r[1] * a[0] + m.r[5] * a[1] + m.r[9] + m.r[13]);
        }

        public Mat4 Transpose()
        {
            return new Mat4(
                r[0], r[4], r[8], r[12],
                r[1], r[5], r[9], r[13],
                r[2], r[6], r[10], r[14],
                r[3], r[7], r[11], r[15]);
        }

        public double this[int index]
        {
            get => r[index];
            set => r[index] = value;
        }

        public static Mat4 XRotation(double angle)
        {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);

            return new Mat4(1, 0, 0, 0,
                            0, c, s, 0,
                            0, -s, c, 0,
                            0, 0, 0, 1);
        }

        public static Mat4 YRotation(double angle)
        {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);

            return new Mat4(c, 0, -s, 0,
                            0, 1, 0, 0,
                            s, 0, c, 0,
                            0, 0, 0, 1);
        }

        public static Mat4 StretchX(double scale)
        {
            return new Mat4(scale, 0, 0, 0,
                            0, 1, 0, 0,
                            0, 0, 1, 0,
                            0, 0, 0, 1);
        }

        public static Mat4 StretchY(double scale)
        {
            return new Mat4(1, 0, 0, 0,
                            0, scale, 0, 0,
                            0, 0, 1, 0,
                            0, 0, 0, 1);
        }

        public static Mat4 StretchZ(double scale)
        {
            return new Mat4(1, 0, 0, 0,
                            0, 1, 0, 0,
                            0, 0, scale, 0,
                            0, 0, 0, 1);
        }

        public static Mat4 ZRotation(double angle)
        {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);

            return new Mat4(c, s, 0, 0,
                            -s, c, 0, 0,
                            0, 0, 1, 0,
                            0, 0, 0, 1);
        }

        public static Mat4 Translation(Vec3 v)
        {
            return new Mat4(1, 0, 0, 0,
                            0, 1, 0, 0,
                            0, 0, 1, 0,
                            v[0], v[1], v[2], 1);
        }

        // TODO: rewrite this
        public Mat4 Inverse()
        {
            double[] inv = new double[16];
            double det;
            int i;

            inv[0] = r[5] * r[10] * r[15] -
                     r[5] * r[11] * r[14] -
                     r[9] * r[6] * r[15] +
                     r[9] * r[7] * r[14] +
                     r[13] * r[6] * r[11] -
                     r[13] * r[7] * r[10];

            inv[4] = -r[4] * r[10] * r[15] +
                      r[4] * r[11] * r[14] +
                      r[8] * r[6] * r[15] -
                      r[8] * r[7] * r[14] -
                      r[12] * r[6] * r[11] +
                      r[12] * r[7] * r[10];

            inv[8] = r[4] * r[9] * r[15] -
                     r[4] * r[11] * r[13] -
                     r[8] * r[5] * r[15] +
                     r[8] * r[7] * r[13] +
                     r[12] * r[5] * r[11] -
                     r[12] * r[7] * r[9];

            inv[12] = -r[4] * r[9] * r[14] +
                       r[4] * r[10] * r[13] +
                       r[8] * r[5] * r[14] -
                       r[8] * r[6] * r[13] -
                       r[12] * r[5] * r[10] +
                       r[12] * r[6] * r[9];

            det = r[0] * inv[0] + r[1] * inv[4] + r[2] * inv[8] + r[3] * inv[12];

            if (det == 0)
            {
                return new Mat4();
            }

            det = 1.0 / det;

            inv[1] = -r[1] * r[10] * r[15] +
                      r[1] * r[11] * r[14] +
                      r[9] * r[2] * r[15] -
                      r[9] * r[3] * r[14] -
                      r[13] * r[2] * r[11] +
                      r[13] * r[3] * r[10];

            inv[5] = r[0] * r[10] * r[15] -
                     r[0] * r[11] * r[14] -
                     r[8] * r[2] * r[15] +
                     r[8] * r[3] * r[14] +
                     r[12] * r[2] * r[11] -
                     r[12] * r[3] * r[10];

            inv[9] = -r[0] * r[9] * r[15] +
                      r[0] * r[11] * r[13] +
                      r[8] * r[1] * r[15] -
                      r[8] * r[3] * r[13] -
                      r[12] * r[1] * r[11] +
                      r[12] * r[3] * r[9];

            inv[13] = r[0] * r[9] * r[14] -
                      r[0] * r[10] * r[13] -
                      r[8] * r[1] * r[14] +
                      r[8] * r[2] * r[13] +
                      r[12] * r[1] * r[10] -
                      r[12] * r[2] * r[9];

            inv[2] = r[1] * r[6] * r[15] -
                     r[1] * r[7] * r[14] -
                     r[5] * r[2] * r[15] +
                     r[5] * r[3] * r[14] +
                     r[13] * r[2] * r[7] -
                     r[13] * r[3] * r[6];

            inv[6] = -r[0] * r[6] * r[15] +
                      r[0] * r[7] * r[14] +
                      r[4] * r[2] * r[15] -
                      r[4] * r[3] * r[14] -
                      r[12] * r[2] * r[7] +
                      r[12] * r[3] * r[6];

            inv[10] = r[0] * r[5] * r[15] -
                      r[0] * r[7] * r[13] -
                      r[4] * r[1] * r[15] +
                      r[4] * r[3] * r[13] +
                      r[12] * r[1] * r[7] -
                      r[12] * r[3] * r[5];

            inv[14] = -r[0] * r[5] * r[14] +
                       r[0] * r[6] * r[13] +
                       r[4] * r[1] * r[14] -
                       r[4] * r[2] * r[13] -
                       r[12] * r[1] * r[6] +
                       r[12] * r[2] * r[5];

            inv[3] = -r[1] * r[6] * r[11] +
                      r[1] * r[7] * r[10] +
                      r[5] * r[2] * r[11] -
                      r[5] * r[3] * r[10] -
                      r[9] * r[2] * r[7] +
                      r[9] * r[3] * r[6];

            inv[7] = r[0] * r[6] * r[11] -
                     r[0] * r[7] * r[10] -
                     r[4] * r[2] * r[11] +
                     r[4] * r[3] * r[10] +
                     r[8] * r[2] * r[7] -
                     r[8] * r[3] * r[6];

            inv[11] = -r[0] * r[5] * r[11] +
                       r[0] * r[7] * r[9] +
                       r[4] * r[1] * r[11] -
                       r[4] * r[3] * r[9] -
                       r[8] * r[1] * r[7] +
                       r[8] * r[3] * r[5];

            inv[15] = r[0] * r[5] * r[10] -
                      r[0] * r[6] * r[9] -
                      r[4] * r[1] * r[10] +
                      r[4] * r[2] * r[9] +
                      r[8] * r[1] * r[6] -
                      r[8] * r[2] * r[5];

            var result = new Mat4();

            for (i = 0; i < 16; i++)
            {
                result[i] = inv[i] * det;
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a point or a vector on a 2D-plane.
    /// </summary>
    public class Vec2
    {
        private readonly double[] v = new double[2];

        public Vec2() { }

        public Vec2(double x, double y)
        {
            Set(x, y);
        }

        public void Set(double x, double y)
        {
            v[0] = x;
            v[1] = y;
        }

        public double this[int index]
        {
            get => v[index];
            set => v[index] = value;
        }

        public double Distance(Vec2 other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public double Dot(Vec2 b)
        {
            return v[0] * b.v[0] + v[1] * b.v[1];
        }

        public double Angle(Vec2 b)
        {
            double dot = Dot(b);
            return Math.Acos(dot / (Length * b.Length));
        }

        public void Normalize()
        {
            double len = Length;
            if (len == 0) return;
            v[0] /= len;
            v[1] /= len;
        }

        /// <summary>
        /// Gets length of the vector
        /// </summary>
        public double Length => Math.Sqrt(v[0] * v[0] + v[1] * v[1]);

        /// <summary>
        /// Calculates sum of two vectors
        /// </summary>
        /// <param name="v1">Left operand</param>
        /// <param name="v2">Right operand</param>
        /// <returns></returns>
        public static Vec2 operator +(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1[0] + v2[0], v1[1] + v2[1]);
        }

        public static Vec2 operator -(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1[0] - v2[0], v1[1] - v2[1]);
        }

        public static Vec2 operator +(Vec2 v, float f)
        {
            return new Vec2(v[0] + f, v[1] - f);
        }

        /// <summary>
        /// Gets or sets X-coordinate of the vector
        /// </summary>
        public double X
        {
            get => v[0];
            set => v[0] = value;
        }

        /// <summary>
        /// Gets or sets Y-coordinate of the vector
        /// </summary>
        public double Y
        {
            get => v[1];
            set => v[1] = value;
        }

        public static implicit operator System.Drawing.Point(Vec2 v) =>
            new System.Drawing.Point((int)v.X, (int)v.Y);

        public static implicit operator System.Drawing.PointF(Vec2 v) =>
            new System.Drawing.PointF((float)v.X, (float)v.Y);

        public static implicit operator Vec2(System.Drawing.PointF p) =>
            new Vec2(p.X, p.Y);
    }

    public class Vec3
    {
        private double[] v = new double[3];

        public Vec3() { }

        public double[] Values => v;

        public Vec3(double x, double y, double z)
        {
            Set(x, y, z);
        }

        public void Set(double x, double y, double z)
        {
            v[0] = x;
            v[1] = y;
            v[2] = z;
        }

        public double this[int index]
        {
            get => v[index];
            set { v[index] = value; }
        }

        /// <summary>
        /// Gets or sets X-coordinate of the vector
        /// </summary>
        public double X
        {
            get => v[0];
            set => v[0] = value;
        }

        /// <summary>
        /// Gets or sets Y-coordinate of the vector
        /// </summary>
        public double Y
        {
            get => v[1];
            set => v[1] = value;
        }
        /// <summary>
        /// Gets or sets Z-coordinate of the vector
        /// </summary>
        public double Z
        {
            get => v[2];
            set => v[2] = value;
        }

        public static Vec3 operator ^(Vec3 a, Vec3 b)
        {
            return new Vec3(a[1] * b[2] - a[2] * b[1],
                            a[2] * b[0] - a[0] * b[2],
                            a[0] * b[1] - a[1] * b[0]);
        }

        public static Vec3 operator *(Vec3 v, double s)
        {
            return new Vec3(s * v[0], s * v[1], s * v[2]);
        }

        public static Vec3 operator +(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1[0] + v2[0], v1[1] + v2[1], v1[2] + v2[2]);
        }

        public static Vec3 operator -(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1[0] - v2[0], v1[1] - v2[1], v1[2] - v2[2]);
        }

        public static Vec3 operator *(double s, Vec3 v)
        {
            return new Vec3(s * v[0], s * v[1], s * v[2]);
        }

        public double Dot(Vec3 b)
        {
            return v[0] * b.v[0] + v[1] * b.v[1] + v[2] * b.v[2];
        }

        public double Angle(Vec3 b)
        {
            double dot = Dot(b);
            return Math.Acos(dot / (Length * b.Length));
        }

        public void Normalize()
        {
            double s = 1.0 / Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            v[0] *= s;
            v[1] *= s;
            v[2] *= s;
        }

        public double Length => Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);

        public override string ToString()
        {
            return $"{{ {v[0]:F2}, {v[1]:F2}, {v[2]:F2} }}";
        }
    }

    public class Vec4
    {
        private double[] v = new double[4];

        public Vec4() { }

        public Vec4(double x, double y, double z, double w)
        {
            Set(x, y, z, w);
        }

        public void Set(double x, double y, double z, double w)
        {
            v[0] = x;
            v[1] = y;
            v[2] = z;
            v[3] = w;
        }

        public double this[int index]
        {
            get => v[index];
            set { v[index] = value; }
        }
    }
}
