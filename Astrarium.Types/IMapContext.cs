using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace Astrarium.Types
{
    public interface IMapContext
    {
        /// <summary>
        /// Gets drawing surface
        /// </summary>
        Graphics Graphics { get; }


        /// <summary>
        /// Value indicating degree of lightness, from 0 to 1 inclusive.
        /// Used for proper rendering of objects with day/night color schema enabled.
        /// 0 value means totally dark sky, 1 is a day, values between are different dusk degrees.
        /// </summary>
        float DayLightFactor { get; }

        /// <summary>
        /// Gets current color schema
        /// </summary>
        ColorSchema Schema { get; }

        /// <summary>
        /// Gets horizontal coordinates of the central point of the canvas.
        /// </summary>
        CrdsHorizontal Center { get; }

        /// <summary>
        /// Gets celestial object the map is locked on
        /// </summary>
        CelestialObject LockedObject { get; }

        CelestialObject SelectedObject { get; }

        CrdsHorizontal MousePosition { get; }

        MouseButton MouseButton { get; }

        void AddDrawnObject(CelestialObject obj);

        double JulianDay { get; }

        double Epsilon { get; }

        CrdsGeographical GeoLocation { get; }

        double SiderealTime { get; }

        void DrawObjectCaption(Font font, Brush brush, string caption, PointF p, float size, StringFormat format = null);

        void Redraw();

        
        Color GetColor(string colorName);
        Color GetColor(Color color);
        Color GetColor(Color colorNight, Color colorDay);
        Color GetSkyColor();
    }

    public static class MapContextExtensions
    {
        /// <summary>
        /// Gets angle between two vectors starting with same point.
        /// </summary>
        /// <param name="p0">Common point of two vectors (starting point for both vectors).</param>
        /// <param name="p1">End point of first vector</param>
        /// <param name="p2">End point of first vector</param>
        /// <returns>Angle between two vectors, in degrees, in range [0...180]</returns>
        public static double AngleBetweenVectors(this IMapContext map, PointF p0, PointF p1, PointF p2)
        {
            float[] a = new float[] { p1.X - p0.X, p1.Y - p0.Y };
            float[] b = new float[] { p2.X - p0.X, p2.Y - p0.Y };

            float ab = a[0] * b[0] + a[1] * b[1];
            double moda = Math.Sqrt(a[0] * a[0] + a[1] * a[1]);
            double modb = Math.Sqrt(b[0] * b[0] + b[1] * b[1]);

            double cos = ab / (moda * modb);

            if (cos < -1)
                cos = -1;

            if (cos > 1)
                cos = 1;

            return Angle.ToDegrees(Math.Acos(cos));
        }

        /// <summary>
        /// Gets distance between two points in pixels
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <returns>Distance between two points, in pixels</returns>
        public static double DistanceBetweenPoints(this IMapContext map, PointF p1, PointF p2)
        {
            double deltaX = p1.X - p2.X;
            double deltaY = p1.Y - p2.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public static void DrawImage(this IMapContext map, Image image, float x, float y, float width, float height)
        {
            map.DrawImage(image, new RectangleF(x, y, width, height), new Rectangle(0, 0, image.Width, image.Height));
        }

        public static void DrawImage(this IMapContext map, Image image, RectangleF destRect, RectangleF srcRect)
        {
            var gs = map.Graphics.Save();

            map.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            map.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            map.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            map.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

            Rectangle destRect2 = new Rectangle((int)destRect.X, (int)destRect.Y, (int)destRect.Width, (int)destRect.Height);

            map.Graphics.DrawImage(image, destRect2, (int)srcRect.X, (int)srcRect.Y, (int)srcRect.Width, (int)srcRect.Height, GraphicsUnit.Pixel, null);

            map.Graphics.Restore(gs);
        }
    }
}
