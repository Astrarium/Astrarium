using Astrarium.Types;
using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;

namespace Astrarium.Plugins.Satellites
{
    public class SatellitesCalculator : BaseCalc, ICelestialObjectCalc<Satellite>, ISatellitesCalculator
    {
        private const double AU = 149597870.691;

        private readonly ISky sky;
        private readonly ISettings settings;

        private List<TLESource> tleSources = new List<TLESource>();
        private List<Satellite> satellites = new List<Satellite>();

        public IEnumerable<Satellite> Satellites => satellites.Where(x => tleSources.Any(s => s.IsEnabled && x.Sources.Contains(s.FileName)));

        public Vec3 SunVector { get; private set; }

        public object Locker { get; private set; } = new object();

        /// <inheritdoc />
        public IEnumerable<Satellite> GetCelestialObjects() => new Satellite[0];

        public double JulianDay { get; private set; }

        private Func<SkyContext, CrdsEquatorial> SunEquatorial;

        public SatellitesCalculator(ISky sky, ISettings settings)
        {
            this.sky = sky;
            this.settings = settings;
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            SunEquatorial = sky.SunEquatorial;

            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string satellitesDataFile = Path.Combine(baseDir, "Data", "Satellites.dat");

            // Load general satellites data (names, magnitude, sizes) 
            LoadSatellitesData(satellitesDataFile);

            GetTleSources();
        }

        private void GetTleSources()
        {
            // get current TLE sources list
            tleSources = settings.Get<List<TLESource>>("SatellitesOrbitalElements");
        }

        public void Calculate()
        {
            GetTleSources();
            Calculate(sky.Context);
        }

        public override void Calculate(SkyContext context)
        {
            lock (Locker)
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
        }

        public Vec3 TopocentricLocationVector(SkyContext context)
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
                    return Text.Get("Satellite.Magnitude.Eclipsed");
                else
                    return base.Format(value);
            }
        }

        private IEphemFormatter distanceFormatter = new Formatters.UnsignedDoubleFormatter(0, " km");
        private IEphemFormatter magnitudeFormatter = new SatelliteMagnitudeFormatter();
        private IEphemFormatter angle4Formatter = new Formatters.UnsignedDoubleFormatter(4, "\u00B0");
        private IEphemFormatter angle1Formatter = new Formatters.UnsignedDoubleFormatter(1, "\u00B0");

        private CrdsHorizontal Horizontal(SkyContext c, Satellite s)
        {
            var vecTopocentric = c.Get(TopocentricSatelliteVector, s);
            return Norad.HorizontalCoordinates(c.GeoLocation, vecTopocentric, c.SiderealTime);
        }

        private CrdsEquatorial Equatorial(SkyContext c, Satellite s)
        {
            var hor = c.Get(Horizontal, s);
            return hor.ToEquatorial(c.GeoLocation, c.SiderealTime);
        }

        private Vec3 GeocentricSatelliteVector(SkyContext c, Satellite s)
        {
            double deltaTime = (c.JulianDay - JulianDay) * 24;
            Norad.SGP4(s.Tle, c.JulianDay, s.Position, s.Velocity);
            return s.Position + deltaTime * s.Velocity;
        }

        public Vec3 TopocentricSatelliteVector(SkyContext c, Satellite s)
        {
            Vec3 vecTopoLocation = c.Get(TopocentricLocationVector);
            Vec3 vecGeocentric = c.Get(GeocentricSatelliteVector, s);
            return Norad.TopocentricSatelliteVector(vecTopoLocation, vecGeocentric);
        }

        public float Magnitude(SkyContext c, Satellite s)
        {
            var vecTopocentric = c.Get(TopocentricSatelliteVector, s);
            var distance = vecTopocentric.Length;
            return Norad.GetSatelliteMagnitude(s.StdMag, distance);
        }

        public void GetInfo(CelestialObjectInfo<Satellite> info)
        {
            Satellite s = info.Body;
            SkyContext c = info.Context;

            var vecGeocentric = c.Get(GeocentricSatelliteVector, s);
            var vecTopocentric = c.Get(TopocentricSatelliteVector, s);

            // distance from the observer, in km
            double distance = vecTopocentric.Length;

            // satellite magnitude
            float magnitude = c.Get(Magnitude, s);

            // coordinates of the satellite
            var hor = c.Get(Horizontal, s);
            var eq = c.Get(Equatorial, s);

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

                .AddRow("Constellation", Constellations.FindConstellation(eq, c.JulianDay))

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

            satellites.ForEach(x => x.Equatorial = context.Get(Equatorial, x as Satellite));

            return satellites;
        }

        public CelestialObject Search(SkyContext context, string bodyType, string bodyName)
        {
            if (bodyType == "Satellite")
            {
                var satellite = Satellites.FirstOrDefault(m => m.CommonName == bodyName);
                if (satellite != null) 
                {
                    satellite.Equatorial = context.Get(Equatorial, satellite);
                }
                return satellite;
            }

            return null;
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

        public void LoadSatellites(string directory, TLESource tleSource)
        {
            int totalCount = 0;
            int newCount = 0;
            string tleFile = Path.Combine(directory, $"{tleSource.FileName}.tle");
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
                        if (!existing.Sources.Contains(tleSource.FileName))
                        {
                            existing.Sources.Add(tleSource.FileName);
                        }
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
                        satellite.Sources.Add(tleSource.FileName);
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
