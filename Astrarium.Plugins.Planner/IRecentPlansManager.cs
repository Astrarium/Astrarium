using Astrarium.Plugins.Planner.ImportExport;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Astrarium.Plugins.Planner
{
    public interface IRecentPlansManager
    {
        event Action RecentPlansListChanged;
        List<RecentPlan> RecentList { get; }
        void AddToRecentList(RecentPlan recentPlan);
        void RemoveFromRecentList(RecentPlan recentPlan);
        void LoadRecentPlansList();
        void ClearRecentPlansList();
    }

    /// <summary>
    /// Represents a single record in recent plans list
    /// </summary>
    public class RecentPlan
    {
        /// <summary>
        /// Type of the plan
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanType Type { get; set; }

        /// <summary>
        /// File path to the plan
        /// </summary>
        public string Path { get; set; }
    }
}