using ADK.Demo.Objects;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace ADK.Demo.Calculators
{
    public class ConstellationsCalc : BaseSkyCalc
    {
        /// <summary>
        /// Constellations labels coordinates
        /// </summary>
        private Dictionary<string, CelestialPoint> ConstLabels = new Dictionary<string, CelestialPoint>();

        /// <summary>
        /// Constellations borders coordinates
        /// </summary>
        private List<List<CelestialPoint>> ConstBorders = new List<List<CelestialPoint>>();

        public ConstellationsCalc(Sky sky) : base(sky) { }

        public override void Calculate()
        {
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, Sky.JulianDay);

            foreach (var b in ConstBorders)
            {
                foreach (var bp in b)
                {
                    // Equatorial coordinates for the mean equinox and epoch of the target date
                    var eq = Precession.GetEquatorialCoordinates(bp.Equatorial0, p);

                    // Apparent horizontal coordinates
                    bp.Horizontal = eq.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);
                }
            }

            foreach (var c in ConstLabels)
            {
                // Equatorial coordinates for the mean equinox and epoch of the target date
                var eq = Precession.GetEquatorialCoordinates(c.Value.Equatorial0, p);

                // Apparent horizontal coordinates
                c.Value.Horizontal = eq.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);
            }
        }

        public override void Initialize()
        {
            LoadBordersData();
            LoadLabelsData();
        }

        /// <summary>
        /// Loads constellation borders data
        /// </summary>
        private void LoadBordersData()
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
                        ConstBorders.Add(block);
                    }

                    block.Add(new CelestialPoint()
                    {
                        Equatorial0 = new CrdsEquatorial(sr.ReadDouble(), sr.ReadDouble())
                    });
                }
            }

            Sky.AddDataProvider("ConstBorders", () => ConstBorders);
        }

        /// <summary>
        /// Loads constellations labels data
        /// </summary>
        private void LoadLabelsData()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Conlabels.dat");
            using (var sr = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                while (sr.BaseStream.Position != sr.BaseStream.Length)
                {
                    string name = sr.ReadString();
                    ConstLabels[name] = new CelestialPoint()
                    {
                        Equatorial0 = new CrdsEquatorial(sr.ReadSingle(), sr.ReadSingle())
                    };
                }
            }
            
            Sky.AddDataProvider("ConstLabels", () => ConstLabels);
        }
    }
}
