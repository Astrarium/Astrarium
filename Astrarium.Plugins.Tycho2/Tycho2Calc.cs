using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Astrarium.Plugins.Tycho2
{
    public interface ITycho2Catalog
    {
        /// <summary>
        /// Gets stars in specified circular area
        /// </summary>
        /// <param name="t">Years since J2000.0 epoch</param>
        /// <param name="eq">Equatorial coordinates of area center, at epoch J2000.0</param>
        /// <param name="angle">Area radius, in degrees</param>
        /// <param name="magFilter">Magnitude filter function, returns true if star is visible and should be included in results</param>
        /// <returns>Collection of <see cref="Tycho2Star"/> objects</returns>
        ICollection<Tycho2Star> GetStars(double t, CrdsEquatorial eq, double angle, Func<float, bool> magFilter);

        /// <summary>
        /// Gets or sets Tycho2Star object that the map is locked on
        /// </summary>
        Tycho2Star LockedStar { get; set; }

        /// <summary>
        /// Gets or sets Tycho2Star object that currently selected
        /// </summary>
        Tycho2Star SelectedStar { get; set; }

        /// <summary>
        /// Calculates number of years since J2000.0 epoch
        /// </summary>
        double YearsSince2000(SkyContext c);

        /// <summary>
        /// Gets J2000.0 precession matrix
        /// </summary>
        Mat4 MatPrecession0 { get; }

        /// <summary>
        /// Gets precession matrix for current epoch
        /// </summary>
        Mat4 MatPrecession { get; }

        /// <summary>
        /// Precessional elements for J2000.0 epoch
        /// </summary>
        PrecessionalElements PrecessionalElements0 { get; }
    }

    public class Tycho2Calc : BaseCalc, ICelestialObjectCalc<Tycho2Star>, ITycho2Catalog
    {
        /// <summary>
        /// Represents a single record from Tycho2 index file.
        /// </summary>
        private class Tycho2Region
        {
            /// <summary>
            /// First record id for this region (numeric 1-based index) in data file
            /// </summary>
            public long FirstStarId { get; set; }

            /// <summary>
            /// Last record id for this region (numeric 1-based index) in data file
            /// </summary>
            public long LastStarId { get; set; }

            /// <summary>
            /// Minimal Right Ascention of stars in this region
            /// </summary>
            public float RAmin { get; set; }

            /// <summary>
            /// Maximal Right Ascention of stars in this region
            /// </summary>
            public float RAmax { get; set; }

            /// <summary>
            /// Minimal Declination of stars in this region
            /// </summary>
            public float DECmin { get; set; }

            /// <summary>
            /// Maximal Declination of stars in this region
            /// </summary>
            public float DECmax { get; set; }
        }

        /// <summary>
        /// Parsed index file: collection of star regions
        /// </summary>
        private ICollection<Tycho2Region> IndexRegions = new List<Tycho2Region>();

        /// <summary>
        /// Star entries that have cross-identifiers with Bright Star Catalog. Should be excluded.
        /// </summary>
        private HashSet<string> SkippedEntries = new HashSet<string>();

        /// <summary>
        /// Dictionary of proper names of Tycho2 stars
        /// </summary>
        private Dictionary<string, string> ProperNames = null;

        /// <summary>
        /// Binary Reader for accessing catalog data
        /// </summary>
        private BinaryReader CatalogReader;

        /// <summary>
        /// Flag indicating the catalog has been initialized
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// Length of catalog record, in bytes
        /// </summary>
        private const int CATALOG_RECORD_LEN = 37;

        /// <summary>
        /// Mean diameter of segment of celestial sphere (in degrees) that's defined by <see cref="Tycho2Region"/>.   
        /// </summary>
        private const double SEGMENT_DIAM = 3.75;

        /// <summary>
        /// Settings instance
        /// </summary>
        private readonly ISettings Settings;

        /// <summary>
        /// Sky instance
        /// </summary>
        private readonly ISky Sky;

        /// <summary>
        /// Gets or sets Tycho2Star object that the map is locked on
        /// </summary>
        public Tycho2Star LockedStar { get; set; }

        /// <summary>
        /// Gets or sets Tycho2Star object that currently selected
        /// </summary>
        public Tycho2Star SelectedStar { get; set; }

        /// <summary>
        /// Precession matrix for current epoch
        /// </summary>
        public Mat4 MatPrecession { get; private set; }

        /// <summary>
        /// Precession matrix for J2000.0 epoch
        /// </summary>
        public Mat4 MatPrecession0 { get; private set; }

        /// <summary>
        /// Precessional elements for J2000.0 epoch
        /// </summary>
        public PrecessionalElements PrecessionalElements0 { get; private set; }

        /// <inheritdoc />
        public IEnumerable<Tycho2Star> GetCelestialObjects() => new Tycho2Star[0];

        /// <summary>
        /// Flag indicating the catalog data has been found and loaded
        /// </summary>
        public bool IsLoaded
        {
            get => GetValue<bool>(nameof(IsLoaded));
            private set
            {
                SetValue(nameof(IsLoaded), value);
            }
        }

        public Tycho2Calc(ISettings settings, ISky sky)
        {
            Settings = settings;
            Settings.SettingValueChanged += Settings_SettingValueChanged;
            Sky = sky;
        }

        private void Settings_SettingValueChanged(string settingName, object settingValue)
        {
            if (isInitialized && settingName == "Tycho2RootDir")
            {
                Initialize();
            }
        }

        public bool Validate(string rootDir, bool verbose = false)
        {
            if (string.IsNullOrEmpty(rootDir))
            {
                Log.Error("Tycho2 root directory is not set.");
                return false;
            }

            if (!Directory.Exists(rootDir))
            {
                Log.Error("Tycho2 root directory is not exist.");
                return false;
            }

            string indexFile = Path.Combine(rootDir, "tycho2.idx");
            string catalogFile = Path.Combine(rootDir, "tycho2.dat");
            string crossRefFile = Path.Combine(rootDir, "tycho2.ref");

            if (!File.Exists(indexFile))
            {
                Log.Error($"Tycho2 index file not found, search path: {indexFile}");
                if (verbose)
                {
                    ViewManager.ShowMessageBox("$Error", Text.Get("Tycho2.Errors.IndexFileNotFound", ("indexFilePath", indexFile)));
                }
                return false;
            }

            if (!File.Exists(catalogFile))
            {
                Log.Error($"Tycho2 catalog file not found, search path: {catalogFile}");
                if (verbose)
                {
                    ViewManager.ShowMessageBox("$Error", Text.Get("Tycho2.Errors.CatalogFileNotFound", ("catalogFilePath", catalogFile)));
                }
                return false;
            }

            if (!File.Exists(crossRefFile))
            {
                Log.Error($"Tycho2 cross-reference file not found, search path: {crossRefFile}");
                if (verbose)
                {
                    ViewManager.ShowMessageBox("$Error", Text.Get("Tycho2.Errors.CrossRefFileNotFound", ("crossRefFilePath", crossRefFile)));
                }
                return false;
            }

            return true;
        }

        public override void Initialize()
        {
            try
            {
                string rootDir = Settings.Get<string>("Tycho2RootDir", null);
                IsLoaded = false;

                if (Validate(rootDir))
                {
                    string indexFile = Path.Combine(rootDir, "tycho2.idx");
                    string catalogFile = Path.Combine(rootDir, "tycho2.dat");
                    string crossRefFile = Path.Combine(rootDir, "tycho2.ref");

                    LoadIndex(indexFile);
                    LoadCrossReference(crossRefFile);

                    ProperNames = Sky.StarNames.Where(x => x.Key.StartsWith("TYC")).ToDictionary(x => x.Key, x => x.Value);

                    // Open Tycho2 catalog file
                    CatalogReader = new BinaryReader(File.Open(catalogFile, FileMode.Open, FileAccess.Read));

                    IsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to initialize Tycho2 calculator: {ex}");
            }
            finally
            {
                isInitialized = true;
            }
        }

        /// <summary>
        /// Read Tycho2 index file and load it into memory.
        /// </summary>
        private void LoadIndex(string indexFile)
        {
            using (StreamReader sr = new StreamReader(indexFile))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] chunks = line.Split(';');
                    IndexRegions.Add(new Tycho2Region()
                    {
                        FirstStarId = Convert.ToInt64(chunks[0].Trim()),
                        LastStarId = Convert.ToInt64(chunks[1].Trim()),
                        RAmin = Convert.ToSingle(chunks[2].Trim(), CultureInfo.InvariantCulture),
                        RAmax = Convert.ToSingle(chunks[3].Trim(), CultureInfo.InvariantCulture),
                        DECmin = Convert.ToSingle(chunks[4].Trim(), CultureInfo.InvariantCulture),
                        DECmax = Convert.ToSingle(chunks[5].Trim(), CultureInfo.InvariantCulture)
                    });
                }
            }
        }

        /// <summary>
        /// Loads cross-reference data (for BSC catalog)
        /// </summary>
        private void LoadCrossReference(string crossRefFile)
        {
            Dictionary<string, string> starsCrossRefs = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(crossRefFile))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] chunks = line.Split(' ');
                    ushort hrIdentifier = ushort.Parse(chunks[0]);
                    string tyc2Identifier = chunks[1];
                    SkippedEntries.Add(tyc2Identifier);
                    starsCrossRefs.Add($"HR {hrIdentifier}", $"TYC {tyc2Identifier}");
                }
            }
            
            Sky.AddCrossReferences("Star", starsCrossRefs);
        }

        public ICollection<Tycho2Star> GetStars(double t, CrdsEquatorial eq, double angle, Func<float, bool> magFilter)
        {
            // take current field of view plus half of a segment
            double ang = angle + SEGMENT_DIAM / 2.0;

            // regions that intersect the current FOV
            var regions = IndexRegions.Where(r => Angle.Separation(eq, new CrdsEquatorial((r.RAmax + r.RAmin) / 2.0, (r.DECmax + r.DECmin) / 2.0)) <= ang);

            // take stars from each region by applying magnitude and FOV filters
            var stars = new List<Tycho2Star>();
            foreach (Tycho2Region r in regions)
            {
                stars.AddRange(GetStarsInRegion(t, r, eq, ang, magFilter));
            }

            return stars;
        }

        /// <summary>
        /// Gets precessional elements to convert equatorial coordinates of stars to current epoch 
        /// </summary>
        private PrecessionalElements PrecessionalElements(SkyContext context)
        {
            return Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);
        }

        /// <summary>
        /// Gets equatorial coordinates of a star for current epoch
        /// </summary>
        private CrdsEquatorial Equatorial(SkyContext context, Tycho2Star star)
        {
            PrecessionalElements p = context.Get(PrecessionalElements);
            double years = context.Get(YearsSince2000);

            double pmDec = star.PmDec / 3600000.0;
            double pmRa = star.PmRA / Math.Cos(Angle.ToRadians(star.Delta0)) / 3600000.0;

            // Equatorial coordinates for J2000.0 epoch, but with respect of proper motion
            var eq0 = new CrdsEquatorial(star.Alpha0 + pmRa * years, star.Delta0 + pmDec * years);

            // Equatorial coordinates for the mean equinox and epoch of the target date
            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, p);

            // Nutation effect
            var eqN = Nutation.NutationEffect(eq, context.NutationElements, context.Epsilon);

            // Aberration effect
            var eqA = Aberration.AberrationEffect(eq, context.AberrationElements, context.Epsilon);

            // Apparent coordinates of the star
            eq += eqN + eqA;

            return eq;
        }

        /// <summary>
        /// Calculates horizontal geocentric coordinates of star for current epoch
        /// </summary>
        /// <param name="c"><see cref="SkyContext"/> instance</param>
        /// <param name="star"><see cref="Tycho2Star"/> object</param>
        /// <returns>Horizontal geocentric coordinates of star for current epoch</returns>
        private CrdsHorizontal Horizontal(SkyContext c, Tycho2Star star)
        {
            return c.Get(Equatorial, star).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rise, transit and set info for the star
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, Tycho2Star star)
        {
            double theta0 = Date.ApparentSiderealTime(c.JulianDayMidnight, c.NutationElements.deltaPsi, c.Epsilon);
            var eq = c.Get(Equatorial, star);
            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        /// <summary>
        /// Gets number of years (with fractions) since J2000.0 epoch
        /// </summary>
        public double YearsSince2000(SkyContext c)
        {
            return (c.JulianDay - Date.EPOCH_J2000) / 365.25;
        }

        private ICollection<Tycho2Star> GetStarsInRegion(double t, Tycho2Region region, CrdsEquatorial eq, double angle, Func<float, bool> magFilter)
        {
            // seek reading position 
            CatalogReader.BaseStream.Seek(CATALOG_RECORD_LEN * (region.FirstStarId - 1), SeekOrigin.Begin);

            // count of records in current region
            int count = (int)(region.LastStarId - region.FirstStarId);

            // read region in memory for fast access
            byte[] buffer = CatalogReader.ReadBytes(CATALOG_RECORD_LEN * count);

            var stars = new List<Tycho2Star>();

            for (int i = 0; i < count; i++)
            {
                Tycho2Star star = GetStar(t, buffer, i * CATALOG_RECORD_LEN, eq, angle, magFilter);
                if (star != null)
                {
                    stars.Add(star);
                }
            }

            return stars;
        }

        private ICollection<Tycho2Star> GetStarsInRegion(Tycho2Region region, double t, short? tyc2, string tyc3)
        {
            // seek reading position 
            CatalogReader.BaseStream.Seek(CATALOG_RECORD_LEN * (region.FirstStarId - 1), SeekOrigin.Begin);

            // count of records in current region
            int count = (int)(region.LastStarId - region.FirstStarId);

            // read region in memory for fast access
            byte[] buffer = CatalogReader.ReadBytes(CATALOG_RECORD_LEN * count);

            var stars = new List<Tycho2Star>();

            for (int i = 0; i < count && stars.Count < 50; i++)
            {
                Tycho2Star star = GetStar(t, buffer, i * CATALOG_RECORD_LEN, tyc2, tyc3);
                if (star != null)
                {
                    stars.Add(star);
                }
            }

            return stars;
        }

        /// <summary>
        /// Reads data from catalog file as <see cref="Tycho2Star" /> instance. 
        /// </summary>
        /// <param name="buffer">Binary buffer with stars data</param>
        /// <param name="offset">Offset value to read the star record</param>
        /// <param name="eqCenter">Equatorial coordinates of map center, for J2000.0 epoch</param>
        /// <param name="angle">Maximal angular separation between map center and star coorinates (both coordinates for J2000.0 epoch)</param>        
        /// <remarks>
        /// Record format:
        /// [Tyc1][Tyc2][Tyc3][RA][Dec][PmRA][PmDec][BTMag][VTMag]
        /// [   2][   2][   1][ 8][  8][   4][    4][    4][    4]
        /// </remarks>
        private Tycho2Star GetStar(double t, byte[] buffer, int offset, CrdsEquatorial eqCenter, double angle, Func<float, bool> magFilter)
        {
            float btmag = BitConverter.ToSingle(buffer, offset + 29);
            float vtmag = BitConverter.ToSingle(buffer, offset + 33);
            float mag = (float)(vtmag - 0.090 * (btmag - vtmag));
            if (magFilter(mag))
            {
                // Star coordinates at epoch J2000.0 
                var eq0 = new CrdsEquatorial(
                    BitConverter.ToDouble(buffer, offset + 5),
                    BitConverter.ToDouble(buffer, offset + 13));

                if (Angle.Separation(eq0, eqCenter) <= angle)
                {
                    return ReadStar(t, buffer, offset);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private Tycho2Star GetStar(double t, byte[] buffer, int offset, short? tyc2, string tyc3)
        {
            short t2 = BitConverter.ToInt16(buffer, offset + 2);
            char t3 = (char)buffer[offset + 4];

            if ((tyc2 == null || tyc2.Value == t2) && (string.IsNullOrEmpty(tyc3) || tyc3[0] == t3))
            {
                return ReadStar(t, buffer, offset);
            }
            else
            {
                return null;
            }
        }

        private Tycho2Star ReadStar(double t, byte[] buffer, int offset)
        {
            Tycho2Star star = new Tycho2Star();

            star.Alpha0 = BitConverter.ToDouble(buffer, offset + 5);
            star.Delta0 = BitConverter.ToDouble(buffer, offset + 13);
            star.Tyc1 = BitConverter.ToInt16(buffer, offset);
            star.Tyc2 = BitConverter.ToInt16(buffer, offset + 2);
            star.Tyc3 = (char)buffer[offset + 4];
            float btmag = BitConverter.ToSingle(buffer, offset + 29);
            float vtmag = BitConverter.ToSingle(buffer, offset + 33);
            float mag = (float)(vtmag - 0.090 * (btmag - vtmag));
            double B_V = 0.850 * (btmag - vtmag);
            star.SpectralClass = SpectralClass(B_V);
            star.Magnitude = mag;
            star.PmRA = BitConverter.ToSingle(buffer, offset + 21);
            star.PmDec = BitConverter.ToSingle(buffer, offset + 25);
            star.ProperName = ProperNames.ContainsKey(star.Names[0]) ? ProperNames[star.Names[0]] : null;

            if (SkippedEntries.Contains($"{star.Tyc1}-{star.Tyc2}-{star.Tyc3}"))
            {
                return null;
            }
            else
            {
                star.Cartesian = CartesianWithProperMotion(star, t);
                return star;
            }
        }

        private Vec3 CartesianWithProperMotion(Tycho2Star star, double years)
        {
            double alpha = Angle.ToRadians(star.Alpha0);
            double delta = Angle.ToRadians(star.Delta0);

            double pmDec = Angle.ToRadians(star.PmDec / 3600000.0);
            double pmRa = Angle.ToRadians(star.PmRA / Math.Cos(delta) / 3600000.0);

            alpha += pmRa * years;
            delta += pmDec * years;

            return Projection.SphericalToCartesian(alpha, delta);
        }

        private char SpectralClass(double B_V)
        {
            // Evaluating Stars Temperature Through the B-V Index
            // Using a Virtual Real Experiment from Distance: A Case
            // Scenario for Secondary Education.
            // https://online-journals.org/index.php/i-joe/article/view/7842
            double T = 4600 * (1.0 / (0.92 * B_V + 1.7) + 1.0 / (0.92 * B_V + 0.62));

            // then, calculate color from spectral class:
            // O	> 25,000K	H; HeI; HeII
            // B	10,000-25,000K	H; HeI; HeII absent
            // A	7,500-10,000K	H; CaII; HeI and HeII absent
            // F	6,000-7,500K	H; metals (CaII, Fe, etc)
            // G	5,000-6,000K	H; metals; some molecular species
            // K	3,500-5,000K	metals; some molecular species
            // M	< 3,500K	metals; molecular species (TiO!)
            // C	< 3,500K	metals; molecular species (C2!)

            if (T > 25000)
                return 'O';
            else if (T <= 25000 && T > 10000)
                return 'B';
            else if (T <= 10000 && T > 7500)
                return 'A';
            else if (T <= 7500 && T > 6000)
                return 'F';
            else if (T <= 6000 && T > 5000)
                return 'G';
            else if (T <= 5000 && T > 3500)
                return 'K';
            else
                return 'M';
        }

        public override void Calculate(SkyContext context)
        {
            // precessional elements from J2000 to current epoch
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            // precessional elements from current epoch to J2000
            var p0 = Precession.ElementsFK5(context.JulianDay, Date.EPOCH_J2000);

            MatPrecession =
                Mat4.ZRotation(Angle.ToRadians(p.z)) *
                Mat4.YRotation(Angle.ToRadians(-p.theta)) *
                Mat4.ZRotation(Angle.ToRadians(p.zeta));

            MatPrecession0 =
                Mat4.ZRotation(Angle.ToRadians(p0.z)) *
                Mat4.YRotation(Angle.ToRadians(-p0.theta)) *
                Mat4.ZRotation(Angle.ToRadians(p0.zeta));

            PrecessionalElements0 = p0;
        }

        public void ConfigureEphemeris(EphemerisConfig<Tycho2Star> e)
        {
            e["Constellation"] = (c, s) => Constellations.FindConstellation(c.Get(Equatorial, s), c.JulianDay);
            e["Equatorial.Alpha"] = (c, s) => c.Get(Equatorial, s).Alpha;
            e["Equatorial.Delta"] = (c, s) => c.Get(Equatorial, s).Delta;
            e["Horizontal.Azimuth"] = (c, s) => c.Get(Horizontal, s).Azimuth;
            e["Horizontal.Altitude"] = (c, s) => c.Get(Horizontal, s).Altitude;
            e["Magnitude"] = (c, s) => s.Magnitude;
            e["RTS.Rise"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s).Rise);
            e["RTS.Transit"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s).Transit);
            e["RTS.Set"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s).Set);
            e["RTS.Duration"] = (c, s) => c.Get(RiseTransitSet, s).Duration;
        }

        public void GetInfo(CelestialObjectInfo<Tycho2Star> info)
        {
            Tycho2Star s = info.Body;
            SkyContext c = info.Context;
            string constellation = Constellations.FindConstellation(c.Get(Equatorial, s), c.JulianDay);

            info
            .SetTitle(string.Join(", ", s.Names))
            .SetSubtitle(Text.Get("Tycho2Star.Type"))

            .AddRow("Constellation", constellation)

            .AddHeader(Text.Get("Tycho2Star.Equatorial"))
            .AddRow("Equatorial.Alpha", c.Get(Equatorial, s).Alpha)
            .AddRow("Equatorial.Delta", c.Get(Equatorial, s).Delta)

            .AddHeader(Text.Get("Tycho2Star.Equatorial0"))
            .AddRow("Equatorial0.Alpha", s.Alpha0)
            .AddRow("Equatorial0.Delta", s.Delta0)

            .AddHeader(Text.Get("Tycho2Star.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Tycho2Star.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("Tycho2Star.Properties"))
            .AddRow("Magnitude", s.Magnitude);
        }

        private readonly Regex searchRegex = new Regex("^tyc\\s*(?<tyc1>\\d{1,4})((\\s*-\\s*|\\s+)(?<tyc2>\\d{1,5})((\\s*-\\s*|\\s+)(?<tyc3>\\d{1}))?)?$");

        public ICollection<CelestialObject> Search(SkyContext c, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            if (!IsLoaded)
            {
                return new CelestialObject[0];
            }

            var starWithProperName = ProperNames.FirstOrDefault(kv => kv.Value.StartsWith(searchString, StringComparison.OrdinalIgnoreCase));
            if (starWithProperName.Key != null)
            {
                searchString = starWithProperName.Key;
            }

            var match = searchRegex.Match(searchString.ToLowerInvariant());
            if (match.Success)
            {
                double t = c.Get(YearsSince2000);
                int tyc1 = int.Parse(match.Groups["tyc1"].Value);
                short? tyc2 = match.Groups["tyc2"].Success ? short.Parse(match.Groups["tyc2"].Value) : (short?)null;
                string tyc3 = match.Groups["tyc3"].Value;
                if (tyc1 > 0 && tyc1 <= 9537)
                {
                    Tycho2Region region = IndexRegions.ElementAt(tyc1 - 1);
                    var stars = GetStarsInRegion(region, t, tyc2, tyc3);
                    return stars.Where(filterFunc).Take(maxCount).ToArray();
                }
            }
            return new CelestialObject[0];
        }
    }
}
