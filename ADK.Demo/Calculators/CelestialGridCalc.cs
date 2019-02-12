using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public interface ICelestialGridProvider
    {
        CelestialGrid LineEcliptic { get; }
        CelestialGrid LineGalactic { get; }
        CelestialGrid GridHorizontal { get; }
        CelestialGrid GridEquatorial { get; }
        CelestialGrid LineHorizon { get; }
    }

    public class CelestialGridCalc : ISkyCalc, ICelestialGridProvider
    {
        private PrecessionalElements peFrom1950 = null;
        private PrecessionalElements peTo1950 = null;

        public CelestialGrid LineEcliptic { get; private set; } = new CelestialGrid("Ecliptic", 1, 24);
        public CelestialGrid LineGalactic { get; private set; } = new CelestialGrid("Galactic", 1, 24);
        public CelestialGrid GridHorizontal { get; private set; } = new CelestialGrid("Horizontal", 17, 24);
        public CelestialGrid GridEquatorial { get; private set; } = new CelestialGrid("Equatorial", 17, 24);
        public CelestialGrid LineHorizon { get; private set; } = new CelestialGrid("Horizon", 1, 24);

        public CelestialGridCalc()
        {
            // Ecliptic
            
            LineEcliptic.FromHorizontal = (h, ctx) =>
            {
                var eq = h.ToEquatorial(ctx.GeoLocation, ctx.SiderealTime);
                var ec = eq.ToEcliptical(ctx.Epsilon);
                return new GridPoint(ec.Lambda, ec.Beta);
            };
            LineEcliptic.ToHorizontal = (c, ctx) =>
            {
                var ec = new CrdsEcliptical(c.Longitude, c.Latitude);
                var eq = ec.ToEquatorial(ctx.Epsilon);
                return eq.ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
            };

            // Galactic equator
            LineGalactic.FromHorizontal = (h, ctx) =>
            {
                var eq = h.ToEquatorial(ctx.GeoLocation, ctx.SiderealTime);
                var eq1950 = Precession.GetEquatorialCoordinates(eq, peTo1950);
                var gal = eq1950.ToGalactical();
                return new GridPoint(gal.l, gal.b);
            };
            LineGalactic.ToHorizontal = (c, ctx) =>
            {
                var gal = new CrdsGalactical(c.Longitude, c.Latitude);
                var eq1950 = gal.ToEquatorial();
                var eq = Precession.GetEquatorialCoordinates(eq1950, peFrom1950);
                return eq.ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
            };

            // Horizontal grid
            GridHorizontal.FromHorizontal = (h, ctx) => new GridPoint(h.Azimuth, h.Altitude);
            GridHorizontal.ToHorizontal = (c, context) => new CrdsHorizontal(c.Longitude, c.Latitude);

            // Equatorial grid
            GridEquatorial.FromHorizontal = (h, ctx) =>
            {
                var eq = h.ToEquatorial(ctx.GeoLocation, ctx.SiderealTime);
                return new GridPoint(eq.Alpha, eq.Delta);
            };
            GridEquatorial.ToHorizontal = (c, ctx) =>
            {
                var eq = new CrdsEquatorial(c.Longitude, c.Latitude);
                return eq.ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
            };

            // Hozizon line            
            LineHorizon.FromHorizontal = (h, ctx) => new GridPoint(h.Azimuth, h.Altitude);
            LineHorizon.ToHorizontal = (c, ctx) => new CrdsHorizontal(c.Longitude, c.Latitude);
        }

        public void Initialize() { }

        public void Calculate(SkyContext context)
        {
            peFrom1950 = Precession.ElementsFK5(Date.EPOCH_B1950, context.JulianDay);
            peTo1950 = Precession.ElementsFK5(context.JulianDay, Date.EPOCH_B1950);
        }
    }
}
