using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Astrarium.Plugins.Planner
{
    public interface IObservationPlanner
    {
        ICollection<Ephemerides> CreatePlan(PlanningFilter filter, CancellationToken? token = null, IProgress<double> progress = null);
        Ephemerides GetObservationDetails(PlanningFilter filter, CelestialObject body);
    }
}