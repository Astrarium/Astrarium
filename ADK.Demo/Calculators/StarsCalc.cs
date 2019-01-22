using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class StarsCalc : BaseSkyCalc, IEphemProvider<Star>, IInfoProvider<Star>
    {
        /// <summary>
        /// Collection of all stars
        /// </summary>
        private ICollection<Star> Stars = new List<Star>();

        public StarsCalc(Sky sky) : base(sky)
        {
            Sky.AddDataProvider("Stars", () => Stars);
        }

        public override void Calculate(SkyContext context)
        {
            foreach (var star in Stars)
            {
                if (star != null)
                {
                    star.Horizontal = context.Get(Horizontal, star);
                    star.Equatorial = context.Get(Equatorial, star);
                }
            }
        }

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Stars.dat");

            string line = "";
            int len = 0;

            using (var sr = new StreamReader(file, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    len = line.Length;

                    if (line.Length < 197) line += new string(' ', 197 - line.Length);

                    Star star = null;

                    if (line[94] != ' ')
                    {
                        star = new Star();

                        star.Equatorial0.Alpha = new HMS(
                                                    Convert.ToUInt32(line.Substring(75, 2)),
                                                    Convert.ToUInt32(line.Substring(77, 2)),
                                                    Convert.ToDouble(line.Substring(79, 4), CultureInfo.InvariantCulture)
                                                ).ToDecimalAngle();

                        star.Equatorial0.Delta = (line[83] == '-' ? -1 : 1) * new DMS(
                                                    Convert.ToUInt32(line.Substring(84, 2)),
                                                    Convert.ToUInt32(line.Substring(86, 2)),
                                                    Convert.ToUInt32(line.Substring(88, 2))
                                                ).ToDecimalAngle();

                        if (line[148] != ' ')
                        {
                            star.PmAlpha = Convert.ToSingle(line.Substring(148, 6), CultureInfo.InvariantCulture);
                        }
                        if (line[154] != ' ')
                        {
                            star.PmDelta = Convert.ToSingle(line.Substring(154, 6), CultureInfo.InvariantCulture);
                        }

                        star.Mag = Convert.ToSingle(line.Substring(102, 5), CultureInfo.InvariantCulture);
                        star.Color = line[129];
                    }

                    Stars.Add(star);
                }
            }
        }

        #region Ephemeris

        /// <summary>
        /// Gets number of years since J2000.0
        /// </summary>
        private double YearsSince2000(SkyContext c)
        {
            return (c.JulianDay - Date.EPOCH_J2000) / 365.25;
        }

        /// <summary>
        /// Gets precessional elements to convert euqtorial coordinates of stars to current epoch 
        /// </summary>
        private PrecessionalElements GetPrecessionalElements(SkyContext c)
        {
            return Precession.ElementsFK5(Date.EPOCH_J2000, c.JulianDay);
        }

        /// <summary>
        /// Gets equatorial coordinates of a star for current epoch
        /// </summary>
        private CrdsEquatorial Equatorial(SkyContext c, Star star)
        {
            PrecessionalElements p = c.Get(GetPrecessionalElements);
            double years = c.Get(YearsSince2000);

            // Initial coodinates for J2000 epoch
            CrdsEquatorial eq0 = new CrdsEquatorial(star.Equatorial0);

            // Take into account effect of proper motion:
            // now coordinates are for the mean equinox of J2000.0,
            // but for epoch of the target date
            eq0.Alpha += star.PmAlpha * years / 3600.0;
            eq0.Delta += star.PmDelta * years / 3600.0;

            // Equatorial coordinates for the mean equinox and epoch of the target date
            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, p);

            // Nutation effect
            var eq1 = Nutation.NutationEffect(eq, c.NutationElements, c.Epsilon);

            // Aberration effect
            var eq2 = Aberration.AberrationEffect(eq, c.AberrationElements, c.Epsilon);

            // Apparent coordinates of the star
            eq += eq1 + eq2;

            return eq;
        }

        /// <summary>
        /// Gets apparent horizontal coordinates of star for given instant
        /// </summary>
        private CrdsHorizontal Horizontal(SkyContext c, Star star)
        {
            return c.Get(Equatorial, star).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rise, transit and set info for the star
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, Star star)
        {
            double theta0 = Date.ApparentSiderealTime(c.JulianDayMidnight, c.NutationElements.deltaPsi, c.Epsilon);
            var eq = c.Get(Equatorial, star); 
            return Appearance.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        #endregion Ephemeris

        public void ConfigureEphemeris(EphemerisConfig<Star> e)
        {
            e.Add("RTS.Rise", (c, s) => c.Get(RiseTransitSet, s).Rise)
                .WithFormatter(Formatters.Time);

            e.Add("RTS.Transit", (c, s) => c.Get(RiseTransitSet, s).Transit)
                .WithFormatter(Formatters.Time);

            e.Add("RTS.Set", (c, s) => c.Get(RiseTransitSet, s).Set)
               .WithFormatter(Formatters.Time);
        }

        string IInfoProvider<Star>.GetInfo(SkyContext c, Star s)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Rise: ").Append(Formatters.Time.Format(c.Get(RiseTransitSet, s).Rise)).AppendLine();
            sb.Append("Transit: ").Append(Formatters.Time.Format(c.Get(RiseTransitSet, s).Transit)).AppendLine();
            sb.Append("Set: ").Append(Formatters.Time.Format(c.Get(RiseTransitSet, s).Set)).AppendLine();

            return sb.ToString();
        }
    }
}
