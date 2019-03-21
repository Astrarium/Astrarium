using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    public class SolarEventsProvider : BaseAstroEventsProvider
    {
        public override void ConfigureAstroEvents(AstroEventsConfig config)
        {
            config.Add("Seasons", EventsSeasons);
        }

        public ICollection<AstroEvent> EventsSeasons(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();

            for (double jd = context.From; jd < context.To; jd += 365)
            {
                events.Add(new AstroEvent(SolarEphem.Season(jd, Season.Spring), "Vernal Equinox"));
                events.Add(new AstroEvent(SolarEphem.Season(jd, Season.Summer), "Summer Solstice"));
                events.Add(new AstroEvent(SolarEphem.Season(jd, Season.Autumn), "Autumnal Equinox"));
                events.Add(new AstroEvent(SolarEphem.Season(jd, Season.Winter), "Winter Solstice"));
            }

            return events;
        }
    }
}
