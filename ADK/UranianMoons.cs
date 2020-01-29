using static System.Math;
using static ADK.Angle;

namespace ADK
{
    public static class UranianMoons
    {
        /// <summary>
        /// Count of Uranus moons 
        /// </summary>
        private const int MOONS_COUNT = 5;

        /// <summary>
        /// 1 a.u. (astronomical unit) in km
        /// </summary>
        private const double AU = 149597870;

        public static double Semidiameter(int index, double distance)
        {
            double[] d = { 480, 1155, 1170, 1576, 1520 };
            return ToDegrees(Atan2(d[index - 1] / 2.0, AU)) / distance * 3600.0;
        }

        public static CrdsRectangular[] Positions(double jd, CrdsHeliocentrical earth, CrdsHeliocentrical uranus)
        {
            CrdsRectangular[] moons = new CrdsRectangular[MOONS_COUNT];

            // Rectangular topocentrical coordinates of Uranus
            CrdsRectangular rectUranus = uranus.ToRectangular(earth);

            // Ecliptical coordinates of Uranus
            CrdsEcliptical eclUranus = rectUranus.ToEcliptical();

            // Distance from Earth to Uranus, in AU
            double distanceUranus = eclUranus.Distance;

            // Angular radius of Uranus, in degrees
            double angular = PlanetEphem.Semidiameter(7, distanceUranus) / 3600.0;

            // light-time effect
            double tau = PlanetPositions.LightTimeEffect(distanceUranus);

            double t = jd - 2444239.5 - tau;
            double[] elem = new double[6 * MOONS_COUNT];
            double[] an = new double[MOONS_COUNT];
            double[] ae = new double[MOONS_COUNT];
            double[] ai = new double[MOONS_COUNT];

            // Calculate GUST86 elements:

            for (int i = 0; i < 5; i++)
            {
                an[i] = IEEERemainder(fqn[i] * t + phn[i], 2 * PI);
                ae[i] = IEEERemainder(fqe[i] * t + phe[i], 2 * PI);
                ai[i] = IEEERemainder(fqi[i] * t + phi[i], 2 * PI);
            }

            elem[0 * 6 + 0] = 4.44352267
                        - Cos(an[0] - an[1] * 3.0 + an[2] * 2.0) * 3.492e-5
                        + Cos(an[0] * 2.0 - an[1] * 6.0 + an[2] * 4.0) * 8.47e-6
                        + Cos(an[0] * 3.0 - an[1] * 9.0 + an[2] * 6.0) * 1.31e-6
                        - Cos(an[0] - an[1]) * 5.228e-5
                        - Cos(an[0] * 2.0 - an[1] * 2.0) * 1.3665e-4;
            elem[0 * 6 + 1] =
                          Sin(an[0] - an[1] * 3.0 + an[2] * 2.0) * .02547217
                        - Sin(an[0] * 2.0 - an[1] * 6.0 + an[2] * 4.0) * .00308831
                        - Sin(an[0] * 3.0 - an[1] * 9.0 + an[2] * 6.0) * 3.181e-4
                        - Sin(an[0] * 4.0 - an[1] * 12 + an[2] * 8.0) * 3.749e-5
                        - Sin(an[0] - an[1]) * 5.785e-5
                        - Sin(an[0] * 2.0 - an[1] * 2.0) * 6.232e-5
                        - Sin(an[0] * 3.0 - an[1] * 3.0) * 2.795e-5
                        + t * 4.44519055 - .23805158;
            elem[0 * 6 + 2] = Cos(ae[0]) * .00131238
                        + Cos(ae[1]) * 7.181e-5
                        + Cos(ae[2]) * 6.977e-5
                        + Cos(ae[3]) * 6.75e-6
                        + Cos(ae[4]) * 6.27e-6
                        + Cos(an[0]) * 1.941e-4
                        - Cos(-an[0] + an[1] * 2.0) * 1.2331e-4
                        + Cos(an[0] * -2.0 + an[1] * 3.0) * 3.952e-5;
            elem[0 * 6 + 3] = Sin(ae[0]) * .00131238
                        + Sin(ae[1]) * 7.181e-5
                        + Sin(ae[2]) * 6.977e-5
                        + Sin(ae[3]) * 6.75e-6
                        + Sin(ae[4]) * 6.27e-6
                        + Sin(an[0]) * 1.941e-4
                        - Sin(-an[0] + an[1] * 2.0) * 1.2331e-4
                        + Sin(an[0] * -2.0 + an[1] * 3.0) * 3.952e-5;
            elem[0 * 6 + 4] = Cos(ai[0]) * .03787171
                        + Cos(ai[1]) * 2.701e-5
                        + Cos(ai[2]) * 3.076e-5
                        + Cos(ai[3]) * 1.218e-5
                        + Cos(ai[4]) * 5.37e-6;
            elem[0 * 6 + 4] = Sin(ai[0]) * .03787171
                        + Sin(ai[1]) * 2.701e-5
                        + Sin(ai[2]) * 3.076e-5
                        + Sin(ai[3]) * 1.218e-5
                        + Sin(ai[4]) * 5.37e-6;

            elem[1 * 6 + 0] = 2.49254257
                        + Cos(an[0] - an[1] * 3.0 + an[2] * 2.0) * 2.55e-6
                        - Cos(an[1] - an[2]) * 4.216e-5
                        - Cos(an[1] * 2.0 - an[2] * 2.0) * 1.0256e-4;
            elem[1 * 6 + 1] =
                        -Sin(an[0] - an[1] * 3.0 + an[2] * 2.0) * .0018605
                        + Sin(an[0] * 2.0 - an[1] * 6.0 + an[2] * 4.0) * 2.1999e-4
                        + Sin(an[0] * 3.0 - an[1] * 9.0 + an[2] * 6.0) * 2.31e-5
                        + Sin(an[0] * 4.0 - an[1] * 12 + an[2] * 8.0) * 4.3e-6
                        - Sin(an[1] - an[2]) * 9.011e-5
                        - Sin(an[1] * 2.0 - an[2] * 2.0) * 9.107e-5
                        - Sin(an[1] * 3.0 - an[2] * 3.0) * 4.275e-5
                        - Sin(an[1] * 2.0 - an[3] * 2.0) * 1.649e-5
                        + t * 2.49295252 + 3.09804641;
            elem[1 * 6 + 2] = Cos(ae[0]) * -3.35e-6
                        + Cos(ae[1]) * .00118763
                        + Cos(ae[2]) * 8.6159e-4
                        + Cos(ae[3]) * 7.15e-5
                        + Cos(ae[4]) * 5.559e-5
                        - Cos(-an[1] + an[2] * 2.0) * 8.46e-5
                        + Cos(an[1] * -2.0 + an[2] * 3.0) * 9.181e-5
                        + Cos(-an[1] + an[3] * 2.0) * 2.003e-5
                        + Cos(an[1]) * 8.977e-5;
            elem[1 * 6 + 3] = Sin(ae[0]) * -3.35e-6
                        + Sin(ae[1]) * .00118763
                        + Sin(ae[2]) * 8.6159e-4
                        + Sin(ae[3]) * 7.15e-5
                        + Sin(ae[4]) * 5.559e-5
                        - Sin(-an[1] + an[2] * 2.0) * 8.46e-5
                        + Sin(an[1] * -2.0 + an[2] * 3.0) * 9.181e-5
                        + Sin(-an[1] + an[3] * 2.0) * 2.003e-5
                        + Sin(an[1]) * 8.977e-5;
            elem[1 * 6 + 4] = Cos(ai[0]) * -1.2175e-4
                        + Cos(ai[1]) * 3.5825e-4
                        + Cos(ai[2]) * 2.9008e-4
                        + Cos(ai[3]) * 9.778e-5
                        + Cos(ai[4]) * 3.397e-5;
            elem[1 * 6 + 5] = Sin(ai[0]) * -1.2175e-4
                        + Sin(ai[1]) * 3.5825e-4
                        + Sin(ai[2]) * 2.9008e-4
                        + Sin(ai[3]) * 9.778e-5
                        + Sin(ai[4]) * 3.397e-5;
            elem[2 * 6 + 0] = 1.5159549
                        + Cos(an[2] - an[3] * 2.0 + ae[2]) * 9.74e-6
                        - Cos(an[1] - an[2]) * 1.06e-4
                        + Cos(an[1] * 2.0 - an[2] * 2.0) * 5.416e-5
                        - Cos(an[2] - an[3]) * 2.359e-5
                        - Cos(an[2] * 2.0 - an[3] * 2.0) * 7.07e-5
                        - Cos(an[2] * 3.0 - an[3] * 3.0) * 3.628e-5;
            elem[2 * 6 + 1] =
                          Sin(an[0] - an[1] * 3.0 + an[2] * 2.0) * 6.6057e-4
                        - Sin(an[0] * 2.0 - an[1] * 6.0 + an[2] * 4.0) * 7.651e-5
                        - Sin(an[0] * 3.0 - an[1] * 9.0 + an[2] * 6.0) * 8.96e-6
                        - Sin(an[0] * 4.0 - an[1] * 12.0 + an[2] * 8.0) * 2.53e-6
                        - Sin(an[2] - an[3] * 4.0 + an[4] * 3.0) * 5.291e-5
                        - Sin(an[2] - an[3] * 2.0 + ae[4]) * 7.34e-6
                        - Sin(an[2] - an[3] * 2.0 + ae[3]) * 1.83e-6
                        + Sin(an[2] - an[3] * 2.0 + ae[2]) * 1.4791e-4
                        + Sin(an[2] - an[3] * 2.0 + ae[1]) * -7.77e-6
                        + Sin(an[1] - an[2]) * 9.776e-5
                        + Sin(an[1] * 2.0 - an[2] * 2.0) * 7.313e-5
                        + Sin(an[1] * 3.0 - an[2] * 3.0) * 3.471e-5
                        + Sin(an[1] * 4.0 - an[2] * 4.0) * 1.889e-5
                        - Sin(an[2] - an[3]) * 6.789e-5
                        - Sin(an[2] * 2.0 - an[3] * 2.0) * 8.286e-5
                        + Sin(an[2] * 3.0 - an[3] * 3.0) * -3.381e-5
                        - Sin(an[2] * 4.0 - an[3] * 4.0) * 1.579e-5
                        - Sin(an[2] - an[4]) * 1.021e-5
                        - Sin(an[2] * 2.0 - an[4] * 2.0) * 1.708e-5
                        + t * 1.51614811 + 2.28540169;
            elem[2 * 6 + 2] = Cos(ae[0]) * -2.1e-7
                        - Cos(ae[1]) * 2.2795e-4
                        + Cos(ae[2]) * .00390469
                        + Cos(ae[3]) * 3.0917e-4
                        + Cos(ae[4]) * 2.2192e-4
                        + Cos(an[1]) * 2.934e-5
                        + Cos(an[2]) * 2.62e-5
                        + Cos(-an[1] + an[2] * 2.0) * 5.119e-5
                        - Cos(an[1] * -2.0 + an[2] * 3.0) * 1.0386e-4
                        - Cos(an[1] * -3.0 + an[2] * 4.0) * 2.716e-5
                        + Cos(an[3]) * -1.622e-5
                        + Cos(-an[2] + an[3] * 2.0) * 5.4923e-4
                        + Cos(an[2] * -2.0 + an[3] * 3.0) * 3.47e-5
                        + Cos(an[2] * -3.0 + an[3] * 4.0) * 1.281e-5
                        + Cos(-an[2] + an[4] * 2.0) * 2.181e-5
                        + Cos(an[2]) * 4.625e-5;
            elem[2 * 6 + 3] = Sin(ae[0]) * -2.1e-7
                        - Sin(ae[1]) * 2.2795e-4
                        + Sin(ae[2]) * .00390469
                        + Sin(ae[3]) * 3.0917e-4
                        + Sin(ae[4]) * 2.2192e-4
                        + Sin(an[1]) * 2.934e-5
                        + Sin(an[2]) * 2.62e-5
                        + Sin(-an[1] + an[2] * 2.0) * 5.119e-5
                        - Sin(an[1] * -2.0 + an[2] * 3.0) * 1.0386e-4
                        - Sin(an[1] * -3.0 + an[2] * 4.0) * 2.716e-5
                        + Sin(an[3]) * -1.622e-5
                        + Sin(-an[2] + an[3] * 2.0) * 5.4923e-4
                        + Sin(an[2] * -2.0 + an[3] * 3.0) * 3.47e-5
                        + Sin(an[2] * -3.0 + an[3] * 4.0) * 1.281e-5
                        + Sin(-an[2] + an[4] * 2.0) * 2.181e-5
                        + Sin(an[2]) * 4.625e-5;
            elem[2 * 6 + 4] = Cos(ai[0]) * -1.086e-5
                        - Cos(ai[1]) * 8.151e-5
                        + Cos(ai[2]) * .00111336
                        + Cos(ai[3]) * 3.5014e-4
                        + Cos(ai[4]) * 1.065e-4;
            elem[2 * 6 + 5] = Sin(ai[0]) * -1.086e-5
                        - Sin(ai[1]) * 8.151e-5
                        + Sin(ai[2]) * .00111336
                        + Sin(ai[3]) * 3.5014e-4
                        + Sin(ai[4]) * 1.065e-4;
            elem[3 * 6 + 0] = .72166316
                        - Cos(an[2] - an[3] * 2.0 + ae[2]) * 2.64e-6
                        - Cos(an[3] * 2.0 - an[4] * 3.0 + ae[4]) * 2.16e-6
                        + Cos(an[3] * 2.0 - an[4] * 3.0 + ae[3]) * 6.45e-6
                        - Cos(an[3] * 2.0 - an[4] * 3.0 + ae[2]) * 1.11e-6
                        + Cos(an[1] - an[3]) * -6.223e-5
                        - Cos(an[2] - an[3]) * 5.613e-5
                        - Cos(an[3] - an[4]) * 3.994e-5
                        - Cos(an[3] * 2.0 - an[4] * 2.0) * 9.185e-5
                        - Cos(an[3] * 3.0 - an[4] * 3.0) * 5.831e-5
                        - Cos(an[3] * 4.0 - an[4] * 4.0) * 3.86e-5
                        - Cos(an[3] * 5.0 - an[4] * 5.0) * 2.618e-5
                        - Cos(an[3] * 6.0 - an[4] * 6.0) * 1.806e-5;
            elem[3 * 6 + 1] =
                          Sin(an[2] - an[3] * 4.0 + an[4] * 3.0) * 2.061e-5
                        - Sin(an[2] - an[3] * 2.0 + ae[4]) * 2.07e-6
                        - Sin(an[2] - an[3] * 2.0 + ae[3]) * 2.88e-6
                        - Sin(an[2] - an[3] * 2.0 + ae[2]) * 4.079e-5
                        + Sin(an[2] - an[3] * 2.0 + ae[1]) * 2.11e-6
                        - Sin(an[3] * 2.0 - an[4] * 3.0 + ae[4]) * 5.183e-5
                        + Sin(an[3] * 2.0 - an[4] * 3.0 + ae[3]) * 1.5987e-4
                        + Sin(an[3] * 2.0 - an[4] * 3.0 + ae[2]) * -3.505e-5
                        - Sin(an[3] * 3.0 - an[4] * 4.0 + ae[4]) * 1.56e-6
                        + Sin(an[1] - an[3]) * 4.054e-5
                        + Sin(an[2] - an[3]) * 4.617e-5
                        - Sin(an[3] - an[4]) * 3.1776e-4
                        - Sin(an[3] * 2.0 - an[4] * 2.0) * 3.0559e-4
                        - Sin(an[3] * 3.0 - an[4] * 3.0) * 1.4836e-4
                        - Sin(an[3] * 4.0 - an[4] * 4.0) * 8.292e-5
                        + Sin(an[3] * 5.0 - an[4] * 5.0) * -4.998e-5
                        - Sin(an[3] * 6.0 - an[4] * 6.0) * 3.156e-5
                        - Sin(an[3] * 7.0 - an[4] * 7.0) * 2.056e-5
                        - Sin(an[3] * 8.0 - an[4] * 8.0) * 1.369e-5
                        + t * .72171851 + .85635879;
            elem[3 * 6 + 2] = Cos(ae[0]) * -2e-8
                        - Cos(ae[1]) * 1.29e-6
                        - Cos(ae[2]) * 3.2451e-4
                        + Cos(ae[3]) * 9.3281e-4
                        + Cos(ae[4]) * .00112089
                        + Cos(an[1]) * 3.386e-5
                        + Cos(an[3]) * 1.746e-5
                        + Cos(-an[1] + an[3] * 2.0) * 1.658e-5
                        + Cos(an[2]) * 2.889e-5
                        - Cos(-an[2] + an[3] * 2.0) * 3.586e-5
                        + Cos(an[3]) * -1.786e-5
                        - Cos(an[4]) * 3.21e-5
                        - Cos(-an[3] + an[4] * 2.0) * 1.7783e-4
                        + Cos(an[3] * -2.0 + an[4] * 3.0) * 7.9343e-4
                        + Cos(an[3] * -3.0 + an[4] * 4.0) * 9.948e-5
                        + Cos(an[3] * -4.0 + an[4] * 5.0) * 4.483e-5
                        + Cos(an[3] * -5.0 + an[4] * 6.0) * 2.513e-5
                        + Cos(an[3] * -6.0 + an[4] * 7.0) * 1.543e-5;
            elem[3 * 6 + 3] = Sin(ae[0]) * -2e-8
                        - Sin(ae[1]) * 1.29e-6
                        - Sin(ae[2]) * 3.2451e-4
                        + Sin(ae[3]) * 9.3281e-4
                        + Sin(ae[4]) * .00112089
                        + Sin(an[1]) * 3.386e-5
                        + Sin(an[3]) * 1.746e-5
                        + Sin(-an[1] + an[3] * 2.0) * 1.658e-5
                        + Sin(an[2]) * 2.889e-5
                        - Sin(-an[2] + an[3] * 2.0) * 3.586e-5
                        + Sin(an[3]) * -1.786e-5
                        - Sin(an[4]) * 3.21e-5
                        - Sin(-an[3] + an[4] * 2.0) * 1.7783e-4
                        + Sin(an[3] * -2.0 + an[4] * 3.0) * 7.9343e-4
                        + Sin(an[3] * -3.0 + an[4] * 4.0) * 9.948e-5
                        + Sin(an[3] * -4.0 + an[4] * 5.0) * 4.483e-5
                        + Sin(an[3] * -5.0 + an[4] * 6.0) * 2.513e-5
                        + Sin(an[3] * -6.0 + an[4] * 7.0) * 1.543e-5;
            elem[3 * 6 + 4] = Cos(ai[0]) * -1.43e-6
                        - Cos(ai[1]) * 1.06e-6
                        - Cos(ai[2]) * 1.4013e-4
                        + Cos(ai[3]) * 6.8572e-4
                        + Cos(ai[4]) * 3.7832e-4;
            elem[3 * 6 + 5] = Sin(ai[0]) * -1.43e-6
                        - Sin(ai[1]) * 1.06e-6
                        - Sin(ai[2]) * 1.4013e-4
                        + Sin(ai[3]) * 6.8572e-4
                        + Sin(ai[4]) * 3.7832e-4;
            elem[4 * 6 + 0] = .46658054
                        + Cos(an[3] * 2.0 - an[4] * 3.0 + ae[4]) * 2.08e-6
                        - Cos(an[3] * 2.0 - an[4] * 3.0 + ae[3]) * 6.22e-6
                        + Cos(an[3] * 2.0 - an[4] * 3.0 + ae[2]) * 1.07e-6
                        - Cos(an[1] - an[4]) * 4.31e-5
                        + Cos(an[2] - an[4]) * -3.894e-5
                        - Cos(an[3] - an[4]) * 8.011e-5
                        + Cos(an[3] * 2.0 - an[4] * 2.0) * 5.906e-5
                        + Cos(an[3] * 3.0 - an[4] * 3.0) * 3.749e-5
                        + Cos(an[3] * 4.0 - an[4] * 4.0) * 2.482e-5
                        + Cos(an[3] * 5.0 - an[4] * 5.0) * 1.684e-5;
            elem[4 * 6 + 1] =
                        -Sin(an[2] - an[3] * 4.0 + an[4] * 3.0) * 7.82e-6
                        + Sin(an[3] * 2.0 - an[4] * 3.0 + ae[4]) * 5.129e-5
                        - Sin(an[3] * 2.0 - an[4] * 3.0 + ae[3]) * 1.5824e-4
                        + Sin(an[3] * 2.0 - an[4] * 3.0 + ae[2]) * 3.451e-5
                        + Sin(an[1] - an[4]) * 4.751e-5
                        + Sin(an[2] - an[4]) * 3.896e-5
                        + Sin(an[3] - an[4]) * 3.5973e-4
                        + Sin(an[3] * 2.0 - an[4] * 2.0) * 2.8278e-4
                        + Sin(an[3] * 3.0 - an[4] * 3.0) * 1.386e-4
                        + Sin(an[3] * 4.0 - an[4] * 4.0) * 7.803e-5
                        + Sin(an[3] * 5.0 - an[4] * 5.0) * 4.729e-5
                        + Sin(an[3] * 6.0 - an[4] * 6.0) * 3e-5
                        + Sin(an[3] * 7.0 - an[4] * 7.0) * 1.962e-5
                        + Sin(an[3] * 8.0 - an[4] * 8.0) * 1.311e-5
                        + t * .46669212 - .9155918;
            elem[4 * 6 + 2] = Cos(ae[1]) * -3.5e-7
                        + Cos(ae[2]) * 7.453e-5
                        - Cos(ae[3]) * 7.5868e-4
                        + Cos(ae[4]) * .00139734
                        + Cos(an[1]) * 3.9e-5
                        + Cos(-an[1] + an[4] * 2.0) * 1.766e-5
                        + Cos(an[2]) * 3.242e-5
                        + Cos(an[3]) * 7.975e-5
                        + Cos(an[4]) * 7.566e-5
                        + Cos(-an[3] + an[4] * 2.0) * 1.3404e-4
                        - Cos(an[3] * -2.0 + an[4] * 3.0) * 9.8726e-4
                        - Cos(an[3] * -3.0 + an[4] * 4.0) * 1.2609e-4
                        - Cos(an[3] * -4.0 + an[4] * 5.0) * 5.742e-5
                        - Cos(an[3] * -5.0 + an[4] * 6.0) * 3.241e-5
                        - Cos(an[3] * -6.0 + an[4] * 7.0) * 1.999e-5
                        - Cos(an[3] * -7.0 + an[4] * 8.0) * 1.294e-5;
            elem[4 * 6 + 3] = Sin(ae[1]) * -3.5e-7
                        + Sin(ae[2]) * 7.453e-5
                        - Sin(ae[3]) * 7.5868e-4
                        + Sin(ae[4]) * .00139734
                        + Sin(an[1]) * 3.9e-5
                        + Sin(-an[1] + an[4] * 2.0) * 1.766e-5
                        + Sin(an[2]) * 3.242e-5
                        + Sin(an[3]) * 7.975e-5
                        + Sin(an[4]) * 7.566e-5
                        + Sin(-an[3] + an[4] * 2.0) * 1.3404e-4
                        - Sin(an[3] * -2.0 + an[4] * 3.0) * 9.8726e-4
                        - Sin(an[3] * -3.0 + an[4] * 4.0) * 1.2609e-4
                        - Sin(an[3] * -4.0 + an[4] * 5.0) * 5.742e-5
                        - Sin(an[3] * -5.0 + an[4] * 6.0) * 3.241e-5
                        - Sin(an[3] * -6.0 + an[4] * 7.0) * 1.999e-5
                        - Sin(an[3] * -7.0 + an[4] * 8.0) * 1.294e-5;
            elem[4 * 6 + 4] = Cos(ai[0]) * -4.4e-7
                        - Cos(ai[1]) * 3.1e-7
                        + Cos(ai[2]) * 3.689e-5
                        - Cos(ai[3]) * 5.9633e-4
                        + Cos(ai[4]) * 4.5169e-4;
            elem[4 * 6 + 5] = Sin(ai[0]) * -4.4e-7
                        - Sin(ai[1]) * 3.1e-7
                        + Sin(ai[2]) * 3.689e-5
                        - Sin(ai[3]) * 5.9633e-4
                        + Sin(ai[4]) * 4.5169e-4;

            // Get rectangular (Uranus-reffered) coordinates of moons
            CrdsRectangular[] gust86Rect = new CrdsRectangular[MOONS_COUNT];

            for (int body = 0; body < MOONS_COUNT; body++)
            {
                double[] elem_body = new double[6];
                for (int i = 0; i < 6; i++)
                {
                    elem_body[i] = elem[body * 6 + i];
                }

                double[] x = new double[3];
                EllipticToRectangularN(gust86_rmu[body], elem_body, ref x);

                gust86Rect[body] = new CrdsRectangular();
                gust86Rect[body].X = GUST86toVsop87[0] * x[0] + GUST86toVsop87[1] * x[1] + GUST86toVsop87[2] * x[2];
                gust86Rect[body].Y = GUST86toVsop87[3] * x[0] + GUST86toVsop87[4] * x[1] + GUST86toVsop87[5] * x[2];
                gust86Rect[body].Z = GUST86toVsop87[6] * x[0] + GUST86toVsop87[7] * x[1] + GUST86toVsop87[8] * x[2];
            }

            for (int i = 0; i < MOONS_COUNT; i++)
            {
                moons[i] = new CrdsRectangular(
                    rectUranus.X + gust86Rect[i].X,
                    rectUranus.Y + gust86Rect[i].Y,
                    rectUranus.Z + gust86Rect[i].Z
                );
            }

            return moons;
        }

        private static void EllipticToRectangularN(double mu, double[] elem, ref double[] xyz)
        {
            double n = elem[0];
            double a = Exp(Log(mu / (n * n)) / 3.0);
            EllipticToRectangular(mu, a, elem, ref xyz);
        }

        private static void EllipticToRectangular(double mu, double a,
                                                    double[] elem, ref double[] xyz)
        {
            double L = IEEERemainder(elem[1], 2.0 * PI);

            double Le = L - elem[2] * Sin(L) + elem[3] * Cos(L);
            for (; ; )
            {
                double cLe = Cos(Le);
                double sLe = Sin(Le);
                double dLe = (L - Le + elem[2] * sLe - elem[3] * cLe)
                                / (1.0 - elem[2] * cLe - elem[3] * sLe);
                Le += dLe;
                if (Abs(dLe) <= 1e-14) break; /* L1: <1e-12 */
            }

            {
                double cLe = Cos(Le);
                double sLe = Sin(Le);

                double dlf = -elem[2] * sLe + elem[3] * cLe;
                double phi = Sqrt(1.0 - elem[2] * elem[2] - elem[3] * elem[3]);
                double psi = 1.0 / (1.0 + phi);

                double x1 = a * (cLe - elem[2] - psi * dlf * elem[3]);
                double y1 = a * (sLe - elem[3] + psi * dlf * elem[2]);

                double elem_4q = elem[4] * elem[4];
                double elem_5q = elem[5] * elem[5];
                double dwho = 2.0 * Sqrt(1.0 - elem_4q - elem_5q);
                double rtp = 1.0 - elem_5q - elem_5q;
                double rtq = 1.0 - elem_4q - elem_4q;
                double rdg = 2.0 * elem[5] * elem[4];

                xyz[0] = x1 * rtp + y1 * rdg;
                xyz[1] = x1 * rdg + y1 * rtq;
                xyz[2] = (-x1 * elem[5] + y1 * elem[4]) * dwho;
            }
        }

        private static double[] fqn = {4.44519055,
                                   2.492952519,
                                   1.516148111,
                                   0.721718509,
                                   0.46669212};
        private static double[] fqe = {20.082*PI/(180*365.25),
                                    6.217*PI/(180*365.25),
                                    2.865*PI/(180*365.25),
                                    2.078*PI/(180*365.25),
                                    0.386*PI/(180*365.25)};
        private static double[] fqi = {-20.309*PI/(180*365.25),
                                    -6.288*PI/(180*365.25),
                                    -2.836*PI/(180*365.25),
                                    -1.843*PI/(180*365.25),
                                    -0.259*PI/(180*365.25)};
        private static double[] phn = {-0.238051,
                                    3.098046,
                                    2.285402,
                                    0.856359,
                                   -0.915592};
        private static double[] phe = {0.611392,
                                   2.408974,
                                   2.067774,
                                   0.735131,
                                   0.426767};
        private static double[] phi = {5.702313,
                                   0.395757,
                                   0.589326,
                                   1.746237,
                                   4.206896};

        private static double[] gust86_rmu = {
               1.291892353675174e-08,
               1.291910570526396e-08,
               1.291910102284198e-08,
               1.291942656265575e-08,
               1.291935967091320e-08};

        private static double[] GUST86toVsop87 = {
           9.753206632086812015e-01, 6.194425668001473004e-02, 2.119257251551559653e-01,
          -2.006444610981783542e-01,-1.519328516640849367e-01, 9.678110398294910731e-01,
           9.214881523275189928e-02,-9.864478281437795399e-01,-1.357544776485127136e-01
        };
    }
}
