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

            double psi = LunarEphem.Elongation(sun, moon);
            Assert.AreEqual(110.79, psi, 1e-2);

            double phaseAngle = LunarEphem.PhaseAngle(psi, sun.Distance, moon.Distance);
            Assert.AreEqual(69.08, phaseAngle, 1e-2);

            double phase = LunarEphem.Phase(phaseAngle);
            Assert.AreEqual(0.68, phase, 1e-2);
        }
    }
}
