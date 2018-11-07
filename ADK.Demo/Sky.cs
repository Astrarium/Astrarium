using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class Sky
    {
        public CrdsGeographical GeoLocation = new CrdsGeographical(56.3333, 44);
        public double Epsilon = Date.MeanObliquity(new Date(DateTime.Now).ToJulianDay());
        public double LocalSiderealTime = 17;

        public CelestialGrid GridHorizontal = new CelestialGrid(17, 24);
        public CelestialGrid GridEquatorial = new CelestialGrid(17, 24);
        public CelestialGrid LineEcliptic = new CelestialGrid(1, 24);

        public Sky()
        {
            // Ecliptic
            LineEcliptic.FromHorizontal = (h) =>
            {
                var eq = h.ToEquatorial(GeoLocation, LocalSiderealTime);
                var ec = eq.ToEcliptical(Epsilon);
                return new GridPoint(ec.Lambda, ec.Beta);
            };
            LineEcliptic.ToHorizontal = (c) =>
            {
                var ec = new CrdsEcliptical(c.Longitude, c.Latitude);
                var eq = ec.ToEquatorial(Epsilon);
                return eq.ToHorizontal(GeoLocation, LocalSiderealTime);
            };

            // Horizontal grid
            GridHorizontal.FromHorizontal = (h) => new GridPoint(h.Azimuth, h.Altitude);
            GridHorizontal.ToHorizontal = (c) => new CrdsHorizontal(c.Longitude, c.Latitude);

            // Equatorial grid
            GridEquatorial.FromHorizontal = (h) =>
            {
                var eq = h.ToEquatorial(GeoLocation, LocalSiderealTime);
                return new GridPoint(eq.Alpha, eq.Delta);
            };
            GridEquatorial.ToHorizontal = (c) =>
            {
                var eq = new CrdsEquatorial(c.Longitude, c.Latitude);
                return eq.ToHorizontal(GeoLocation, LocalSiderealTime);
            };
        }
    }
}
