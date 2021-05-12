using Astrarium.Algorithms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Novae
{
    public class NovaeReader
    {
        private class JsonNovaeList
        {
            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("nova")]
            public Dictionary<string, JsonNova> Novae { get; set; }
        }

        private class JsonNova
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("maxMagnitude")]
            public float MaxMagnitude { get; set; }

            [JsonProperty("minMagnitude")]
            public float MinMagnitude { get; set; }

            [JsonProperty("peakJD")]
            public double JulianDayPeak { get; set; }

            [JsonProperty("m2")]
            public int? M2 { get; set; }

            [JsonProperty("m3")]
            public int? M3 { get; set; }

            [JsonProperty("m6")]
            public int? M6 { get; set; }

            [JsonProperty("m9")]
            public int? M9 { get; set; }

            [JsonProperty("RA")]
            public string RA { get; set; }

            [JsonProperty("Dec")]
            public string Dec { get; set; }
        }

        public ICollection<Nova> Read(string filePath)
        {
            using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                var json = sr.ReadToEnd();

                var list = JsonConvert.DeserializeObject<JsonNovaeList>(json);

                return list.Novae.Select(i => new Nova() 
                { 
                    ProperName = i.Value.Name,
                    Name = i.Key,
                    JulianDayPeak = i.Value.JulianDayPeak,
                    MinMagnitude = i.Value.MinMagnitude,
                    MaxMagnitude = i.Value.MaxMagnitude,
                    NovaType = i.Value.Type,
                    M2 = i.Value.M2,
                    M3 = i.Value.M3,
                    M6 = i.Value.M6,
                    M9 = i.Value.M9,
                    Equatorial0 = new CrdsEquatorial(new HMS(i.Value.RA), new DMS(i.Value.Dec))
                })
                .ToArray();
            }
        }
    }
}
