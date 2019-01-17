using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADK.Tests
{
    [TestClass]
    public class AppearanceTests
    {
        [TestMethod]
        public void RiseTransitSet()
        {
            double jd = Date.JulianEphemerisDay(new Date(1988, 3, 20));

            CrdsEquatorial[] eq = new CrdsEquatorial[]
            {
                new CrdsEquatorial(new HMS("2h42m43.25s"), new DMS("+18*02'51.4''")),
                new CrdsEquatorial(new HMS("2h46m55.51s"), new DMS("+18*26'27.3''")),
                new CrdsEquatorial(new HMS("2h51m07.69s"), new DMS("+18*49'38.7''"))
            };

            CrdsGeographical location = new CrdsGeographical(42.3333, 71.0833);

            var rts = Appearance.RiseTransitSet(jd, eq, location, 177.74208, -0.5667);

            Assert.AreEqual(24 * 0.51766, rts.Rise);
        }
    }
}
