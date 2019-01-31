using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class StarsCalc : BaseSkyCalc, IEphemProvider<Star>, IInfoProvider<Star>
    {
        private readonly string STARS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Stars.dat");
        
        private readonly string NAMES_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/StarNames.dat");

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
                }
            }
        }

        public override void Initialize()
        {
            string line = "";

            using (var sr = new StreamReader(STARS_FILE, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();

                    Star star = null;

                    if (line[94] != ' ')
                    {
                        star = new Star();

                        star.Number = ushort.Parse(line.Substring(0, 4).Trim());

                        star.Name = line.Substring(4, 10);

                        string hdNumber = line.Substring(25, 6).Trim();
                        if (!string.IsNullOrEmpty(hdNumber))
                        {
                            star.HDNumber = uint.Parse(hdNumber);
                        }

                        string saoNumber = line.Substring(31, 6).Trim();
                        if (!string.IsNullOrEmpty(saoNumber))
                        {
                            star.SAONumber = uint.Parse(saoNumber);
                        }

                        string fk5Number = line.Substring(37, 4).Trim();
                        if (!string.IsNullOrEmpty(fk5Number))
                        {
                            star.FK5Number = ushort.Parse(fk5Number);
                        }

                        string varName = line.Substring(51, 9).Trim();
                        if (!string.IsNullOrEmpty(varName) && !varName.Equals("Var?") && !varName.Equals(star.Name.Substring(3)) && !varName.All(char.IsDigit))
                        {
                            star.VariableName = varName;
                        }

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

            using (var sr = new StreamReader(NAMES_FILE, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    string[] parts = line.Split(';');
                    int number = int.Parse(parts[0].Trim());
                    Stars.ElementAt(number - 1).ProperName = parts[1].Trim();
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
            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        /// <summary>
        /// Gets precessional elements for converting from current to B1875 epoch
        /// </summary>
        private PrecessionalElements PrecessionalElements1875(SkyContext c)
        {
            return Precession.ElementsFK5(c.JulianDay, Date.EPOCH_B1875);
        }

        /// <summary>
        /// Gets equatorial coordinates of star for B1875 epoch
        /// </summary>
        private CrdsEquatorial Equatorial1875(SkyContext c, Star s)
        {
            return Precession.GetEquatorialCoordinates(c.Get(Equatorial, s), c.Get(PrecessionalElements1875));
        }

        /// <summary>
        /// Gets constellation where the star is located for current context instant
        /// </summary>
        private string Constellation(SkyContext c, Star s)
        {
            return Constellations.FindConstellation(c.Get(Equatorial1875, s));
        }

        /// <summary>
        /// Gets detailed info about star
        /// </summary>
        private StarDetails ReadStarDetails(SkyContext c, Star s)
        {
            var details = new StarDetails();

            using (var sr = new StreamReader(STARS_FILE, Encoding.Default))
            {
                sr.BaseStream.Seek((s.Number - 1) * 199, SeekOrigin.Begin);               
                string line = sr.ReadLine();

                details.IsInfraredSource = line[41] == 'I';
                details.SpectralClass = line.Substring(127, 20).Trim();
                details.Pecularity = line.Substring(147, 1).Trim();

                string radialVelocity = line.Substring(166, 4).Trim();

                details.RadialVelocity = string.IsNullOrEmpty(radialVelocity) ? (int?)null : int.Parse(radialVelocity);
            }

            return details;
        }

        #endregion Ephemeris

        public void ConfigureEphemeris(EphemerisConfig<Star> e)
        {
            e.Add("RTS.Rise", (c, s) => c.Get(RiseTransitSet, s).Rise);
            e.Add("RTS.Transit", (c, s) => c.Get(RiseTransitSet, s).Transit);
            e.Add("RTS.Set", (c, s) => c.Get(RiseTransitSet, s).Set);
        }

        public CelestialObjectInfo GetInfo(SkyContext c, Star s)
        {
            var rts = c.Get(RiseTransitSet, s);
            var det = c.Get(ReadStarDetails, s);

            var info = new CelestialObjectInfo();
            info.SetSubtitle("Star").SetTitle(string.Join(" / ", GetStarNames(s)))

            .AddRow("Constellation", c.Get(Constellation, s))

            .AddHeader("Equatorial coordinates (current epoch)")
            .AddRow("Equatorial.Alpha", c.Get(Equatorial, s).Alpha)
            .AddRow("Equatorial.Delta", c.Get(Equatorial, s).Delta)

            .AddHeader("Equatorial coordinates (J2000.0 epoch)")
            .AddRow("Equatorial0.Alpha", s.Equatorial0.Alpha)
            .AddRow("Equatorial0.Delta", s.Equatorial0.Delta)

            .AddHeader("Horizontal coordinates")
            .AddRow("Horizontal.Azimuth", c.Get(Horizontal, s).Azimuth)
            .AddRow("Horizontal.Altitude", c.Get(Horizontal, s).Altitude)

            .AddHeader("Visibility")
            .AddRow("RTS.Rise", rts.Rise, c.JulianDayMidnight + rts.Rise)
            .AddRow("RTS.Transit", rts.Transit, c.JulianDayMidnight + rts.Transit)
            .AddRow("RTS.Set", rts.Set, c.JulianDayMidnight + rts.Set)
            .AddRow("RTS.Duration", rts.Duration)

            .AddHeader("Properties")
            .AddRow("Magnitude", s.Mag)
            .AddRow("Is Infrared Source", det.IsInfraredSource)
            .AddRow("SpectralClass", det.SpectralClass)
            .AddRow("Pecularity", det.Pecularity)
            .AddRow("Radial velocity", det.RadialVelocity + " km/s");

            return info;
        }

        private ICollection<string> GetStarNames(Star s)
        {
            var constellations = Sky.Get<ICollection<Constellation>>("Constellations");
            List<string> names = new List<string>();

            string conName = s.Name.Substring(7, 3).Trim();
            if (!string.IsNullOrEmpty(conName))
            {
                conName = constellations.FirstOrDefault(c => c.Code.StartsWith(conName, StringComparison.OrdinalIgnoreCase)).Genitive;
            }

            if (s.ProperName != null)
            {
                names.Add(s.ProperName);
            }
            if (s.BayerName != null)
            {
                names.Add($"{s.BayerName} {conName}");
            }
            if (s.FlamsteedNumber != null)
            {
                names.Add($"{s.FlamsteedNumber} {conName}");
            }
            if (s.VariableName != null)
            {
                names.Add(s.VariableName);
            }
            if (s.HDNumber > 0)
            {
                names.Add($"HD {s.HDNumber}");
            }
            if (s.SAONumber > 0)
            {
                names.Add($"SAO {s.SAONumber}");
            }
            if (s.FK5Number > 0)
            {
                names.Add($"FK5 {s.FK5Number}");
            }
            names.Add($"HR {s.Number}");

            return names;
        }

        private class StarDetails
        {
            public int? RadialVelocity { get; set; }
            public bool IsInfraredSource { get; set; }
            public string SpectralClass { get; set; }
            public string Pecularity { get; set; }
        }
    }
}
