using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public class SolarEventsProvider : BaseAstroEventsProvider
    {
        private readonly SolarCalc calc;

        public SolarEventsProvider(SolarCalc calc)
        {
            this.calc = calc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig phenomena)
        {
            phenomena["SunEvents.Seasons"] = EventsSeasons;
            phenomena["Daily.Sun.RiseSet"] = RiseSet;
        }

        public ICollection<AstroEvent> RiseSet(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();

            for (double jd = context.From; jd < context.To; jd += 1)
            {
                // check for cancel
                if (context.CancelToken?.IsCancellationRequested == true)
                    return new AstroEvent[0];

                var ctx = new SkyContext(jd, context.GeoLocation);
                var rts = calc.RiseTransitSet(ctx);

                if (rts.Rise != RTS.None)
                {
                    events.Add(new AstroEvent(ctx.JulianDayMidnight + rts.Rise, Text.Get("Daily.Sun.Rise"), calc.Sun));
                }

                if (rts.Set != RTS.None)
                {
                    events.Add(new AstroEvent(ctx.JulianDayMidnight + rts.Set, Text.Get("Daily.Sun.Set"), calc.Sun));
                }
            }

            return events;
        }

        public ICollection<AstroEvent> EventsSeasons(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();

            for (double jd = context.From; jd < context.To; jd += 365)
            {
                // check for cancel
                if (context.CancelToken?.IsCancellationRequested == true)
                    return new AstroEvent[0];

                events.Add(new AstroEvent(SolarEphem.Season(jd, Season.Spring), Text.Get("SunEvents.Seasons.Spring")));
                events.Add(new AstroEvent(SolarEphem.Season(jd, Season.Summer), Text.Get("SunEvents.Seasons.Summer")));
                events.Add(new AstroEvent(SolarEphem.Season(jd, Season.Autumn), Text.Get("SunEvents.Seasons.Autumn")));
                events.Add(new AstroEvent(SolarEphem.Season(jd, Season.Winter), Text.Get("SunEvents.Seasons.Winter")));
            }

            return events;
        }
    }
}
