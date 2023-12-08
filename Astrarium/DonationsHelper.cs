using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace Astrarium
{
    public interface IDonationsHelper
    {
        void CheckDonates(Func<DonationResult> onRequestDonation, Action<Exception> onError = null);
    }

    public enum DonationResult
    {
        Delayed = 0,
        Donated = 1
    }

    public class DonationsHelper : IDonationsHelper
    {
        public void CheckDonates(Func<DonationResult> onRequestDonation, Action<Exception> onError = null)
        {
            string settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium");
            string statsFile = Path.Combine(settingsDir, "Stats.json");

            if (!File.Exists(statsFile))
            {
                Directory.CreateDirectory(settingsDir);
                RunStats stats = new RunStats()
                {
                    FirstRun = DateTime.UtcNow,
                    RunsCount = 1
                };
                File.WriteAllText(statsFile, JsonConvert.SerializeObject(stats));
                File.SetAttributes(statsFile, FileAttributes.Hidden);
            }
            else
            {
                // try to read file
                RunStats stats = JsonConvert.DeserializeObject<RunStats>(File.ReadAllText(statsFile));

                // suppose donated
                if (stats.DialogShown) return;

                if (DateTime.UtcNow.Subtract(stats.FirstRun).TotalDays > 10 && 
                    stats.RunsCount > 5 &&
                    new Ping().Send("www.google.com").Status == IPStatus.Success)
                {
                    DonationResult result = onRequestDonation();
                    if (result == DonationResult.Delayed)
                    {
                        // TODO: delay logic
                    }
                    else if (result == DonationResult.Donated)
                    {
                        // TODO: donated logic
                    }
                }

                stats.RunsCount++;
                File.WriteAllText(statsFile, JsonConvert.SerializeObject(stats));
            }
        }

        private class RunStats
        {
            public DateTime FirstRun { get; set; }
            public int RunsCount { get; set; }
            public bool DialogShown { get; set; }
        }
    }
}
