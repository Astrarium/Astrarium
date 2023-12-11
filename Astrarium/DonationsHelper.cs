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
        Donated = 1,
        Blocked = 2
    }

    public class DonationsHelper : IDonationsHelper
    {
        public void CheckDonates(Func<DonationResult> onRequestDonation, Action<Exception> onError = null)
        {
            string settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium");
            string statsFile = Path.Combine(settingsDir, "Stats.json");

            RunStats stats;

            if (!File.Exists(statsFile))
            {
                Directory.CreateDirectory(settingsDir);
                stats = new RunStats()
                {
                    StartTime = DateTime.UtcNow,
                    RunsCount = 0
                };
            }
            else
            {
                // try to read file
                stats = JsonConvert.DeserializeObject<RunStats>(File.ReadAllText(statsFile));

                // suppose donated
                if (stats.Stopped) return;

                if (DateTime.UtcNow.Subtract(stats.StartTime).TotalDays > 10 &&
                    stats.RunsCount > 5 &&
                    new Ping().Send("www.google.com").Status == IPStatus.Success)
                {
                    DonationResult result = onRequestDonation();
                    if (result == DonationResult.Delayed)
                    {
                        stats.StartTime = DateTime.Now;
                        stats.RunsCount = 0;
                    }
                    else if (result == DonationResult.Donated)
                    {
                        stats.Stopped = true;
                    }
                }
            }

            stats.RunsCount++;
            try
            {
                File.SetAttributes(statsFile, FileAttributes.Normal);
                File.WriteAllText(statsFile, JsonConvert.SerializeObject(stats));
                File.SetAttributes(statsFile, FileAttributes.Hidden);
            }
            catch (Exception ex)
            {

            }
        }

        private class RunStats
        {
            public DateTime StartTime { get; set; }
            public int RunsCount { get; set; }
            public bool Stopped { get; set; }
        }
    }
}
