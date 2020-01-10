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
    }
}
