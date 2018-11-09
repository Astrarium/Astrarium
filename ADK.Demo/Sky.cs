using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class Sky
    {
        public double JulianDay { get; set; }
        public CrdsGeographical GeoLocation { get; set; }
        public double LocalSiderealTime { get; private set; }

        private NutationElements NutationElements;
        private double Epsilon;

        public ICollection<CelestialGrid> Grids { get; private set; } = new List<CelestialGrid>();

        public Sky()
        {
            JulianDay = new Date(DateTime.Now).ToJulianDay();
            GeoLocation = new CrdsGeographical(56.3333, 44);

            NutationElements = Nutation.NutationElements(JulianDay);
            Epsilon = Date.TrueObliquity(JulianDay, NutationElements.deltaEpsilon);
            LocalSiderealTime = Date.ApparentSiderealTime(JulianDay, NutationElements.deltaPsi, Epsilon);

            // TODO: move initialization of grids to separate class
            // CelestialGrid

            // Ecliptic
            CelestialGrid LineEcliptic = new CelestialGrid("Ecliptic", 1, 24);
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
            Grids.Add(LineEcliptic);

            // Horizontal grid
            CelestialGrid GridHorizontal = new CelestialGrid("Horizontal", 17, 24);
            GridHorizontal.FromHorizontal = (h) => new GridPoint(h.Azimuth, h.Altitude);
            GridHorizontal.ToHorizontal = (c) => new CrdsHorizontal(c.Longitude, c.Latitude);
            Grids.Add(GridHorizontal);

            // Equatorial grid
            CelestialGrid GridEquatorial = new CelestialGrid("Equatorial", 17, 24);
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
            Grids.Add(GridEquatorial);
        }
    }
}
