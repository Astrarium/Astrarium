using Astrarium.Algorithms;
using Astrarium.Types;
using Astrarium.Plugins.SolarSystem.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astrarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc : BaseCalc, 
        ICelestialObjectCalc<Planet>, 
        ICelestialObjectCalc<Pluto>,
        ICelestialObjectCalc<MarsMoon>, 
        ICelestialObjectCalc<JupiterMoon>, 
        ICelestialObjectCalc<SaturnMoon>, 
        ICelestialObjectCalc<UranusMoon>,
        ICelestialObjectCalc<NeptuneMoon>,
        ICelestialObjectCalc<GenericMoon>
    {
        private ISettings settings;
        private Planet[] planets = new Planet[8];
        private Pluto pluto = new Pluto();
        private JupiterMoon[] jupiterMoons = new JupiterMoon[4];
        private MarsMoon[] marsMoons = new MarsMoon[2];
        private SaturnMoon[] saturnMoons = new SaturnMoon[8];
        private UranusMoon[] uranusMoons = new UranusMoon[5];
        private NeptuneMoon[] neptuneMoons = new NeptuneMoon[2];
        private List<GenericMoon> genericMoons = new List<GenericMoon>();

        public IReadOnlyCollection<Planet> Planets => planets;
        public Pluto Pluto => pluto;
        public IReadOnlyCollection<MarsMoon> MarsMoons => marsMoons;
        public IReadOnlyCollection<JupiterMoon> JupiterMoons => jupiterMoons;
        public IReadOnlyCollection<SaturnMoon> SaturnMoons => saturnMoons;
        public IReadOnlyCollection<UranusMoon> UranusMoons => uranusMoons;
        public IReadOnlyCollection<NeptuneMoon> NeptuneMoons => neptuneMoons;
        public IReadOnlyCollection<GenericMoon> GenericMoons => genericMoons;
        public RingsAppearance SaturnRings { get; private set; } = new RingsAppearance();
        public double GreatRedSpotLongitude { get; private set; }
        public double MarsNPCWidth { get; private set; }
        public double MarsSPCWidth { get; private set; }

        IEnumerable<Planet> ICelestialObjectCalc<Planet>.GetCelestialObjects() => planets.Where(p => p.Number != 3);
        IEnumerable<Pluto> ICelestialObjectCalc<Pluto>.GetCelestialObjects() => new Pluto[] { pluto };
        IEnumerable<MarsMoon> ICelestialObjectCalc<MarsMoon>.GetCelestialObjects() => marsMoons;
        IEnumerable<JupiterMoon> ICelestialObjectCalc<JupiterMoon>.GetCelestialObjects() => jupiterMoons;
        IEnumerable<SaturnMoon> ICelestialObjectCalc<SaturnMoon>.GetCelestialObjects() => saturnMoons;
        IEnumerable<UranusMoon> ICelestialObjectCalc<UranusMoon>.GetCelestialObjects() => uranusMoons;
        IEnumerable<NeptuneMoon> ICelestialObjectCalc<NeptuneMoon>.GetCelestialObjects() => neptuneMoons;
        IEnumerable<GenericMoon> ICelestialObjectCalc<GenericMoon>.GetCelestialObjects() => genericMoons;

        private readonly Func<Planet, bool> IsMars = p => p.Number == Planet.MARS;
        private readonly Func<Planet, bool> IsJupiter = p => p.Number == Planet.JUPITER;
        private readonly Func<Planet, bool> IsSaturn = p => p.Number == Planet.SATURN;

        public override void Initialize()
        {
            var orbits = new OrbitalElementsManager(settings).Load();
            foreach (var orbit in orbits)
            {
                genericMoons.Add(new GenericMoon() { Data = orbit });
            }
        }

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

            for (int i = 0; i < NeptuneMoons.Count; i++)
            {
                neptuneMoons[i] = new NeptuneMoon(i + 1);
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
                p.PhaseAngle = Math.Sign(context.Get(Planet_Elongation, n)) * context.Get(Planet_PhaseAngle, n);
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
                        m.Magnitude = context.Get(MarsMoon_Magnitude, mn);
                        m.DistanceFromEarth = context.Get(MarsMoon_Ecliptical, mn).Distance;
                    }

                    MarsNPCWidth = context.Get(Mars_PolarCap, PolarCap.Northern);
                    MarsSPCWidth = context.Get(Mars_PolarCap, PolarCap.Southern);
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
                        m.Magnitude = context.Get(UranusMoon_Magnitude, mn);
                        m.DistanceFromEarth = context.Get(UranusMoon_Ecliptical, mn).Distance;
                    }
                }

                if (p.Number == Planet.NEPTUNE)
                {
                    foreach (var m in neptuneMoons)
                    {
                        int mn = m.Number;
                        m.Equatorial = context.Get(NeptuneMoon_Equatorial, mn);
                        m.Horizontal = context.Get(NeptuneMoon_Horizontal, mn);
                        m.Semidiameter = context.Get(NeptuneMoon_Semidiameter, mn);
                        m.Magnitude = context.Get(NeptuneMoon_Magnitude, mn);
                        m.DistanceFromEarth = context.Get(NeptuneMoon_Ecliptical, mn).Distance;                        
                    }
                }

                foreach (var m in genericMoons)
                {
                    m.Equatorial = context.Get(GenericMoon_Equatorial, m.Id);
                    m.Horizontal = context.Get(GenericMoon_Horizontal, m.Id);
                    m.Semidiameter = context.Get(GenericMoon_Semidiameter, m.Id);
                    m.Magnitude = context.Get(GenericMoon_Magnitude, m.Id);
                    m.DistanceFromEarth = context.Get(GenericMoon_Ecliptical, m.Id).Distance;
                }
            }

            pluto.Equatorial = context.Get(Pluto_Equatorial);
            pluto.Horizontal = context.Get(Pluto_Horizontal);
            pluto.Appearance = context.Get(Pluto_Appearance);
            pluto.Semidiameter = context.Get(Pluto_Semidiameter);
            pluto.Magnitude = context.Get(Pluto_Magnitude);
            pluto.DistanceFromEarth = context.Get(Pluto_DistanceFromEarth);
            pluto.Ecliptical = context.Get(Pluto_Ecliptical);
        }
       
        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            // common names
            for (int p = 0; p < Planet.NAMES.Length; p++)
            {
                if (p != Planet.EARTH - 1 &&  p < Planet.PLUTO - 1 && searchString.Equals(Planets.ElementAt(p).CommonName, StringComparison.OrdinalIgnoreCase) && filterFunc(Planets.ElementAt(p)))
                    return new[] { Planets.ElementAt(p) };
            }

            for (int m = 0; m < marsMoons.Length; m++)
            {
                if (searchString.Equals(marsMoons[m].CommonName, StringComparison.OrdinalIgnoreCase) && filterFunc(marsMoons.ElementAt(m)))
                    return new[] { marsMoons.ElementAt(m) };
            }

            for (int m = 0; m < jupiterMoons.Length; m++)
            {
                if (searchString.Equals(jupiterMoons[m].CommonName, StringComparison.OrdinalIgnoreCase) && filterFunc(jupiterMoons.ElementAt(m)))
                    return new[] { jupiterMoons.ElementAt(m) };
            }

            for (int m = 0; m < saturnMoons.Length; m++)
            {
                if (searchString.Equals(saturnMoons[m].CommonName, StringComparison.OrdinalIgnoreCase) && filterFunc(saturnMoons.ElementAt(m)))
                    return new[] { saturnMoons.ElementAt(m) };
            }

            for (int m = 0; m < uranusMoons.Length; m++)
            {
                if (searchString.Equals(uranusMoons[m].CommonName, StringComparison.OrdinalIgnoreCase) && filterFunc(uranusMoons.ElementAt(m)))
                    return new[] { uranusMoons.ElementAt(m) };
            }

            for (int m = 0; m < neptuneMoons.Length; m++)
            {
                if (searchString.Equals(neptuneMoons[m].CommonName, StringComparison.OrdinalIgnoreCase) && filterFunc(neptuneMoons.ElementAt(m)))
                    return new[] { neptuneMoons.ElementAt(m) };
            }

            for (int m = 0; m < genericMoons.Count; m++)
            {
                if (searchString.Equals(genericMoons[m].CommonName, StringComparison.OrdinalIgnoreCase) && filterFunc(genericMoons.ElementAt(m)))
                    return new[] { genericMoons.ElementAt(m) };
            }

            if (pluto.CommonName.Equals(searchString, StringComparison.OrdinalIgnoreCase) && filterFunc(pluto))
                return new[] { pluto };

            var s1 = planets.Where(p => p.Number != Planet.EARTH && p.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase) && filterFunc(p))
                .Select(p => p as CelestialObject);

            var s2 = marsMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).Where(filterFunc);

            var s3 = jupiterMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).Where(filterFunc);

            var s4 = saturnMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).Where(filterFunc);

            var s5 = uranusMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).Where(filterFunc);

            var s6 = neptuneMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).Where(filterFunc);

            var s7 = genericMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).Where(filterFunc);

            var s8 = new[] { pluto }.Where(p => p.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).Where(filterFunc);

            return s1.Concat(s2).Concat(s3).Concat(s4).Concat(s5).Concat(s6).Concat(s7).Concat(s8).Take(maxCount).ToArray();
        }
    }
}
