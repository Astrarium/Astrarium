using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// Represents the Solar Region Summary
    /// See http://www.swpc.noaa.gov/ftpdir/forecasts/SRS/README for more details.
    /// </summary>
    public class SolarRegionSummary
    {
        public List<SolarRegionI> RegionsI { get; private set; } = new List<SolarRegionI>();
        public List<SolarRegionIa> RegionsIa { get; private set; } = new List<SolarRegionIa>();
        public List<SolarRegionII> RegionsII { get; private set; } = new List<SolarRegionII>();

        public SolarRegionSummary(string file)
        {
            StreamReader sr = new StreamReader(file);
            string line;
            int section = 0;
            Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
            
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine().Trim();
                line = regex.Replace(line, " ");
                if (line.Length == 0) continue;
                if (line.ToLower().StartsWith("none") ||
                    line.StartsWith("#") ||
                    line.StartsWith(":")) continue;

                if (line.StartsWith("I."))
                {
                    // read header
                    sr.ReadLine();
                    section = 1;
                    continue;
                }
                if (line.StartsWith("IA."))
                {
                    // read header
                    sr.ReadLine();
                    section = 2;
                    continue;
                }
                if (line.StartsWith("II."))
                {
                    // read header
                    sr.ReadLine();
                    section = 3;
                    continue;
                }

                if (section == 1)
                {
                    string[] data = line.Split(' ');
                    SolarRegionI region = new SolarRegionI();
                    region.Nmbr = Convert.ToInt32(data[0].Trim());
                    region.Location.Latitude = Convert.ToInt32(data[1].Substring(1, 2)) * (data[1][0] == 'S' ? -1 : 1);
                    region.Location.Longitude = Convert.ToInt32(data[1].Substring(4, 2)) * (data[1][3] == 'W' ? -1 : 1);
                    region.Lo = Convert.ToInt32(data[2].Trim());
                    region.Area = Convert.ToInt32(data[3].Trim());
                    region.Z = data[4].Trim();
                    region.LL = Convert.ToInt32(data[5].Trim());
                    region.NN = Convert.ToInt32(data[6].Trim());
                    region.MagType = data[7].Trim().ToLower();
                    RegionsI.Add(region);
                }
                if (section == 2)
                {
                    string[] data = line.Split(' ');
                    SolarRegionIa region = new SolarRegionIa();
                    region.Nmbr = Convert.ToInt32(data[0].Trim());
                    region.Location.Latitude = Convert.ToInt32(data[1].Substring(1, 2)) * (data[1][0] == 'S' ? -1 : 1);
                    region.Location.Longitude = Convert.ToInt32(data[1].Substring(4, 2)) * (data[1][3] == 'W' ? -1 : 1);
                    region.Lo = Convert.ToInt32(data[2].Trim());
                    RegionsIa.Add(region);
                }
                if (section == 3)
                {
                    string[] data = line.Split(' ');
                    SolarRegionII region = new SolarRegionII();
                    region.Nmbr = Convert.ToInt32(data[0].Trim());
                    region.Lat = Convert.ToInt32(data[1].Substring(1, 2)) * (data[1][0] == 'S' ? -1 : 1);
                    region.Lo = Convert.ToInt32(data[2].Trim());
                    RegionsII.Add(region);
                }
            }

            if (section != 3)
            {
                throw new Exception("Error on parsing SRS file.");
            }
        }

        /// <summary>
        /// Gets current Wolf number
        /// </summary>
        public int WolfNumber => 10 * RegionsI.Count + RegionsI.Sum(x => x.NN);
    }
}
