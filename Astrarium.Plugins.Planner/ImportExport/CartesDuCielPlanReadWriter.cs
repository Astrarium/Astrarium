using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public class CartesDuCielPlanReadWriter : IPlanReadWriter
    {
        private readonly ISky sky = null;

        public CartesDuCielPlanReadWriter(ISky sky)
        {
            this.sky = sky;
        }

        public ICollection<CelestialObject> Read(string filePath, CancellationToken? token = null, IProgress<double> progress = null)
        {
            var bodies = new List<CelestialObject>();

            using (StreamReader file = File.OpenText(filePath))
            {
                string line = file.ReadLine();

                bool firstLine = true;

                // Get total lines count. This method lazily enumerates lines rather than greedily reading them all.
                double linesCount = File.ReadLines(filePath).Count();
                long counter = 1;

                while ((line = file.ReadLine()) != null)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        bodies.Clear();
                        break;
                    }

                    progress?.Report(counter++ / linesCount * 100);

                    // skip the file header (1 line)
                    if (firstLine)
                    {
                        firstLine = false;
                        continue;
                    }

                    string name = line.Substring(0, 32).Trim();
                    //string ra = line.Substring(32, 10).Trim();
                    //string dec = line.Substring(42, 10).Trim();
                    string exactName = line.Substring(52, Math.Max(line.Length, 32) - 52).Trim();

                    var body = sky.CelestialObjects.Where(x => x.Type != null && x.CommonName.Equals(exactName) || x.CommonName.Replace(" ", "").Equals(exactName) ||
                                                                                 x.Names.Select(n => n.Replace(" ", "")).Contains(exactName) || x.Names.Contains(exactName) || 
                                                                                 x.Names.Select(n => n.Replace(" ", "")).Contains(name) || x.Names.Contains(name)).FirstOrDefault();
                    if (body != null)
                    {
                        bodies.Add(body);
                    }
                    else
                    {
                        // TODO: log it.
                    }
                }
            }

            return bodies;
        }

        public void Write(ICollection<Ephemerides> plan, string filePath)
        {
            using (var file = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                file.WriteLine("Observation plan. Created with Astraruim https://astrarium.space");
                foreach (var item in plan)
                {
                    CelestialObject body = item.CelestialObject;
                    string name = body.CommonName.PadRight(32);
                    double ra = item.GetValue<double>("Equatorial.Alpha");
                    double dec = item.GetValue<double>("Equatorial.Delta");
                    if (double.IsNaN(ra)) ra = 0;
                    if (double.IsNaN(dec)) dec = 0;
                    string strRa = ra.ToString("0.00000", CultureInfo.InvariantCulture).PadRight(10);
                    string strDec = dec.ToString("0.00000", CultureInfo.InvariantCulture).PadRight(10);
                    file.WriteLine($"{name}{strRa}{strDec}{name}");
                }
            }
        }
    }
}
