using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    /// <summary>
    /// Calculates coordinates of Deep Sky objects
    /// </summary>
    public class DeepSkyCalc : BaseSkyCalc, IInfoProvider<DeepSky>
    {
        private static string LOCATION = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string NGCIC_FILE = Path.Combine(LOCATION, "Data/NGCIC.dat");
        private static string NAMES_FILE = Path.Combine(LOCATION, "Data/NGCICNames.dat");
        private static string OUTLINES_FILE = Path.Combine(LOCATION, "Data/Outlines.dat");

        /// <summary>
        /// Collection of NGC/IC object
        /// </summary>
        private ICollection<DeepSky> DeepSkies = new List<DeepSky>();

        /// <summary>
        /// Creates new instance of DeepSkyCalc
        /// </summary>
        /// <param name="sky"></param>
        public DeepSkyCalc(Sky sky) : base(sky)
        {
            Sky.AddDataProvider("DeepSky", () => DeepSkies);
        }

        public override void Calculate(SkyContext context)
        {
            // precessional elements
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            foreach (var ds in DeepSkies)
            {
                {
                    // Initial coodinates for J2000 epoch
                    CrdsEquatorial eq0 = new CrdsEquatorial(ds.Equatorial0);

                    // Equatorial coordinates for the mean equinox and epoch of the target date
                    ds.Equatorial = Precession.GetEquatorialCoordinates(eq0, p);

                    // Nutation effect
                    var eq1 = Nutation.NutationEffect(ds.Equatorial, context.NutationElements, context.Epsilon);

                    // Aberration effect
                    var eq2 = Aberration.AberrationEffect(ds.Equatorial, context.AberrationElements, context.Epsilon);

                    // Apparent coordinates of the object
                    ds.Equatorial += eq1 + eq2;

                    // Apparent horizontal coordinates
                    ds.Horizontal = ds.Equatorial.ToHorizontal(context.GeoLocation, context.SiderealTime);
                }

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
            }
        }

        public CelestialObjectInfo GetInfo(SkyContext context, DeepSky ds)
        {
            var info = new CelestialObjectInfo();
            info.SetSubtitle("Deep Sky").SetTitle(string.Join(" / ", ds.AllNames));

            return info;
        }

        public override void Initialize()
        {
            // Load NGC/IC catalogs data
            using (var reader = new StreamReader(NGCIC_FILE))
            {
                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(';');

                    var ra = new HMS(
                        uint.Parse(line[8]),
                        uint.Parse(line[9]),
                        double.Parse(line[10], CultureInfo.InvariantCulture))
                        .ToDecimalAngle();

                    var dec =
                        (line[11].Equals("-") ? -1 : 1) * new DMS(
                        uint.Parse(line[12]),
                        uint.Parse(line[13]),
                        uint.Parse(line[14]))
                        .ToDecimalAngle();

                    string mag = line[16];
                    string sizeA = line[19];
                    string sizeB = line[20];
                    string PA = line[21];

                    // collect names from different catalogs
                    List<string> names = new List<string>();
                    for (int i = 0; i < 11; i++)
                    {
                        if (!string.IsNullOrEmpty(line[27 + i]))
                        {
                            names.Add(line[27 + i]);
                        }
                    }

                    // detect comments/proper name in last name column, and remove it
                    if (names.Any() && names.Last().All(c => !char.IsDigit(c)))
                    {
                        names.Remove(names.Last());
                    }

                    var ds = new DeepSky()
                    {
                        CatalogNumber = $"{line[1]} {line[2]}{line[3]}".Trim(),
                        IC = line[0].Equals("I"),
                        Equatorial0 = new CrdsEquatorial(ra, dec),
                        Status = (DeepSkyStatus)(int.Parse(line[5]) % 10),
                        Mag = float.Parse(string.IsNullOrWhiteSpace(mag) ? "0" : mag, CultureInfo.InvariantCulture),
                        SizeA = float.Parse(string.IsNullOrWhiteSpace(sizeA) ? "0" : sizeA, CultureInfo.InvariantCulture),
                        SizeB = float.Parse(string.IsNullOrWhiteSpace(sizeB) ? "0" : sizeB, CultureInfo.InvariantCulture),
                        PA = short.Parse(string.IsNullOrWhiteSpace(PA) ? "0" : PA),
                        OtherNames = names.Any() ? names.ToArray() : null,
                    };

                    DeepSkies.Add(ds);
                }
            }

            // Load proper names of deep sky objects and assign them
            using (var reader = new StreamReader(NAMES_FILE))
            {
                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(';');
                    var ds = DeepSkies.FirstOrDefault(d => d.CatalogName.Replace(" ", "").Equals(line[0].Replace(" ", "")));
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
                        var ds = DeepSkies.FirstOrDefault(d => d.AllNames.Any(n => n.Replace(" ", "").Equals(name)));
                        if (ds != null)
                        {
                            ds.Outline = outline;
                        }
                        else
                        {

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
    }
}
