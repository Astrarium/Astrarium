using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
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
            c["AsteroidsEvents.Oppositions"] = Oppositions;
        }

        private ICollection<AstroEvent> Oppositions(AstroEventsContext context)
        {
            // resulting collection of events
            List<AstroEvent> events = new List<AstroEvent>();

            // ephemeris data for the requested period
            ICollection<AsteroidData> data = context.Get(AsteroidEphemeris);

            // Time shifts, in days, from starting point to each point in a range to be interpolated
            double[] t = { 0, 1, 2, 3, 4 };

            var brightestAsteroids = asteroidsCalc.Asteroids.Where(a => a.MaxBrightness <= 10);

            foreach (Asteroid a in brightestAsteroids)
            {
                // current day index
                int day = 0;

                var asteroidData = data
                    .Where(d => d.Asteroid == a)
                    .OrderBy(d => d.JulianDay);

                for (double jd = context.From; jd < context.To; jd++)
                {
                    // "diff" is a difference in longitude with the Sun, five values
                    double[] diff = asteroidData.Skip(day).Take(5).Select(d => Math.Abs(d.LongitudeDifference)).ToArray();

                    // If difference in longitude has maximum value at central point
                    if (diff[2] > 170 && diff[1] < diff[2] && diff[2] > diff[3])
                    {
                        // find instant of the opposition
                        Interpolation.FindMaximum(t, diff, 1e-6, out double t0, out double diff0);
                        double jdOpposition = jd - 2 + t0;

                        // context to calculate asteroid brightness
                        var ctx = new SkyContext(jdOpposition, context.GeoLocation, true);

                        // visible magnitude
                        float mag = ctx.Get(asteroidsCalc.Magnitude, a);

                        // take events for asteroids that brighter than 10.0m
                        if (mag <= 10)
                        {
                            events.Add(new AstroEvent(jdOpposition, 
                                Text.Get("AsteroidsEvents.Opposition.Text", 
                                    ("AsteroidName", a.Name), 
                                    ("AsteroidMagnitude", Formatters.Magnitude.Format(mag))), a
                                )
                            );
                        }
                    }

                    day++;
                }
            }

            return events;
        }

        private ICollection<AsteroidData> AsteroidEphemeris(AstroEventsContext context)
        {
            List<AsteroidData> results = new List<AsteroidData>();

            SkyContext ctx = new SkyContext(context.From, context.GeoLocation, true);

            var brightestAsteroids = asteroidsCalc.Asteroids.Where(a => a.MaxBrightness <= 10);

            for (double jd = context.From - 2; jd < context.To + 2; jd++)
            {
                ctx.JulianDay = jd;
                foreach (Asteroid a in brightestAsteroids)
                {
                    var data = new AsteroidData();
                    data.Asteroid = a;
                    data.JulianDay = jd;
                    data.LongitudeDifference = ctx.Get(asteroidsCalc.LongitudeDifference, a);
                    results.Add(data);
                }
            }

            return results;
        }

        private class AsteroidData
        {
            /// <summary>
            /// Julian Day
            /// </summary>
            public double JulianDay { get; set; }

            /// <summary>
            /// Asteroid 
            /// </summary>
            public Asteroid Asteroid { get; set; }

            /// <summary>
            /// Difference in longitude with the Sun
            /// </summary>
            public double LongitudeDifference { get; set; }
        }
    }
}
