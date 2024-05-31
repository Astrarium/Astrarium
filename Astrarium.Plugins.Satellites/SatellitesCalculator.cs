using Astrarium.Types;
using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Net;
using System.Threading;

namespace Astrarium.Plugins.Satellites
{
    public class SatellitesCalculator : BaseCalc, ICelestialObjectCalc<Satellite>
    {
        private const double AU = 149597870.691;

        private List<Satellite> satellites = new List<Satellite>();
        public ICollection<Satellite> Satellites => satellites;

        public Vec3 SunVector { get; private set; }

        /// <inheritdoc />
        public IEnumerable<Satellite> GetCelestialObjects() => new Satellite[0];

        public double JulianDay { get; private set; }

        private Func<SkyContext, CrdsEquatorial> SunEquatorial;

        private readonly ISky sky;
        private readonly ISettings settings;

        public SatellitesCalculator (ISky sky, ISettings settings)
        {
            this.sky = sky;
            this.settings = settings;
        }

        /// <inheritdoc />
        public override async void Initialize()
        {
            SunEquatorial = sky.SunEquatorial;

            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string satellitesDataFile = Path.Combine(baseDir, "Data/Satellites.dat");
            string tleFile = Path.Combine(baseDir, "Data/Brightest.tle");

            // Load general satellites data (names, magnitude, sizes) 
            LoadSatellitesData(satellitesDataFile);

            // TLE sources from settings
            List<TLESource> tleSources = settings.Get<List<TLESource>>("SatellitesOrbitalElements");

            // use app data path to satellites data (downloaded by user)
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Satellites");

            // user directory for satellites data exists and contains TLE files
            if (Directory.Exists(directory) && 
                Directory.EnumerateFiles(directory, "*.tle").Any())
            {
                // load TLE files that match settings
                var tleFiles = Directory.EnumerateFiles(directory, "*.tle")
                    .Where(fileName => tleSources.Any(x => x.IsEnabled && x.FileName == Path.GetFileNameWithoutExtension(fileName)));

                foreach (string file in tleFiles)
                {
                    LoadSatellites(file);
                }
            }

            // update TLEs
            foreach (var tleSource in tleSources)
            {
                if (tleSource.IsEnabled &&
                   (tleSource.LastUpdated == null || DateTime.Now.Subtract(tleSource.LastUpdated.Value).TotalDays >= 1))
                {
                    Log.Info($"Obital elements of satellites ({tleSource.FileName}) needs to be updated, updating...");
                    await Task.Run(() =>
                    {
                        UpdateOrbitalElements(tleSource, silent: true);
                        tleSource.LastUpdated = DateTime.Now;
                        settings.SetAndSave("SatellitesOrbitalElements", tleSources);
                        string path = Path.Combine(directory, tleSource.FileName + ".tle");
                        LoadSatellites(path);
                    });
                }
            }
        }

        private const int BUFFER_SIZE = 1024;

        public void UpdateOrbitalElements(TLESource tleSource, bool silent)
        {
            string tempFile = Path.GetTempFileName();
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            try
            {
                ServicePointManager.SecurityProtocol =
                                SecurityProtocolType.Tls |
                                SecurityProtocolType.Tls11 |
                                SecurityProtocolType.Tls12 |
                                SecurityProtocolType.Ssl3;

                // use app data path to satellites data (downloaded by user)
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Satellites");

                string targetPath = Path.Combine(directory, tleSource.FileName + ".tle");

                WebRequest request = WebRequest.Create(tleSource.Url);
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                using (Stream fileStream = new FileStream(tempFile, FileMode.OpenOrCreate))
                using (BinaryWriter streamWriter = new BinaryWriter(fileStream))
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    int bytesRead = 0;
                    StringBuilder remainder = new StringBuilder();

                    do
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        bytesRead = responseStream.Read(buffer, 0, BUFFER_SIZE);
                        streamWriter.Write(buffer, 0, bytesRead);
                    }
                    while (bytesRead > 0);

                    File.Copy(tempFile, targetPath, overwrite: true);
                }
            }
            catch 
            {
                
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public override void Calculate(SkyContext context)
        {
            // Calculate rectangular coordinates of the Sun
            var eq = SunEquatorial.Invoke(context);
            var ecl = eq.ToEcliptical(context.Epsilon);
            ecl.Distance = 1;
            var r = ecl.ToRectangular(context.Epsilon);
            SunVector = AU * new Vec3(r.X, r.Y, r.Z);

            // To reduce CPU load, it's enough to calculate
            // satellites positions once in 5 minutes
            //if (Math.Abs(JulianDay - context.JulianDay) > TimeSpan.FromMinutes(5).TotalDays)
            {
                double deltaT = Date.DeltaT(context.JulianDay);
                var jd = context.JulianDay - deltaT / 86400;
                foreach (var s in Satellites)
                {
                    Norad.SGP4(s.Tle, jd, s.Position, s.Velocity);
                }

                JulianDay = context.JulianDay;
            }
        }

        public Vec3 GetTopocentricLocationVector(SkyContext context)
        {
            return Norad.TopocentricLocationVector(context.GeoLocation, context.SiderealTime);
        }

        public void ConfigureEphemeris(EphemerisConfig<Satellite> e)
        {
            // Satellites do not provide epheremeris
        }

        private class SatelliteMagnitudeFormatter : Formatters.SignedDoubleFormatter
        {
            public SatelliteMagnitudeFormatter() : base(1, "ᵐ") { }
            public override string Format(object value)
            {
                if (value == null)
                    // TODO: localize
                    return "[В тени]";
                else
                    return base.Format(value);
            }
        }

        private IEphemFormatter distanceFormatter = new Formatters.UnsignedDoubleFormatter(0, " km");
        private IEphemFormatter magnitudeFormatter = new SatelliteMagnitudeFormatter();
        private IEphemFormatter angle4Formatter = new Formatters.UnsignedDoubleFormatter(4, "\u00B0");
        private IEphemFormatter angle1Formatter = new Formatters.UnsignedDoubleFormatter(1, "\u00B0");

        public void GetInfo(CelestialObjectInfo<Satellite> info)
        {
            Satellite s = info.Body;
            SkyContext c = info.Context;

            double deltaTime = (c.JulianDay - JulianDay) * 24;

            // current satellite position vector
            Vec3 vecGeocentric = s.Position + deltaTime * s.Velocity;

            Vec3 vecTopoLocation = GetTopocentricLocationVector(c);
            Vec3 vecTopocentric = Norad.TopocentricSatelliteVector(vecTopoLocation, vecGeocentric);

            // distance from the observer, in km
            double distance = vecTopocentric.Length;

            // satellite magnitude
            float magnitude = Norad.GetSatelliteMagnitude(s.StdMag, distance);

            // coordinates of the satellite
            var hor = Norad.HorizontalCoordinates(c.GeoLocation, vecTopocentric, c.SiderealTime);
            var eq = hor.ToEquatorial(c.GeoLocation, c.SiderealTime);

            bool isEclipsed = Norad.IsSatelliteEclipsed(vecGeocentric, SunVector);
            double ssoAngle = Angle.ToDegrees((-1 * vecTopocentric).Angle(SunVector - vecTopocentric));

            double age = c.JulianDay - s.Tle.Epoch;

            string haBaseUri = "https://heavens-above.com/";
            string haQuery = "?satid=" + Uri.EscapeDataString(s.Tle.SatelliteNumber) + $"&lat={c.GeoLocation.Latitude.ToString(CultureInfo.InvariantCulture)}&lng={(-c.GeoLocation.Longitude).ToString(CultureInfo.InvariantCulture)}";

            string n2yoBaseUri = "https://www.n2yo.com/";
            string n2yoQuery = $"?s={s.Tle.SatelliteNumber}";

            string openInBrowser = Text.Get("Satellite.WebBrowser.OpenLink");

            info
                .SetTitle(string.Join(", ", s.Names))
                .SetSubtitle(Text.Get("Satellite.Type"))

                .AddRow("Constellation", Constellations.FindConstellation(s.Equatorial, c.JulianDay))

                .AddHeader(Text.Get("Satellite.Equatorial"))
                .AddRow("Equatorial.Alpha", eq.Alpha)
                .AddRow("Equatorial.Delta", eq.Delta)

                .AddHeader(Text.Get("Satellite.Horizontal"))
                .AddRow("Horizontal.Azimuth", hor.Azimuth)
                .AddRow("Horizontal.Altitude", hor.Altitude)

                .AddHeader(Text.Get("Satellite.Characteristics"))
                .AddRow("Magnitude", isEclipsed ? (float?)null : magnitude, magnitudeFormatter)
                .AddRow("Distance", distance, distanceFormatter)
                .AddRow("SSOAngle", ssoAngle, angle1Formatter)
                .AddRow("TopocentricPositionVector", vecTopocentric, Formatters.Simple)
                .AddRow("GeocentricPositionVector", vecGeocentric, Formatters.Simple)
                .AddRow("GeocentricVelocityVector", s.Velocity, Formatters.Simple)

                .AddHeader(Text.Get("Satellite.AlternateNames"))
                .AddRow(Text.Get("Satellite.SatcatId"), new Uri("https://celestrak.org/satcat/table-satcat.php?CATNR=" + Uri.EscapeDataString(s.Tle.SatelliteNumber)), s.Tle.SatelliteNumber)
                .AddRow(Text.Get("Satellite.CosparId"), new Uri("https://nssdc.gsfc.nasa.gov/nmc/spacecraft/display.action?id=" + Uri.EscapeDataString(s.Tle.InternationalDesignator)), s.Tle.InternationalDesignator)

                .AddHeader(Text.Get("Satellite.OrbitalData"))
                .AddRow("Epoch", new Date(s.Tle.Epoch, c.GeoLocation.UtcOffset), Formatters.DateTime)
                .AddRow("Inclination", s.Tle.Inclination, Formatters.Inclination)
                .AddRow("Eccentricity", s.Tle.Eccentricity, Formatters.Simple)
                .AddRow("ArgumentOfPerigee", s.Tle.ArgumentOfPerigee, angle4Formatter)
                .AddRow("LongitudeOfAscendingNode", s.Tle.LongitudeAscNode, angle4Formatter)
                .AddRow("MeanAnomaly", s.Tle.MeanAnomaly, angle4Formatter)
                .AddRow("Period", TimeSpan.FromMinutes(s.Tle.Period), Formatters.TimeSpan)
                .AddRow("Apogee", Norad.GetSatelliteApogee(s.Tle), distanceFormatter)
                .AddRow("Perigee", Norad.GetSatellitePerigee(s.Tle), distanceFormatter)
                .AddRow("OrbitalDataAge", TimeSpan.FromDays(age), Formatters.TimeSpan)

                .AddHeader("heavens-above.com")
                .AddRow(Text.Get("Satellite.HeavensAbove.Info"), new Uri(haBaseUri + "satinfo.aspx" + haQuery), openInBrowser)
                .AddRow(Text.Get("Satellite.HeavensAbove.Orbit"), new Uri(haBaseUri + "orbit.aspx" + haQuery), openInBrowser)
                .AddRow(Text.Get("Satellite.HeavensAbove.Passes"), new Uri(haBaseUri + "PassSummary.aspx" + haQuery), openInBrowser)
                .AddRow(Text.Get("Satellite.HeavensAbove.CloseEncounters"), new Uri(haBaseUri + "CloseEncounters.aspx" + haQuery), openInBrowser)

                .AddHeader("N2YO.com")
                .AddRow(Text.Get("Satellite.N2YO.Info"), new Uri(n2yoBaseUri + "satellite/" + n2yoQuery), openInBrowser)
                .AddRow(Text.Get("Satellite.N2YO.LiveTracking"), new Uri(n2yoBaseUri + n2yoQuery + "&live=1"), openInBrowser)
                .AddRow(Text.Get("Satellite.N2YO.Passes"), new Uri(n2yoBaseUri + "passes/" + n2yoQuery), openInBrowser);
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            var satellites = Satellites
                .Where(s => s.Names.Any(n => n.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0))
                .Where(filterFunc)
                .Take(maxCount)
                .ToList();

            // TODO: calculate equatorial position
            // satellites.ForEach(x => x.Equatorial = Eq)

            return satellites;
        }

        private Dictionary<string, float> stdMagnitudes = new Dictionary<string, float>();
        private float averageStdMagnitude;

        private void LoadSatellitesData(string satellitesDataFile)
        {
            using (var sr = new StreamReader(satellitesDataFile, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.Length < 37) continue;
                    string number = line.Substring(0, 5);
                    if (number == "00001" || number == "99999") continue;

                    string mag = line.Substring(33, 4).Trim();
                    if (!string.IsNullOrEmpty(mag))
                    {
                        stdMagnitudes[number] = float.Parse(mag, CultureInfo.InvariantCulture);
                    }
                }
                sr.Close();
            }

            averageStdMagnitude = stdMagnitudes.Values.Average();
        }

        private float GetStdMagnitude(string satNumber)
        {
            if (stdMagnitudes.ContainsKey(satNumber))
            {
                return stdMagnitudes[satNumber];
            }
            else
            {
                return averageStdMagnitude;
            }
        }

        private void LoadSatellites(string tleFile)
        {
            int totalCount = 0;
            int newCount = 0;
            using (var sr = new StreamReader(tleFile, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string name = sr.ReadLine();
                    string line1 = sr.ReadLine();
                    string line2 = sr.ReadLine();
                    var tle = new TLE(line1, line2);

                    // check satellite already exists
                    var existing = satellites.FirstOrDefault(x => x.Tle.SatelliteNumber == tle.SatelliteNumber);

                    totalCount++;

                    // update TLE if newer
                    if (existing != null)
                    {
                        if (existing.Tle.Epoch < tle.Epoch)
                        {
                            existing.Tle = tle;
                        }
                    }
                    // add new satellite instance
                    else
                    {
                        var satellite = new Satellite(name.Trim(), tle);
                        satellite.StdMag = GetStdMagnitude(satellite.Tle.SatelliteNumber);
                        satellites.Add(satellite);
                        newCount++;
                    }
                }
                sr.Close();
            }

            Log.Debug($"Loaded {newCount} satellites from file {tleFile} (total records in file: {totalCount})");
        }
    }
}
