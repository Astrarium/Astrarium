using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses
{
    public class EclipsesCalculator : BaseAstroEventsProvider
    {
        public EclipsesCalculator()
        {

        }

        public override void ConfigureAstroEvents(AstroEventsConfig cfg)
        {
            cfg["Eclipses.Solar"] = FindSolarEclipses;
        }

        private ICollection<AstroEvent> FindSolarEclipses(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();

            double jd = context.From;

            do
            {
                SolarEclipse eclipse = SolarEclipses.NearestEclipse(jd, next: true);
                jd = eclipse.JulianDayMaximum; 
                
                if (jd <= context.To)
                {
                    string type = eclipse.EclipseType.ToString();
                    string subtype = eclipse.IsNonCentral ? " non-central" : "";
                    string phase = eclipse.EclipseType == SolarEclipseType.Partial ? $"(max phase {Formatters.Phase.Format(eclipse.Phase)}) " : "";
                    string regio = eclipse.Regio.ToString();

                    events.Add(new AstroEvent(jd, $"{type}{subtype} solar eclipse {phase}visible in {regio} regio."));
                    jd += LunarEphem.SINODIC_PERIOD;
                }
                else
                {
                    break;
                }
            }
            while (true);

            return events;
        }
    }
}
