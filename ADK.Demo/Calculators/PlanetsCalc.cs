using ADK.Demo.Objects;
using System;
using System.Collections.Generic;

namespace ADK.Demo.Calculators
{
    public class PlanetsCalc : BaseSkyCalc
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

        public override void Calculate(CalculationContext context)
        {
            Calculate(context, Planets);
        }

        public void Calculate(CalculationContext context, params Planet[] planets)
        {
            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            CrdsEcliptical sunEcliptical = context.Formula<CrdsEcliptical>("SunEcliptical");

            // correct solar coordinates to FK5 system
            sunEcliptical += PlanetPositions.CorrectionForFK5(Sky.JulianDay, sunEcliptical); ;

            // add nutation effect to ecliptical coordinates of the Sun
            sunEcliptical += Nutation.NutationEffect(Sky.NutationElements.deltaPsi);

            // add aberration effect, so we have an final ecliptical coordinates of the Sun 
            sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);

            foreach (Planet p in planets)
            {
                // Skip Earth
                if (p.Number == Planet.EARTH) continue;

                // time taken by the light to reach the Earth
                double tau = 0;

                // previous value of tau to calculate the difference
                double tau0 = 1;

                CrdsHeliocentrical hEarth;

                // Iterative process to find ecliptical coordinates of planet
                while (Math.Abs(tau - tau0) > deltaTau)
                {
                    // Heliocentrical coordinates of Earth
                    hEarth = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, Sky.JulianDay - tau, highPrecision: true);

                    // Heliocentrical coordinates of planet
                    p.Heliocentrical = PlanetPositions.GetPlanetCoordinates(p.Number, Sky.JulianDay - tau, highPrecision: true);

                    // Distance from planet to Sun
                    p.Distance = p.Heliocentrical.R;

                    // Ecliptical coordinates of planet
                    p.Ecliptical = p.Heliocentrical.ToRectangular(hEarth).ToEcliptical();

                    tau0 = tau;
                    tau = PlanetPositions.LightTimeEffect(p.Ecliptical.Distance);
                }

                // Correction for FK5 system
                p.Ecliptical += PlanetPositions.CorrectionForFK5(Sky.JulianDay, p.Ecliptical);

                // Take nutation into account
                p.Ecliptical += Nutation.NutationEffect(Sky.NutationElements.deltaPsi);

                // Apparent geocentrical equatorial coordinates of planet
                p.Equatorial0 = p.Ecliptical.ToEquatorial(Sky.Epsilon);

                // Planet semidiameter
                p.Semidiameter = PlanetEphem.Semidiameter(p.Number, p.Ecliptical.Distance);

                // Planet parallax
                p.Parallax = PlanetEphem.Parallax(p.Ecliptical.Distance);

                // Apparent topocentric coordinates of planet
                p.Equatorial = p.Equatorial0.ToTopocentric(Sky.GeoLocation, Sky.SiderealTime, p.Parallax);

                // Local horizontal coordinates of planet
                p.Horizontal = p.Equatorial.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);

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
                    SaturnRings = PlanetEphem.SaturnRings(Sky.JulianDay, p.Heliocentrical, hEarth, Sky.Epsilon);
                    p.Magnitude += SaturnRings.GetRingsMagnitude();
                }

                // Planet appearance
                p.Appearance = PlanetEphem.PlanetAppearance(Sky.JulianDay, p.Number, p.Equatorial0, p.Ecliptical.Distance);
            }
        }

        //public void ConfigureObjectInfo(ObjectInfo info)
        //{
        //    info.AddHeader("");
        //    info.AddProperty("Equatorial.Alpha", Formatters.RA);
        //    info.AddProperty("Equatorial.Delta");
        //}

       
        public void ConfigureEphemeris(EphemerisConfig<Planet> config) // Dictionary<string, Func<CalculationContext, Planet, object>> formulae)
        {
            // Define formulae for object ephemeris

            config.AddFormula("Sun.Ecliptical", GetSunEcliptical);

            config.AddFormula("Magnitude", (ctx, p) => p.Magnitude).AsEphem();
                
            config.AddFormula("Equatorial", (ctx, p) => p.Equatorial)
                .AsEphem("Equatorial.Alpha", eq => eq.Alpha, Formatters.RA)
                .AsEphem("Equatorial.Delta", eq => eq.Delta, Formatters.Dec);




            formulae["Equatorial"] = (ctx, p) => p.Equatorial;
            formulae["Horizontal"] = (ctx, p) => p.Horizontal;
            formulae["Magnitude"] = (ctx, p) => p.Magnitude;
            formulae["DistanceFromSun"] = (ctx, p) => p.Distance;
            formulae["DistanceFromEarth"] = (ctx, p) => p.Ecliptical.Distance;

            formulae["RTS"] = (ctx, p) =>
            {
                Calculate(ctx /* TODO: context for JD-1 */, p);
                var eqJdBefore = new CrdsEquatorial(p.Equatorial);

                Calculate(ctx /* TODO: context for JD+1 */, p);
                var eqJdAfter = new CrdsEquatorial(p.Equatorial);

                Calculate(ctx, p);
                var eqJd = new CrdsEquatorial(p.Equatorial);

                // TODO: calculate rise/transit/set

                return null;
            };
        }

        private CrdsEcliptical GetSunEcliptical(CalculationContext calculationContext, Planet p)
        {
            // get Earth coordinates
            CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, Sky.JulianDay, highPrecision: true);

            // transform to ecliptical coordinates of the Sun
            CrdsEcliptical sunEcliptical = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // correct solar coordinates to FK5 system
            sunEcliptical += PlanetPositions.CorrectionForFK5(Sky.JulianDay, sunEcliptical); ;

            // add nutation effect to ecliptical coordinates of the Sun
            sunEcliptical += Nutation.NutationEffect(Sky.NutationElements.deltaPsi);

            // add aberration effect, so we have an final ecliptical coordinates of the Sun 
            sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);

            return sunEcliptical;
        }
    }

    public class EphemerisConfig<TObject> where TObject : CelestialObject
    {
        public void FormulaFor<TResult>(string key, Func<CalculationContext, TObject, TResult> formula, EphemFormatter formatter)
        {

        }

        public EphemerisConfigItem<TObject, TResult> AddFormula<TResult>(string key, Func<CalculationContext, TObject, TResult> formula)
        {
            return new EphemerisConfigItem<TObject, TResult>();
        }
    }

    public class EphemerisConfigItem<TObject, TEphemType> where TObject : CelestialObject
    {
        public EphemerisConfigItem<TObject, TEphemType> AsEphem<TResult>(string key, Func<TEphemType, TResult> formula, EphemFormatter formatter = null)
        {
            return this;
        }

        public EphemerisConfigItem<TObject, TEphemType> AsEphem(EphemFormatter formatter = null)
        {
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
