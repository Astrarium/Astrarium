using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADK.Tests
{
    [TestClass]
    public class LunarEphemTests
    {
        /// <summary>
        /// AA(II), example 48.a
        /// </summary>
        [TestMethod]
        public void Phase_PhaseAngle_Elongation()
        {
            CrdsEquatorial eqSun = new CrdsEquatorial(20.6579, 8.6964);
            CrdsEquatorial eqMoon = new CrdsEquatorial(134.6855, 13.7684);

            const double epslion = 23.440636;

            CrdsEcliptical sun = eqSun.ToEcliptical(epslion);
            sun.Distance = 149971520;

            CrdsEcliptical moon = eqMoon.ToEcliptical(epslion);
            moon.Distance = 368410;

            double psi = Appearance.Elongation(sun, moon);
            Assert.AreEqual(110.79, psi, 1e-2);

            double phaseAngle = Appearance.PhaseAngle(psi, sun.Distance, moon.Distance);
            Assert.AreEqual(69.08, phaseAngle, 1e-2);

            double phase = Appearance.Phase(phaseAngle);
            Assert.AreEqual(0.68, phase, 1e-2);
        }

        /// <summary>
        /// AA(II), example 47.a
        /// </summary>
        [TestMethod]
        public void Parallax()
        {
            double pi = LunarEphem.Parallax(368409.7);
            Assert.AreEqual(0.991990, pi, 1e-6);
        }

        /// <summary>
        /// PAWC, ex. 55 (p. 111)
        /// </summary>
        [TestMethod]
        public void PositionAngleOfBrightLimb()
        {
            CrdsEquatorial eqMoon = new CrdsEquatorial(new HMS("14h 35m 02s"), new DMS("-12* 45' 46''"));
            CrdsEquatorial eqSun = new CrdsEquatorial(15.843611 * 15, -20.117778);

            double pa = LunarEphem.PositionAngleOfBrightLimb(eqSun, eqMoon);
            Assert.AreEqual(114.6, pa, 0.1);
        }

        /// <summary>
        /// AA(II), p. 374, example 53.a.
        /// </summary>
        [TestMethod]
        public void PositionAngleOfAxis()
        {
            double jd = 2448724.5;
            CrdsEcliptical ecl = new CrdsEcliptical(new DMS("133* 10' 00''"), new DMS("-3* 13' 45''"));
            double epsilon = 23.440636;
            double deltaPsi = 0.004610;

            double P = LunarEphem.PositionAngleOfAxis(jd, ecl, epsilon, deltaPsi);
            Assert.AreEqual(15.08, P, 1e-2);
        }

        /// <summary>
        /// AA(II), p. 374, example 53.a.
        /// </summary>
        [TestMethod]
        public void Libration()
        {
            double jd = 2448724.5;
            CrdsEcliptical ecl = new CrdsEcliptical(133.167260, -3.229127);
            double deltaPsi = 0.004610;

            Libration le = LunarEphem.Libration(jd, ecl, deltaPsi);
            Assert.AreEqual(-1.23, le.l, 1e-2);
            Assert.AreEqual(4.20, le.b, 1e-2);
        }

        /// <summary>
        /// AA(II), p. 353, examples 49.a, 49.b.
        /// </summary>
        [TestMethod]
        public void NearestPhase()
        {
            {
                // New Moon takes place in February 1977
                Date date = new Date(1977, 2, 15);
                double jd = date.ToJulianEphemerisDay();

                double jdNewMoon = LunarEphem.NearestPhase(jd, MoonPhase.NewMoon);

                // 1 second error
                double error = 1.0 / (24 * 60 * 60);

                Assert.AreEqual(2443192.65118, jdNewMoon, error);
            }

            {
                // First last quarter of 2044
                Date date = new Date(2044, 1, 1);
                double jd = date.ToJulianEphemerisDay();

                double jdLastQuarter = LunarEphem.NearestPhase(jd, MoonPhase.LastQuarter);

                // 1 second error
                double error = 1.0 / (24 * 60 * 60);

                Assert.AreEqual(2467636.49186, jdLastQuarter, error);
            }
        }

        /// <summary>
        /// AA(II), p. 357, example 50.a.
        /// </summary>
        [TestMethod]
        public void NearestApsis()
        {
            Date date = new Date(1988, 10, 1);
            double jd = date.ToJulianEphemerisDay();

            double jsApogee = LunarEphem.NearestApsis(jd, MoonApsis.Apogee);

            // 1 minute error
            double error = 1.0 / (24 * 60);

            Assert.AreEqual(2447442.3537, jsApogee, error);
        }
    }
}
