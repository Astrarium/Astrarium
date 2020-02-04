using ADK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    internal class LunarFeaturesReader
    {
        private readonly string FEATURES_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/LunarFeatures.dat");

        public ICollection<SurfaceFeature> Read()
        {
            List<SurfaceFeature> features = new List<SurfaceFeature>();
            
            string line = "";

            //CrdsGeographical center = new CrdsGeographical(0, 0);

            using (var sr = new StreamReader(FEATURES_FILE, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    string[] chunks = line.Split('\t');
                    string name = chunks[0].Trim();
                    double diam = double.Parse(chunks[1].Trim(), CultureInfo.InvariantCulture);
                    double lat = double.Parse(chunks[2].Trim(), CultureInfo.InvariantCulture);
                    double lon = double.Parse(chunks[3].Trim(), CultureInfo.InvariantCulture);
                    string type = chunks[4].Trim();
                    features.Add(new SurfaceFeature(name, type, lon, lat, diam));
                }
            }

            return features;

            //features = features.OrderByDescending(f => f.Diameter).Where(f => Angle.Separation(f.Coordinates, center) <= 98).ToList();

            //using (var sw = new StreamWriter("D:\\LunarFeatures.dat"))
            //{
            //    foreach (var feature in features)
            //    {
            //        sw.WriteLine($"{feature.Name}\t{feature.Diameter}\t{feature.Coordinates.Latitude}\t{feature.Coordinates.Longitude}\t{feature.TypeCode}");
            //    }
            //    sw.Flush();
            //    sw.Close();
            //}

            //return features;
        }
    }
}
