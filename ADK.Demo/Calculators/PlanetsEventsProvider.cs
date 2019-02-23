using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class PlanetsEventsProvider : BaseAstroEventsProvider
    {
        private readonly IPlanetsCalc planetsCalc;

        public PlanetsEventsProvider(IPlanetsCalc planetsCalc)
        {
            this.planetsCalc = planetsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig config)
        {
            config
               .Add("Planets.ConjunctionsInRightAscension", ConjunctionsInRightAscension)
               .Add("Planets.ConjunctionsInEclipticalLongitude", ConjunctionsInEclipticalLongitude)
               .Add("Planets.MaximalMagnitude", MaximalMagnitude)
               .Add("Planets.CloseApproaches", CloseApproaches)
               .Add("Planets.Stationaries", Stationaries);
        }

        private ICollection<AstroEvent> ConjunctionsInRightAscension(AstroEventsContext context)
        {
            return
                MutualConjunctions(context, 
                    d => d.Equatorial.Alpha, 
                    d => d.Equatorial.Delta)
                .Select(c => new AstroEvent(c.JulianDay, $"{c.Planet1} ({c.Magnitude1}) passes {c.AngularDistance} {c.Direction} to {c.Planet2} ({c.Magnitude2})"))
                .ToArray();
        }

        private ICollection<AstroEvent> ConjunctionsInEclipticalLongitude(AstroEventsContext context)
        {
            return MutualConjunctions(context, 
                    d => d.Ecliptical.Lambda, 
                    d => d.Ecliptical.Beta)
                .Select(c => new AstroEvent(c.JulianDay, $"{c.Planet1} ({c.Magnitude1}) in conjunction ({c.AngularDistance} {c.Direction}) with {c.Planet2} ({c.Magnitude2})."))
                .ToArray();
        }

        private ICollection<Conjunction> MutualConjunctions(
            AstroEventsContext context,
            Func<PlanetData, double> longitude,
            Func<PlanetData, double> latitude)
        {
            // resulting collection of conjunctions
            List<Conjunction> conjunctions = new List<Conjunction>();

            // planets ephemeris data for the requested period
            ICollection<PlanetData[]> data = context.Get(PlanetEphemeris);

            // current index in data array
            int day = 0;

            // current calculated value of Julian Day
            double jd = context.From;

            // Time shifts, in days, from starting point to each point in a range to be interpolated
            double[] t = { 0, 1, 2, 3, 4 };

            for (jd = context.From; jd < context.To; jd++)
            {
                // "a" is a difference in longitude coordinate (Right Ascension or Ecliptical Longitude) between two planets (5 points)
                double[] a = new double[5];

                // "d" is a difference in latitude coordinate (Declination or Ecliptical Latitude) between two planets (5 points)
                double[] d = new double[5];

                // p1 is a number of first planet
                for (int p1 = 1; p1 <= 8; p1++)
                {
                    // p2 is a number for second planet
                    for (int p2 = p1 + 1; p2 <= 8; p2++)
                    {
                        // skip Earth
                        if (p1 != 3 && p2 != 3)
                        {
                            // "a1" is longitude coordinates for the first planet (5 points)
                            double[] a1 = new double[5];

                            // "a2" is latitude coordinates for the second planet (5 points)
                            double[] a2 = new double[5];

                            // collect longitudes for both planets
                            for (int i = 0; i < 5; i++)
                            {
                                a1[i] = longitude(data.ElementAt(day + i)[p1]);
                                a2[i] = longitude(data.ElementAt(day + i)[p2]);
                            }

                            // Align values to avoid 360 degrees point crossing
                            Angle.Align(a1);
                            Angle.Align(a2);

                            for (int i = 0; i < 5; i++)
                            {
                                // "a" is a difference in longitudes between two planets (5 points)
                                a[i] = a1[i] - a2[i];

                                // "d" is a difference in latitudes between two planets (5 points)
                                d[i] = latitude(data.ElementAt(day + i)[p1]) - latitude(data.ElementAt(day + i)[p2]);
                            }

                            // If difference in longitude changes its sign, it means a conjunction takes place
                            // Will use interpolation to find a point where the conjunction occurs ("zero point")
                            if (a[1] * a[2] < 0)
                            {
                                Interpolation.FindRoot(t, a, 1e-6, out double t0);

                                Conjunction conj = new Conjunction();

                                conj.JulianDay = jd - 2 + t0;

                                // planet names
                                conj.Planet1 = planetsCalc.GetPlanetName(p1);
                                conj.Planet2 = planetsCalc.GetPlanetName(p2);

                                // passage direction
                                conj.Direction = latitude(data.ElementAt(day + 2)[p1]) > latitude(data.ElementAt(day + 2)[p2]) ? "north" : "south";

                                // find the angular distance at the "zero point"
                                conj.AngularDistance = Formatters.ConjunctionSeparation.Format(Math.Abs(Interpolation.Lagrange(t, d, t0)));

                                // magnitude of the first planet
                                conj.Magnitude1 = Formatters.Magnitude.Format(data.ElementAt(day + 2)[p1].Magnitude);

                                // magnitude of the second planet
                                conj.Magnitude2 = Formatters.Magnitude.Format(data.ElementAt(day + 2)[p2].Magnitude);

                                conjunctions.Add(conj);
                            }
                        }
                    }
                }

                day++;
            }

            return conjunctions;
        }

        private ICollection<AstroEvent> CloseApproaches(AstroEventsContext context)
        {
            // resulting collection of events
            List<AstroEvent> events = new List<AstroEvent>();

            // planets ephemeris data for the requested period
            ICollection<PlanetData[]> data = context.Get(PlanetEphemeris);

            // current index in data array
            int day = 0;

            // current calculated value of Julian Day
            double jd = context.From;

            // Time shifts, in days, from starting point to each point in a range to be interpolated
            double[] t = { 0, 1, 2, 3, 4 };

            for (jd = context.From; jd < context.To; jd++)
            {
                // "ad" is a angular distance between two planets (5 points)
                double[] ad = new double[5];

                // p1 is a number of first planet
                for (int p1 = 1; p1 <= 8; p1++)
                {
                    // p2 is a number for second planet
                    for (int p2 = p1 + 1; p2 <= 8; p2++)
                    {
                        // skip Earth
                        if (p1 != 3 && p2 != 3)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                ad[i] = Angle.Separation(data.ElementAt(day + i)[p1].Ecliptical, data.ElementAt(day + i)[p2].Ecliptical);
                            }

                            // If central point has lowest distance, it's a minimum point
                            if (ad[2] < 10 && ad[1] >= ad[2] && ad[2] <= ad[3])
                            {
                                Interpolation.FindMinimum(t, ad, 1e-6, out double t0, out double ad0);

                                // planet names
                                string name1 = planetsCalc.GetPlanetName(p1);
                                string name2 = planetsCalc.GetPlanetName(p2);

                                // find the exact value of angular distance at extremum point
                                var ctx = new SkyContext(jd - 2 + t0, context.GeoLocation, true);
                                ad0 = Angle.Separation(ctx.Get(planetsCalc.Ecliptical, p1), ctx.Get(planetsCalc.Ecliptical, p2));
                                string dist = Formatters.ConjunctionSeparation.Format(ad0);

                                // magnitude of the first planet
                                string mag1 = Formatters.Magnitude.Format(data.ElementAt(day + 2)[p1].Magnitude);

                                // magnitude of the second planet
                                string mag2 = Formatters.Magnitude.Format(data.ElementAt(day + 2)[p2].Magnitude);

                                events.Add(new AstroEvent(jd - 2 + t0, $"{name1} ({mag1}) has minimum angular separation ({dist}) with {name2} ({mag2})"));
                            }
                        }
                    }
                }

                day++;
            }

            return events;
        }

        private ICollection<AstroEvent> MaximalMagnitude(AstroEventsContext context)
        {
            // resulting collection of events
            List<AstroEvent> events = new List<AstroEvent>();

            // planets ephemeris data for the requested period
            ICollection<PlanetData[]> data = context.Get(PlanetEphemeris);

            // current index in data array
            int day = 0;

            // current calculated value of Julian Day
            double jd = context.From;

            // Time shifts, in days, from starting point to each point in a range to be interpolated
            double[] t = { 0, 1, 2, 3, 4 };

            for (jd = context.From; jd < context.To; jd++)
            {
                // "m" is a planet magnitude
                double[] m = new double[5];

                // p is a number of the planet
                for (int p = 1; p <= 8; p++)
                {
                    // skip Earth
                    if (p != 3)
                    {
                        // "m" is a planet magnitude (5 points)
                        for (int i = 0; i < 5; i++)
                        {
                            m[i] = data.ElementAt(day + i)[p].Magnitude;
                        }

                        // If magnitude has minimum value at central point
                        if (m[1] > m[2] && m[2] < m[3])
                        {
                            // ecliptical coordinates of the Sun
                            var ecl = planetsCalc.SunEcliptical(new SkyContext(jd, context.GeoLocation, true));

                            // check that the planet is not in conjunction with the Sun. 
                            // 5 degrees is an empirical value of angluar separation to make sure that
                            // the planet has enough separation from the Sun
                            if (Angle.Separation(ecl, data.ElementAt(day + 2)[p].Ecliptical) > 5)
                            {
                                Interpolation.FindMinimum(t, m, 1e-6, out double t0, out double m0);

                                string mag = Formatters.Magnitude.Format(m0);
                                string name = planetsCalc.GetPlanetName(p);

                                events.Add(new AstroEvent(jd - 2 + t0, $"{name} has maximal brightness ({mag})"));
                            }
                        }
                    }
                }

                day++;
            }

            return events;
        }

        private ICollection<AstroEvent> Stationaries(AstroEventsContext context)
        {
            // resulting collection of events
            List<AstroEvent> events = new List<AstroEvent>();

            // planets ephemeris data for the requested period
            ICollection<PlanetData[]> data = context.Get(PlanetEphemeris);

            // current index in data array
            int day = 0;

            // current calculated value of Julian Day
            double jd = context.From;

            // Time shifts, in days, from starting point to each point in a range to be interpolated
            double[] t = { 0, 1, 2, 3, 4 };

            for (jd = context.From; jd < context.To; jd++)
            {
                // "lon" is a planet ecliptical longitude 
                double[] lon = new double[5];

                // p is a number of the planet
                for (int p = 1; p <= 8; p++)
                {
                    // skip Earth
                    if (p != 3)
                    {
                        // "lon" is a planet ecliptical longitude (5 points)
                        for (int i = 0; i < 5; i++)
                        {
                            lon[i] = data.ElementAt(day + i)[p].Ecliptical.Lambda;
                        }

                        // Align values to avoid 360 degrees point crossing
                        Angle.Align(lon);

                        // If magnitude has minimum value at central point
                        if (lon[1] > lon[2] && lon[2] < lon[3])
                        {
                            Interpolation.FindMinimum(t, lon, 1e-6, out double t0, out double lon0);

                            string name = planetsCalc.GetPlanetName(p);
                            events.Add(new AstroEvent(jd - 2 + t0, $"Stationary of {name}. Planet gets apparent prograde motion."));
                        }
                        else if (lon[1] < lon[2] && lon[2] > lon[3])
                        {
                            Interpolation.FindMaximum(t, lon, 1e-6, out double t0, out double lon0);

                            string name = planetsCalc.GetPlanetName(p);
                            events.Add(new AstroEvent(jd - 2 + t0, $"Stationary of {name}. Planet gets apparent retrograde motion."));
                        }
                    }
                }

                day++;
            }

            return events;
        }

        private ICollection<PlanetData[]> PlanetEphemeris(AstroEventsContext context)
        {
            List<PlanetData[]> results = new List<PlanetData[]>();

            // calculation context with "preferFast" flag enabled
            SkyContext ctx = new SkyContext(context.From, context.GeoLocation, true);

            // equatorial coordinates of planets
            CrdsEquatorial[] eq = new CrdsEquatorial[9];

            // current calculated value of Julian Day
            double jd = context.From;

            for (jd = context.From - 2; jd < context.To + 2; jd++)
            {
                ctx.JulianDay = jd;

                PlanetData[] data = new PlanetData[9];

                // calculate coordinates of planets
                for (int p = 1; p <= 8; p++)
                {
                    if (p != 3)
                    {
                        data[p] = new PlanetData();
                        data[p].Equatorial = ctx.Get(planetsCalc.Equatorial, p);
                        data[p].Ecliptical = ctx.Get(planetsCalc.Ecliptical, p);
                        data[p].Magnitude = ctx.Get(planetsCalc.Magnitude, p);
                    }
                }

                results.Add(data);
            }

            return results;
        }

        private class PlanetData
        {
            public CrdsEquatorial Equatorial { get; set; }
            public CrdsEcliptical Ecliptical { get; set; }
            public float Magnitude { get; set; }
        }

        private class Conjunction
        {
            public double JulianDay { get; set; }
            public string Planet1 { get; set; }
            public string Planet2 { get; set; }
            public string Magnitude1 { get; set; }
            public string Magnitude2 { get; set; }
            public string Direction { get; set; }
            public string AngularDistance { get; set; }
        }
    }
}
