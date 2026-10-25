using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Astrarium.Plugins.Hipparcos
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
        private Dictionary<string, string> Alphabet;

        /// <summary>
        /// Greek alphabet abbreviations
        /// </summary>
        private readonly List<string> AlphabetAbbr = new List<string> {
            "alf", "bet", "gam", "del", "eps", "zet", "eta", "the", "iot", "kap", "lam", "mu",
            "nu", "ksi", "omi", "pi", "rho", "FSg", "sig", "tau", "ups", "phi", "chi", "psi", "ome" };

        /// <summary>
        /// Superscript digits
        /// </summary>
        private static string[] SuperscriptDigits = new string[] {
            "⁰", "¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹"
        };

        /// <summary>
        /// Collection of all stars
        /// </summary>
        private ICollection<Star> Stars;

        /// <summary>
        /// Stars regions (quick access index)
        /// </summary>
        private ICollection<Region> Regions;

        /// <inheritdoc />
        public IEnumerable<Star> GetCelestialObjects() => Stars;

        /// <summary>
        /// Precessional elements for J2000.0 epoch
        /// </summary>
        internal PrecessionalElements PrecessionalElements0 { get; private set; }

        /// <summary>
        /// Number of years since J2000.0 epoch
        /// </summary>
        private double YearsSinceJ2000 { get; set; }

        /// <summary>
        /// Stars data reader
        /// </summary>
        private readonly IStarsReader dataReader;

        public StarsCalc(ISky sky, IStarsReader dataReader)
        {
            this.sky = sky;
            this.dataReader = dataReader;

            Star.GetNamesFunc = GetStarNames;
        }

        public override void Calculate(SkyContext context)
        {
            // precessional elements from current epoch to J2000
            PrecessionalElements0 = Precession.ElementsFK5(context.JulianDay, Date.EPOCH_J2000);

            // number of years since J2000.0 epoch
            YearsSinceJ2000 = (context.JulianDay - Date.EPOCH_J2000) / 365.25;
        }

        public Star GetStar(SkyContext ctx, int index)
        {
            Star star = Stars.ElementAt(index);
            star.Equatorial = Equatorial(ctx, index).RoundToSecondsOfArc();
            return star;
        }

        public IEnumerable<Star> GetStars(SkyContext ctx, CrdsEquatorial eq0, double fov, Func<float, bool> magFilter)
        {
            return Regions
                    .Where(r => Angle.Separation(eq0, r.Center) < fov + r.Radius)
                    .SelectMany(r => Stars.Skip(r.FirstStarIndex)
                        .TakeWhile(s => s.Index < r.FirstStarIndex + r.StarsCount)
                        .TakeWhile(s => magFilter(s.Magnitude))
                        .Where(s => Angle.Separation(eq0, EquatorialEquinoxJ2000(ctx, s.Index)) < fov)
                        .Select(s => GetStar(ctx, s.Index)));

        }

        public override void Initialize()
        {
            Stars = dataReader.ReadStars();
            Regions = dataReader.ReadIndex();
            Alphabet = dataReader.ReadAlphabet();
        }

        #region Ephemeris

        /// <summary>
        /// Gets equatorial coordinates of a star for the J2000.0 epoch and equinox of J2000.0
        /// </summary>
        public CrdsEquatorial Equatorial0(SkyContext c, int index)
        {
            Star star = Stars.ElementAt(index);
            return new CrdsEquatorial(star.Alpha0, star.Delta0);
        }

        /// <summary>
        /// Gets equatorial coordinates of a star for the current epoch and equinox of J2000.0
        /// </summary>
        private CrdsEquatorial EquatorialEquinoxJ2000(SkyContext c, int index)
        {
            Star star = Stars.ElementAt(index);
            double years = YearsSinceJ2000;
            var pmRA = star.PmAlpha / Math.Cos(Angle.ToRadians(star.Delta0)) / 3600000.0;
            var pmDec = star.PmDelta / 3600000.0;

            return new CrdsEquatorial(star.Alpha0 + pmRA * years, star.Delta0 + pmDec * years);
        }

        /// <summary>
        /// Gets equatorial coordinates of a star for current epoch and equinox of date
        /// </summary>
        public CrdsEquatorial Equatorial(SkyContext c, int index)
        {
            var eq0 = EquatorialEquinoxJ2000(c, index);

            // Equatorial coordinates for the mean equinox and epoch of the target date
            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, c.PrecessionElements);

            // Nutation effect
            //var eq1 = Nutation.NutationEffect(eq, c.NutationElements, c.Epsilon);

            // Aberration effect
            //var eq2 = Aberration.AberrationEffect(eq, c.AberrationElements, c.Epsilon);

            // Apparent coordinates of the star
            //eq += eq1 + eq2;

            return eq;
        }

        /// <summary>
        /// Gets apparent horizontal coordinates of star for given instant
        /// </summary>
        private CrdsHorizontal Horizontal(SkyContext c, int index)
        {
            return c.Get(Equatorial, index).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rise, transit and set info for the star
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, int index)
        {
            double theta0 = Date.ApparentSiderealTime(c.JulianDayMidnight, c.NutationElements.deltaPsi, c.Epsilon);
            var eq = c.Get(Equatorial, index); 
            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        /// <summary>
        /// Gets visibility info for the star
        /// </summary>
        private VisibilityDetails VisibilityDetails(SkyContext c, int index)
        {
            var ctx = c.Copy(c.JulianDayMidnight);
            var eq = ctx.Get(Equatorial, index);
            var eqSun = ctx.Get(sky.SunEquatorial);
            double minBodyAltitude = ctx.MinBodyAltitudeForVisibilityCalculations ?? 5;
            double minSunAltitude = ctx.MaxSunAltitudeForVisibilityCalculations ?? 0;
            return Visibility.Details(eq, eqSun, ctx.GeoLocation, ctx.SiderealTime, minBodyAltitude, minSunAltitude);
        }

        private double Distance(SkyContext c, int index)
        {
            Star star = Stars.ElementAt(index);
            const double LyPerParsec = 3.2616;
            const double masToLy = 1000.0 * LyPerParsec;
            return masToLy / star.Plx;
        }

        #endregion Ephemeris

        public void ConfigureEphemeris(EphemerisConfig<Star> e)
        {
            e["Constellation"] = (c, s) => Constellations.FindConstellation(c.Get(Equatorial, s.Index), c.JulianDay);
            e["Equatorial.Alpha"] = (c, s) => c.Get(Equatorial, s.Index).Alpha;
            e["Equatorial.Delta"] = (c, s) => c.Get(Equatorial, s.Index).Delta;
            e["Horizontal.Azimuth"] = (c, s) => c.Get(Horizontal, s.Index).Azimuth;
            e["Horizontal.Altitude"] = (c, s) => c.Get(Horizontal, s.Index).Altitude;
            e["Magnitude"] = (c, s) => s.Magnitude;
            e["RTS.Rise"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Index).Rise);
            e["RTS.Transit"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Index).Transit);
            e["RTS.Set"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Index).Set);
            e["RTS.Duration"] = (c, s) => c.Get(RiseTransitSet, s.Index).Duration;
            e["RTS.RiseAzimuth"] = (c, s) => c.Get(RiseTransitSet, s.Index).RiseAzimuth;
            e["RTS.TransitAltitude"] = (c, s) => c.Get(RiseTransitSet, s.Index).TransitAltitude;
            e["RTS.SetAzimuth"] = (c, s) => c.Get(RiseTransitSet, s.Index).SetAzimuth;
            e["Visibility.Begin"] = (c, s) => c.GetDateFromTime(c.Get(VisibilityDetails, s.Index).Begin);
            e["Visibility.End"] = (c, s) => c.GetDateFromTime(c.Get(VisibilityDetails, s.Index).End);
            e["Visibility.Duration"] = (c, s) => c.Get(VisibilityDetails, s.Index).Duration;
            e["Visibility.Period"] = (c, s) => c.Get(VisibilityDetails, s.Index).Period;
        }

        public void GetInfo(CelestialObjectInfo<Star> info)
        {
            Star s = info.Body;
            SkyContext c = info.Context;

            info
            .SetTitle(string.Join(", ", s.Names))
            .SetSubtitle(Text.Get("Star.Type"))

            .AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial, s.Index), c.JulianDay))

            .AddHeader(Text.Get("Star.Equatorial"))
            .AddRow("Equatorial.Alpha", c.Get(Equatorial, s.Index).Alpha)
            .AddRow("Equatorial.Delta", c.Get(Equatorial, s.Index).Delta)

            .AddHeader(Text.Get("Star.Equatorial0"))
            .AddRow("Equatorial0.Alpha", c.Get(Equatorial0, s.Index).Alpha)
            .AddRow("Equatorial0.Delta", c.Get(Equatorial0, s.Index).Delta)

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
            .AddRow("SpectralClass", s.SpType)
            .AddRow("Parallax", s.Plx, Formatters.Mas)
            .AddRow("Distance", c.Get(Distance, s.Index), Formatters.Distance)

            .AddHeader(Text.Get("Star.ProperMotion"))
            .AddRow("PmAlpha", s.PmAlpha, Formatters.ProperMotion)
            .AddRow("PmDelta", s.PmDelta, Formatters.ProperMotion);

            if (s.IsMultiple)
            {
                info
                    .AddHeader(Text.Get("Star.Multiplicity"))
                    .AddRow("NComps", s.NComps)
                    .AddRow("Comps", string.Join(", ", s.Comps.ToCharArray()))
                    .AddRow("Sep", s.Sep, Formatters.Sep)
                    .AddRow("PosA", (int)s.PosA, Formatters.PosA)
                    .AddRow("MagDiff", s.MagDiff, Formatters.MagDiff);
            }

            if (s.IsVariable)
            {
                info
                    .AddHeader(Text.Get("Star.Variability"))
                    .AddRow("VarFlag", s.VarFlag, Formatters.VarFlag)
                    .AddRow("VarType", s.VarType, Formatters.VarType);

                if (s.VarPeriod != 0)
                {
                    info.AddRow("VarPeriod", s.VarPeriod, Formatters.VarPeriod);
                }
            }
        }

        private static Regex regexSpaceRemover = new Regex("[ ]{2,}", RegexOptions.None);
        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            searchString = regexSpaceRemover.Replace(searchString, " ").Trim();

            return Stars
                .Where(s => GetStarNames(s, forSearch: true)
                .Any(name => name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                .Where(filterFunc)
                .Take(maxCount)
                .Select(s =>
                {
                    s.Equatorial = Equatorial(context, (s as Star).Index);
                    return s;
                })
                .ToList();
        }

        public CelestialObject Search(SkyContext context, string bodyType, string bodyName)
        {
            if (bodyType != Star.BodyType)
                return null;
            else
            {
                var star = Stars.FirstOrDefault(m => m.CommonName == bodyName);
                if (star != null)
                {
                    star.Equatorial = Equatorial(context, star.Index).RoundToSecondsOfArc();
                }
                return star;
            }
        }

        internal string FormatLetterAndDigit(string letter, int digit)
        {
            int i = AlphabetAbbr.IndexOf(letter);
            if (i >= 0)
            {
                letter = ((char)('\u03b1' + i)).ToString();
            }

            if (digit > 0)
            {
                if (digit < 10) 
                {
                    return $"{letter}\u2006{SuperscriptDigits[digit]}";
                }
                else
                {
                    return $"{letter}{digit}";
                }
            }
            else
            {
                return letter;
            }
        }

        public string GetBayerNameForPrint(Star s)
        {
            string bayer = s.Bayer.Trim();
            if (!string.IsNullOrWhiteSpace(bayer))
            {
                int splitIndex = bayer.TakeWhile(char.IsLetter).Count();
                string bayerLetter = bayer.Substring(0, splitIndex);
                if (bayerLetter == "V")
                {
                    return bayer;
                }
                string digit = bayer.Substring(splitIndex).Trim(' ', '.');
                int bayerDigit = 0;
                if (!string.IsNullOrEmpty(digit))
                {
                    bayerDigit = int.Parse(digit);
                }
                return FormatLetterAndDigit(bayerLetter, bayerDigit);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<string> GetStarNames(Star s)
        {
            return GetStarNames(s, forSearch: false);
        }

        private IEnumerable<string> GetStarNames(Star s, bool forSearch)
        {
            // proper name
            if (s.ProperName != null)
            {
                yield return s.ProperName;
            }

            // constellation name synonims
            var constSynonyms = new List<string>();

            if (!string.IsNullOrWhiteSpace(s.Con))
            {
                var constellation = sky.GetConstellation(s.Con);
                if (constellation != null)
                {
                    if (forSearch)
                    {
                        constSynonyms.Add(s.Con);
                        constSynonyms.Add(constellation.LatinName);
                        constSynonyms.Add(constellation.LocalName);
                        constSynonyms.Add(constellation.LatinGenitiveName);
                    }
                    constSynonyms.Add(constellation.LocalGenitiveName);                    
                }
            }

            // star name idenitifier synonims
            var idSynonyms = new List<string>();

            // Star has Bayer letter identifier
            string bayer = s.Bayer.Trim();
            if (!string.IsNullOrWhiteSpace(bayer))
            {
                int splitIndex = bayer.TakeWhile(char.IsLetter).Count();
                string bayerLetter = bayer.Substring(0, splitIndex);
                string digit = bayer.Substring(splitIndex).Trim(' ', '.');
                int bayerDigit = 0;
                if (!string.IsNullOrEmpty(digit))
                {
                    bayerDigit = int.Parse(digit);
                }

                if (forSearch)
                {
                    idSynonyms.Add(bayerLetter);

                    // transliterated greek letter (alpha, beta, ...)
                    if (Alphabet.ContainsKey(bayerLetter))
                    {
                        idSynonyms.Add(Alphabet[bayerLetter]);
                    }

                    if (bayerDigit > 0)
                    {
                        idSynonyms = idSynonyms.SelectMany(x => new[] { x, $"{x}{bayerDigit}", $"{x} {bayerDigit}" }).ToList();
                    }
                }
                else
                {
                    idSynonyms.Add(FormatLetterAndDigit(bayerLetter, bayerDigit));
                }
            }

            // star has digital Flamsteed identifier ("33" Andromedae)
            if (s.Flamsteed != 0)
            {
                idSynonyms.Add(s.Flamsteed.ToString());
            }

            foreach (string id in idSynonyms)
            {
                foreach (string con in constSynonyms)
                {
                    yield return $"{id} {con}";
                }
            }

            if (s.HR != 0)
            {
                yield return  $"HR {s.HR}";
                if (forSearch)
                {
                    yield return $"HR{s.HR}";
                    yield return $"{s.HR}";
                }             }

            if (s.HD != 0)
            {
                yield return $"HD {s.HD}";
                if (forSearch)
                {
                    yield return $"HD{s.HD}";
                    yield return $"{s.HD}";
                }
            }

            yield return $"HIP {s.HIP}";

            if (s.GC != 0)
            {
                yield return $"GC {s.GC}";
                if (forSearch)
                {
                    yield return $"GC{s.GC}";
                    yield return $"{s.GC}";
                }
            }
            if (!string.IsNullOrWhiteSpace(s.DM))
            {
                yield return s.DM.Trim();
            }

            var crossRefNames = GetCrossReferences(s);
            if (crossRefNames != null)
            {
                foreach (var crossRef in crossRefNames)
                {
                    yield return crossRef;
                }
            }
        }

        private ICollection<string> GetCrossReferences(Star s)
        {
            return sky.GetCrossReferences(s);
        }
    }
}
