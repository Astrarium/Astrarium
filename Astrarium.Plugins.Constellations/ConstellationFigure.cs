using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;

namespace Astrarium.Plugins.Constellations
{
    public class ConstellationFigure
    {
        public Vec2[] TextureAnchors { get; private set; }
        public Vec3[] SkyAnchors { get; private set; }

        public string File { get; private set; }
        public int TextureId { get; set; }
        public string Abbr { get; private set; }

        public ConstellationFigure(string rootFolder, JsonFigure figure)
        {
            File = Path.Combine(rootFolder, figure.File);
            Abbr = figure.Abbr;
            TextureAnchors = new Vec2[] 
            {
                new Vec2(figure.X1, figure.Y1),
                new Vec2(figure.X2, figure.Y2),
                new Vec2(figure.X3, figure.Y3)
            };
            SkyAnchors = new Vec3[]
            {
                SphericalToCartesian(new CrdsEquatorial(figure.RA1, figure.Dec1)),
                SphericalToCartesian(new CrdsEquatorial(figure.RA2, figure.Dec2)),
                SphericalToCartesian(new CrdsEquatorial(figure.RA3, figure.Dec3))
            };
        }

        public CrdsEquatorial TextureToSkyCoords(Vec2 texPoint)
        {
            Vec3 b = CalculateBarycentricCoords(texPoint);
            Vec3 s = b.X * SkyAnchors[0] + b.Y * SkyAnchors[1] + b.Z * SkyAnchors[2];
            s.Normalize();

            return CartesianToSpherical(s);
        }

        private Vec3 SphericalToCartesian(CrdsEquatorial eq)
        {
            double ra = Angle.ToRadians(eq.Alpha);
            double dec = Angle.ToRadians(eq.Delta);

            double x = Math.Cos(dec) * Math.Cos(ra);
            double y = Math.Cos(dec) * Math.Sin(ra);
            double z = Math.Sin(dec);

            return new Vec3(x, y, z);
        }

        private CrdsEquatorial CartesianToSpherical(Vec3 vec)
        {
            double dec = Angle.ToDegrees(Math.Asin(vec.Z));
            double ra = Angle.To360(Angle.ToDegrees(Math.Atan2(vec.Y, vec.X)));
            return new CrdsEquatorial(ra, dec);
        }

        private Vec3 CalculateBarycentricCoords(Vec2 tex)
        {
            double detT = (TextureAnchors[1].Y - TextureAnchors[2].Y) * (TextureAnchors[0].X - TextureAnchors[2].X) + (TextureAnchors[2].X - TextureAnchors[1].X) * (TextureAnchors[0].Y - TextureAnchors[2].Y);
            double u = ((TextureAnchors[1].Y - TextureAnchors[2].Y) * (tex.X - TextureAnchors[2].X) + (TextureAnchors[2].X - TextureAnchors[1].X) * (tex.Y - TextureAnchors[2].Y)) / detT;
            double v = ((TextureAnchors[2].Y - TextureAnchors[0].Y) * (tex.X - TextureAnchors[2].X) + (TextureAnchors[0].X - TextureAnchors[2].X) * (tex.Y - TextureAnchors[2].Y)) / detT;
            double w = 1 - u - v;
            return new Vec3(u, v, w);
        }
    }

    internal class LocalizedValue
    {
        public string Lang { get; set; }
        public string Value { get; set; }
    }

    internal class JsonFigures
    {
        public List<LocalizedValue> Name { get; set; }
        public List<LocalizedValue> Description { get; set; }
        public List<JsonFigure> Figures { get; set; }
    }

    public class JsonFigure
    {
        public string File { get; set; }
        public string Abbr { get; set; }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float RA1 { get; set; }
        public float Dec1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float RA2 { get; set; }
        public float Dec2 { get; set; }
        public float X3 { get; set; }
        public float Y3 { get; set; }
        public float RA3 { get; set; }
        public float Dec3 { get; set; }
    }
}
