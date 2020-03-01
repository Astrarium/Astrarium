using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalElementsDownloader
{
    public class SatelliteOrbit
    {
        /// <summary>
        /// Orbital elements epoch
        /// </summary>
        public double jd0 { get; set; }

        /// <summary>
        /// Mean anomaly at epoch, degrees
        /// </summary>
        public double M0 { get; set; }

        /// <summary>
        /// Mean motion, degrees/day  
        /// </summary>
        public double n { get; set; }

        /// <summary>
        /// Eccentricity
        /// </summary>
        public double e { get; set; }

        /// <summary>
        /// Semi-major axis, au
        /// </summary>
        public double a { get; set; }

        /// <summary>
        /// Inclination w.r.t XY-plane, degrees
        /// </summary>
        public double i { get; set; }

        /// <summary>
        /// Argument of perifocus, degrees
        /// </summary>
        public double omega0 { get; set; }

        /// <summary>
        /// Longitude of Ascending Node, degrees
        /// </summary>
        public double node0 { get; set; }

        /// <summary>
        /// Argument of periapsis precession period (mean value), years
        /// From https://ssd.jpl.nasa.gov/?sat_elem
        /// </summary>
        public double Pw { get; set; }

        /// <summary>
        /// Longitude of the ascending node precession period (mean value), years
        /// From https://ssd.jpl.nasa.gov/?sat_elem
        /// </summary>
        public double Pnode { get; set; }
    }

    public class SatelliteData : SatelliteOrbit
    {
        /// <summary>
        /// Satellite number, 1-based 
        /// </summary>
        public int satellite { get; set; }

        /// <summary>
        /// Planet number, 1-based
        /// </summary>
        public int planet { get; set; }

        /// <summary>
        /// Satellite names, key is language code, value is localized name
        /// </summary>
        public Dictionary<string, string> names { get; set; }

        /// <summary>
        /// Absolute magnitude
        /// </summary>
        public double mag { get; set; }

        /// <summary>
        /// Mean radius of satellite, in km
        /// </summary>
        public double radius { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<SatelliteData> orbits = new List<SatelliteData>();

            using (StreamReader file = File.OpenText(@"OrbitalElements.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                orbits = (List<SatelliteData>)serializer.Deserialize(file, typeof(List<SatelliteData>));
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
                        ParseResponse(orbit, reader.ReadToEnd());
                    }
                }
            }

            using (StreamWriter file = File.CreateText(@"OrbitalElementsUpdated.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, orbits);
            }
        }

        static void ParseResponse(SatelliteData orbit, string response)
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
                string radius = radiusItems.ElementAt(1).Split(' ').First();
                orbit.radius = double.Parse(radius, CultureInfo.InvariantCulture);
            }
        }
    }
}
