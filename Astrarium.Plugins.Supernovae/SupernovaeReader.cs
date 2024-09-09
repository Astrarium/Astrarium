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

namespace Astrarium.Plugins.Supernovae
{
    public class SupernovaeReader
    {
        private class JsonSupernovaeList
        {
            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("supernova")]
            public Dictionary<string, JsonSupernova> Supernovae { get; set; }
        }

        private class JsonSupernova
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("maxMagnitude")]
            public float MaxMagnitude { get; set; }

            [JsonProperty("peakJD")]
            public double JulianDayPeak { get; set; }

            [JsonProperty("alpha")]
            public string RA { get; set; }

            [JsonProperty("delta")]
            public string Dec { get; set; }
        }

        public ICollection<Supernova> Read(string filePath)
        {
            using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                var json = sr.ReadToEnd();

                var list = JsonConvert.DeserializeObject<JsonSupernovaeList>(json);

                return list.Supernovae.Select(i => new Supernova() 
                { 
                    Name = $"SN {i.Key}",
                    JulianDayPeak = i.Value.JulianDayPeak,
                    MaxMagnitude = i.Value.MaxMagnitude,
                    SupernovaType = i.Value.Type,
                    Equatorial0 = new CrdsEquatorial(new HMS(i.Value.RA), new DMS(i.Value.Dec))
                })
                .ToArray();
            }
        }
    }
}
