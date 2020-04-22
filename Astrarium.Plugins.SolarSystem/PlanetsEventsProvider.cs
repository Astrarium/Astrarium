using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public class PlanetsEventsProvider : BaseAstroEventsProvider
    {
        private readonly PlanetsCalc planetsCalc;
        private readonly SolarCalc solarCalc;

        private readonly IEphemFormatter conjunctionSeparationFormatter = new Formatters.UnsignedDoubleFormatter(1, "\u00B0");

        public PlanetsEventsProvider(PlanetsCalc planetsCalc, SolarCalc solarCalc)
        {
            this.planetsCalc = planetsCalc;
            this.solarCalc = solarCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig c)
        {
            c["PlanetEvents.ConjunctionsInRightAscension"] = ConjunctionsInRightAscension;
            c["PlanetEvents.ConjunctionsInEclipticalLongitude"] = ConjunctionsInEclipticalLongitude;
            c["PlanetEvents.CloseApproaches"] = CloseApproaches;
            c["PlanetEvents.MaximalMagnitude"] = MaximalMagnitude;
            c["PlanetEvents.Stationaries"] = Stationaries;
            c["PlanetEvents.GreatestElongations"] = GreatestElongations;
            c["PlanetEvents.Oppositions"] = Oppositions;
            c["PlanetEvents.Conjunctions"] = Conjunctions;
            c["PlanetEvents.VisibilityPeriods"] = VisibilityPeriods;
        }

        private ICollection<AstroEvent> ConjunctionsInRightAscension(AstroEventsContext context)
        {
            return
                MutualConjunctions(context,
                    d => d.Equatorial.Alpha,
                    d => d.Equatorial.Delta)
                .Select(c => new AstroEvent(c.JulianDay, 
                    Text.Get("PlanetEvents.ConjunctionsInRightAscension.Text", 
                    ("planetName1", GetPlanetName(c.Planet1)),
                    ("planetGenitiveName1", GetPlanetGenitiveName(c.Planet1)),
                    ("planetMagnitude1", c.Magnitude1), 
                    ("angularDistance", c.AngularDistance), 
                    ("direction", Text.Get($"PlanetEvents.ConjunctionsInRightAscension.Text.{c.Direction}")), 
                    ("planetName2", GetPlanetName(c.Planet2)),
                    ("planetGenitiveName2", GetPlanetGenitiveName(c.Planet2)),
                    ("planetMagnitude2", c.Magnitude2))))
                .ToArray();
        }

        private ICollection<AstroEvent> ConjunctionsInEclipticalLongitude(AstroEventsContext context)
        {
            return MutualConjunctions(context, 
                    d => d.Ecliptical.Lambda, 
                    d => d.Ecliptical.Beta)
                .Select(c => new AstroEvent(c.JulianDay,
                    Text.Get("PlanetEvents.ConjunctionsInEclipticalLongitude.Text",
                    ("planetName1", GetPlanetName(c.Planet1)),
                    ("planetGenitiveName1", GetPlanetGenitiveName(c.Planet1)),
                    ("planetMagnitude1", c.Magnitude1),
                    ("angularDistance", c.AngularDistance),
                    ("direction", Text.Get($"PlanetEvents.ConjunctionsInEclipticalLongitude.Text.{c.Direction}")),
                    ("planetName2", GetPlanetName(c.Planet2)),
                    ("planetGenitiveName2", GetPlanetGenitiveName(c.Planet2)),
                    ("planetMagnitude2", c.Magnitude2))))
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
                                conj.Planet1 = p1;
                                conj.Planet2 = p2;

                                // passage direction
                                conj.Direction = latitude(data.ElementAt(day + 2)[p1]) < latitude(data.ElementAt(day + 2)[p2]) ? "North" : "South";

                                // find the angular distance at the "zero point"
                                conj.AngularDistance = conjunctionSeparationFormatter.Format(Math.Abs(Interpolation.Lagrange(t, d, t0)));

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

                                // find the exact value of angular distance at extremum point
                                var ctx = new SkyContext(jd - 2 + t0, context.GeoLocation, true);
                                ad0 = Angle.Separation(ctx.Get(planetsCalc.Planet_Ecliptical, p1), ctx.Get(planetsCalc.Planet_Ecliptical, p2));
                                string dist = conjunctionSeparationFormatter.Format(ad0);

                                // magnitude of the first planet
                                string mag1 = Formatters.Magnitude.Format(data.ElementAt(day + 2)[p1].Magnitude);

                                // magnitude of the second planet
                                string mag2 = Formatters.Magnitude.Format(data.ElementAt(day + 2)[p2].Magnitude);

                                events.Add(new AstroEvent(jd - 2 + t0,
                                    Text.Get("PlanetEvents.CloseApproaches.Text",
                                    ("planetName1", GetPlanetName(p1)),
                                    ("planetGenitiveName1", GetPlanetGenitiveName(p1)),
                                    ("planetMagnitude1", mag1),
                                    ("angularDistance", dist),
                                    ("planetName2", GetPlanetName(p2)),
                                    ("planetGenitiveName2", GetPlanetGenitiveName(p2)),
                                    ("planetMagnitude2", mag2))));
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
                            // check that the planet is not in conjunction with the Sun. 
                            // 5 degrees is an empirical value of angluar separation to make sure that
                            // the planet has enough separation from the Sun
                            if (Math.Abs(data.ElementAt(day + 2)[p].Elongation) > 5)
                            {
                                Interpolation.FindMinimum(t, m, 1e-6, out double t0, out double m0);

                                string mag = Formatters.Magnitude.Format(m0);
                                string name = GetPlanetName(p);

                                events.Add(new AstroEvent(jd - 2 + t0,
                                    Text.Get("PlanetEvents.MaximalMagnitude.Text",
                                        ("planetName", name),
                                        ("planetMagnitude", mag))));
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

                            events.Add(new AstroEvent(jd - 2 + t0,
                                Text.Get("PlanetEvents.Stationaries.ProgradeText",
                                    ("planetName", GetPlanetName(p)),
                                    ("planetGenitiveName", GetPlanetGenitiveName(p)))));
                        }
                        else if (lon[1] < lon[2] && lon[2] > lon[3])
                        {
                            Interpolation.FindMaximum(t, lon, 1e-6, out double t0, out double lon0);

                            events.Add(new AstroEvent(jd - 2 + t0,
                                Text.Get("PlanetEvents.Stationaries.RetrogradeText",
                                    ("planetName", GetPlanetName(p)),
                                    ("planetGenitiveName", GetPlanetGenitiveName(p)))));
                        }
                    }
                }

                day++;
            }

            return events;
        }

        private ICollection<AstroEvent> GreatestElongations(AstroEventsContext context)
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
                // "el" is a planet elongation
                double[] el = new double[5];

                // p is a number of inner planet
                for (int p = 1; p <= 2; p++)
                {
                    // "el" is a planet elongation (5 points)
                    for (int i = 0; i < 5; i++)
                    {
                        el[i] = Math.Abs(data.ElementAt(day + i)[p].Elongation);
                    }

                    // If elongation has maximum value at central point
                    if (el[1] < el[2] && el[2] > el[3])
                    {
                        Interpolation.FindMaximum(t, el, 1e-6, out double t0, out double el0);

                        string direction = data.ElementAt(day + 2)[p].Elongation > 0 ? "East" : "West";
                        string elongation = conjunctionSeparationFormatter.Format(el0);
                        events.Add(new AstroEvent(jd - 2 + t0,
                            Text.Get("PlanetEvents.GreatestElongations.Text",
                                ("planetName", GetPlanetName(p)),
                                ("planetGenitiveName", GetPlanetGenitiveName(p)),
                                ("direction", Text.Get($"PlanetEvents.GreatestElongations.{direction}")),
                                ("elongation", elongation))
                            ));
                    }                   
                }

                day++;
            }

            return events;
        }

        private ICollection<AstroEvent> Oppositions(AstroEventsContext context)
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
                // "diff" is a planet difference in longitude with the Sun
                double[] diff = new double[5];

                // p is a number of outer planet
                for (int p = 4; p <= 8; p++)
                {
                    // "diff" is a planet difference in longitude with the Sun (5 points)
                    for (int i = 0; i < 5; i++)
                    {
                        diff[i] = Math.Abs(data.ElementAt(day + i)[p].LongitudeDifference);
                    }

                    // If difference in longitude has maximum value at central point
                    if (diff[2] > 170 && diff[1] < diff[2] && diff[2] > diff[3])
                    {
                        Interpolation.FindMaximum(t, diff, 1e-6, out double t0, out double diff0);
       
                        events.Add(new AstroEvent(jd - 2 + t0,
                            Text.Get("PlanetEvents.Oppositions.Text",
                                ("planetName", GetPlanetName(p)),
                                ("planetGenitiveName", GetPlanetGenitiveName(p)))));
                    }
                }

                day++;
            }

            return events;
        }

        private ICollection<AstroEvent> Conjunctions(AstroEventsContext context)
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
                // "diff" is a planet difference in longitude with the Sun
                double[] diff = new double[5];

                // p is a number of a planet
                for (int p = 1; p <= 8; p++)
                {
                    // Skip Earth
                    if (p != 3)
                    {
                        // "diff" is a planet difference in longitude with the Sun (5 points)
                        for (int i = 0; i < 5; i++)
                        {
                            diff[i] = data.ElementAt(day + i)[p].LongitudeDifference;
                        }
                        
                        if (Math.Abs(diff[2]) < 5 && diff[2] * diff[3] <= 0)
                        {
                            Interpolation.FindRoot(t, diff, 1e-6, out double t0);
                            double jdConj = jd - 2 + t0;
                            string text;

                            if (p < 3)
                            {                                
                                string conjType = data.ElementAt(day + 2)[p].Ecliptical.Distance < 1 ? 
                                    "Inferior" : "Superior";
                                
                                var ctx = new SkyContext(jdConj, context.GeoLocation, false);
                                double sd = solarCalc.Semidiameter(ctx) / 3600;
                                double ad = Math.Abs(planetsCalc.Planet_Elongation(ctx, p));

                                text = Text.Get($"PlanetEvents.Conjunctions.{conjType}", 
                                    ("planetName", GetPlanetName(p)),
                                    ("planetGenitiveName", GetPlanetGenitiveName(p)));

                                if (ad < sd)
                                {
                                    text = $"{text}{Text.Get("PlanetEvents.Conjunctions.Transit")}";
                                }
                            }
                            else
                            {
                                text = Text.Get($"PlanetEvents.Conjunctions.Text",
                                    ("planetName", GetPlanetName(p)),
                                    ("planetGenitiveName", GetPlanetGenitiveName(p)));
                            }

                            events.Add(new AstroEvent(jdConj, text));
                        }
                    }
                }

                day++;
            }

            return events;
        }

        private ICollection<AstroEvent> VisibilityPeriods(AstroEventsContext context)
        {
            // resulting collection of events
            List<AstroEvent> events = new List<AstroEvent>();

            // planets ephemeris data for the requested period
            ICollection<PlanetData[]> data = context.Get(PlanetEphemeris);

            // current index in data array
            int day = 0;

            for (double jd = context.From; jd < context.To; jd++)
            {
                // p is a number of a planet
                for (int p = 1; p <= 8; p++)
                {
                    // Skip Earth
                    if (p != 3)
                    {
                        var vis = data.ElementAt(day + 2)[p].Visibility;
                        var prev = data.ElementAt(day + 1)[p].Visibility;

                        if (vis.Period != prev.Period)
                        {
                            string text = null;
                           
                            if (vis.Period == VisibilityPeriod.Invisible)
                            {
                                text = "PlanetEvents.VisibilityPeriods.End";
                            }

                            if ((vis.Period & VisibilityPeriod.Morning) != 0)
                            {
                                text = "PlanetEvents.VisibilityPeriods.BeginMorning";
                            }
                            else if ((vis.Period & VisibilityPeriod.Evening) != 0)
                            {
                                text = "PlanetEvents.VisibilityPeriods.BeginEvening";
                            }
                            else if ((vis.Period & VisibilityPeriod.Night) != 0)
                            {
                                text = "PlanetEvents.VisibilityPeriods.BeginNight";
                            }

                            if (text != null)
                            {
                                events.Add(new AstroEvent(jd, 
                                    Text.Get(text,
                                        ("planetName", GetPlanetName(p)),
                                        ("planetGenitiveName", GetPlanetGenitiveName(p))
                                ), noExactTime: true));
                            }
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
                        data[p].Equatorial = ctx.Get(planetsCalc.Planet_Equatorial, p);
                        data[p].Ecliptical = ctx.Get(planetsCalc.Planet_Ecliptical, p);
                        data[p].Magnitude = ctx.Get(planetsCalc.Planet_Magnitude, p);
                        data[p].Elongation = ctx.Get(planetsCalc.Planet_Elongation, p);
                        data[p].Visibility = ctx.Get(planetsCalc.Planet_Visibility, p);
                        data[p].LongitudeDifference = ctx.Get(planetsCalc.Planet_LongitudeDifference, p);
                    }
                }

                results.Add(data);
            }

            return results;
        }

        private string GetPlanetName(int planetNumber)
        {
            return Text.Get($"Planet.{planetNumber}.Name");
        }

        private string GetPlanetGenitiveName(int planetNumber)
        {
            return Text.Get($"Planet.{planetNumber}.GenitiveName");
        }

        private class PlanetData
        {
            /// <summary>
            /// Apparent equatorial coordinates
            /// </summary>
            public CrdsEquatorial Equatorial { get; set; }

            /// <summary>
            /// Apparent ecliptical coordinates
            /// </summary>
            public CrdsEcliptical Ecliptical { get; set; }

            /// <summary>
            /// Elongation angle
            /// </summary>
            public double Elongation { get; set; }

            /// <summary>
            /// Apparent magnitude
            /// </summary>
            public float Magnitude { get; set; }

            /// <summary>
            /// Difference in longitude with the Sun
            /// </summary>
            public double LongitudeDifference { get; set; }

            /// <summary>
            /// Visibility details
            /// </summary>
            public VisibilityDetails Visibility { get; set; }
        }

        private class Conjunction
        {
            public double JulianDay { get; set; }
            public int Planet1 { get; set; }
            public int Planet2 { get; set; }
            public string Magnitude1 { get; set; }
            public string Magnitude2 { get; set; }
            public string Direction { get; set; }
            public string AngularDistance { get; set; }
        }
    }
}
