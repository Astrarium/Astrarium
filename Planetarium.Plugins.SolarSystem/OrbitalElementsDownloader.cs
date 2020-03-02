using Newtonsoft.Json;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    public class OrbitalElementsDownloader
    {
        public List<GenericMoonData> Download()
        {
            List<GenericMoonData> orbits = new List<GenericMoonData>();

            using (StreamReader file = File.OpenText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/SatellitesOrbits.json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                orbits = (List<GenericMoonData>)serializer.Deserialize(file, typeof(List<GenericMoonData>));
            }

            string startDate = DateTime.Now.ToString("yyyy-MM-dd");
            string endDate = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            foreach (var orbit in orbits)
            {
                WebRequest request = WebRequest.Create($"https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1&COMMAND='{(orbit.planet * 100 + orbit.satellite)}'&CENTER='500@{(orbit.planet * 100 + 99)}'&MAKE_EPHEM='YES'&TABLE_TYPE='ELEMENTS'&START_TIME='{startDate}'&STOP_TIME='{endDate}'&STEP_SIZE='2 d'&OUT_UNITS='AU-D'&REF_PLANE='ECLIPTIC'&REF_SYSTEM='J2000'&TP_TYPE='ABSOLUTE'&CSV_FORMAT='YES'&OBJ_DATA='YES'");
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var receiveStream = response.GetResponseStream())
                using (var reader = new StreamReader(receiveStream))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        ParseOrbit(orbit, reader.ReadToEnd());
                    }
                }
            }

            using (StreamWriter file = File.CreateText(@"Data\SatellitesOrbits.dat"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, orbits);
            }

            return orbits;
        }

        static void ParseOrbit(GenericMoonData orbit, string response)
        {
            List<string> lines = response.Split('\n').ToList();
            string soeMarker = lines.FirstOrDefault(ln => ln == "$$SOE");
            int soeMarkerIndex = lines.IndexOf(soeMarker);
            string header = lines[soeMarkerIndex - 2];
            string orbitLine = lines[soeMarkerIndex + 1];

            List<string> headerItems = header.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();
            List<string> orbitItems = orbitLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();

            orbit.jd0 = double.Parse(orbitItems[headerItems.IndexOf("JDTDB")], CultureInfo.InvariantCulture);
            orbit.e = double.Parse(orbitItems[headerItems.IndexOf("EC")], CultureInfo.InvariantCulture);
            orbit.i = double.Parse(orbitItems[headerItems.IndexOf("IN")], CultureInfo.InvariantCulture);
            orbit.node0 = double.Parse(orbitItems[headerItems.IndexOf("OM")], CultureInfo.InvariantCulture);
            orbit.omega0 = double.Parse(orbitItems[headerItems.IndexOf("W")], CultureInfo.InvariantCulture);
            orbit.n = double.Parse(orbitItems[headerItems.IndexOf("N")], CultureInfo.InvariantCulture);
            orbit.M0 = double.Parse(orbitItems[headerItems.IndexOf("MA")], CultureInfo.InvariantCulture);
            orbit.a = double.Parse(orbitItems[headerItems.IndexOf("A")], CultureInfo.InvariantCulture);

            string magLine = lines.FirstOrDefault(ln => ln.Contains("V(1,0)"));
            if (!string.IsNullOrEmpty(magLine))
            {
                magLine = magLine.Substring(magLine.IndexOf("V(1,0)"));
                List<string> magItems = magLine.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();
                string mag = magItems.ElementAt(1).Split(' ').First();
                orbit.mag = double.Parse(mag, CultureInfo.InvariantCulture);
            }

            string radiusLine = lines.FirstOrDefault(ln => ln.Contains("Radius"));
            if (!string.IsNullOrEmpty(radiusLine))
            {
                radiusLine = radiusLine.Substring(radiusLine.IndexOf("Radius"));
                List<string> radiusItems = radiusLine.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();
                string radius = radiusItems.ElementAt(1).Split(' ').First().Split('x').First();
                orbit.radius = double.Parse(radius, CultureInfo.InvariantCulture);
            }
        }
    }
}
