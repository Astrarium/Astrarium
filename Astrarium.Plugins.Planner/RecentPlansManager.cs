using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner
{
    // TODO: singleton via interface
    public static class RecentPlansManager
    {
        private static List<string> recentPlansList = new List<string>(10);
        private static string plannerDirectory;
        private static string recentPlansFile;

        public static event Action RecentPlansListChanged;

        public static void LoadRecentPlansList()
        {
            try
            {
                plannerDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Planner");
                Directory.CreateDirectory(plannerDirectory);
                recentPlansFile = Path.Combine(plannerDirectory, ".recent");
                if (File.Exists(recentPlansFile))
                {
                    recentPlansList.AddRange(File.ReadAllLines(recentPlansFile));
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

        public static void AddToRecentList(string filePath)
        {
            int existingIndex = recentPlansList.FindIndex(f => f.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                recentPlansList.RemoveAt(existingIndex);
            }
            recentPlansList.Insert(0, filePath);
            recentPlansList.TrimExcess();

            try
            {
                File.WriteAllLines(recentPlansFile, recentPlansList, Encoding.UTF8);
                //File.SetAttributes(recentPlansFile, File.GetAttributes(recentPlansFile) | FileAttributes.Hidden);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to save recent plans list. Error: {ex}");
            }

            RecentPlansListChanged?.Invoke();
        }

        public static List<string> RecentList
        {
            get => new List<string>(recentPlansList);
        }
    }
}
