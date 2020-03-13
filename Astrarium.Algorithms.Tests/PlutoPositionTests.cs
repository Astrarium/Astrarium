using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class PlutoPositionTests
    {
        /// <summary>
        /// Example 27.a, AA(II), page 266
        /// </summary>
        [TestMethod]
        public void Position()
        {
            // Mean obliquity of the ecliptic for J2000.0 epoch
            const double epsilonJ2000 = 23.4392912510;

            double jd = 2448908.5;
            double tau = 0;
            double tau0 = 1;

            int iteration = 1;

            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            CrdsHeliocentrical posPluto = null;
            CrdsHeliocentrical hEarth = null;

            while (Math.Abs(tau - tau0) > deltaTau)
            {
                posPluto = PlutoPosition.Position(jd - tau);

                if (iteration == 1)
                {
                    Assert.AreEqual(232.74071, posPluto.L, 1e-5);
                    Assert.AreEqual(14.58782, posPluto.B, 1e-5);
                    Assert.AreEqual(29.711111, posPluto.R, 1e-6);
                }
                else if (iteration == 2)
                {
                    Assert.AreEqual(232.73949, posPluto.L, 1e-5);
                    Assert.AreEqual(14.58801, posPluto.B, 1e-5);
                    Assert.AreEqual(29.711094, posPluto.R, 1e-6);
                }

                // get Earth coordinates
                hEarth = PlanetPositions.GetPlanetCoordinates(3, jd, highPrecision: false, epochOfDate: false);

                // transform to ecliptical coordinates of the Sun
                CrdsEcliptical eclSun = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

                CrdsRectangular rSun = eclSun.ToRectangular(epsilonJ2000);

                if (iteration == 1)
                {
                    Assert.AreEqual(-0.9373959, rSun.X, 1e-6);
                    Assert.AreEqual(-0.3131679, rSun.Y, 1e-6);
                    Assert.AreEqual(-0.1357792, rSun.Z, 1e-6);
                }
                
                var rPluto = new CrdsEcliptical(posPluto.L, posPluto.B, posPluto.R).ToRectangular(epsilonJ2000);

                if (iteration == 1)
                {
                    Assert.AreEqual(-17.4079141, rPluto.X, 1e-5);
                    Assert.AreEqual(-23.9730804, rPluto.Y, 1e-5);
                    Assert.AreEqual(-2.2374228, rPluto.Z, 1e-5);
                }
                else if (iteration == 2)
                {
                    Assert.AreEqual(-17.4083780, rPluto.X, 1e-5);
                    Assert.AreEqual(-23.9727452, rPluto.Y, 1e-5);
                    Assert.AreEqual(-2.2371797, rPluto.Z, 1e-5);
                }

                double x = rPluto.X + rSun.X;
                double y = rPluto.Y + rSun.Y;
                double z = rPluto.Z + rSun.Z;
                double dist = Math.Sqrt(x * x + y * y + z * z);

                if (iteration == 1)
                {
                    Assert.AreEqual(30.528746, dist, 1e-5);
                }
                else if (iteration == 2)
                {
                    Assert.AreEqual(30.528739, dist, 1e-5);
                }

                tau0 = tau;
                tau = PlanetPositions.LightTimeEffect(dist);

                if (iteration == 1 || iteration == 2)
                {
                    Assert.AreEqual(0.17632, tau, 1e-5);
                }

                iteration++;
            }

            // should be only 2 iterations
            Assert.AreEqual(2, iteration - 1);

            // ecliptical coordinates of Pluto, J2000.0 epoch
            var eclPluto = posPluto.ToRectangular(hEarth).ToEcliptical();

            // geocentric astrometric equatorial coordinates of Pluto, J2000.0 epoch
            var eqPluto2000 = eclPluto.ToEquatorial(epsilonJ2000);

            // check coordinates with possible error with 1 arcsecond
            Assert.AreEqual(new HMS("15h 31m 43.8s").ToDecimalAngle(), eqPluto2000.Alpha, 1 / 3600.0);
            Assert.AreEqual(new DMS("-4* 27' 29''").ToDecimalAngle(), eqPluto2000.Delta, 1 / 3600.0);
        }
    }
}
