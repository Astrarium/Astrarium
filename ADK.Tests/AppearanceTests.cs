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

                var rts = Appearance.RiseTransitSet(eq, location, 56, 177.74208, -0.5667);

                const double MIN_PER_DAY = 24 * 60;

                Assert.AreEqual(0.51766 * MIN_PER_DAY, rts.Rise * MIN_PER_DAY, 2);
                Assert.AreEqual(0.81980 * MIN_PER_DAY, rts.Transit * MIN_PER_DAY, 2);
                Assert.AreEqual(0.12130 * MIN_PER_DAY, rts.Set * MIN_PER_DAY, 2);
            }

            //{
                

            //    // Moon 18 Jan 2019

            //    CrdsEquatorial[] eq = new CrdsEquatorial[]
            //    {
            //        new CrdsEquatorial(new HMS("3h44m9.2s"), new DMS("+14*30'18''")),
            //        new CrdsEquatorial(new HMS("4h40m44.8s"), new DMS("+17*46'41''")),
            //        new CrdsEquatorial(new HMS("5h41m17.9s"), new DMS("+20*1'56''")),
            //    };


            //    Date date = new Date(new DateTime(2019, 1, 18, 0, 0, 0, DateTimeKind.Utc).Subtract(TimeSpan.FromHours(3)));
            //    double jd = date.ToJulianEphemerisDay();

            //    var nutation = Nutation.NutationElements(jd);
            //    double epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

            //    double theta0 = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);

            //    CrdsGeographical location = new CrdsGeographical(56.33333, -44.0);

            //    double deltaT = Date.DeltaT(jd);

            //    var rts = Appearance.RiseTransitSet(eq, location, deltaT, theta0, 0.125);

            //    const double MIN_PER_DAY = 24 * 60;

            //    // TODO: not finished yet
            //    Assert.AreEqual(0, rts.Rise * MIN_PER_DAY, 2);
            //    Assert.AreEqual(0, rts.Transit * MIN_PER_DAY, 2);
            //    Assert.AreEqual(0, rts.Set * MIN_PER_DAY, 2);
            //}
        }
    }
}
