using Astrarium.Types;
using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Satellites
{
    public class SatellitesCalculator : BaseCalc
    {
        public ICollection<Satellite> Satellites { get; private set; }

        /// <inheritdoc />
        public IEnumerable<Satellite> GetCelestialObjects() => Satellites;

        /// <inheritdoc />
        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Brightest.tle");
            Satellites = LoadSatellites(file);
        }

        public SatellitesCalculator()
        {

        }

        public override void Calculate(SkyContext context)
        {
            Vec3 topocentricLocationVector = Norad.TopocentricLocationVector(context.GeoLocation, context.SiderealTime);

            foreach (var s in Satellites)
            {
                s.Geocentric = Norad.SGP4(s.Tle, context.JulianDay);
                s.Topocentric = Norad.TopocentricSatelliteVector(topocentricLocationVector, s.Geocentric);
                var h = Norad.HorizontalCoordinates(context.GeoLocation, s.Topocentric, context.SiderealTime);
                s.Equatorial = h.ToEquatorial(context.GeoLocation, context.SiderealTime);
            }
        }

        private ICollection<Satellite> LoadSatellites(string file)
        {
            var satellites = new List<Satellite>();

            using (var sr = new StreamReader(file, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string name = sr.ReadLine();
                    string line1 = sr.ReadLine();
                    string line2 = sr.ReadLine();
                    satellites.Add(new Satellite(name.Trim(), new TLE(line1, line2)));
                }
                sr.Close();
            }

            return satellites;
        }
    }
}
