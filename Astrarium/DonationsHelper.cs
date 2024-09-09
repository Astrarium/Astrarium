using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.NetworkInformation;

namespace Astrarium
{
    public interface IDonationsHelper
    {
        void CheckDonates(Func<IDonationContext, DonationResult> onRequestDonation, Action<Exception> onError = null);
        void StopChecks();
    }

    public enum DonationResult
    {
        Delayed = 0,
        Donated = 1,
        Blocked = 2
    }

    public class DonationsHelper : IDonationsHelper
    {
        private readonly string settingsDir;
        private readonly string statsFile;

        public DonationsHelper()
        {
            settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium");
            statsFile = Path.Combine(settingsDir, "Stats.json");
        }

        public void CheckDonates(Func<IDonationContext, DonationResult> onRequestDonation, Action<Exception> onError = null)
        {
            EnsureCreated();
  
            // try to read file
            var stats = JsonConvert.DeserializeObject<DonationFileInfo>(File.ReadAllText(statsFile));

            // suppose donated or canceled by user
            if (stats.Stopped) return;

            // check at least 10 days passed and the app run 5 times
            if (DateTime.UtcNow.Subtract(stats.StartTime).TotalDays > 10 &&
                stats.RunsCount > 5 &&
                new Ping().Send("www.google.com").Status == IPStatus.Success)
            {
                DonationResult result = onRequestDonation(stats);
                if (result == DonationResult.Delayed)
                {
                    stats.StartTime = DateTime.Now;
                    stats.RunsCount = 0;
                    stats.Delayed = true;
                }
                else if (result == DonationResult.Blocked ||
                         result == DonationResult.Donated)
                {
                    stats.Stopped = true;
                }
            }
            
            stats.RunsCount++;
            WriteToFile(stats);
        }

        private void WriteToFile(IDonationContext stats)
        {
            try
            {
                File.SetAttributes(statsFile, FileAttributes.Normal);
                File.WriteAllText(statsFile, JsonConvert.SerializeObject(stats));
                File.SetAttributes(statsFile, FileAttributes.Hidden);
            }
            catch { }
        }

        private void EnsureCreated()
        {
            if (!File.Exists(statsFile))
            {
                Directory.CreateDirectory(settingsDir);

                WriteToFile(new DonationFileInfo()
                {
                    StartTime = DateTime.UtcNow,
                    RunsCount = 0
                });
            }
        }

        public void StopChecks()
        {
            EnsureCreated();
            WriteToFile(new ManualDonationFileInfo());
        }

        private class ManualDonationFileInfo : IDonationContext
        {
            [JsonIgnore]
            public bool Delayed => false;

            [JsonProperty("Stopped")]
            public bool Stopped => true;

            [JsonIgnore]
            public bool OpenedByUser => false;
        }

        private class DonationFileInfo : IDonationContext
        {
            [JsonProperty("StartTime")]
            public DateTime StartTime { get; set; }

            [JsonProperty("RunsCount")]
            public int RunsCount { get; set; }

            [JsonProperty("Delayed")]
            public bool Delayed { get; set; }

            [JsonProperty("Stopped")]
            public bool Stopped { get; set; }

            [JsonIgnore]
            public bool OpenedByUser => false;
        }
    }

    public interface IDonationContext
    {
        bool Delayed { get; }
        bool OpenedByUser { get; }
        bool Stopped { get; }
    }
}
