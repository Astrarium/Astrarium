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
    public partial class PlanetsCalc : BaseCalc, 
        ICelestialObjectCalc<Planet>, 
        ICelestialObjectCalc<MarsMoon>, 
        ICelestialObjectCalc<JupiterMoon>, 
        ICelestialObjectCalc<SaturnMoon>, 
        ICelestialObjectCalc<UranusMoon>
    {
        private ISettings settings;
        private Planet[] planets = new Planet[8];
        private JupiterMoon[] jupiterMoons = new JupiterMoon[4];
        private MarsMoon[] marsMoons = new MarsMoon[2];
        private SaturnMoon[] saturnMoons = new SaturnMoon[8];
        private UranusMoon[] uranusMoons = new UranusMoon[5];

        public ICollection<Planet> Planets => planets;
        public ICollection<MarsMoon> MarsMoons => marsMoons;
        public ICollection<JupiterMoon> JupiterMoons => jupiterMoons;
        public ICollection<SaturnMoon> SaturnMoons => saturnMoons;
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

            for (int i = 0; i < MarsMoons.Count; i++)
            {
                marsMoons[i] = new MarsMoon(i + 1);
            }

            for (int i = 0; i < JupiterMoons.Count; i++)
            {
                jupiterMoons[i] = new JupiterMoon(i + 1);
            }

            for (int i = 0; i < SaturnMoons.Count; i++)
            {
                saturnMoons[i] = new SaturnMoon(i + 1);
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

                p.Equatorial = context.Get(Planet_Equatorial, n);
                p.Horizontal = context.Get(Planet_Horizontal, n);
                p.Appearance = context.Get(Planet_Appearance, n);
                p.Magnitude = context.Get(Planet_Magnitude, n);
                p.DistanceFromSun = context.Get(Planet_DistanceFromSun, n);
                p.Semidiameter = context.Get(Planet_Semidiameter, n);
                p.Phase = context.Get(Planet_Phase, n);
                p.Elongation = context.Get(Planet_Elongation, n);
                p.Ecliptical = context.Get(Planet_Ecliptical, n);

                if (p.Number == Planet.MARS)
                {
                    foreach (var s in marsMoons)
                    {
                        int m = s.Number;
                        s.Rectangular = context.Get(MarsMoon_Rectangular, m);
                        s.Equatorial = context.Get(MarsMoon_Equatorial, m);
                        s.Horizontal = context.Get(MarsMoon_Horizontal, m);
                        s.Semidiameter = context.Get(MarsMoon_Semidiameter, m);
                        s.DistanceFromEarth = context.Get(MarsMoon_Ecliptical, m).Distance;
                    }
                }

                if (p.Number == Planet.JUPITER)
                {
                    foreach (var j in JupiterMoons)
                    {
                        int m = j.Number;
                        j.Rectangular = context.Get(JupiterMoon_Rectangular, m);
                        j.RectangularS = context.Get(JupiterMoonShadow_Rectangular, m);
                        j.Equatorial = context.Get(JupiterMoon_Equatorial, m);
                        j.Horizontal = context.Get(JupiterMoon_Horizontal, m);
                        j.Semidiameter = context.Get(JupiterMoon_Semidiameter, m);
                        j.CM = context.Get(JupiterMoon_CentralMeridian, m);
                        j.Magnitude = context.Get(JupiterMoon_Magnitude, m);
                        j.DistanceFromEarth = context.Get(JupiterMoon_DistanceFromEarth, m);
                    }

                    GreatRedSpotLongitude = context.Get(Jupiter_GreatRedSpotLongitude);
                }

                if (p.Number == Planet.SATURN)
                {
                    foreach (var j in SaturnMoons)
                    {
                        int m = j.Number;
                        j.Rectangular = context.Get(SaturnMoon_Rectangular, m);
                        j.Equatorial = context.Get(SaturnMoon_Equatorial, m);
                        j.Horizontal = context.Get(SaturnMoon_Horizontal, m);
                        j.Semidiameter = context.Get(SaturnMoon_Semidiameter, m);
                        j.Magnitude = context.Get(SaturnMoon_Magnitude, m);
                        j.DistanceFromEarth = context.Get(SaturnMoon_DistanceFromEarth, m);
                    }

                    SaturnRings = context.Get(Saturn_RingsAppearance, n);
                }

                if (p.Number == Planet.URANUS)
                {
                    foreach (var u in uranusMoons)
                    {
                        int m = u.Number;
                        u.Rectangular = context.Get(UranusMoon_Rectangular, m);
                        u.Equatorial = context.Get(UranusMoon_Equatorial, m);
                        u.Horizontal = context.Get(UranusMoon_Horizontal, m);
                        u.Semidiameter = context.Get(UranusMoon_Semidiameter, m);
                        u.DistanceFromEarth = context.Get(UranusMoon_Ecliptical, m).Distance;
                    }
                }
            }
        }
       
        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            var s1 = planets.Where(p => p.Number != Planet.EARTH && p.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            var s2 = marsMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            var s3 = jupiterMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            var s4 = saturnMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            var s5 = uranusMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            return s1.Concat(s2).Concat(s3).Concat(s4).Concat(s5).ToArray();
        }
    }
}
