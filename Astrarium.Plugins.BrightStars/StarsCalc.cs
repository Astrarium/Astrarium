using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Astrarium.Plugins.BrightStars
{
    public class StarsCalc : BaseCalc, ICelestialObjectCalc<Star>
    {
        /// <summary>
        /// ISky instance
        /// </summary>
        private readonly ISky sky;

        /// <summary>
        /// Alphabet
        /// </summary>
        private Dictionary<string, string> Alphabet = new Dictionary<string, string>();

        /// <summary>
        /// Collection of all stars
        /// </summary>
        internal ICollection<Star> Stars = new List<Star>();

        /// <inheritdoc />
        public IEnumerable<Star> GetCelestialObjects() => Stars.Where(s => s != null);

        /// <summary>
        /// Stars data reader
        /// </summary>
        private readonly IStarsReader dataReader;

        public StarsCalc(ISky sky, IStarsReader dataReader)
        {
            this.sky = sky;
            this.dataReader = dataReader;
            Star.GetNames = GetStarNames;
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
            Stars = dataReader.ReadStars();
            Alphabet = dataReader.ReadAlphabet();
        }

        #region Ephemeris

        /// <summary>
        /// Gets number of years (with fractions) since J2000.0 epoch
        /// </summary>
        private double YearsSince2000(SkyContext c)
        {
            return (c.JulianDay - Date.EPOCH_J2000) / 365.25;
        }

        /// <summary>
        /// Gets precessional elements to convert equatorial coordinates of stars to current epoch 
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
        /// Gets visibility info for the star
        /// </summary>
        private VisibilityDetails VisibilityDetails(SkyContext c, ushort hrNumber)
        {
            var ctx = c.Copy(c.JulianDayMidnight);
            var eq = ctx.Get(Equatorial, hrNumber);
            var eqSun = ctx.Get(sky.SunEquatorial);
            double minBodyAltitude = ctx.MinBodyAltitudeForVisibilityCalculations ?? 5;
            double minSunAltitude = ctx.MaxSunAltitudeForVisibilityCalculations ?? 0;
            return Visibility.Details(eq, eqSun, ctx.GeoLocation, ctx.SiderealTime, minBodyAltitude, minSunAltitude);
        }

        /// <summary>
        /// Gets detailed info about star
        /// </summary>
        private StarDetails ReadStarDetails(SkyContext c, ushort hrNumber)
        {
            return dataReader.GetStarDetails(hrNumber);
        }

        #endregion Ephemeris

        public void ConfigureEphemeris(EphemerisConfig<Star> e)
        {
            e["Constellation"] = (c, s) => Constellations.FindConstellation(c.Get(Equatorial, s.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, s) => c.Get(Equatorial, s.Number).Alpha;
            e["Equatorial.Delta"] = (c, s) => c.Get(Equatorial, s.Number).Delta;
            e["Horizontal.Azimuth"] = (c, s) => c.Get(Horizontal, s.Number).Azimuth;
            e["Horizontal.Altitude"] = (c, s) => c.Get(Horizontal, s.Number).Altitude;
            e["Magnitude"] = (c, s) => s.Magnitude;
            e["RTS.Rise"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Number).Rise);
            e["RTS.Transit"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Number).Transit);
            e["RTS.Set"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Number).Set);
            e["RTS.Duration"] = (c, s) => c.Get(RiseTransitSet, s.Number).Duration;
            e["RTS.RiseAzimuth"] = (c, s) => c.Get(RiseTransitSet, s.Number).RiseAzimuth;
            e["RTS.TransitAltitude"] = (c, s) => c.Get(RiseTransitSet, s.Number).TransitAltitude;
            e["RTS.SetAzimuth"] = (c, s) => c.Get(RiseTransitSet, s.Number).SetAzimuth;
            e["Visibility.Begin"] = (c, s) => c.GetDateFromTime(c.Get(VisibilityDetails, s.Number).Begin);
            e["Visibility.End"] = (c, s) => c.GetDateFromTime(c.Get(VisibilityDetails, s.Number).End);
            e["Visibility.Duration"] = (c, s) => c.Get(VisibilityDetails, s.Number).Duration;
            e["Visibility.Period"] = (c, s) => c.Get(VisibilityDetails, s.Number).Period;
        }

        public void GetInfo(CelestialObjectInfo<Star> info)
        {
            Star s = info.Body;
            SkyContext c = info.Context;
            StarDetails details = c.Get(ReadStarDetails, s.Number);

            info
            .SetTitle(string.Join(", ", s.Names))
            .SetSubtitle(Text.Get("Star.Type"))

            .AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial, s.Number), c.JulianDay))

            .AddHeader(Text.Get("Star.Equatorial"))
            .AddRow("Equatorial.Alpha", c.Get(Equatorial, s.Number).Alpha)
            .AddRow("Equatorial.Delta", c.Get(Equatorial, s.Number).Delta)

            .AddHeader(Text.Get("Star.Equatorial0"))
            .AddRow("Equatorial0.Alpha", s.Equatorial0.Alpha)
            .AddRow("Equatorial0.Delta", s.Equatorial0.Delta)

            .AddHeader(Text.Get("Star.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Star.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("Star.Visibility"))
            .AddRow("Visibility.Begin")
            .AddRow("Visibility.End")
            .AddRow("Visibility.Duration")
            .AddRow("Visibility.Period")

            .AddHeader(Text.Get("Star.Properties"))
            .AddRow("Magnitude", s.Magnitude)
            .AddRow("IsInfraredSource", details.IsInfraredSource)
            .AddRow("SpectralClass", details.SpectralClass);

            if (!string.IsNullOrEmpty(details.Pecularity))
            {
                info.AddRow("Pecularity", details.Pecularity);
            }

            if (details.RadialVelocity != null)
            {
                info.AddRow("RadialVelocity", details.RadialVelocity + " km/s");
            }
        }

        private static Regex regexSpaceRemover = new Regex("[ ]{2,}", RegexOptions.None);
        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            searchString = regexSpaceRemover.Replace(searchString, " ").Trim();

            return Stars.Where(s => s != null &&
                GetStarNamesForSearch(s)
                .Any(name => name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                .Where(filterFunc)
                .Take(maxCount)
                .ToArray();
        }

        private ICollection<string> GetStarNamesForSearch(Star s)
        {
            List<string> names = new List<string>();

            // constellation name synonims
            List<string> constSynonyms = new List<string>();

            string conCode = s.Name.Substring(7, 3).Trim();
            if (string.IsNullOrEmpty(conCode) && s.VariableName != null)
            {
                string[] varName = s.VariableName.Split(' ');
                if (varName.Length > 1)
                {
                    conCode = varName[1];
                }
            }

            if (!string.IsNullOrEmpty(conCode))
            {
                constSynonyms.Add(conCode);
                var constellation = sky.GetConstellation(conCode);
                if (constellation != null)
                {
                    constSynonyms.Add(constellation.LatinGenitiveName);
                    constSynonyms.Add(constellation.LatinName);
                    constSynonyms.Add(constellation.LocalGenitiveName);
                    constSynonyms.Add(constellation.LocalName);
                }
            }

            constSynonyms = constSynonyms.Where(c => c != null).ToList();

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

            var crossRefNames = GetCrossReferences(s);
            if (crossRefNames != null && crossRefNames.Any())
            {
                names.AddRange(crossRefNames);
            }

            return names;
        }

        private ICollection<string> GetCrossReferences(Star s)
        {
            return sky.GetCrossReferences(s);
        }

        private ICollection<string> GetStarNames(Star s)
        {
            List<string> names = new List<string>();

            string conName = s.Name.Substring(7, 3).Trim();

            if (!string.IsNullOrEmpty(conName))
            {
                conName = sky.GetConstellation(conName).LatinGenitiveName;
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
                string[] varName = s.VariableName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (varName.Length > 1)
                {
                    conName = sky.GetConstellation(varName[1]).LatinGenitiveName;
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
            var crossRefNames = GetCrossReferences(s);
            if (crossRefNames != null && crossRefNames.Any())
            {
                names.AddRange(crossRefNames);
            }
            return names;
        }
    }
}
