using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ADK.Tests
{
    [TestClass]
    public class AppearanceTests
    {
        [TestMethod]
        public void RiseTransitSet()
        {
            {
                CrdsEquatorial[] eq = new CrdsEquatorial[]
                {
                    new CrdsEquatorial(new HMS("2h42m43.25s"), new DMS("+18*02'51.4''")),
                    new CrdsEquatorial(new HMS("2h46m55.51s"), new DMS("+18*26'27.3''")),
                    new CrdsEquatorial(new HMS("2h51m07.69s"), new DMS("+18*49'38.7''"))
                };

                CrdsGeographical location = new CrdsGeographical(42.3333, 71.0833);

                var rts = Appearance.RiseTransitSet(eq, location, 177.74208, 0, 0);

                const double MIN_PER_DAY = 24 * 60;

                Assert.AreEqual(0.51766 * MIN_PER_DAY, rts.Rise * MIN_PER_DAY, 2);
                Assert.AreEqual(0.81980 * MIN_PER_DAY, rts.Transit * MIN_PER_DAY, 2);
                Assert.AreEqual(0.12130 * MIN_PER_DAY, rts.Set * MIN_PER_DAY, 2);
            }
        }
    }
}
