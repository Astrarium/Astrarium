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
    public class CometsEventsProvider : BaseAstroEventsProvider
    {
        private readonly CometsCalc cometsCalc;

        public CometsEventsProvider(CometsCalc cometsCalc)
        {
            this.cometsCalc = cometsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig c)
        {
            c["Comets.PerihelionPassages"] = PerihelionPassages;
            
            // TODO: optimize speed if possible
            // c["Comets.MaximalTailLength"] = MaximalTailLength;
        }

        private ICollection<AstroEvent> PerihelionPassages(AstroEventsContext context)
        {
            return
                cometsCalc.Comets.Where(c =>
                    c.Orbit.Epoch >= context.From &&
                    c.Orbit.Epoch <= context.To)
                    .Select(c => new AstroEvent(c.Orbit.Epoch, $"Comet {c.Names.First()} at perihelion"))
                    .ToArray();
        }

        private ICollection<AstroEvent> MaximalTailLength(AstroEventsContext context)
        {
            // resulting collection of events
            List<AstroEvent> events = new List<AstroEvent>();

            foreach (Comet c in cometsCalc.Comets)
            {
                ICollection<CometData> data = context.Get(CometEphemeris, c);

                // current index in data array
                int day = 0;

                // current calculated value of Julian Day
                double jd = context.From;

                // Time shifts, in days, from starting point to each point in a range to be interpolated
                double[] t = { 0, 1, 2, 3, 4 };

                if (data.Any(d => d.TailVisibleLength > 1))
                {
                    for (jd = context.From; jd < context.To; jd++)
                    {
                        // "s" is a tail length
                        double[] s = new double[5];

                        for (int i = 0; i < 5; i++)
                        {
                            s[i] = data.ElementAt(day + i).TailVisibleLength;
                        }

                        // If length has maximum value at central point
                        if (s[1] < s[2] && s[2] > s[3])
                        {

                            Interpolation.FindMaximum(t, s, 1e-6, out double t0, out double s0);

                            // s0 - maximal tail lenght
                            string name = c.Name;

                            events.Add(new AstroEvent(jd - 2 + t0,
                                $"Comet {name} has maximal visible tail length"));

                        }

                        day++;
                    }
                }
            }

            return events;
        }

        private ICollection<CometData> CometEphemeris(AstroEventsContext context, Comet c)
        {
            List<CometData> results = new List<CometData>();

            // calculation context with "preferFast" flag enabled
            SkyContext ctx = new SkyContext(context.From, context.GeoLocation, true);

            // current calculated value of Julian Day
            double jd = context.From;

            for (jd = context.From - 2; jd < context.To + 2; jd++)
            {
                ctx.JulianDay = jd;

                CometData data = new CometData();
                data.TailVisibleLength = ctx.Get(cometsCalc.TailVisibleLength, c);
   
                results.Add(data);
            }

            return results;
        }

        private class CometData
        {
            /// <summary>
            /// Visible length of comet tail, in degrees of arc
            /// </summary>
            public double TailVisibleLength { get; set; }
        }
    }
}
