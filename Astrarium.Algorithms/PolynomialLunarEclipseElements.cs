using System.Linq;
using static System.Math;
using static Astrarium.Algorithms.Angle;

namespace Astrarium.Algorithms
{
    public class PolynomialLunarEclipseElements
    {
        /// <summary>
        /// Julian Day of elements t0 instant.
        /// </summary>
        public double JulianDay0 { get; set; }

        /// <summary>
        /// Instant of eclipse maximum.
        /// </summary>
        public double JulianDayMaximum { get; set; }

        /// <summary>
        /// DeltaT value (difference between Dynamical and Universal Times).
        /// If not specified, calculated automatically for the <see cref="JulianDay0"/> value.
        /// </summary>
        public double DeltaT { get; set; }

        /// <summary>
        /// Step, in days, between each item in instant Besselian elements series
        /// used to produce this polynomial coefficients.
        /// </summary>
        public double Step { get; set; }

        /// <summary>
        /// Coefficients of X (X-coordinate of center of the Moon in fundamental plane), 
        /// index is a power of t.
        /// </summary>
        public double[] X { get; set; }

        /// <summary>
        /// Coefficients of Y (Y-coordinate of center of the Moon in fundamental plane), 
        /// index is a power of t.
        /// </summary>
        public double[] Y { get; set; }

        /// <summary>
        /// Coefficients of F1 (Earth penumbra radius, in degrees), 
        /// index is a power of t.
        /// </summary>
        public double[] F1 { get; set; }

        /// <summary>
        /// Coefficients of F2 (Earth umbra radius, in degrees), 
        /// index is a power of t.
        /// </summary>
        public double[] F2 { get; set; }

        /// <summary>
        /// Coefficients of F3 (Lunar radius (semidiameter), in degrees), 
        /// index is a power of t.
        /// </summary>
        public double[] F3 { get; set; }

        /// <summary>
        /// Coefficients of Alpha (Geocentric right ascension of the Moon, in degrees), 
        /// index is a power of t.
        /// </summary>
        public double[] Alpha { get; set; }

        /// <summary>
        /// Coefficients of Delta (Geocentric declination of the Moon, in degrees), 
        /// index is a power of t.
        /// </summary>
        public double[] Delta { get; set; }

        /// <summary>
        /// Gets Besselian elements values for specified Juluan Day.
        /// </summary>
        /// <param name="jd">Julian Day of interest.</param>
        /// <returns>Besselian elements for the given instant.</returns>
        public InstantLunarEclipseElements GetInstantBesselianElements(double jd)
        {
            //if (jd < From || jd > To)
            //    throw new ArgumentException($"Polynomial Besselian elements valid only for Julian Day in range [{From} ... {To}].", nameof(jd));

            // difference, with t0, in step units
            double t = (jd - JulianDay0) / Step;

            return new InstantLunarEclipseElements()
            {
                JulianDay = jd,
                DeltaT = DeltaT,
                X = X.Select((x, n) => x * Pow(t, n)).Sum(),
                Y = Y.Select((y, n) => y * Pow(t, n)).Sum(),
                F1 = F1.Select((y, n) => y * Pow(t, n)).Sum(),
                F2 = F2.Select((y, n) => y * Pow(t, n)).Sum(),
                F3 = F3.Select((y, n) => y * Pow(t, n)).Sum(),
                Alpha = To360(Alpha.Select((y, n) => y * Pow(t, n)).Sum()),
                Delta = Delta.Select((y, n) => y * Pow(t, n)).Sum(),
            };
        }
    }
}
