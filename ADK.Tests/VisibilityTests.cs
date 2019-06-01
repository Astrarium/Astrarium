using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ADK.Tests
{
    [TestClass]
    public class VisibilityTests
    {
        [TestMethod]
        public void RiseTransitSet()
        {
            // Venus at Boston, 20 March 1988
            {
                CrdsEquatorial[] eq = new CrdsEquatorial[]
                {                    
                    new CrdsEquatorial(new HMS("02h 46m 25.086s"), new DMS("+18° 23' 36.14''")),
                    new CrdsEquatorial(new HMS("02h 48m 31.193s"), new DMS("+18° 35' 16.41''")),
                    new CrdsEquatorial(new HMS("02h 50m 37.273s"), new DMS("+18° 46' 50.47''"))
                };

                CrdsGeographical location = new CrdsGeographical(71.0833, 42.3333);

                var rts = Visibility.RiseTransitSet(eq, location, 177.74208, 0, 0);

                // 2 minute error
                const double error = 2.0 / (24 * 60);
               
                Assert.AreEqual(new TimeSpan(12, 27, 0).TotalDays, rts.Rise, error);
                Assert.AreEqual(new TimeSpan(19, 39, 0).TotalDays, rts.Transit, error);
                Assert.AreEqual(new TimeSpan(02, 50, 0).TotalDays, rts.Set, error);
            }

            // Antares at Boston, 20 March 1988
            {
                var eq = new CrdsEquatorial(new HMS("16h 28m 41s"), new DMS("-26° 24' 30''"));

                CrdsGeographical location = new CrdsGeographical(71.0833, 42.3333);

                var rts = Visibility.RiseTransitSet(eq, location, 177.74208);

                // 2 minute error
                const double error = 2.0 / (24 * 60);

                Assert.AreEqual(new TimeSpan(05, 07, 0).TotalDays, rts.Rise, error);
                Assert.AreEqual(new TimeSpan(09, 19, 0).TotalDays, rts.Transit, error);
                Assert.AreEqual(new TimeSpan(13, 31, 0).TotalDays, rts.Set, error);
            }
        }
    }
}
