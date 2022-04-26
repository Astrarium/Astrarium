using System;
using System.Collections.Generic;

namespace Astrarium.Plugins.Planner
{
    public interface IRecentPlansManager
    {
        List<string> RecentList { get; }

        event Action RecentPlansListChanged;

        void AddToRecentList(string filePath);
        void LoadRecentPlansList();
    }
}