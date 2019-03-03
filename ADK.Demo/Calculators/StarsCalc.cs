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
    public interface IStarsProvider
    {
        ICollection<Star> Stars { get; }
    }

    public interface IStarsCalc
    {
        CrdsEquatorial Equatorial(SkyContext ctx, ushort hrNumber);
        string GetPrimaryStarName(ushort hrNumber);
    }

    public class StarsCalc : BaseCalc<Star>, IStarsProvider, IStarsCalc
    {
        private readonly string STARS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Stars.dat");
        private readonly string NAMES_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/StarNames.dat");
        private readonly string ALPHABET_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Alphabet.dat");

        private Dictionary<string, string> Alphabet = new Dictionary<string, string>();

        /// <summary>
        /// Collection of all stars
        /// </summary>
        public ICollection<Star> Stars { get; private set; } = new List<Star>();

        /// <summary>
        /// Stars data reader
        /// </summary>
        private StarsReader DataReader = new StarsReader();

        private IConstellationsProvider ConstellationsProvider;

        public StarsCalc(IConstellationsProvider constellationsProvider)
        {
            ConstellationsProvider = constellationsProvider;
        }

        public override void Calculate(SkyContext context)
        {
            foreach (var star in Stars)
            {
                if (star != null)
                {
                    star.Horizontal = context.Get(Horizontal, star.Number);
                }
            }
        }

        public override void Initialize()
        {
            DataReader.StarsDataFilePath = STARS_FILE;
            DataReader.StarsNamesFilePath = NAMES_FILE;
            DataReader.AlphabetFilePath = ALPHABET_FILE;
            Stars = DataReader.ReadStars();
            Alphabet = DataReader.ReadAlphabet();
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
        public CrdsEquatorial Equatorial(SkyContext c, ushort hrNumber)
        {
            Star star = Stars.ElementAt(hrNumber - 1);

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
        private CrdsHorizontal Horizontal(SkyContext c, ushort hrNumber)
        {
            return c.Get(Equatorial, hrNumber).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rise, transit and set info for the star
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, ushort hrNumber)
        {
            double theta0 = Date.ApparentSiderealTime(c.JulianDayMidnight, c.NutationElements.deltaPsi, c.Epsilon);
            var eq = c.Get(Equatorial, hrNumber); 
            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        /// <summary>
        /// Gets detailed info about star
        /// </summary>
        private StarDetails ReadStarDetails(SkyContext c, ushort hrNumber)
        {
            return DataReader.GetStarDetails(hrNumber);
        }

        #endregion Ephemeris

        public override void ConfigureEphemeris(EphemerisConfig<Star> e)
        {
            e.Add("RTS.Rise", (c, s) => c.Get(RiseTransitSet, s.Number).Rise);
            e.Add("RTS.Transit", (c, s) => c.Get(RiseTransitSet, s.Number).Transit);
            e.Add("RTS.Set", (c, s) => c.Get(RiseTransitSet, s.Number).Set);
        }

        public override CelestialObjectInfo GetInfo(SkyContext c, Star s)
        {
            var rts = c.Get(RiseTransitSet, s.Number);
            var det = c.Get(ReadStarDetails, s.Number);

            var info = new CelestialObjectInfo();
            info.SetSubtitle("Star").SetTitle(string.Join(", ", GetStarNames(s)))

            .AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial, s.Number), c.JulianDay))

            .AddHeader("Equatorial coordinates (current epoch)")
            .AddRow("Equatorial.Alpha", c.Get(Equatorial, s.Number).Alpha)
            .AddRow("Equatorial.Delta", c.Get(Equatorial, s.Number).Delta)

            .AddHeader("Equatorial coordinates (J2000.0 epoch)")
            .AddRow("Equatorial0.Alpha", s.Equatorial0.Alpha)
            .AddRow("Equatorial0.Delta", s.Equatorial0.Delta)

            .AddHeader("Horizontal coordinates")
            .AddRow("Horizontal.Azimuth", c.Get(Horizontal, s.Number).Azimuth)
            .AddRow("Horizontal.Altitude", c.Get(Horizontal, s.Number).Altitude)

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

        private static Regex regexSpaceRemover = new Regex("[ ]{2,}", RegexOptions.None);
        public override ICollection<SearchResultItem> Search(string searchString, int maxCount = 50)
        {
            searchString = regexSpaceRemover.Replace(searchString, " ").Trim();

            return Stars.Where(s => s != null && 
                GetStarNamesForSearch(s)
                .Any(name => name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                .Take(maxCount)
                .Select(s => new SearchResultItem(s, string.Join(", ", GetStarNames(s))))
                .ToArray();
        }

        private ICollection<string> GetStarNamesForSearch(Star s)
        {
            List<string> names = new List<string>();

            // constellation name synonims
            List<string> constSynonyms = new List<string>();

            string conCode = s.Name.Substring(7, 3).Trim();
            if (!string.IsNullOrEmpty(conCode))
            {
                constSynonyms.Add(conCode);
                var constellation = ConstellationsProvider.Constellations.FirstOrDefault(c => c.Code.StartsWith(conCode, StringComparison.OrdinalIgnoreCase));
                if (constellation != null)
                {
                    constSynonyms.Add(constellation.Genitive);
                    constSynonyms.Add(constellation.Name);
                }
            }

            if (s.ProperName != null)
            {
                names.Add(s.ProperName);
            }

            // star name idenitifier synonims
            List<string> idSynonyms = new List<string>();

            // Star has Bayer letter identifier
            string bayerLetter = s.Name.Substring(3, 3).Trim();
            if (!string.IsNullOrEmpty(bayerLetter))
            {
                // transliterated greek letter (alpha, beta, ...)
                if (Alphabet.ContainsKey(bayerLetter))
                {
                    idSynonyms.Add(Alphabet[bayerLetter]);
                }

                // greek letter abbreviation (Alp, Bet, ...)
                idSynonyms.Add(bayerLetter);

                // greek letter (α, β)
                idSynonyms.Add(s.BayerName[0].ToString());

                // if star has multiple components 
                char digit = s.Name[6];
                if (digit != ' ')
                {
                    string[] digitSynonims = idSynonyms.Select(v => $"{v}{digit}").ToArray();
                    string[] digitAndSpaceSynonims = idSynonyms.Select(v => $"{v} {digit}").ToArray();

                    idSynonyms.AddRange(digitSynonims);
                    idSynonyms.AddRange(digitAndSpaceSynonims);
                }
            }

            // star has digital Flamsteed identifier ("33" Andromedae)
            string flamsteedNumber = s.Name.Substring(0, 3).Trim();
            if (!string.IsNullOrEmpty(flamsteedNumber))
            {
                idSynonyms.Add(flamsteedNumber);
            }

            foreach (string id in idSynonyms)
            {
                foreach (string con in constSynonyms)
                {
                    names.Add($"{id} {con}");
                }
            }
            
            string variableName = s.VariableName;
            if (variableName != null)
            {
                string[] varName = variableName.Split(' ');
                if (varName.Length > 1)
                {
                    foreach (string con in constSynonyms)
                    {
                        names.Add($"{varName[0]} {con}");
                    }
                }
                else
                {
                    names.Add($"NSV {variableName}");
                    names.Add($"NSV{variableName}");
                    names.Add($"{variableName}");
                }
            }
            if (s.HDNumber > 0)
            {
                names.Add($"HD {s.HDNumber}");
                names.Add($"HD{s.HDNumber}");
                names.Add($"{s.HDNumber}");
            }
            if (s.SAONumber > 0)
            {
                names.Add($"SAO {s.SAONumber}");
                names.Add($"SAO{s.SAONumber}");
                names.Add($"{s.SAONumber}");
            }
            if (s.FK5Number > 0)
            {
                names.Add($"FK5 {s.FK5Number}");
                names.Add($"{s.FK5Number}");
            }
            names.Add($"HR {s.Number}");
            names.Add($"HR{s.Number}");
            names.Add($"{s.Number}");

            names.AddRange(constSynonyms);

            return names;
        }

        public string GetPrimaryStarName(ushort hrNumber)
        {
            return GetStarNames(Stars.ElementAt(hrNumber - 1)).First();
        }

        private ICollection<string> GetStarNames(Star s)
        {
            List<string> names = new List<string>();

            string conName = s.Name.Substring(7, 3).Trim();

            if (!string.IsNullOrEmpty(conName))
            {
                conName = ConstellationsProvider.Constellations.FirstOrDefault(c => c.Code.StartsWith(conName, StringComparison.OrdinalIgnoreCase)).Genitive;
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
                    conName = ConstellationsProvider.Constellations.FirstOrDefault(c => c.Code.StartsWith(varName[1], StringComparison.OrdinalIgnoreCase)).Genitive;
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
