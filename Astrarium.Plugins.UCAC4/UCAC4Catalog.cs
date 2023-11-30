using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Astrarium.Plugins.UCAC4
{
    public class UCAC4Catalog : BaseCalc, ICelestialObjectCalc<UCAC4Star>
    {
        private const int RECORD_LEN = 78;
        private const int ZONES_COUNT = 900;
        private const int BINS_IN_ZONE = 1440;

        private object indexLocker = new object();
        private object[] zoneLockers = new object[ZONES_COUNT];

        private readonly ISettings settings;
        private BinaryReader indexReader;
        private Dictionary<string, string> properNames;
        private readonly ISky sky;
        private readonly BinaryReader[] zoneReaders = new BinaryReader[ZONES_COUNT];
        private readonly bool[] zoneAvailable = new bool[ZONES_COUNT];
        private readonly List<UCAC4HPMStarData> hpmStars = new List<UCAC4HPMStarData>();
        private bool isInitialized = false;

        public PrecessionalElements PrecessionElements0 { get; private set; }

        /// <summary>
        /// Flag indicating the catalog data has been found and loaded
        /// </summary>
        public bool IsLoaded 
        {
            get => GetValue<bool>(nameof(IsLoaded));
            private set
            {
                SetValue(nameof(IsLoaded), value);
                NotifyPropertyChanged(nameof(ZonesCount));
            }
        }

        /// <summary>
        /// Count of available catalog zones
        /// </summary>
        public int ZonesCount
        {
            get => zoneAvailable.Where(z => z).Count();
        }

        public UCAC4Catalog(ISky sky, ISettings settings)
        {
            this.sky = sky;
            this.settings = settings;
            this.settings.SettingValueChanged += Settings_SettingValueChanged;
        }

        private void Settings_SettingValueChanged(string settingName, object settingValue)
        {
            if (isInitialized && settingName == "UCAC4RootDir")
            {
                Initialize();
            }
        }

        public bool Validate(string rootDir, bool verbose = false)
        {
            string indexFilePath = Path.Combine(rootDir, "u4i", "u4index.unf");

            if (string.IsNullOrEmpty(rootDir))
            {
                Log.Error("UCAC4 root directory is not set.");
                return false;
            }

            if (!Directory.Exists(rootDir))
            {
                Log.Error("UCAC4 root directory is not exist.");
                return false;
            }

            if (!File.Exists(indexFilePath))
            {
                Log.Error($"UCAC4 index file not found, search path: {indexFilePath}");
                if (verbose)
                {
                    ViewManager.ShowMessageBox("$Error", Text.Get("UCAC4.Errors.IndexFileNotFound", ("indexFilePath", indexFilePath)));
                }
                return false;
            }

            int matchingZoneFiles = Directory.GetFiles(Path.Combine(rootDir, "u4b"), $"z*").Where(f => Regex.IsMatch(Path.GetFileName(f), @"z\d{3}")).Count();

            if (matchingZoneFiles == 0)
            {
                if (verbose)
                {
                    ViewManager.ShowMessageBox("$Error", Text.Get("UCAC4.Errors.NoZoneFiles", ("zoneFilesDir", Path.Combine(rootDir, "u4b"))));
                }
                return false;
            }

            string hpmFilePath = Path.Combine(rootDir, "u4i", "u4hpm.dat");
            if (!File.Exists(hpmFilePath))
            {
                if (verbose)
                {
                    Log.Error($"UCAC4 HPM data file not found, search path: {hpmFilePath}");
                }
                ViewManager.ShowMessageBox("Error", Text.Get("UCAC4.Errors.HPMFileNotFound", ("hpmFilePath", hpmFilePath)));
                return false;
            }

            return true;
        }

        public override void Initialize()
        {
            try
            {
                string rootDir = settings.Get<string>("UCAC4RootDir", null);
                IsLoaded = false;

                if (Validate(rootDir))
                {
                    string indexFilePath = Path.Combine(rootDir, "u4i", "u4index.unf");

                    try
                    {
                        indexReader = new BinaryReader(File.Open(indexFilePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Could not get access to UCAC4 index file: {indexFilePath}, error: {ex}");
                        return;
                    }

                    for (int z = 1; z <= ZONES_COUNT; z++)
                    {
                        string path = Path.Combine(rootDir, "u4b", $"z{z:000}");
                        if (File.Exists(path))
                        {
                            zoneReaders[z - 1] = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                            zoneLockers[z - 1] = new object();
                            zoneAvailable[z - 1] = true;
                        }
                        else
                        {
                            zoneReaders[z - 1] = null;
                            zoneLockers[z - 1] = null;
                            zoneAvailable[z - 1] = false;
                        }
                    }

                    string hpmFilePath = Path.Combine(rootDir, "u4i", "u4hpm.dat");
                    if (File.Exists(hpmFilePath))
                    {
                        ReadHPMStars(hpmFilePath);
                    }
                    else
                    {
                        Log.Error($"UCAC4 HPM data file not found, search path: {hpmFilePath}");
                    }

                    properNames = sky.StarNames.Where(x => x.Key.StartsWith("UCAC4")).ToDictionary(x => x.Key, x => x.Value);

                    IsLoaded = true;
                }
            }
            finally
            {
                isInitialized = true;
            }
        }

        /// <summary>
        /// Reads High Proper Motion stars from the file
        /// </summary>
        /// <param name="path">Full path to the HPM stars data</param>
        private void ReadHPMStars(string path)
        {
            string[] lines = File.ReadAllLines(path);
            hpmStars.Clear();
            foreach (var line in lines)
            {
                string[] cols = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length == 8)
                {
                    int zn = int.Parse(cols[1]);
                    int rn = int.Parse(cols[2]);
                    int pmrc = int.Parse(cols[3]);
                    int pmd = int.Parse(cols[4]);
                    hpmStars.Add(new UCAC4HPMStarData()
                    {
                        ZoneNumber = zn,
                        RunningNumber = rn,
                        PmRac = pmrc,
                        PmDc = pmd
                    });
                }
            }
        }

        /// <summary>
        /// Checks is the bin (part of catalog zone) visible with specified center of the screen and FOV angle
        /// </summary>
        /// <param name="eq">Center of the screen, J2000.0</param>
        /// <param name="angle">FOV angle, degrees</param>
        /// <param name="zn">Zone number, 1...900</param>
        /// <param name="j">Bin's number</param>
        /// <returns>True if the bin is visible, false otherwise</returns>
        private bool IsBinVisible(CrdsEquatorial eq, double angle, int zn, int j)
        {
            double dec = -90 + 0.1 + (zn - 1) * 0.2;
            double ra = 0.125 + (j - 1) * 0.25;
            CrdsEquatorial eqBinCenter = new CrdsEquatorial(ra, dec);
            return Angle.Separation(eq, eqBinCenter) <= angle + 0.2;
        }

        /// <summary>
        /// Gets stars in specified circular area
        /// </summary>
        /// <param name="eq0">Equatorial coordinates of area center, at epoch J2000.0</param>
        /// <param name="angle">Area radius, in degrees</param>
        /// <param name="magFilter">Magnitude filter function, returns true if star is visible and should be included in results</param>
        /// <returns>Collection of <see cref="UCAC4Star"/> objects</returns>
        public IEnumerable<UCAC4Star> GetStars(SkyContext context, CrdsEquatorial eq0, double angle, Func<float, bool> magFilter)
        {
            int j = (int)(eq0.Alpha / 0.25) + 1;
            int zn = (int)((eq0.Delta + 90) / 0.2) + 1;

            if (indexReader != null)
            {
                if (zn <= 5 && angle > 0.25)
                {
                    for (int z = 1; z <= 10; z++)
                    {
                        var bin = GetBin(z, BINS_IN_ZONE);

                        foreach (var star in ReadStarsForZone(context, z, (int)(bin.N0 + bin.NN), magFilter))
                        {
                            yield return star;
                        }
                    }
                }
                else if (zn >= ZONES_COUNT - 5 && angle > 0.25)
                {
                    for (int z = ZONES_COUNT - 10; z <= ZONES_COUNT; z++)
                    {
                        var bin = GetBin(z, BINS_IN_ZONE);
                        foreach (var star in ReadStarsForZone(context, z, (int)(bin.N0 + bin.NN), magFilter))
                        {
                            yield return star;
                        }
                    }
                }
                else
                {
                    foreach (var bin in FindVisibleBins(eq0, angle, zn))
                    {
                        foreach (var star in ReadStarsForBin(context, bin, magFilter))
                        {
                            yield return star;
                        }
                    }
                }
            }
        }

        private IEnumerable<Bin> FindVisibleBins(CrdsEquatorial eq, double angle, int zn)
        {
            bool flag;
            int z = zn;

            for (int j = 1; j <= BINS_IN_ZONE; j++)
            {
                if (IsBinVisible(eq, angle, z, j))
                {
                    var bin = GetBin(z, j);
                    if (bin.NN > 0)
                    {
                        yield return bin;
                    }
                }
            }

            z = zn;
            do
            {
                flag = false;
                z++;

                if (z == ZONES_COUNT + 1) break;

                for (int j = 1; j <= BINS_IN_ZONE; j++)
                {
                    if (IsBinVisible(eq, angle, z, j))
                    {
                        var bin = GetBin(z, j);
                        if (bin.NN > 0)
                        {
                            yield return bin;
                        }
                        flag = true;
                    }
                }
            }
            while (flag);

            z = zn;
            do
            {
                flag = false;
                z--;

                if (z == 0) break;

                for (int j = 1; j <= BINS_IN_ZONE; j++)
                {
                    if (IsBinVisible(eq, angle, z, j))
                    {
                        var bin = GetBin(z, j);
                        if (bin.NN > 0)
                        {
                            yield return bin;
                        }
                        flag = true;
                    }
                }
            }
            while (flag);
        }

        private Bin GetBin(int zn, int j)
        {
            const long HALF_FILE = BINS_IN_ZONE * ZONES_COUNT * 4;
            long offset = ((j - 1) * ZONES_COUNT + (zn - 1)) * 4;

            uint n0, nn;

            lock (indexLocker)
            {
                indexReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                n0 = indexReader.ReadUInt32();

                indexReader.BaseStream.Seek(HALF_FILE + offset, SeekOrigin.Begin);
                nn = indexReader.ReadUInt32();
            }

            return new Bin()
            {
                J = j,
                ZN = zn,
                N0 = n0,
                NN = nn
            };
        }

        private UCAC4StarPosData ParsePositionData(byte[] starDataBuffer, int offset, int zn, int starIndex)
        {
            UCAC4StarPosData p = new UCAC4StarPosData();

            p.RA2000 = BitConverter.ToUInt32(starDataBuffer, offset) / 3600000.0;
            p.Dec2000 = -90 + BitConverter.ToUInt32(starDataBuffer, 4 + offset) / 3600000.0;

            int pmrac = BitConverter.ToInt16(starDataBuffer, 24 + offset);
            int pmdc = BitConverter.ToInt16(starDataBuffer, 26 + offset);

            // HPM stars
            if (pmrac == 32767 || pmdc == 32767)
            {
                var hpmData = hpmStars.FirstOrDefault(s => s.ZoneNumber == zn && s.RunningNumber == starIndex + 1);
                if (hpmData != null)
                {
                    pmrac = hpmData.PmRac;
                    pmdc = hpmData.PmDc;
                }
            }
            
            p.PmRA = pmrac / Math.Cos(Angle.ToRadians(p.Dec2000)) / 36000000.0;
            p.PmDec = pmdc / 36000000.0;

            return p;
        }

        /// <param name="starsData"></param>
        /// <param name="offset"></param>
        /// <param name="zn"></param>
        /// <param name="starIndex"></param>
        /// <param name="magFilter"></param>
        /// <param name="context">Current observation context</param>
        /// <returns></returns>
        private UCAC4Star ReadStar(SkyContext context, byte[] starsData, int offset, int zn, int starIndex, Func<float, bool> magFilter)
        {
            float mag = BitConverter.ToInt16(starsData, 8 + offset) / 1000.0f;
            if (magFilter(mag))
            {
                float bmag = BitConverter.ToInt16(starsData, 46 + offset) / 1000.0f;
                float vmag = BitConverter.ToInt16(starsData, 48 + offset) / 1000.0f;
                var posData = ParsePositionData(starsData, offset, zn, starIndex);

                string catName = $"UCAC4 {zn:000}-{starIndex + 1:000000}";

                return new UCAC4Star()
                {
                    ZoneNumber = (ushort)zn,
                    RunningNumber = (uint)(starIndex + 1),
                    Magnitude = mag,
                    SpectralClass = SpectralClass(bmag, vmag),
                    Equatorial = Equatorial(context, posData),
                    ProperName = properNames.ContainsKey(catName) ? properNames[catName] : null
                };
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<UCAC4Star> ReadStarsForZone(SkyContext context, int zn, int totalCount, Func<float, bool> magFilter)
        {
            if (zoneAvailable[zn - 1])
            {
                byte[] starsData = ReadDataBuffer(zn, 0, totalCount);
                for (int i = 0; i < totalCount; i++)
                {
                    var star = ReadStar(context, starsData, i * RECORD_LEN, zn, i, magFilter);
                    if (star != null)
                    {
                        yield return star;
                    }
                }
            }
        }

        private UCAC4Star ReadStarAtPosition(SkyContext context, int zn, int runningNumber)
        {
            byte[] starsData = ReadDataBuffer(zn, runningNumber - 1, 1);
            return ReadStar(context, starsData, 0, zn, runningNumber - 1, m => true);
        }

        private IEnumerable<UCAC4Star> ReadStarsForBin(SkyContext context, Bin bin, Func<float, bool> magFilter)
        {
            if (zoneAvailable[bin.ZN - 1])
            {
                byte[] starsData = ReadDataBuffer(bin.ZN, (int)bin.N0, (int)bin.NN);
                for (int i = 0; i < bin.NN; i++)
                {
                    int offset = i * RECORD_LEN;
                    var star = ReadStar(context, starsData, offset, bin.ZN, (int)(bin.N0 + i), magFilter);
                    if (star != null)
                    {
                        yield return star;
                    }
                }
            }
        }

        public void ConfigureEphemeris(EphemerisConfig<UCAC4Star> e)
        {
            e["Constellation"] = (c, s) => c.Get(FindConstellation, s);
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

        /// <summary>
        /// Reads data buffer from catalog file
        /// </summary>
        /// <param name="zoneNumber">Number of zone (1...900)</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="starsCount">Records count to read</param>
        /// <returns>Binary buffer of data</returns>
        private byte[] ReadDataBuffer(int zoneNumber, int skipCount, int starsCount)
        {
            byte[] starsData = new byte[RECORD_LEN * starsCount];
            lock (zoneLockers[zoneNumber - 1])
            {
                zoneReaders[zoneNumber - 1].BaseStream.Seek(RECORD_LEN * skipCount, SeekOrigin.Begin);
                zoneReaders[zoneNumber - 1].Read(starsData, 0, starsData.Length);
            }
            return starsData;
        }

        /// <summary>
        /// Loads data needed for calculating star position
        /// </summary>
        /// <param name="star"></param>
        private void LoadPositionData(UCAC4Star star)
        {
            if (star.PositionData == null)
            {
                byte[] starDataBuffer = ReadDataBuffer(star.ZoneNumber, (int)(star.RunningNumber - 1), 1);
                star.PositionData = ParsePositionData(starDataBuffer, 0, star.ZoneNumber, (int)(star.RunningNumber - 1));
            }
        }

        private CrdsEquatorial Equatorial0(SkyContext context, UCAC4Star star)
        {
            LoadPositionData(star);
            return new CrdsEquatorial(star.PositionData.RA2000, star.PositionData.Dec2000);
        }

        private CrdsEquatorial Equatorial(SkyContext context, UCAC4Star star)
        {
            LoadPositionData(star);
            return Equatorial(context, star.PositionData);
        }

        private CrdsHorizontal Horizontal(SkyContext context, UCAC4Star star)
        {
            return context.Get(Equatorial, star).ToHorizontal(context.GeoLocation, context.SiderealTime);
        }

        private string FindConstellation(SkyContext context, UCAC4Star star)
        {
            return Constellations.FindConstellation(context.Get(Equatorial, star), context.JulianDay);
        }

        private CrdsEquatorial Equatorial(SkyContext context, UCAC4StarPosData pos)
        {
            // Years since J2000.0, with fractions
            double years = (context.JulianDay - Date.EPOCH_J2000) / 365.25;

            // Take into account effect of proper motion:
            // now coordinates are for the mean equinox of J2000.0,
            // but for epoch of the target date
            CrdsEquatorial eq0 = new CrdsEquatorial(pos.RA2000 + pos.PmRA * years, pos.Dec2000 + pos.PmDec * years);

            // Equatorial coordinates for the mean equinox and epoch of the target date
            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, context.PrecessionElements);

            // Nutation effect
            var eqN = Nutation.NutationEffect(eq, context.NutationElements, context.Epsilon);

            // Aberration effect
            var eqA = Aberration.AberrationEffect(eq, context.AberrationElements, context.Epsilon);

            // Apparent coordinates of the star
            eq += eqN + eqA;

            return eq;
        }

        private char SpectralClass(float b, float v)
        {
            double B_V = b - v;

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

        /// <summary>
        /// Gets rise, transit and set info for the star
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, UCAC4Star star)
        {
            double theta0 = Date.ApparentSiderealTime(c.JulianDayMidnight, c.NutationElements.deltaPsi, c.Epsilon);
            var eq = c.Get(Equatorial, star);
            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        public void GetInfo(CelestialObjectInfo<UCAC4Star> info)
        {
            SkyContext c = info.Context;
            UCAC4Star s = info.Body;

            info
            .SetTitle(s.ToString())
            .SetSubtitle(Text.Get("UCAC4Star.Type"))
            .AddRow("Constellation")

            .AddHeader(Text.Get("UCAC4Star.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("UCAC4Star.Equatorial0"))
            .AddRow("Equatorial0.Alpha", c.Get(Equatorial0, s).Alpha)
            .AddRow("Equatorial0.Delta", c.Get(Equatorial0, s).Delta)

            .AddHeader(Text.Get("UCAC4Star.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("UCAC4Star.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("UCAC4Star.Properties"))
            .AddRow("Magnitude", s.Magnitude);
        }

        private readonly Regex searchRegex = new Regex(@"^ucac4((\s*\-\s*)|\s+)(?<zone>\d{1,3})(((\s*\-\s*)|\s+)(?<number>\d{1,6})?)?$");

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            List<CelestialObject> stars = new List<CelestialObject>();

            if (!IsLoaded)
            {
                return stars;
            }

            var starWithProperName = properNames.FirstOrDefault(kv => kv.Value.StartsWith(searchString, StringComparison.OrdinalIgnoreCase));
            if (starWithProperName.Key != null)
            {
                searchString = starWithProperName.Key;
            }

            var match = searchRegex.Match(searchString.Trim().ToLowerInvariant());
            if (match.Success)
            {
                ushort zone = ushort.Parse(match.Groups["zone"].Value);

                uint? number = match.Groups["number"].Success ? (uint?)uint.Parse(match.Groups["number"].Value) : null;

                if (zone >= 1 && zone <= ZONES_COUNT && zoneAvailable[zone - 1])
                {
                    var bin = GetBin(zone, BINS_IN_ZONE);
                    int starsInBin = (int)(bin.N0 + bin.NN);

                    if (number == null)
                    {
                        for (int i = 0; i < Math.Min(maxCount, starsInBin); i++)
                        {
                            stars.Add(ReadStarAtPosition(context, zone, i + 1));
                        }
                    }
                    else if (number <= starsInBin)
                    {
                        if (number > 0)
                        {
                            stars.Add(ReadStarAtPosition(context, zone, (int)number.Value));
                        }

                        for (int i = 0; i < maxCount - 1; i++)
                        {
                            string sn = $"{match.Groups["number"].Value}{i}";
                            if (sn.Length > 6) break;
                            
                            uint n = uint.Parse(sn);
                            if (n <= starsInBin)
                            {
                                if (n > 0)
                                {
                                    stars.Add(ReadStarAtPosition(context, zone, (int)n));
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return stars;
        }

        /// <inheritdoc />
        public IEnumerable<UCAC4Star> GetCelestialObjects() => new UCAC4Star[0];

        public override void Calculate(SkyContext context)
        {
            PrecessionElements0 = Precession.ElementsFK5(context.JulianDay, Date.EPOCH_J2000);
        }
    }
}
