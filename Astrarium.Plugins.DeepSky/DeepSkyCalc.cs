using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Astrarium.Plugins.DeepSky
{
    /// <summary>
    /// Calculates coordinates of Deep Sky objects
    /// </summary>
    public class DeepSkyCalc : BaseCalc, ICelestialObjectCalc<DeepSky>
    {
        private static string LOCATION = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string NGCIC_FILE = Path.Combine(LOCATION, "Data/NGCIC.dat");
        private static string NAMES_FILE = Path.Combine(LOCATION, "Data/NGCICNames.dat");
        private static string OUTLINES_FILE = Path.Combine(LOCATION, "Data/Outlines.dat");

        /// <inheritdoc/>
        public IEnumerable<DeepSky> GetCelestialObjects() => deepSkies.Where(ds => !ds.Status.IsEmpty());

        /// <summary>
        /// Collection of NGC/IC object
        /// </summary>
        internal List<DeepSky> deepSkies = new List<DeepSky>();

        /// <summary>
        /// Length of single record in data file
        /// </summary>
        private long RecordLength = 0;

        private readonly ISky sky;

        private readonly IEphemFormatter angularSizeFormatter = new DeepSkyAngularSizeFormatter();
        private readonly IEphemFormatter surfaceBrightnessFormatter = new DeepSkySurfaceBrightnessFormatter();
        private readonly IEphemFormatter posAngleFormatter = new DeepSkyPositionAngleFormatter();

        public DeepSkyCalc(ISky sky)
        {
            this.sky = sky;
        }

        public override void Calculate(SkyContext context)
        {
            /*
            // precessional elements
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            foreach (var ds in deepSkies)
            {
                ds.Equatorial = context.Get(Equatorial, ds);
                ds.Horizontal = context.Get(Horizontal, ds);

                if (ds.Outline != null)
                {
                    foreach (var op in ds.Outline)
                    {
                        CrdsEquatorial eq0 = new CrdsEquatorial(op.Equatorial0);

                        // Equatorial coordinates for the mean equinox and epoch of the target date
                        var eq = Precession.GetEquatorialCoordinates(eq0, p);

                        // Nutation effect
                        var eq1 = Nutation.NutationEffect(eq, context.NutationElements, context.Epsilon);

                        // Aberration effect
                        var eq2 = Aberration.AberrationEffect(eq, context.AberrationElements, context.Epsilon);

                        // Apparent coordinates of the object
                        eq += eq1 + eq2;

                        // Apparent horizontal coordinates
                        op.Horizontal = eq.ToHorizontal(context.GeoLocation, context.SiderealTime);
                    }
                }
            }*/
        }

        /// <summary>
        /// Gets precessional elements to convert equatorial coordinates of stars to current epoch 
        /// </summary>
        private PrecessionalElements GetPrecessionalElements(SkyContext c)
        {
            return Precession.ElementsFK5(Date.EPOCH_J2000, c.JulianDay);
        }

        /// <summary>
        /// Gets equatorial coordinates of deep sky object for current epoch
        /// </summary>
        private CrdsEquatorial Equatorial(SkyContext c, DeepSky ds)
        {
            PrecessionalElements p = c.Get(GetPrecessionalElements);
            
            // Equatorial coordinates for the mean equinox and epoch of the target date
            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(ds.Equatorial0, p);

            // Nutation effect
            var eq1 = Nutation.NutationEffect(eq, c.NutationElements, c.Epsilon);

            // Aberration effect
            var eq2 = Aberration.AberrationEffect(eq, c.AberrationElements, c.Epsilon);

            // Apparent coordinates of the object
            eq += eq1 + eq2;

            return eq;
        }

        /// <summary>
        /// Gets apparent horizontal coordinates of deep sky object for given instant
        /// </summary>
        private CrdsHorizontal Horizontal(SkyContext c, DeepSky ds)
        {
            return c.Get(Equatorial, ds).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rise, transit and set info for the deep sky object
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, DeepSky ds)
        {
            double theta0 = Date.ApparentSiderealTime(c.JulianDayMidnight, c.NutationElements.deltaPsi, c.Epsilon);
            var eq = c.Get(Equatorial, ds);
            return Algorithms.Visibility.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        private VisibilityDetails Visibility(SkyContext c, DeepSky body)
        {
            var ctx = c.Copy(c.JulianDayMidnight);
            var eq = ctx.Get(Equatorial, body);
            var eqSun = ctx.Get(sky.SunEquatorial);

            double minBodyAltitude = ctx.MinBodyAltitudeForVisibilityCalculations ?? 5;
            double minSunAltitude = ctx.MaxSunAltitudeForVisibilityCalculations ?? -10;
            return Algorithms.Visibility.Details(eq, eqSun, ctx.GeoLocation, ctx.SiderealTime, minBodyAltitude, minSunAltitude);
        }

        public void ConfigureEphemeris(EphemerisConfig<DeepSky> e)
        {
            e["Constellation"] = (c, ds) => Constellations.FindConstellation(c.Get(Equatorial, ds), c.JulianDay);
            e["Horizontal.Altitude"] = (c, ds) => c.Get(Horizontal, ds).Altitude;
            e["Horizontal.Azimuth"] = (c, ds) => c.Get(Horizontal, ds).Azimuth;
            e["Equatorial.Alpha"] = (c, ds) => c.Get(Equatorial, ds).Alpha;
            e["Equatorial.Delta"] = (c, ds) => c.Get(Equatorial, ds).Delta;
            e["Magnitude"] = (c, ds) => ds.Magnitude;
            e["ObjectType", Formatters.Simple] = (c, ds) => c.Get(ReadDeepSkyDetails, ds)?.ObjectType;
            e["PositionAngle", posAngleFormatter] = (c, ds) => ds.PA;
            e["LargeDiameter", angularSizeFormatter] = (c, ds) => ds.LargeDiameter;
            e["SmallDiameter", angularSizeFormatter] = (c, ds) => ds.SmallDiameter;
            e["SurfaceBrightness", surfaceBrightnessFormatter] = (c, ds) => c.Get(ReadDeepSkyDetails, ds)?.SurfaceBrightness;
            e["RTS.Rise"] = (c, ds) => c.GetDateFromTime(c.Get(RiseTransitSet, ds).Rise);
            e["RTS.Transit"] = (c, ds) => c.GetDateFromTime(c.Get(RiseTransitSet, ds).Transit);
            e["RTS.Set"] = (c, ds) => c.GetDateFromTime(c.Get(RiseTransitSet, ds).Set);
            e["RTS.Duration"] = (c, ds) => c.Get(RiseTransitSet, ds).Duration;
            e["RTS.RiseAzimuth"] = (c, ds) => c.Get(RiseTransitSet, ds).RiseAzimuth;
            e["RTS.TransitAltitude"] = (c, ds) => c.Get(RiseTransitSet, ds).TransitAltitude;
            e["RTS.SetAzimuth"] = (c, ds) => c.Get(RiseTransitSet, ds).SetAzimuth;
            e["Visibility.Begin"] = (c, p) => c.GetDateFromTime(c.Get(Visibility, p).Begin);
            e["Visibility.End"] = (c, p) => c.GetDateFromTime(c.Get(Visibility, p).End);
            e["Visibility.Duration"] = (c, p) => c.Get(Visibility, p).Duration;
            e["Visibility.Period"] = (c, p) => c.Get(Visibility, p).Period;
        }

        public void GetInfo(CelestialObjectInfo<DeepSky> info)
        {
            DeepSky ds = info.Body;
            SkyContext c = info.Context;
            DeepSkyInfo details = c.Get(ReadDeepSkyDetails, ds);
            string constellation = Constellations.FindConstellation(c.Get(Equatorial, ds), c.JulianDay);

            info
            .SetSubtitle(Text.Get($"DeepSky.{ds.Status}.Type"))
            .SetTitle(string.Join(" / ", ds.Names))
            .AddRow("Constellation", constellation)

            .AddHeader(Text.Get("DeepSky.Equatorial"))
            .AddRow("Equatorial.Alpha", ds.Equatorial.Alpha)
            .AddRow("Equatorial.Delta", ds.Equatorial.Delta)

            .AddHeader(Text.Get("DeepSky.Equatorial0"))
            .AddRow("Equatorial0.Alpha", ds.Equatorial0.Alpha)
            .AddRow("Equatorial0.Delta", ds.Equatorial0.Delta)

            .AddHeader(Text.Get("DeepSky.Horizontal"))
            .AddRow("Horizontal.Azimuth", ds.Horizontal.Azimuth)
            .AddRow("Horizontal.Altitude", ds.Horizontal.Altitude)

            .AddHeader(Text.Get("DeepSky.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("DeepSky.Visibility"))
            .AddRow("Visibility.Begin")
            .AddRow("Visibility.End")
            .AddRow("Visibility.Duration")
            .AddRow("Visibility.Period")

            .AddHeader(Text.Get("DeepSky.Properties"));

            info.AddRow("ObjectType", details.ObjectType);
            if (ds.Magnitude != float.NaN)
            {
                info.AddRow("VisualMagnitude", ds.Magnitude, Formatters.Magnitude);
            }
            if (details.PhotoMagnitude != null)
            {
                info.AddRow("PhotoMagnitude", details.PhotoMagnitude, Formatters.Magnitude);
            }
            if (details.SurfaceBrightness != null)
            {
                info.AddRow("SurfaceBrightness");
            }

            if (ds.LargeDiameter != null)
            {
                string size = angularSizeFormatter.Format(ds.LargeDiameter);
                if (ds.SmallDiameter != null)
                {
                    size += $" x {angularSizeFormatter.Format(ds.SmallDiameter)}";
                }
                info.AddRow("AngularDiameter", size, Formatters.Simple);
            }
            if (ds.PA != null)
            {
                info.AddRow("PositionAngle");
            }

            if (details.Identifiers.Any() || details.PGC != null)
            {
                info.AddHeader(Text.Get("DeepSky.Designations"));
                if (details.Identifiers.Any())
                {
                    info.AddRow("OtherCatalogsIdentifiers", string.Join(", ", details.Identifiers));
                }
                if (details.PGC != null)
                {
                    info.AddRow("PGCCatalogNumber", string.Join(", ", details.PGC));
                }
            }

            if (!string.IsNullOrEmpty(details.Remarks))
            {
                info.AddRow("Remarks", details.Remarks);
            }
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            return deepSkies
                .Where(ds => ds.CommonName.Equals(searchString) || ds.Names.Any(name => Regex.Replace(name, @"\s", "").StartsWith(searchString.Replace(" ", ""), StringComparison.OrdinalIgnoreCase)))
                .Where(filterFunc)
                .Take(maxCount)
                .ToArray();
        }

        public override void Initialize()
        {
            // Load NGC/IC catalogs data
            using (var reader = new StreamReader(NGCIC_FILE))
            {
                short recordNumber = 0;

                while (!reader.EndOfStream)
                {
                    recordNumber++;
                    string line = reader.ReadLine();

                    string strStatus = line.Substring(10, 2).Trim();
                    if (strStatus == "")
                    {
                        continue;
                    }

                    DeepSkyStatus status = (DeepSkyStatus)(Convert.ToInt32(strStatus) % 10);
                    if (status == DeepSkyStatus.Duplicate || 
                        status == DeepSkyStatus.DuplicateIC)
                    {
                        continue;
                    }

                    var ra = new HMS(
                        uint.Parse(line.Substring(20, 2)),
                        uint.Parse(line.Substring(23, 2)),
                        double.Parse(line.Substring(26, 4), CultureInfo.InvariantCulture))
                        .ToDecimalAngle();

                    var dec =
                        (line[31]  == '-' ? -1 : 1) * new DMS(
                        uint.Parse(line.Substring(32, 2)),
                        uint.Parse(line.Substring(35, 2)),
                        uint.Parse(line.Substring(38, 2)))
                        .ToDecimalAngle();

                    string mag = line.Substring(50, 4).Trim();
                    string sizeA = line.Substring(61, 7).Trim();
                    string sizeB = line.Substring(67, 6).Trim();
                    string PA = line.Substring(74, 3).Trim();
                    string id1 = line.Substring(96, 16).Trim();
                    byte messier = 0;
                    if (id1.StartsWith("M "))
                    {
                        messier = (byte)Convert.ToInt32(id1.Substring(2));
                    }

                    var ds = new DeepSky()
                    {
                        RecordNumber = recordNumber,
                        Number = Convert.ToUInt16(line.Substring(0, 4).Trim()),
                        Letter = line.Substring(4, 1)[0],
                        Component = line.Substring(5, 1)[0],
                        Equatorial0 = new CrdsEquatorial(ra, dec),
                        Status = status,
                        Magnitude = string.IsNullOrWhiteSpace(mag) ? float.NaN : float.Parse(mag, CultureInfo.InvariantCulture),
                        LargeDiameter = !string.IsNullOrWhiteSpace(sizeA) ? float.Parse(sizeA, CultureInfo.InvariantCulture) : (float?)null,
                        SmallDiameter = !string.IsNullOrWhiteSpace(sizeB) ? float.Parse(sizeB, CultureInfo.InvariantCulture) : (float?)null,
                        PA = !string.IsNullOrWhiteSpace(PA) ? short.Parse(PA) : (short?)null,
                        Messier = messier,
                    };

                    deepSkies.Add(ds);
                }

                RecordLength = (long)Math.Round(reader.BaseStream.Length / (double)recordNumber);
            }

            // Load proper names of deep sky objects and assign them
            using (var reader = new StreamReader(NAMES_FILE))
            {
                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(';');
                    var ds = deepSkies.FirstOrDefault(d => d.CatalogName.Replace(" ", "").Equals(line[0].Replace(" ", "")));
                    if (ds != null)
                    {
                        ds.ProperName = line[1].Trim();
                    }
                }
            }

            // Load deep sky object outlines
            using (var reader = new StreamReader(OUTLINES_FILE))
            {
                List<CelestialPoint> outline = new List<CelestialPoint>();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Trim();
                    if (line.Length == 0) continue;

                    // End previos outline and begin a new one
                    if (line.StartsWith("//"))
                    {
                        outline = new List<CelestialPoint>();

                        string name = line.Substring(2).Trim().ToUpper();
                        var ds = deepSkies.FirstOrDefault(d => d.Names.Any(n => n.Replace(" ", "").Equals(name)));
                        if (ds != null && ds.Status != DeepSkyStatus.Galaxy)
                        {
                            ds.Outline = outline;
                        }

                        continue;
                    }

                    // Process outline points
                    uint raH = Convert.ToUInt32(line.Substring(0, 2).Trim());
                    uint raM = Convert.ToUInt32(line.Substring(3, 2).Trim());
                    double raS = Convert.ToDouble(line.Substring(6, 7).Trim(), CultureInfo.InvariantCulture);
                    uint decD = Convert.ToUInt32(line.Substring(15, 2).Trim());
                    uint decM = Convert.ToUInt32(line.Substring(18, 2).Trim());
                    double decS = Convert.ToDouble(line.Substring(21).Trim(), CultureInfo.InvariantCulture);
                    CelestialPoint cp = new CelestialPoint();
                    cp.Equatorial0.Alpha = new HMS(raH, raM, raS).ToDecimalAngle();
                    cp.Equatorial0.Delta = new DMS(decD, decM, decS).ToDecimalAngle() * (line[14] == '-' ? -1 : 1);

                    outline.Add(cp);
                }
            }
        }

        private DeepSkyInfo ReadDeepSkyDetails(SkyContext c, DeepSky ds)
        {
            try
            {
                var details = new DeepSkyInfo();
                using (StreamReader sr = new StreamReader(NGCIC_FILE, Encoding.UTF8))
                {
                    sr.BaseStream.Seek((ds.RecordNumber - 1) * RecordLength, SeekOrigin.Begin);
                    string line = sr.ReadLine();

                    string sb = line.Substring(56, 4).Trim();
                    if (!string.IsNullOrEmpty(sb))
                        details.SurfaceBrightness = Convert.ToSingle(sb, CultureInfo.InvariantCulture);

                    string bmag = line.Substring(43, 4).Trim();

                    if (!string.IsNullOrEmpty(bmag))
                        details.PhotoMagnitude = Convert.ToSingle(bmag, CultureInfo.InvariantCulture);

                    details.ObjectType = line.Substring(78, 8).Trim();

                    string pgc = line.Substring(86, 8).Trim();

                    if (!string.IsNullOrEmpty(pgc))
                        details.PGC = Convert.ToInt64(pgc);

                    List<string> ids = new List<string>();
                    ids.Add(line.Substring(96, 16).Trim());
                    ids.Add(line.Substring(112, 16).Trim());
                    ids.Add(line.Substring(128, 16).Trim());

                    details.Identifiers = ids.Where(id => !string.IsNullOrEmpty(id)).ToArray();
                    details.Remarks = line.Substring(144).Trim();

                    return details;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Contains detailed info about deep sky object
        /// </summary>
        private class DeepSkyInfo
        {
            public float? SurfaceBrightness { get; set; }

            public float? PhotoMagnitude { get; set; }

            public string ObjectType { get; set; }

            public long? PGC { get; set; }

            public string[] Identifiers { get; set; }

            public string Remarks { get; set; }
        }
    }
}
