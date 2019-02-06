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
    public class StarsCalc : BaseSkyCalc, IEphemProvider<Star>, IInfoProvider<Star>, ISearchProvider<Star>
    {
        private readonly string STARS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Stars.dat");
        
        private readonly string NAMES_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/StarNames.dat");

        /// <summary>
        /// Collection of all stars
        /// </summary>
        private ICollection<Star> Stars = new List<Star>();

        /// <summary>
        /// Stars data reader
        /// </summary>
        private StarsReader DataReader = new StarsReader();

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
            DataReader.StarsDataFilePath = STARS_FILE;
            DataReader.StarsNamesFilePath = NAMES_FILE;
            Stars = DataReader.ReadStars();
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
        /// Gets constellation where the star is located
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
            return DataReader.GetStarDetails(s);
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
            info.SetSubtitle("Star").SetTitle(string.Join(", ", GetStarNames(s)))

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

        public ICollection<SearchResultItem> Search(string searchString, int maxCount = 50)
        {
            return Stars.Where(s => s != null && GetStarNames(s).Any(name => name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                .Take(maxCount)
                .Select(s => new SearchResultItem(s, string.Join(", ", GetStarNames(s))))
                .ToArray();
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
                string[] varName = s.VariableName.Split(' ');
                if (varName.Length > 1)
                {
                    conName = constellations.FirstOrDefault(c => c.Code.StartsWith(varName[1], StringComparison.OrdinalIgnoreCase)).Genitive;
                    names.Add($"{varName[0]} {conName}");
                }
                else
                {
                    names.Add($"NSV {s.VariableName}");
                }
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
    }
}
