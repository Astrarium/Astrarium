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
    public class DeepSkyCalc : BaseSkyCalc
    {
        private ICollection<DeepSky> DeepSkies = new List<DeepSky>();

        /// <summary>
        /// Creates new instance of DeepSkyCalc
        /// </summary>
        /// <param name="sky"></param>
        public DeepSkyCalc(Sky sky) : base(sky)
        {
            Sky.AddDataProvider("DeepSky", () => DeepSkies);
        }

        public override void Calculate(CalculationContext context)
        {
            // precessional elements
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, Sky.JulianDay);

            foreach (var ds in DeepSkies)
            {
                // Initial coodinates for J2000 epoch
                CrdsEquatorial eq0 = new CrdsEquatorial(ds.Equatorial0);

                // Equatorial coordinates for the mean equinox and epoch of the target date
                ds.Equatorial = Precession.GetEquatorialCoordinates(eq0, p);

                // Nutation effect
                var eq1 = Nutation.NutationEffect(ds.Equatorial, Sky.NutationElements, Sky.Epsilon);

                // Aberration effect
                var eq2 = Aberration.AberrationEffect(ds.Equatorial, Sky.AberrationElements, Sky.Epsilon);

                // Apparent coordinates of the object
                ds.Equatorial += eq1 + eq2;

                // Apparent horizontal coordinates
                ds.Horizontal = ds.Equatorial.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);
            }
        }

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/NGCIC.dat");

            using (var reader = new StreamReader(file))
            {                
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    var ra = new HMS(
                        uint.Parse(line.Substring(12, 2)),
                        uint.Parse(line.Substring(14, 2)),
                        double.Parse(line.Substring(16, 4), CultureInfo.InvariantCulture))
                        .ToDecimalAngle();

                    var dec =                         
                        (line[21] == '-' ? -1 : 1) * new DMS(
                        uint.Parse(line.Substring(22, 2)),
                        uint.Parse(line.Substring(24, 2)),
                        uint.Parse(line.Substring(26, 2)))
                        .ToDecimalAngle();

                    string mag = line.Substring(29, 4);
                    string sizeA = line.Substring(34, 6);
                    string sizeB = line.Substring(41, 5);
                    string PA = line.Substring(47, 3);

                    var ds = new DeepSky()
                    {
                        Equatorial0 = new CrdsEquatorial(ra, dec),
                        Status = (DeepSkyStatus)int.Parse(line.Substring(10, 1)),
                        Mag = float.Parse(string.IsNullOrWhiteSpace(mag) ? "0" : mag, CultureInfo.InvariantCulture),
                        SizeA =  float.Parse(string.IsNullOrWhiteSpace(sizeA) ? "0" : sizeA, CultureInfo.InvariantCulture),
                        SizeB = float.Parse(string.IsNullOrWhiteSpace(sizeB) ? "0" : sizeB, CultureInfo.InvariantCulture),
                        PA = short.Parse(string.IsNullOrWhiteSpace(PA) ? "0" : PA),
                        Type = line.Substring(51, 8).Trim()
                    };

                    DeepSkies.Add(ds);
                }                
            }
        }
    }
}
