using ADK.Demo.Objects;
using System;
using System.Collections.Generic;

namespace ADK.Demo.Calculators
{
    public interface IEphemProvider
    {
        void Calculate(SkyContext context, CelestialObject obj);
    }

    public interface IEphemProvider<TCelestialObject> : IEphemProvider where TCelestialObject : CelestialObject
    {
        void ConfigureEphemeris(EphemerisConfig<TCelestialObject> config);        
    }


    public class PlanetsCalc : BaseSkyCalc, IEphemProvider<Planet>
    {
        private Planet[] Planets = new Planet[8];

        private RingsAppearance SaturnRings = new RingsAppearance();

        /// <summary>
        /// Heliocentrical coordinates of Earth
        /// </summary>
        private CrdsHeliocentrical earthHeliocentrical = null;

        /// <summary>
        /// Ecliptical coordinates of the Sun
        /// </summary>
        private CrdsEcliptical sunEcliptical = null;

        private string[] PlanetNames = new string[]
        {
            "Mercury",
            "Venus",
            "Earth",
            "Mars",
            "Jupiter",
            "Saturn",
            "Uranus",
            "Neptune"
        };

        public PlanetsCalc(Sky sky) : base(sky)
        {
            for (int i = 0; i < Planets.Length; i++)
            {
                Planets[i] = new Planet() { Number = i + 1, Names = new string[] { PlanetNames[i] } };
            }

            Planets[Planet.JUPITER - 1].Flattening = 0.064874f;
            Planets[Planet.SATURN - 1].Flattening = 0.097962f;

            Sky.AddDataProvider("Planets", () => Planets);
            Sky.AddDataProvider("SaturnRings", () => SaturnRings);
        }

        public override void Calculate(SkyContext context)
        {
            CalcSunEarthPositions(context);
            foreach (var p in Planets)
            {
                CalcPlanetPosition(context, p);
            }
            CalcSaturnRings(context);
        }

        public void Calculate(SkyContext context, CelestialObject planet)
        {
            CalcSunEarthPositions(context);
            CalcPlanetPosition(context, (Planet)planet);
        }

        private void CalcSunEarthPositions(SkyContext context)
        {
            // get Earth coordinates
            earthHeliocentrical = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, context.JulianDay, highPrecision: true);

            // Ecliptical coordinates of the Sun
            sunEcliptical = new CrdsEcliptical(Angle.To360(earthHeliocentrical.L + 180), -earthHeliocentrical.B, earthHeliocentrical.R);

            // Corrected solar coordinates to FK5 system
            sunEcliptical += PlanetPositions.CorrectionForFK5(context.JulianDay, sunEcliptical);

            // Add nutation effect to ecliptical coordinates of the Sun
            sunEcliptical += Nutation.NutationEffect(context.NutationElements.deltaPsi);

            // Add aberration effect, so we have an final ecliptical coordinates of the Sun 
            sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);
        }

        private void CalcSaturnRings(SkyContext context)
        {
            Planet p = Planets[Planet.SATURN - 1];
            SaturnRings = PlanetEphem.SaturnRings(context.JulianDay, p.Heliocentrical, earthHeliocentrical, context.Epsilon);
        }

        public void CalcPlanetPosition(SkyContext context, Planet p)
        {
            // Skip Earth
            if (p.Number == Planet.EARTH) return;

            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;
           
            // time taken by the light to reach the Earth
            double tau = 0;

            // previous value of tau to calculate the difference
            double tau0 = 1;

            // Iterative process to find ecliptical coordinates of planet
            while (Math.Abs(tau - tau0) > deltaTau)
            {
                // Heliocentrical coordinates of Earth
                earthHeliocentrical = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, context.JulianDay - tau, highPrecision: true);

                // Heliocentrical coordinates of planet
                p.Heliocentrical = PlanetPositions.GetPlanetCoordinates(p.Number, context.JulianDay - tau, highPrecision: true);

                // Distance from planet to Sun
                p.Distance = p.Heliocentrical.R;

                // Ecliptical coordinates of planet
                p.Ecliptical = p.Heliocentrical.ToRectangular(earthHeliocentrical).ToEcliptical();

                tau0 = tau;
                tau = PlanetPositions.LightTimeEffect(p.Ecliptical.Distance);
            }

            // Correction for FK5 system
            p.Ecliptical += PlanetPositions.CorrectionForFK5(context.JulianDay, p.Ecliptical);

            // Take nutation into account
            p.Ecliptical += Nutation.NutationEffect(context.NutationElements.deltaPsi);

            // Apparent geocentrical equatorial coordinates of planet
            p.Equatorial0 = p.Ecliptical.ToEquatorial(context.Epsilon);

            // Planet semidiameter
            p.Semidiameter = PlanetEphem.Semidiameter(p.Number, p.Ecliptical.Distance);

            // Planet parallax
            p.Parallax = PlanetEphem.Parallax(p.Ecliptical.Distance);

            // Apparent topocentric coordinates of planet
            p.Equatorial = p.Equatorial0.ToTopocentric(context.GeoLocation, context.SiderealTime, p.Parallax);

            // Local horizontal coordinates of planet
            p.Horizontal = p.Equatorial.ToHorizontal(context.GeoLocation, context.SiderealTime);

            // Elongation of the Planet
            p.Elongation = Appearance.Elongation(sunEcliptical, p.Ecliptical);

            // Phase angle
            p.PhaseAngle = Appearance.PhaseAngle(p.Elongation, sunEcliptical.Distance, p.Ecliptical.Distance);

            // Planet phase                
            p.Phase = Appearance.Phase(p.PhaseAngle);

            // Planet magnitude
            p.Magnitude = PlanetEphem.Magnitude(p.Number, p.Ecliptical.Distance, p.Distance, p.PhaseAngle);
            if (p.Number == Planet.SATURN)
            {
                var saturnRings = PlanetEphem.SaturnRings(context.JulianDay, p.Heliocentrical, earthHeliocentrical, context.Epsilon);
                p.Magnitude += saturnRings.GetRingsMagnitude();
            }

            // Planet appearance
            p.Appearance = PlanetEphem.PlanetAppearance(context.JulianDay, p.Number, p.Equatorial0, p.Ecliptical.Distance);            
        }
        
        private class RTS
        {
            public double Rise { get; set; }
            public double Transit { get; set; }
            public double Set { get; set; }
        }

        public void ConfigureEphemeris(EphemerisConfig<Planet> config)
        {
            // Define formulae for object ephemeris

            config.Ephem("Magnitude", (ctx, p) => p.Magnitude);


            config.Ephem("Horizontal", (ctx, p) => p.Horizontal)
                .As("Horizontal.Altitude", h => h.Altitude, Formatters.RA)
                .As("Horizontal.Azimuth", h => h.Azimuth, Formatters.RA);

            config.Ephem("Equatorial", (ctx, p) => p.Equatorial)
                .As("Equatorial.Alpha", eq => eq.Alpha, Formatters.RA)
                .As("Equatorial.Delta", eq => eq.Delta, Formatters.Dec);

            config.Ephem("RTS", GetRiseTransitSet)
                .As("Rise", rts => rts.Rise)
                .As("Transit", rts => rts.Transit)
                .As("Set", rts => rts.Set);
        }

        private RTS GetRiseTransitSet(SkyContext ctx, Planet p)
        {
            CrdsEquatorial[] eq = new CrdsEquatorial[3];

            eq[1] = new CrdsEquatorial(p.Equatorial);

            Calculate(new SkyContext(ctx.JulianDay - 1, ctx.GeoLocation), p);
            eq[0] = new CrdsEquatorial(p.Equatorial);

            Calculate(new SkyContext(ctx.JulianDay + 1, ctx.GeoLocation), p);
            eq[2] = new CrdsEquatorial(p.Equatorial);

            // TODO: calculate RTS with ADK lib
            return new RTS();
        }
    }

    public abstract class EphemerisConfig
    {
        public List<EphemerisConfigItem> Items { get; } = new List<EphemerisConfigItem>();
    }

    public class EphemerisConfig<TCelestialObject> : EphemerisConfig where TCelestialObject : CelestialObject
    {
        

        public void Ephem<TResult>(string key, Func<SkyContext, TCelestialObject, TResult> formula, EphemFormatter formatter)
        {
            Items.Add(new EphemerisConfigItem<TCelestialObject, TResult>(key, formula));
        }

        public EphemerisConfigItem<TCelestialObject, TResult> Ephem<TResult>(string key, Func<SkyContext, TCelestialObject, TResult> formula)
        {
            var item = new EphemerisConfigItem<TCelestialObject, TResult>(key, formula);
            Items.Add(item);
            return item;
        }
    }

    public abstract class EphemerisConfigItem
    {
        public string Key { get; protected set; }
        public Delegate Formula { get; protected set; }
        public Dictionary<string, Delegate> Formulae { get; } = new Dictionary<string, Delegate>();

        public EphemerisConfigItem(string key, Delegate func)
        {
            Key = key;
            Formula = func;
        }
    }

    public class EphemerisConfigItem<TCelestialObject, T> : EphemerisConfigItem where TCelestialObject : CelestialObject
    {
        public EphemerisConfigItem(string key, Func<SkyContext, TCelestialObject, T> formula) : base (key, formula) 
        {

        }

        public EphemerisConfigItem<TCelestialObject, T> As<TResult>(string key, Func<T, TResult> formula, EphemFormatter formatter = null)
        {
            Formulae.Add(key, formula);
            return this;
        }

    }

    public class EphemFormatter
    {

    }

    public static class Formatters
    {
        public static readonly EphemFormatter RA = new EphemFormatter();
        public static readonly EphemFormatter Dec = new EphemFormatter();
    }
}
