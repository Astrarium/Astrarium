using ADK.Demo.Objects;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ADK.Demo.Calculators
{
    public class BordersCalc : BaseSkyCalc
    {
        private List<List<CelestialPoint>> Borders = new List<List<CelestialPoint>>();

        public BordersCalc(Sky sky) : base(sky) { }

        public override void Calculate()
        {
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, Sky.JulianDay);

            foreach (var b in Borders)
            {
                foreach (var bp in b)
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
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Borders.dat");

            using (var sr = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                List<CelestialPoint> block = null;
                while (sr.BaseStream.Position != sr.BaseStream.Length)
                {
                    bool start = sr.ReadBoolean();
                    if (start)
                    {
                        block = new List<CelestialPoint>();
                        Borders.Add(block);
                    }

                    block.Add(new CelestialPoint()
                    {
                        Equatorial0 = new CrdsEquatorial(sr.ReadDouble(), sr.ReadDouble())
                    });
                }
            }

            Sky.AddDataProvider("Borders", () => Borders);
        }
    }
}
