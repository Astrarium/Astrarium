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
    public class AstrariumPlan : IPlan
    {
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

        public AstrariumPlan(ISky sky)
        {
            this.sky = sky;
        }

        public ICollection<CelestialObject> Read(string filePath, CancellationToken? token = null, IProgress<double> progress = null)
        {
            var bodies = new List<CelestialObject>();
            long counter = 0;

            PlanItem[] plan = new PlanItem[0];
            double itemsCount = 0;

            using (StreamReader file = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                plan = (PlanItem[])serializer.Deserialize(file, typeof(PlanItem[]));
                itemsCount = plan.Count();
            }

            foreach (var item in plan)
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

            return bodies;
        }

        public void Write(ICollection<Ephemerides> plan, string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(plan.Select(x => new PlanItem(x.CelestialObject))), Encoding.UTF8);
        }
    }
}
