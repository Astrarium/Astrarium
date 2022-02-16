using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public class SkySafariPlan : IPlan
    {
        private const string FILE_HEADER = "SkySafariObservingListVersion=3.0";
        private const string BEGIN_OBJECT = "SkyObject=BeginObject";
        private const string END_OBJECT = "EndObject=SkyObject";

        private readonly ISky sky = null;
        private readonly IgnoreSpaceStringComparer comparer = new IgnoreSpaceStringComparer();

        public SkySafariPlan(ISky sky)
        {
            this.sky = sky;
        }

        private class IgnoreSpaceStringComparer : StringComparer
        {
            public override int Compare(string x, string y)
            {
                return string.Compare(x, y, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols);
            }

            public override bool Equals(string x, string y)
            {
                return Compare(x, y) == 0;
            }

            public override int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }

        public ICollection<CelestialObject> Read(string filePath, CancellationToken? token = null, IProgress<double> progress = null)
        {
            var bodies = new List<CelestialObject>();

            using (StreamReader file = File.OpenText(filePath))
            {
                string line = file.ReadLine();
                if (line != FILE_HEADER)
                {
                    throw new FileFormatException($"Incorrect format of the file. Expected {FILE_HEADER}");
                }

                // Get total lines count. This method lazily enumerates lines rather than greedily reading them all.
                double linesCount = File.ReadLines(filePath).Count();
                long counter = 0;

                List<string> catalogNumbers = new List<string>();
                List<string> commonNames = new List<string>();
                string skySafariType = null;

                while ((line = file.ReadLine()) != null)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        bodies.Clear();
                        break;
                    }

                    progress?.Report(counter++ / linesCount * 100);

                    line = line.Trim();
                    if (line == BEGIN_OBJECT)
                    {
                        catalogNumbers.Clear();
                        commonNames.Clear();
                        skySafariType = null;
                    }
                    else if (line.StartsWith("ObjectID=1,"))
                    {
                        skySafariType = "SolarSystem";
                    }
                    else if (line.StartsWith("ObjectID=2,"))
                    {
                        skySafariType = "Star";
                    }
                    else if (line.StartsWith("ObjectID=4,"))
                    {
                        skySafariType = "DeepSky";
                    }
                    else if (line.StartsWith("CatalogNumber="))
                    {
                        catalogNumbers.Add(line.Substring("CatalogNumber=".Length));
                    }
                    else if (line.StartsWith("CommonName="))
                    {
                        commonNames.Add(line.Substring("CommonName=".Length));
                    }
                    else if (line == END_OBJECT)
                    {
                        CelestialObject body = null;
                        if (skySafariType == "SolarSystem")
                        {
                            body = sky.CelestialObjects.FirstOrDefault(
                                x =>
                                (x.Type == "Sun" || x.Type == "Moon" || x.Type == "Planet" || x.Type == "PlanetMoon") && commonNames.Any(n => n.Equals(x.CommonName, StringComparison.OrdinalIgnoreCase)) ||
                                (x.Type == "Asteroid" && catalogNumbers.Count == 1 && commonNames.Count == 1 && x.CommonName.Equals($"({catalogNumbers[0]}) {commonNames[0]}", StringComparison.OrdinalIgnoreCase)) ||
                                (x.Type == "Comet" && commonNames.Count == 2 && x.CommonName.Equals($"{commonNames[1]} {commonNames[0]}", StringComparison.OrdinalIgnoreCase)) ||
                                (x.Type == "Comet" && commonNames.Count == 2 && x.CommonName.Equals($"{commonNames[1]} ({commonNames[0]})", StringComparison.OrdinalIgnoreCase)) ||
                                (x.Type == "Comet" && commonNames.Count == 1 && x.CommonName.Equals(commonNames[0], StringComparison.OrdinalIgnoreCase))
                            );
                        }
                        else if (skySafariType == "Star")
                        {
                            body = sky.CelestialObjects.FirstOrDefault(
                                x => x.Type == "Star" &&
                                catalogNumbers.Any(n => string.Compare(x.CommonName, n, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                            );
                        }
                        else if (skySafariType == "DeepSky")
                        {
                            body = sky.CelestialObjects.FirstOrDefault(
                                x => x.Type != null && 
                                x.Type.StartsWith("DeepSky") &&
                                (catalogNumbers.Any(n => string.Compare(x.CommonName, n, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0) ||
                                 x.Names.Intersect(catalogNumbers, comparer).Any())
                            );
                        }

                        if (body != null)
                        {
                            bodies.Add(body);
                        }
                        else
                        {
                            Log.Debug($"{GetType().Name}: unable to identify celestial object (CommonNames=[{string.Join(",", commonNames)}],CatalogNumbers=[{string.Join(",", catalogNumbers)}],Type={skySafariType})");
                        }
                    }
                }
            }

            return bodies;
        }

        public void Write(ICollection<Ephemerides> plan, string filePath)
        {
            using (var file = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                file.WriteLine(FILE_HEADER);
                foreach (var item in plan)
                {
                    file.WriteLine(BEGIN_OBJECT);

                    CelestialObject body = item.CelestialObject;
                    string type = item.CelestialObject.Type;
                    if (type.StartsWith("DeepSky."))
                    {
                        type = "DeepSky";
                    }

                    switch (type)
                    {
                        case "Star":
                            {
                                file.WriteLine($"\tObjectID=2,-1,-1");
                                foreach (string name in body.Names)
                                {
                                    file.WriteLine($"\tCatalogNumber={name}");
                                }
                                break;
                            }

                        case "Sun":
                        case "Moon":
                        case "Planet":
                        case "PlanetMoon":
                            {
                                file.WriteLine($"\tObjectID=1,-1,-1");
                                file.WriteLine($"\tCommonName={body.CommonName}");
                                break;
                            }

                        case "Asteroid":
                            {
                                file.WriteLine($"\tObjectID=1,-1,-1");
                                Regex pattern = new Regex(@"\((?<number>\d+)\)\s*(?<name>.+)");
                                Match match = pattern.Match(body.CommonName);
                                string number = match.Groups["number"].Value;
                                string name = match.Groups["name"].Value;
                                if (match.Success)
                                {
                                    file.WriteLine($"\tCommonName={name}");
                                    file.WriteLine($"\tCatalogNumber={number}");
                                }
                                else
                                {
                                    file.WriteLine($"\tCommonName={body.CommonName}");
                                }
                                break;
                            }

                        case "Comet":
                            {
                                file.WriteLine($"\tObjectID=1,-1,-1");
                                Regex pattern = new Regex(@"(?<name2>.+)\s*\((?<name1>\w+\s*)\)");
                                Match match = pattern.Match(body.CommonName);
                                string name1 = match.Groups["name1"].Value.Trim();
                                string name2 = match.Groups["name2"].Value.Trim();
                                if (match.Success)
                                {
                                    file.WriteLine($"\tCommonName={name1}");
                                    file.WriteLine($"\tCommonName={name2}");
                                }
                                else
                                {
                                    file.WriteLine($"\tCommonName={body.CommonName}");
                                }
                                break;
                            }

                        case "DeepSky":
                            {
                                file.WriteLine($"\tObjectID=4,-1,-1");
                                foreach (string name in body.Names)
                                {
                                    file.WriteLine($"\tCatalogNumber={name}");
                                }
                                break;
                            }
                        default:
                            // UNKNOWN TYPE, SKIP
                            break;
                    }

                    file.WriteLine(END_OBJECT);
                }
            }
        }
    }
}
