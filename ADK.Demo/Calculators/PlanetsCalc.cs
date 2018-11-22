using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class PlanetsCalc : BaseSkyCalc
    {
        private Planet[] Planets = new Planet[8];

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

            Planets[4].Flattening = 0.064874f;
            Planets[5].Flattening = 0.097962f;

            Sky.AddDataProvider("Planets", () => Planets);
        }

        public override void Calculate()
        {
            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            for (int p = 0; p < Planets.Length; p++)
            {
                // Skip Earth
                if (p + 1 == 3) continue;

                // time taken by the light to reach the Earth
                double tau = 0;

                // previous value of tau to calculate the difference
                double tau0 = 1;

                // Iterative process to find ecliptical coordinates of planet
                while (Math.Abs(tau - tau0) > deltaTau)
                {
                    // Heliocentrical coordinates of Earth
                    var hEarth = PlanetPositions.GetPlanetCoordinates(3, Sky.JulianDay - tau, highPrecision: true);

                    // Heliocentrical coordinates of planet
                    var hPlanet = PlanetPositions.GetPlanetCoordinates(p + 1, Sky.JulianDay - tau, highPrecision: true);

                    // Distance from planet to Sun
                    Planets[p].Distance = hPlanet.R;

                    // Ecliptical coordinates of planet
                    Planets[p].Ecliptical = hPlanet.ToRectangular(hEarth).ToEcliptical();

                    tau0 = tau;
                    tau = PlanetPositions.LightTimeEffect(Planets[p].Ecliptical.Distance);
                }

                // Correction for FK5 system
                Planets[p].Ecliptical += PlanetPositions.CorrectionForFK5(Sky.JulianDay, Planets[p].Ecliptical);

                // Take nutation into account
                Planets[p].Ecliptical += Nutation.NutationEffect(Sky.NutationElements.deltaPsi);

                // Apparent geocentrical equatorial coordinates of planet
                Planets[p].Equatorial0 = Planets[p].Ecliptical.ToEquatorial(Sky.Epsilon);

                // Planet semidiameter
                Planets[p].Semidiameter = PlanetPositions.Semidiameter(p + 1, Planets[p].Ecliptical.Distance);

                // Planet parallax
                Planets[p].Parallax = PlanetPositions.Parallax(Planets[p].Ecliptical.Distance);

                // Apparent topocentric coordinates of planet
                Planets[p].Equatorial = Planets[p].Equatorial0.ToTopocentric(Sky.GeoLocation, Sky.SiderealTime, Planets[p].Parallax);

                // Local horizontal coordinates of planet
                Planets[p].Horizontal = Planets[p].Equatorial.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);

                // Sun ephemerides should be already calculated
                Sun sun = Sky.Get<Sun>("Sun");

                // Elongation of the Planet
                Planets[p].Elongation = LunarEphem.Elongation(sun.Ecliptical, Planets[p].Ecliptical);

                // Phase angle
                Planets[p].PhaseAngle = LunarEphem.PhaseAngle(Planets[p].Elongation, sun.Ecliptical.Distance, Planets[p].Ecliptical.Distance);

                // Planet phase
                Planets[p].Phase = LunarEphem.Phase(Planets[p].PhaseAngle);

                // Planet magnitude
                Planets[p].Magnitude = PlanetPositions.GetPlanetMagnitude(p + 1, Planets[p].Ecliptical.Distance, Planets[p].Distance, Planets[p].PhaseAngle);
            }
        }
    }
}
