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
        public override void ConfigureAstroEvents(AstroEventsConfig phenomena)
        {
            phenomena["SunEvents.Seasons"] = EventsSeasons;
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
