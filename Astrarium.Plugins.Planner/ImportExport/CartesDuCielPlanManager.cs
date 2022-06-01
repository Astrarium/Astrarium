using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public class CartesDuCielPlanManager : IPlanManager
    {
        private readonly ISky sky = null;

        public CartesDuCielPlanManager(ISky sky)
        {
            this.sky = sky;
        }

        private bool CompareStringsIgnoreCaseAndSpaces(string s1, string s2)
        {
            return string.Compare(s1, s2, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
        }

        public PlanImportData Read(string filePath, CancellationToken? token = null, IProgress<double> progress = null)
        {
            var planData = new PlanImportData() { FilePath = filePath };

            using (StreamReader file = File.OpenText(filePath))
            {
                // TODO: try parse header data and extract date/time
                string line = file.ReadLine();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    Regex regex = new Regex(@".*Date\s*=\s*(\d{4}\-\d{2}\-\d{2})\s*,\s*From\s*=(\s*\d{2}:\d{2}:\d{2})\s*,\s*To\s*=(\s*\d{2}:\d{2}:\d{2}).*");
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        DateTime date;
                        if (DateTime.TryParseExact(match.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        {
                            planData.Date = date;
                        }

                        TimeSpan begin;
                        if (TimeSpan.TryParseExact(match.Groups[2].Value, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out begin))
                        {
                            planData.Begin = begin;
                        }

                        TimeSpan end;
                        if (TimeSpan.TryParseExact(match.Groups[3].Value, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out end))
                        {
                            planData.End = end;
                        }
                    }
                }

                // Get total lines count. This method lazily enumerates lines rather than greedily reading them all.
                double linesCount = File.ReadLines(filePath).Count();
                long counter = 0;

                //Func<string, string, bool> CompareString = (string s1, string s2) =>
                //{

                //};

                while ((line = file.ReadLine()) != null)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        planData.Objects.Clear();
                        break;
                    }

                    progress?.Report(++counter / linesCount * 100);

                    string name = line.Substring(0, 32).Trim();
                    string exactName = line.Substring(52, Math.Max(line.Length, 32) - 52).Trim();

                    var body = sky.CelestialObjects.Where(x => x.Type != null)
                        .FirstOrDefault(x =>
                            CompareStringsIgnoreCaseAndSpaces(x.CommonName, exactName) ||
                            CompareStringsIgnoreCaseAndSpaces(x.CommonName, name) ||
                            x.Names.Any(n => CompareStringsIgnoreCaseAndSpaces(exactName, n)) ||
                            x.Names.Any(n => CompareStringsIgnoreCaseAndSpaces(name, n)));

                    if (body != null && !planData.Objects.Any(x => x.Type == body.Type && x.CommonName == body.CommonName))
                    {
                        planData.Objects.Add(body);
                    }
                    else
                    {
                        Log.Debug($"{GetType().Name}: unable to identify celestial object (Name={name},ExactName={exactName})");
                    }
                }
            }

            return planData;
        }

        public void Write(PlanExportData data, string filePath)
        {
            using (var file = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                file.WriteLine($"Date={data.Date.Value:yyyy-MM-dd}, From={data.Begin.Value}, To={data.End.Value}");
                foreach (var item in data.Ephemerides)
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
