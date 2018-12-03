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
    public class MilkyWayCalc : BaseSkyCalc
    {
        private List<MilkyWayPoint>[] MilkyWay = new List<MilkyWayPoint>[11];

        public MilkyWayCalc(Sky sky) : base(sky) { }

        public override void Calculate()
        {
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, Sky.JulianDay);

            foreach (var block in MilkyWay)
            {
                foreach (var bp in block)
                {
                    // Equatorial coordinates for the mean equinox and epoch of the target date
                    var eq = Precession.GetEquatorialCoordinates(bp.Equatorial0, p);

                    // Apparent horizontal coordinates
                    bp.Horizontal = eq.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);
                }
            }
        }

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/MilkyWay.dat");

            string line = "";
            string[] chunks;
            List<ConstBorderPoint> block = new List<ConstBorderPoint>();
            for (int i = 0; i < 11; i++)
            {
                MilkyWay[i] = new List<MilkyWayPoint>();
            }
            using (var sr = new StreamReader(file, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    chunks = line.Split(';');
                    int fragment = Convert.ToInt32(chunks[0].Trim());
                    double ra = Convert.ToDouble(chunks[1].Trim(), CultureInfo.InvariantCulture);
                    double dec = Convert.ToDouble(chunks[2].Trim(), CultureInfo.InvariantCulture);
                    MilkyWayPoint point = new MilkyWayPoint();
                    point.Equatorial0 = new CrdsEquatorial(ra, dec);
                    MilkyWay[fragment].Add(point);
                }
            }

            Sky.AddDataProvider("MilkyWay", () => MilkyWay);
        }
    }
}
