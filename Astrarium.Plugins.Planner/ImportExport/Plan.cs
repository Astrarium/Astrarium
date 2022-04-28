using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public abstract class PlanData
    {
        /// <summary>
        /// Date of observation (date only, no timezone)
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Begin of observation (time only)
        /// </summary>
        public TimeSpan? Begin { get; set; }

        /// <summary>
        /// End of observation (time only)
        /// </summary>
        public TimeSpan? End { get; set; }
    }

    public class PlanImportData : PlanData
    {
        /// <summary>
        /// Original plan file name, may be null
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Collection of celestial objects
        /// </summary>
        public ICollection<CelestialObject> Objects { get; set; } = new List<CelestialObject>();
    }

    public class PlanExportData : PlanData
    {
        public ICollection<Ephemerides> Ephemerides { get; set; }
    }
}
