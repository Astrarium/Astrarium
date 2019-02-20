using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class PlanetsEventsProvider : BaseAstroEventsProvider
    {
        private readonly IPlanetsCalc planetsCalc;

        /// <summary>
        /// Average daily motions of planets
        /// </summary>
        public static readonly double[] DAILY_MOTIONS = new[] { 1.3833, 1.2, 0, 0.542, 0.0831, 0.0336, 0.026666, 0.006668 };

        public static readonly double[] SINODAL_PERIODS = new[] { 115.88, 583.92, 0, 779.94, 398.88, 378.09, 369.66, 367.49 };

        public PlanetsEventsProvider(IPlanetsCalc planetsCalc)
        {
            this.planetsCalc = planetsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig config)
        {
            
        }
    }
}
