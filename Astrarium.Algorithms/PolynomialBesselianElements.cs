using System;
using System.Linq;
using static System.Math;
using static Astrarium.Algorithms.Angle;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents polynomial coefficients to obtain Besselian elements
    /// </summary>
    /// <remarks>
    /// See description of polynomial form here:
    /// https://eclipse.gsfc.nasa.gov/SEmono/reference/explain.html
    /// </remarks>
    public class PolynomialBesselianElements
    {
        /// <summary>
        /// Julian Day of eclipse maximum.
        /// </summary>
        public double JulianDay0 { get; set; }

        /// <summary>
        /// DeltaT value (difference between Dynamical and Universal Times).
        /// If not specified, calculated automatically for the <see cref="JulianDay0"/> value.
        /// </summary>
        public double? DeltaT { get; set; }

        /// <summary>
        /// Step, in days, between each item in instant Besselian elements series
        /// used to produce this polynomial coefficients.
        /// </summary>
        public double Step { get; set; }

        /// <summary>
        /// Coefficients of X (x-coordinate of projection of Moon shadow on fundamental plane), 
        /// index is a power of t.
        /// </summary>
        public double[] X { get; set; }

        /// <summary>
        /// Coefficients of Y (y-coordinate of projection of Moon shadow on fundamental plane), 
        /// index is a power of t.
        /// </summary>
        public double[] Y { get; set; }

        /// <summary>
        /// Coefficients of L1 (radius of penumbral cone projection on fundamental plane, in Earth radii), 
        /// index is a power of t.
        /// </summary>
        public double[] L1 { get; set; }

        /// <summary>
        /// Coefficients of L2 (radius of umbral cone projection on fundamental plane, in Earth radii), 
        /// index is a power of t.
        /// </summary>
        public double[] L2 { get; set; }

        /// <summary>
        /// Coefficients of D (declination of Moon shadow vector, expressed in degrees), 
        /// index is a power of t.
        /// </summary>
        public double[] D { get; set; }

        /// <summary>
        /// Coefficients of Mu (hour angle of Moon shadow vector, expressed in degrees), 
        /// index is a power of t.
        /// </summary>
        public double[] Mu { get; set; }

        /// <summary>
        /// Coefficients of angle of penumbral cone, in degrees
        /// </summary>
        
        // TODO: remove this
        public double[] F1 { get; set; }

        /// <summary>
        /// Coefficients of angle of umbral cone, in degrees
        /// </summary>
        /// 
        // TODO: remove this
        public double[] F2 { get; set; }

        public double tanF1 { get; set; }
        public double tanF2 { get; set; }

        /// <summary>
        /// Gets Besselian elements values for specified Juluan Day.
        /// </summary>
        /// <param name="jd">Julian Day of interest</param>
        /// <returns></returns>
        internal InstantBesselianElements GetInstantBesselianElements(double jd)
        {
            //if (jd < From || jd > To)
            //    throw new ArgumentException($"Polynomial Besselian elements valid only for Julian Day in range [{From} ... {To}].", nameof(jd));

            // difference, with t0, in step units
            double t = (jd - JulianDay0) / Step;

            return new InstantBesselianElements()
            {
                X = X.Select((x, n) => x * Pow(t, n)).Sum(),
                Y = Y.Select((y, n) => y * Pow(t, n)).Sum(),
                L1 = L1.Select((l1, n) => l1 * Pow(t, n)).Sum(),
                L2 = L2.Select((l2, n) => l2 * Pow(t, n)).Sum(),
                D = D.Select((d, n) => d * Pow(t, n)).Sum(),
                Mu = To360(Mu.Select((mu, n) => mu * Pow(t, n)).Sum()),
                F1 = F1.Select((f1, n) => f1 * Pow(t, n)).Sum(),
                F2 = F2.Select((f2, n) => f2 * Pow(t, n)).Sum(),
                dX = Derivative(X, t),
                dY = Derivative(Y, t),
                //dL1 = Derivative(L1, t),
                //dL2 = Derivative(L2, t),
                //dD = Derivative(D, t),
                //dMu = Derivative(Mu, t)
            };
        }

        /// <summary>
        /// Finds derivative of specified function at desired time instant t.
        /// </summary>
        /// <param name="f">Coefficients defining the function</param>
        /// <param name="t">Time instant.</param>
        /// <returns>Value of derivate of function at time t.</returns>
        private double Derivative(double[] f, double t)
        {
            return f[1] + 2 * f[2] * t + 3 * f[3] * t * t;
        }

        /// <summary>
        /// Gets minimal value of Julian Day range
        /// the polynomial Besselian elements are valid from
        /// </summary>
        public double From
        {
            get
            {
                return JulianDay0 - 2 * Step;
            }
        }

        /// <summary>
        /// Gets maximal value of Julian Day range
        /// the polynomial Besselian elements are valid to
        /// </summary>
        public double To
        {
            get
            {
                return JulianDay0 + 2 * Step;
            }
        }
    }
}
