using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner
{
    [Singleton(typeof(IRecentPlansManager))]
    public class RecentPlansManager : IRecentPlansManager
    {
        private List<RecentPlan> recentPlansList = new List<RecentPlan>(10);
        private string plannerDirectory;
        private string recentPlansFile;

        public event Action RecentPlansListChanged;

        public void LoadRecentPlansList()
        {
            try
            {
                plannerDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Planner");
                Directory.CreateDirectory(plannerDirectory);
                recentPlansFile = Path.Combine(plannerDirectory, "RecentPlans.json");
                if (File.Exists(recentPlansFile))
                {
                    string json = File.ReadAllText(recentPlansFile);
                    recentPlansList.Clear();
                    recentPlansList.AddRange(JsonConvert.DeserializeObject<List<RecentPlan>>(json));
                    Log.Debug($"Loaded {recentPlansList.Count} items from recent plans list.");
                }
                else
                {
                    Log.Debug("Recent plans file is missing, skip.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to load recent plans list. Error: {ex}");
            }
        }

        public void AddToRecentList(RecentPlan recentPlan)
        {
            int existingIndex = recentPlansList.FindIndex(x => x.Path.Equals(recentPlan.Path, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                recentPlansList.RemoveAt(existingIndex);
            }
            recentPlansList.Insert(0, recentPlan);
            recentPlansList.TrimExcess();

            SaveFile();
        }

        private void SaveFile()
        {
            try
            {
                string json = JsonConvert.SerializeObject(recentPlansList);
                File.WriteAllText(recentPlansFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to save recent plans to file. Error: {ex}");
            }

            RecentPlansListChanged?.Invoke();
        }

        public void RemoveFromRecentList(RecentPlan recentPlan)
        {
            int existingIndex = recentPlansList.FindIndex(x => x.Path.Equals(recentPlan.Path, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                recentPlansList.RemoveAt(existingIndex);
                SaveFile();
            }
        }

        public void ClearRecentPlansList()
        {
            recentPlansList.Clear();
            try
            {
                File.Delete(recentPlansFile);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to delete recent plans file. Error: {ex}");
            }
            RecentPlansListChanged?.Invoke();
        }

        public List<RecentPlan> RecentList
        {
            get => new List<RecentPlan>(recentPlansList);
        }
    }
}
