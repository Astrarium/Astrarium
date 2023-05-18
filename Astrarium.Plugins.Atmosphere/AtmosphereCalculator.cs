using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace Astrarium.Plugins.Atmosphere
{
    public class AtmosphereCalculator : BaseCalc
    {
        private double thetaSun;

        // Darkening or brightening of the horizon
        private double[] A = new double[3];

        // Luminance gradient near the horizon
        private double[] B = new double[3];

        // Relative intensity of circumsolar region
        private double[] C = new double[3];

        // Width of the circumsolar region
        private double[] D = new double[3];

        // Relative backscattered light received at the earth surface
        private double[] E = new double[3];

        private double[] Z = new double[3];

        private CrdsHorizontal sun;
        private CrdsHorizontal moon;

        /// <summary>
        /// Semidiameter of the Sun, in arcseconds
        /// </summary>
        private double sdSun;

        /// <summary>
        /// Semidiameter of the Moon, in arcseconds
        /// </summary>
        private double sdMoon;

        // turbidity
        private double T = 3;

        /*
        public double CoeffA { get; set; } = 1;
        public double CoeffB { get; set; } = 1;
        public double CoeffC { get; set; } = 1;
        public double CoeffD { get; set; } = 1;
        public double CoeffE { get; set; } = 1;
        */

        private readonly ISky sky;
        private readonly ISkyMap map;

        public AtmosphereCalculator(ISky sky, ISkyMap map)
        {
            this.sky = sky;
            this.map = map;

            // 0 = Y
            A[0] = 0.17872 * T - 1.46303;
            B[0] = -0.35540 * T + 0.42749;
            C[0] = -0.0227 * T + 5.3251;
            D[0] = 0.1206 * T - 2.5771;
            E[0] = -0.0670 * T + 0.3703;

            // 1 = x
            A[1] = -0.0193 * T - 0.2592;
            B[1] = -0.0665 * T + 0.0008;
            C[1] = -0.0004 * T + 0.2125;
            D[1] = -0.0641 * T - 0.8989;
            E[1] = -0.0033 * T + 0.0452;

            // 2 = y
            A[2] = -0.0167 * T - 0.2608;
            B[2] = -0.0950 * T + 0.0092;
            C[2] = -0.0079 * T + 0.2102;
            D[2] = -0.0441 * T - 1.6537;
            E[2] = -0.0109 * T + 0.0529;
        }

        public override void Calculate(SkyContext context)
        {
            sun = context.Get(sky.SunEquatorial).ToHorizontal(context.GeoLocation, context.SiderealTime);


            // solar zenith distance, in radians
            thetaSun = Angle.ToRadians(90 - sun.Altitude);

            double chi = (4.0 / 9.0 - T / 120) * (PI - 2.0 * thetaSun);

            Z[0] = (4.0453 * T - 4.9710) * Tan(chi) - 0.2155 * T + 2.4192;

            double thetaSun2 = thetaSun * thetaSun;
            double thetaSun3 = thetaSun2 * thetaSun;
            double T2 = T * T;

            Z[1] = (0.00166 * thetaSun3 - 0.00375 * thetaSun2 + 0.00209 * thetaSun) * T2 +
                   (-0.02903 * thetaSun3 + 0.06377 * thetaSun2 - 0.03202 * thetaSun + 0.00394) * T +
                   (0.11693 * thetaSun3 - 0.21196 * thetaSun2 + 0.06052 * thetaSun + 0.25886);

            Z[2] = (0.00275 * thetaSun3 - 0.00610 * thetaSun2 + 0.00317 * thetaSun) * T2 +
                (-0.04214 * thetaSun3 + 0.08970 * thetaSun2 - 0.04153 * thetaSun + 0.00516) * T +
                (0.15346 * thetaSun3 - 0.26756 * thetaSun2 + 0.06670 * thetaSun + 0.26688);


            if (sun.Altitude > -18)
            {
                Z[0] = Z[0] + 20;
            }

            // this will compensate sky color during twilight
            if (sun.Altitude < 0 && sun.Altitude > -18)
            {
                Z[1] = 0.28;
                Z[2] = 0.30;
            }

            // Daylight factor (1 = Day, 0 = Night, transparency of the sky background)
            map.DaylightFactor = CalcDaylightFactor();
        }

        private float CalcDaylightFactor()
        {
            double alt = sun.Altitude;

            if (alt >= 0)
            {
                // Angular separation between Sun and Moon disks, in arcseconds
                double delta = Angle.Separation(sun, moon) * 3600.0;

                if (delta < Abs(sdSun - sdMoon))
                {
                    return 0;
                }

                // Solar eclipse (disks are overlapping)
                if (delta <= sdSun + sdMoon)
                {
                    // find overlapping area of two circles
                    // (https://abakbot.ru/online-2/73-ploshhad-peresecheniya-okruzhnostej)

                    double r1 = sdSun;
                    double r2 = sdMoon;
                    double f1 = 2 * Math.Acos((r1 * r1 - r2 * r2 + delta * delta) / (2 * r1 * delta));
                    double f2 = 2 * Math.Acos((r2 * r2 - r1 * r1 + delta * delta) / (2 * r2 * delta));

                    double s1 = r1 * r1 * Math.Sin(f1 - Math.Sin(f1)) / 2;
                    double s2 = r2 * r2 * Math.Sin(f2 - Math.Sin(f2)) / 2;

                    // area of overlapping area
                    double s = s1 + s2;

                    // area of Sun disk
                    double ss = Math.PI * r1 * r1;

                    double percentage = Math.Abs(s / ss);

                    if (percentage <= 0.1)
                    {
                        return (float)(percentage / 0.1);
                    }
                }
            }

            // Absolute value of solar altitude 
            // at the end of Nautical twilight / beginning of Astronomical twilight
            const double nightAlt = 12;

            if (alt >= 0)
                return 1;
            else if (alt < 0 && alt > -nightAlt)
                return 1 - (float)(-alt / nightAlt);
            else
                return 0;
        }

        // Perez function
        private double F(int i, double theta, double gamma)
        {
            /*
            double a = 0.5 * Cos(Angle.ToRadians(sun.Altitude));
            double b = 1 +  10 * Abs(Sin(Angle.ToRadians(sun.Altitude)));
            double c = 1.5 - Cos(Angle.ToRadians(sun.Altitude));
            double d = 2 - Cos(Angle.ToRadians(sun.Altitude));
            double e = 1;
            */

            /*
            double a = CoeffA;
            double b = CoeffB;
            double c = CoeffC;
            double d = CoeffD;
            double e = CoeffE;
            */

            //return (1 + CoeffA * A[i] * Exp(CoeffB * B[i] / Cos(theta))) * (1 + CoeffC * C[i] * Exp(CoeffD * D[i] * gamma) + CoeffE * E[i] * Cos(gamma) * Cos(gamma));


            double alt = sun.Altitude;

            double a = alt <= -12 ? 0.001 : (alt >= -5 ? 0.516 : (0.883857 + 0.0735714 * alt));
            double b = -0.000423647 * alt * alt + 0.0149816 * alt + 0.47203; // (quadratic);// 10; // -6.65374e-7 * Pow(alt, 4) + 0.000121364 * Pow(alt, 3) - 0.00412819 * Pow(alt, 2) - 0.0215041 * alt + 0.554583;
            double c = alt <= -5 ? 0.001 : (alt > 0 ? 1.028 : 0.2054 * alt + 1.028);
            double d = alt <= -5 ? 10 : (alt >= 0 ? 2.504 : (2.504 - 1.4992 * alt));
            double e = 1;// alt <= 0 ? 0.001 : 0.847;

            return (1 + a * A[i] * Exp(b * B[i] / Cos(theta))) * (1 + c * C[i] * Exp(d * D[i] * gamma) + e * E[i] * Cos(gamma) * Cos(gamma));
        }

        public double DaylightFactor { get; private set; } = 1;

        public Color GetColor(CrdsHorizontal p)
        {
            double thetaP = Angle.ToRadians(90 - p.Altitude);

            double gammaP = Angle.ToRadians(Angle.Separation(p, sun));

            double[] L = new double[3];

            for (int i = 0; i < 3; i++)
            {
                L[i] = Z[i] * (F(i, thetaP, gammaP) / F(i, 0, thetaSun));
            }

            L[0] *= DaylightFactor;
            //L[1] = 1;
            //L[2] = 1;

            return ConvertYxyToRGB(L);
        }

        /*
        public double GetTransparency()
        {
            const double nightAlt = 18;
            double alt = sun.Altitude;

            if (alt >= 0)
                return 1 * CoeffT;
            else if (alt < 0 && alt > -nightAlt)
                return 1 - (double)(-alt / nightAlt);
            else
                return 0;
        }
        */


        // Convert from Yxy color system to RGB
        private Color ConvertYxyToRGB(double[] color)
        {
            // rescale Y
            color[0] = 1 - Exp(-color[0] / 25.0);
            double ratio = color[0] / color[2];

            // Convert from Yxy to XYZ
            double X = color[1] * ratio;
            double Y = color[0];
            double Z = ratio - X - Y;

            double r = 3.240479 * X - 1.53715 * Y - 0.498535 * Z;
            double g = -0.969256 * X + 1.875991 * Y + 0.041556 * Z;
            double b = 0.055648 * X - 0.204043 * Y + 1.057311 * Z;

            int R = (int)(r * 255);
            int G = (int)(g * 255);
            int B = (int)(b * 255);

            if (R > 255) R = 255;
            if (G > 255) G = 255;
            if (B > 255) B = 255;

            if (R < 0) R = 0;
            if (G < 0) G = 0;
            if (B < 0) B = 0;

            //int alpha = (int)(255 * GetTransp(sun.Altitude));

            return Color.FromArgb(R, G, B);
        }
    }
}
