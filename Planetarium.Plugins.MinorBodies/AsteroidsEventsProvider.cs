using ADK;
using Planetarium.Types;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    public class AsteroidsEventsProvider : BaseAstroEventsProvider
    {
        private readonly AsteroidsCalc asteroidsCalc;

        public AsteroidsEventsProvider(AsteroidsCalc asteroidsCalc)
        {
            this.asteroidsCalc = asteroidsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig c)
        {
            c["Asteroids.Oppositions"] = Oppositions;
        }

        private ICollection<AstroEvent> Oppositions(AstroEventsContext context)
        {
            // resulting collection of events
            List<AstroEvent> events = new List<AstroEvent>();

            // ephemeris data for the requested period
            ICollection<ICollection<AsteroidData>> data = context.Get(AsteroidEphemeris);

            // Time shifts, in days, from starting point to each point in a range to be interpolated
            double[] t = { 0, 1, 2, 3, 4 };

            for (int a = 0; a < asteroidsCalc.Asteroids.Count; a++) 
            {
                // current index in data array
                int day = 0;

                for (double jd = context.From; jd < context.To; jd++)
                {
                    // "diff" is a difference in longitude with the Sun
                    double[] diff = new double[5];

                    for (int i = 0; i < 5; i++)
                    {
                        diff[i] = Math.Abs(data.ElementAt(day + i).ElementAt(a).LongitudeDifference);
                    }

                    // If difference in longitude has maximum value at central point
                    if (diff[2] > 170 && diff[1] < diff[2] && diff[2] > diff[3])
                    {
                        Interpolation.FindMaximum(t, diff, 1e-6, out double t0, out double diff0);
                        events.Add(new AstroEvent(jd - 2 + t0, $"Asteroid {data.ElementAt(day).ElementAt(a).AsteroidName} in opposition"));
                    }

                    day++;
                }
            }

            return events;
        }

        // TODO: take only brightest asteroids!
        private ICollection<ICollection<AsteroidData>> AsteroidEphemeris(AstroEventsContext context)
        {
            List<ICollection<AsteroidData>> results = new List<ICollection<AsteroidData>>();

            // calculation context with "preferFast" flag enabled
            SkyContext ctx = new SkyContext(context.From, context.GeoLocation, true);

            // current calculated value of Julian Day
            double jd = context.From;

            for (jd = context.From - 2; jd < context.To + 2; jd++)
            {
                ctx.JulianDay = jd;

                ICollection<AsteroidData> data = new List<AsteroidData>();

                // calculate ephemeris data
                foreach (Asteroid a in asteroidsCalc.Asteroids)
                {
                    var item = new AsteroidData();
                    item.AsteroidName = a.Name;
                    item.LongitudeDifference = ctx.Get(asteroidsCalc.LongitudeDifference, a);
                    data.Add(item);
                }

                results.Add(data);
            }

            return results;
        }

        private class AsteroidData
        {
            public string AsteroidName { get; set; }

            /// <summary>
            /// Difference in longitude with the Sun
            /// </summary>
            public double LongitudeDifference { get; set; }
        }
    }
}
