using ADK;
using Planetarium.Config;
using Planetarium.Objects;
using Planetarium.Types;
using Planetarium.Types.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Planetarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc : BaseCalc, ICelestialObjectCalc<Planet>, ICelestialObjectCalc<JupiterMoon>, ICelestialObjectCalc<UranusMoon>
    {
        private ISettings settings;
        private Planet[] planets = new Planet[8];
        private JupiterMoon[] jupiterMoons = new JupiterMoon[4];
        private UranusMoon[] uranusMoons = new UranusMoon[5];

        public ICollection<Planet> Planets => planets;
        public ICollection<JupiterMoon> JupiterMoons => jupiterMoons;
        public ICollection<UranusMoon> UranusMoons => uranusMoons;
        public RingsAppearance SaturnRings { get; private set; } = new RingsAppearance();
        public double GreatRedSpotLongitude { get; private set; }

        private readonly Func<Planet, bool> IsSaturn = p => p.Number == Planet.SATURN;
        private readonly Func<Planet, bool> IsJupiter = p => p.Number == Planet.JUPITER;

        public PlanetsCalc(ISettings settings)
        {
            this.settings = settings;

            for (int i = 0; i < planets.Length; i++)
            {
                planets[i] = new Planet(i + 1);
            }

            for (int i = 0; i < JupiterMoons.Count; i++)
            {
                jupiterMoons[i] = new JupiterMoon(i + 1);
            }

            for (int i = 0; i < UranusMoons.Count; i++)
            {
                uranusMoons[i] = new UranusMoon(i + 1);
            }

            planets[Planet.JUPITER - 1].Flattening = 0.064874f;
            planets[Planet.SATURN - 1].Flattening = 0.097962f;
        }

        public override void Calculate(SkyContext context)
        {
            foreach (var p in planets)
            {
                if (p.Number == Planet.EARTH) continue;

                int n = p.Number;

                p.Equatorial = context.Get(Equatorial, n);
                p.Horizontal = context.Get(Horizontal, n);
                p.Appearance = context.Get(Appearance, n);
                p.Magnitude = context.Get(Magnitude, n);
                p.DistanceFromSun = context.Get(DistanceFromSun, n);
                p.Semidiameter = context.Get(Semidiameter, n);
                p.Phase = context.Get(Phase, n);
                p.Elongation = context.Get(Elongation, n);
                p.Ecliptical = context.Get(Ecliptical, n);

                if (p.Number == Planet.JUPITER)
                {
                    foreach (var j in JupiterMoons)
                    {
                        int m = j.Number;
                        j.Rectangular = context.Get(JupiterMoonRectangular, m);
                        j.RectangularS = context.Get(JupiterMoonRectangularS, m);
                        j.Equatorial = context.Get(JupiterMoonEquatorial, m);
                        j.Horizontal = context.Get(JupiterMoonHorizontal, m);
                        j.Semidiameter = context.Get(JupiterMoonSemidiameter, m);
                        j.CM = context.Get(JupiterMoonCentralMeridian, m);
                        j.Magnitude = context.Get(JupiterMoonMagnitude, m);
                        j.DistanceFromEarth = context.Get(JupiterMoonDistanceFromEarth, m);
                    }

                    GreatRedSpotLongitude = context.Get(JupiterGreatRedSpotLongitude);
                }

                if (p.Number == Planet.SATURN)
                {
                    SaturnRings = context.Get(GetSaturnRings, n);
                }

                if (p.Number == Planet.URANUS)
                {
                    foreach (var u in uranusMoons)
                    {
                        int m = u.Number;
                        u.Rectangular = context.Get(UranusMoonRectangular, m);
                        u.Equatorial = context.Get(UranusMoonEquatorial, m);
                        u.Horizontal = context.Get(UranusMoonHorizontal, m);
                        u.Semidiameter = context.Get(UranusMoonSemidiameter, m);
                        u.DistanceFromEarth = context.Get(UranusMoonEcliptical, m).Distance;
                    }
                }
            }
        }
       
        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            var s1 = planets.Where(p => p.Number != Planet.EARTH && p.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            var s2 = jupiterMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            var s3 = uranusMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            return s1.Concat(s2).Concat(s3).ToArray();
        }
    }
}
