using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public class SkySafariPlanReadWriter : IPlanReadWriter
    {
        private const string FILE_HEADER = "SkySafariObservingListVersion=3.0";
        private const string BEGIN_OBJECT = "SkyObject=BeginObject";
        private const string END_OBJECT = "EndObject=SkyObject";

        private readonly ISky sky = null;

        public SkySafariPlanReadWriter(ISky sky)
        {
            this.sky = sky;
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
                        if (skySafariType == "DeepSky")
                        {
                            var body = sky.CelestialObjects.Where(x => x.Type != null && x.Type.StartsWith("DeepSky") && catalogNumbers.Any(cn => x.CommonName.Equals(cn))).FirstOrDefault();
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
                                    file.WriteLine($"\tCatalogNumber={number}");
                                    file.WriteLine($"\tCatalogNumber={name}");
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
