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
    public class BordersCalc : BaseSkyCalc
    {
        private ICollection<ConstBorderPoint> Borders = new List<ConstBorderPoint>();

        public BordersCalc(Sky sky) : base(sky) { }

        public override void Calculate()
        {
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, Sky.JulianDay);

            foreach (var bp in Borders)
            {
                // Equatorial coordinates for the mean equinox and epoch of the target date
                bp.Equatorial = Precession.GetEquatorialCoordinates(bp.Equatorial0, p);

                // Apparent horizontal coordinates
                bp.Horizontal = bp.Equatorial.ToHorizontal(Sky.GeoLocation, Sky.LocalSiderealTime);
            }
        }

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Borders.dat");

            string line = "";
            string[] chunks;
            using (var sr = new StreamReader(file, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine().Trim();
                    if (line == "" || line.StartsWith("//")) continue;

                    chunks = line.Split(';');
                    ConstBorderPoint border = new ConstBorderPoint();
                    border.Start = chunks[0].Trim() == "1";
                    border.Equatorial0.Alpha = 15.0 * Convert.ToDouble(chunks[1].Trim(), CultureInfo.InvariantCulture);
                    border.Equatorial0.Delta = Convert.ToDouble(chunks[2].Trim(), CultureInfo.InvariantCulture);
                    Borders.Add(border);
                }
            }

            Sky.AddDataProvider("Borders", () => Borders);
        }
    }
}
