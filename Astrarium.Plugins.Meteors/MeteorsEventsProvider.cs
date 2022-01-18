using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Meteors
{
    public class MeteorsEventsProvider : BaseAstroEventsProvider
    {
        private readonly MeteorsCalculator meteorsCalc;

        public MeteorsEventsProvider(MeteorsCalculator meteorsCalc)
        {
            this.meteorsCalc = meteorsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig c)
        {
            c["MeteorsEvents.Begin"] = ctx => FindInstants(ctx, m => m.Begin, m => Text.Get("MeteorsEvents.Begin.Text", ("Name", m.Name)));
            c["MeteorsEvents.Max"] = ctx => FindInstants(ctx, m => m.Max, m => Text.Get("MeteorsEvents.Max.Text", ("Name", m.Name)));
            c["MeteorsEvents.End"] = ctx => FindInstants(ctx, m => m.End, m => Text.Get("MeteorsEvents.End.Text", ("Name", m.Name)));
        }

        private ICollection<AstroEvent> FindInstants(AstroEventsContext context, Func<Meteor, short> func, Func<Meteor, string> text)
        {
            var events = new List<AstroEvent>();

            int fromYear = new Date(context.From, context.GeoLocation.UtcOffset).Year;
            int toYear = new Date(context.To, context.GeoLocation.UtcOffset).Year;

            for (int year = fromYear; year <= toYear; year++)
            {
                double jd0 = Date.JulianDay0(year);

                events.AddRange(
                    meteorsCalc.Meteors
                        // TODO: limit by setting
                        .Where(m => m.ActivityClass <= 3)
                        .Select(m => new AstroEvent(jd0 + func(m), text(m), noExactTime: true))
                        .Where(e => e.JulianDay >= context.From && e.JulianDay <= context.To));
            }

            return events;
        }
    }
}
