using ADK.Demo.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            var earthHeliocentrical = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, context.JulianDay, highPrecision: true);

            // Ecliptical coordinates of the Sun
            var sunEcliptical = new CrdsEcliptical(Angle.To360(earthHeliocentrical.L + 180), -earthHeliocentrical.B, earthHeliocentrical.R);

            // Corrected solar coordinates to FK5 system
            sunEcliptical += PlanetPositions.CorrectionForFK5(context.JulianDay, sunEcliptical);

            // Add nutation effect to ecliptical coordinates of the Sun
            sunEcliptical += Nutation.NutationEffect(context.NutationElements.deltaPsi);

            // Add aberration effect, so we have an final ecliptical coordinates of the Sun 
            sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);

            // Save data to context
            context.Data.SunEcliptical = sunEcliptical;
            context.Data.EarthHeliocentrical = earthHeliocentrical;
        }

        private void CalcSaturnRings(SkyContext context)
        {
            Planet p = Planets[Planet.SATURN - 1];
            SaturnRings = PlanetEphem.SaturnRings(context.JulianDay, p.Heliocentrical, context.Data.EarthHeliocentrical, context.Epsilon);
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
                var earthHeliocentrical = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, context.JulianDay - tau, highPrecision: true);

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
            p.Elongation = Appearance.Elongation(context.Data.SunEcliptical, p.Ecliptical);

            // Phase angle
            p.PhaseAngle = Appearance.PhaseAngle(p.Elongation, context.Data.SunEcliptical.Distance, p.Ecliptical.Distance);

            // Planet phase                
            p.Phase = Appearance.Phase(p.PhaseAngle);

            // Planet magnitude
            p.Magnitude = PlanetEphem.Magnitude(p.Number, p.Ecliptical.Distance, p.Distance, p.PhaseAngle);
            if (p.Number == Planet.SATURN)
            {
                var saturnRings = PlanetEphem.SaturnRings(context.JulianDay, p.Heliocentrical, context.Data.EarthHeliocentrical, context.Epsilon);
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

            config.Define("Magnitude", (ctx, p) => p.Magnitude);

            config.Define("Horizontal.Altitude", (ctx, p) => p.Horizontal.Altitude)
                .WithBeforeAction(CalcPlanetPosition)
                .AttachToGroup("Horizontal");

            config.Define("Horizontal.Azimuth", (ctx, p) => p.Horizontal.Azimuth)
                .WithBeforeAction(CalcPlanetPosition)
                .AttachToGroup("Horizontal");

            config.Define("Equatorial.Alpha", (ctx, p) => p.Equatorial.Alpha)
                .WithBeforeAction(CalcPlanetPosition)
                .AttachToGroup("Equatorial");

            config.Define("Equatorial.Delta", (ctx, p) => p.Equatorial.Delta)
                .WithBeforeAction(CalcPlanetPosition)
                .AttachToGroup("Equatorial");

            config.Define("Rise", (ctx, p) => ctx.Data.RTS.Rise)
                .WithBeforeAction(CalcRiseTransitSet)
                .AttachToGroup("RTS");

            config.Define("Transit", (ctx, p) => ctx.Data.RTS.Transit)
                .WithBeforeAction(CalcRiseTransitSet)
                .AttachToGroup("RTS");

            config.Define("Set", (ctx, p) => ctx.Data.RTS.Set)
                .WithBeforeAction(CalcRiseTransitSet)
                .AttachToGroup("RTS");
        }

        private void CalcRiseTransitSet(SkyContext ctx, Planet p)
        {
            CrdsEquatorial[] eq = new CrdsEquatorial[3];

            eq[1] = new CrdsEquatorial(p.Equatorial);

            Calculate(new SkyContext(ctx.JulianDay - 1, ctx.GeoLocation), p);
            eq[0] = new CrdsEquatorial(p.Equatorial);

            Calculate(new SkyContext(ctx.JulianDay + 1, ctx.GeoLocation), p);
            eq[2] = new CrdsEquatorial(p.Equatorial);

            // TODO: calculate RTS with ADK lib
            ctx.Data.RTS = new RTS()
            {
                Rise = 1,
                Transit = 2,
                Set = 3
            };
        }
    }

    public abstract class EphemerisConfig : IEnumerable<EphemerisConfigItem>
    {
        internal List<EphemerisConfigItem> Items { get; } = new List<EphemerisConfigItem>();

        public IEnumerator<EphemerisConfigItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public ICollection<EphemerisConfigItem> Filter(ICollection<string> keys)
        {
            return Items.Where(i => keys.Contains(i.Key)).ToArray();
        }
    }

    public class EphemerisConfig<TCelestialObject> : EphemerisConfig where TCelestialObject : CelestialObject
    {
        public EphemerisConfigItem<TCelestialObject, TResult> Define<TResult>(string key, Func<SkyContext, TCelestialObject, TResult> formula)
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
        public string Group { get; protected set; }
        public List<Delegate> Actions { get; } = new List<Delegate>();
        public EphemFormatter Formatter { get; protected set; }

        public EphemerisConfigItem(string key, Delegate func)
        {
            Key = key;
            Formula = func;
        }
    }

    public class EphemerisConfigItem<TCelestialObject, TResult> : EphemerisConfigItem where TCelestialObject : CelestialObject
    {
        public EphemerisConfigItem(string key, Func<SkyContext, TCelestialObject, TResult> formula) : base (key, formula) 
        {

        }

        public EphemerisConfigItem<TCelestialObject, TResult> AttachToGroup(string groupName)
        {
            Group = groupName;
            return this;
        }

        public EphemerisConfigItem<TCelestialObject, TResult> WithBeforeAction(Action<SkyContext, TCelestialObject> action)
        {
            Actions.Add(action);
            return this;
        }

        public EphemerisConfigItem<TCelestialObject, TResult> WithFormatter(EphemFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }
    }

    public interface IEphemFormatter
    {
        string Format(object value);
    }

    public class EphemFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            return value?.ToString();
        }
    }

    public static class Formatters
    {
        public static readonly IEphemFormatter RA = new EphemFormatter();
        public static readonly IEphemFormatter Dec = new EphemFormatter();
    }
}
