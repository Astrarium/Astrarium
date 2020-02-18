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
                    foreach (var m in marsMoons)
                    {
                        int mn = m.Number;
                        m.Rectangular = context.Get(MarsMoon_Rectangular, mn);
                        m.Equatorial = context.Get(MarsMoon_Equatorial, mn);
                        m.Horizontal = context.Get(MarsMoon_Horizontal, mn);
                        m.Semidiameter = context.Get(MarsMoon_Semidiameter, mn);
                        m.DistanceFromEarth = context.Get(MarsMoon_Ecliptical, mn).Distance;
                    }
                }

                if (p.Number == Planet.JUPITER)
                {
                    foreach (var m in JupiterMoons)
                    {
                        int mn = m.Number;
                        m.Rectangular = context.Get(JupiterMoon_Rectangular, mn);
                        m.RectangularS = context.Get(JupiterMoonShadow_Rectangular, mn);
                        m.Equatorial = context.Get(JupiterMoon_Equatorial, mn);
                        m.Horizontal = context.Get(JupiterMoon_Horizontal, mn);
                        m.Semidiameter = context.Get(JupiterMoon_Semidiameter, mn);
                        m.CM = context.Get(JupiterMoon_CentralMeridian, mn);
                        m.Magnitude = context.Get(JupiterMoon_Magnitude, mn);
                        m.DistanceFromEarth = context.Get(JupiterMoon_DistanceFromEarth, mn);
                    }

                    GreatRedSpotLongitude = context.Get(Jupiter_GreatRedSpotLongitude);
                }

                if (p.Number == Planet.SATURN)
                {
                    foreach (var m in SaturnMoons)
                    {
                        int mn = m.Number;
                        m.Rectangular = context.Get(SaturnMoon_Rectangular, mn);
                        m.Equatorial = context.Get(SaturnMoon_Equatorial, mn);
                        m.Horizontal = context.Get(SaturnMoon_Horizontal, mn);
                        m.Semidiameter = context.Get(SaturnMoon_Semidiameter, mn);
                        m.Magnitude = context.Get(SaturnMoon_Magnitude, mn);
                        m.DistanceFromEarth = context.Get(SaturnMoon_DistanceFromEarth, mn);
                    }

                    SaturnRings = context.Get(Saturn_RingsAppearance, n);
                }

                if (p.Number == Planet.URANUS)
                {
                    foreach (var m in uranusMoons)
                    {
                        int mn = m.Number;
                        m.Rectangular = context.Get(UranusMoon_Rectangular, mn);
                        m.Equatorial = context.Get(UranusMoon_Equatorial, mn);
                        m.Horizontal = context.Get(UranusMoon_Horizontal, mn);
                        m.Semidiameter = context.Get(UranusMoon_Semidiameter, mn);
                        m.DistanceFromEarth = context.Get(UranusMoon_Ecliptical, mn).Distance;
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
