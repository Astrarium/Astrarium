using ADK;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    public class CometsEventsProvider : BaseAstroEventsProvider
    {
        private readonly CometsCalc cometsCalc;

        public CometsEventsProvider(CometsCalc cometsCalc)
        {
            this.cometsCalc = cometsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig config)
        {
            config
               .Add("Comets.PerihelionPassages", PerihelionPassages)
               .Add("Comets.PerihelionPassages2", PerihelionPassages);
        }

        private ICollection<AstroEvent> PerihelionPassages(AstroEventsContext context)
        {
            // resulting collection of events
            List<AstroEvent> events = new List<AstroEvent>();

            // current calculated value of Julian Day
            double jd = context.From;

            // Time shifts, in days, from starting point to each point in a range to be interpolated
            double[] t = { 0, 1, 2, 3, 4 };

            for (int c = 0; c < cometsCalc.Comets.Count; c++) {

                for (jd = context.From - 2; jd < context.To + 2; jd++)
                {
                    // "dist" is a distance from Sun (5 points)
                    double[] dist = new double[5];

                    SkyContext ctx = new SkyContext(jd, context.GeoLocation, true);

                    for (int i = 0; i < 5; i++)
                    {
                        dist[i] = ctx.Get(cometsCalc.DistanceFromSun, c);
                    }

                    if (dist[1] >= dist[2] && dist[2] <= dist[3])
                    {
                        Interpolation.FindMinimum(t, dist, 1e-6, out double t0, out double dist0);

                        // find the exact value of angular distance at extremum point
                        ctx = new SkyContext(jd - 2 + t0, context.GeoLocation, true);

                        dist0 = ctx.Get(cometsCalc.DistanceFromSun, c);

                        events.Add(new AstroEvent(jd - 2 + t0, $"{cometsCalc.GetName(cometsCalc.Comets.ElementAt(c))} passes perihelion."));
                    }
                }
            }


            return events;
        }
    }
}
