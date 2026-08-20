using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Astrarium.Plugins.SolarSystem
{
    internal class SurfaceFeaturesReader
    {        
        public ICollection<SurfaceFeature> Read(string file)
        {
            List<SurfaceFeature> features = new List<SurfaceFeature>();            
            string line = "";
            using (var sr = new StreamReader(file, Encoding.UTF8))
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
        }
    }
}
