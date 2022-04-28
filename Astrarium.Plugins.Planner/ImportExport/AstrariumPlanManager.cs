using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public class AstrariumPlanManager : IPlanManager
    {
        private class Plan
        {
            public DateTime Date { get; set; }
            public TimeSpan Begin { get; set; }
            public TimeSpan End { get; set; }
            public List<PlanItem> Objects { get; set; } = new List<PlanItem>();
        }

        private class PlanItem
        {
            public string Type { get; set; }
            public string Name { get; set; }

            public PlanItem() { }

            public PlanItem(CelestialObject celestialObject)
            {
                Type = celestialObject.Type;
                Name = celestialObject.CommonName;
            }
        }

        private readonly ISky sky = null;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public AstrariumPlanManager(ISky sky)
        {
            this.sky = sky;
            this.jsonSerializerSettings = new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd" };
        }

        public PlanImportData Read(string filePath, CancellationToken? token = null, IProgress<double> progress = null)
        {
            var bodies = new List<CelestialObject>();
            long counter = 0;

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            var plan = JsonConvert.DeserializeObject<Plan>(json, jsonSerializerSettings);
            double itemsCount = plan.Objects.Count;

            foreach (var item in plan.Objects)
            {
                if (token.HasValue && token.Value.IsCancellationRequested)
                {
                    bodies.Clear();
                    break;
                }

                progress?.Report(++counter / itemsCount * 100);

                var body = sky.CelestialObjects.FirstOrDefault(x => x.Type != null && x.Type == item.Type && x.CommonName == item.Name);

                if (body != null)
                {
                    bodies.Add(body);
                }
                else
                {
                    Log.Debug($"{GetType().Name}: unable to identify celestial object (Name={item.Name},Type={item.Type})");
                }
            }

            return new PlanImportData()
            {
                FilePath = filePath,
                Date = plan.Date,
                Begin = plan.Begin,
                End = plan.End,
                Objects = bodies
            };
        }

        public void Write(PlanExportData data, string filePath)
        {
            var plan = new Plan();
            plan.Date = DateTime.SpecifyKind(data.Date.Value.Date, DateTimeKind.Unspecified);
            plan.Begin = data.Begin.Value;
            plan.End = data.End.Value;
            plan.Objects = data.Ephemerides.Select(x => new PlanItem(x.CelestialObject)).ToList();
            File.WriteAllText(filePath, JsonConvert.SerializeObject(plan, Formatting.Indented, jsonSerializerSettings), Encoding.UTF8);
        }
    }
}
