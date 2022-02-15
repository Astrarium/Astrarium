using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public class CartesDuCielPlan : IPlan
    {
        private readonly ISky sky = null;

        public CartesDuCielPlan(ISky sky)
        {
            this.sky = sky;
        }

        public ICollection<CelestialObject> Read(string filePath, CancellationToken? token = null, IProgress<double> progress = null)
        {
            var bodies = new List<CelestialObject>();

            using (StreamReader file = File.OpenText(filePath))
            {
                string line = file.ReadLine();

                // Get total lines count. This method lazily enumerates lines rather than greedily reading them all.
                double linesCount = File.ReadLines(filePath).Count();
                long counter = 0;

                while ((line = file.ReadLine()) != null)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        bodies.Clear();
                        break;
                    }

                    progress?.Report(++counter / linesCount * 100);

                    // skip the file header (first line)
                    if (counter == 1)
                    {
                        continue;
                    }

                    string name = line.Substring(0, 32).Trim();
                    string exactName = line.Substring(52, Math.Max(line.Length, 32) - 52).Trim();

                    var body = sky.CelestialObjects.FirstOrDefault(x => x.Type != null &&
                        ((string.Compare(x.CommonName, exactName, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0) ||
                         x.Names.Any(n => string.Compare(exactName, n, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)));

                    if (body != null)
                    {
                        bodies.Add(body);
                    }
                    else
                    {
                        Debug.WriteLine("");
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
