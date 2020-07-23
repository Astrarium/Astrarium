using static System.Math;

namespace Astrarium.Algorithms
{
    internal class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector() { }

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector operator -(Vector v)
        {
            return new Vector(-v.X, -v.Y, -v.Z);
        }

        public static Vector operator *(double n, Vector v)
        {
            return new Vector(n * v.X, n * v.Y, n * v.Z);
        }

        public static Vector operator *(Vector v, double n)
        {
            return new Vector(n * v.X, n * v.Y, n * v.Z);
        }

        public static Vector operator /(Vector v, double n)
        {
            return new Vector(v.X / n, v.Y / n, v.Z / n);
        }

        public static Vector operator *(Matrix m, Vector v)
        {
            return new Vector(
                m.Values[0, 0] * v.X + m.Values[0, 1] * v.Y + m.Values[0, 2] * v.Z,
                m.Values[1, 0] * v.X + m.Values[1, 1] * v.Y + m.Values[1, 2] * v.Z,
                m.Values[2, 0] * v.X + m.Values[2, 1] * v.Y + m.Values[2, 2] * v.Z
            );
        }

        /// <summary>
        /// Dot product
        /// </summary>
        public static double Dot(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Norm
        /// </summary>
        public static double Norm(Vector v)
        {
            return Sqrt(Dot(v, v));
        }
    }
}
