using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class CelestialGridCalc : BaseSkyCalc
    {
        private PrecessionalElements peFrom1950 = null;
        private PrecessionalElements peTo1950 = null;

        public CelestialGridCalc(Sky sky) : base(sky)
        {
            // Ecliptic
            CelestialGrid LineEcliptic = new CelestialGrid("Ecliptic", 1, 24);
            LineEcliptic.FromHorizontal = (h) =>
            {
                var eq = h.ToEquatorial(Sky.GeoLocation, Sky.SiderealTime);
                var ec = eq.ToEcliptical(Sky.Epsilon);
                return new GridPoint(ec.Lambda, ec.Beta);
            };
            LineEcliptic.ToHorizontal = (c) =>
            {
                var ec = new CrdsEcliptical(c.Longitude, c.Latitude);
                var eq = ec.ToEquatorial(Sky.Epsilon);
                return eq.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);
            };
            Sky.AddDataProvider("LineEcliptic", () => LineEcliptic);

            // Galactic equator
            CelestialGrid LineGalactic = new CelestialGrid("Galactic", 1, 24);
            LineGalactic.FromHorizontal = (h) =>
            {
                var eq = h.ToEquatorial(Sky.GeoLocation, Sky.SiderealTime);
                var eq1950 = Precession.GetEquatorialCoordinates(eq, peTo1950);
                var gal = eq1950.ToGalactical();
                return new GridPoint(gal.l, gal.b);
            };
            LineGalactic.ToHorizontal = (c) =>
            {
                var gal = new CrdsGalactical(c.Longitude, c.Latitude);
                var eq1950 = gal.ToEquatorial();
                var eq = Precession.GetEquatorialCoordinates(eq1950, peFrom1950);
                return eq.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);
            };
            Sky.AddDataProvider("LineGalactic", () => LineGalactic);

            // Horizontal grid
            CelestialGrid GridHorizontal = new CelestialGrid("Horizontal", 17, 24);
            GridHorizontal.FromHorizontal = (h) => new GridPoint(h.Azimuth, h.Altitude);
            GridHorizontal.ToHorizontal = (c) => new CrdsHorizontal(c.Longitude, c.Latitude);
            Sky.AddDataProvider("GridHorizontal", () => GridHorizontal);

            // Equatorial grid
            CelestialGrid GridEquatorial = new CelestialGrid("Equatorial", 17, 24);
            GridEquatorial.FromHorizontal = (h) =>
            {
                var eq = h.ToEquatorial(Sky.GeoLocation, Sky.SiderealTime);
                return new GridPoint(eq.Alpha, eq.Delta);
            };
            GridEquatorial.ToHorizontal = (c) =>
            {
                var eq = new CrdsEquatorial(c.Longitude, c.Latitude);
                return eq.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);
            };
            Sky.AddDataProvider("GridEquatorial", () => GridEquatorial);
        }

        public override void Calculate(CalculationContext context)
        {
            peFrom1950 = Precession.ElementsFK5(Date.EPOCH_B1950, Sky.JulianDay);
            peTo1950 = Precession.ElementsFK5(Sky.JulianDay, Date.EPOCH_B1950);
        }
    }
}
