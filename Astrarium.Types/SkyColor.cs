using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    /// <summary>
    /// Represents colors for colouring with different <see cref="ColorSchema">ColorSchema</see>s.
    /// </summary>
    public class SkyColor
    {
        public Color Night { get; set; }
        public Color Red { get; set; }
        public Color White { get; set; }

        public SkyColor() { }

        public SkyColor(SkyColor other) 
        {
            Night = other.Night;
            Red = other.Red;
            White = other.White;
        }

        public SkyColor(byte r, byte g, byte b) : this(Color.FromArgb(r, g, b)) { }

        public SkyColor(Color color)
        {
            Night = color;
            Red = GetNightModeColor(color);
            White = GetWhiteMapColor(color);
        }

        public static Color GetColor(ColorSchema schema, Color colorNight, float daylightFactor)
        {
            switch (schema)
            {
                default:
                case ColorSchema.Night:
                    return colorNight;
                case ColorSchema.Red:
                    return GetNightModeColor(colorNight);
                case ColorSchema.White:
                    return GetWhiteMapColor(colorNight);
                case ColorSchema.Day:
                    return GetIntermediateColor(daylightFactor, colorNight, GetDaylightColor(colorNight));
            }
        }

        public static Color GetColor(ColorSchema schema, Color colorNight, Color colorDay, float daylightFactor)
        {
            switch (schema)
            {
                default:
                case ColorSchema.Night:
                    return colorNight;
                case ColorSchema.Red:
                    return GetNightModeColor(colorNight);
                case ColorSchema.White:
                    return GetWhiteMapColor(colorNight);
                case ColorSchema.Day:
                    return GetIntermediateColor(daylightFactor, colorNight, colorDay);
            }
        }

        public Color GetColor(ColorSchema schema, float daylightFactor)
        {
            switch (schema)
            {
                default:
                case ColorSchema.Night:
                    return Night;
                case ColorSchema.Red:
                    return Red;
                case ColorSchema.White:
                    return White;
                case ColorSchema.Day:
                    return GetIntermediateColor(daylightFactor, Night, GetDaylightColor(Night));
            }
        }

        public Color GetColor(ColorSchema schema)
        {
            switch (schema)
            {
                default:
                case ColorSchema.Night:
                case ColorSchema.Day:
                    return Night;
                case ColorSchema.Red:
                    return Red;
                case ColorSchema.White:
                    return White;
            }
        }

        public void SetColor(Color color, ColorSchema schema)
        {
            switch (schema)
            {
                default:
                case ColorSchema.Night:
                case ColorSchema.Day:
                    Night = color;
                    break;
                case ColorSchema.Red:
                    Red = color;
                    break;
                case ColorSchema.White:
                    White = color;
                    break;
            }
        }


        private static Color GetIntermediateColor(float factor, Color from, Color to)
        {
            if (factor == 0)
                return from;
            else if (factor == 1)
                return to;
            else
            {
                int rMax = to.R;
                int rMin = from.R;
                int gMax = to.G;
                int gMin = from.G;
                int bMax = to.B;
                int bMin = from.B;
                int aMax = to.A;
                int aMin = from.A;

                int a = aMin + (int)((aMax - aMin) * factor);
                int r = rMin + (int)((rMax - rMin) * factor);
                int g = gMin + (int)((gMax - gMin) * factor);
                int b = bMin + (int)((bMax - bMin) * factor);

                return Color.FromArgb(a, r, g, b);
            }
        }

        private static Color COLOR_DAY_SKY = Color.FromArgb(116, 184, 255);

        private static Color GetDaylightColor(Color night)
        {
            float brightness = GetBrightness(night) / 255f;

            return Color.FromArgb(
                (int)(COLOR_DAY_SKY.R + brightness * (255 - COLOR_DAY_SKY.R)),
                (int)(COLOR_DAY_SKY.G + brightness * (255 - COLOR_DAY_SKY.G)),
                (int)(COLOR_DAY_SKY.B + brightness * (255 - COLOR_DAY_SKY.B))
                );
        }

        private static Color GetNightModeColor(Color night)
        {
            int brightness = GetBrightness(night);
            return Color.FromArgb(night.A, brightness, 0, 0);
        }

        private static Color GetWhiteMapColor(Color night)
        {
            int brightness = 255 - GetBrightness(night);
            return Color.FromArgb(night.A, brightness, brightness, brightness);
        }

        private static int GetBrightness(Color night)
        {
            return (int)(0.299 * night.R + 0.587 * night.G + 0.114 * night.B);
        }
    }
}
