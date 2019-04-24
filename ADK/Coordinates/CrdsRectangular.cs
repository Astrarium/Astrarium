using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    public class CrdsRectangular
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public CrdsRectangular() { }

        public CrdsRectangular(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static CrdsRectangular operator +(CrdsRectangular lhs, CrdsRectangular rhs)
        {
            return new CrdsRectangular()
            {
                X = lhs.X + rhs.X,
                Y = lhs.Y + rhs.Y,
                Z = lhs.Z + rhs.Z,
            };
        }

        public static CrdsRectangular operator -(CrdsRectangular lhs, CrdsRectangular rhs)
        {
            return new CrdsRectangular()
            {
                X = lhs.X - rhs.X,
                Y = lhs.Y - rhs.Y,
                Z = lhs.Z - rhs.Z,
            };
        }
    }
}
