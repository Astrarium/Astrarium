using Astrarium.Algorithms;
using Newtonsoft.Json;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Astrarium.Plugins.SolarSystem.Objects;

namespace Astrarium.Plugins.SolarSystem
{
    internal class OrbitalElementsManager
    {
        private static readonly string OrbitalElementsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "OrbitalElements");

        private ISettings settings;

        internal OrbitalElementsManager(ISettings settings)
        {
            this.settings = settings;

            if (!Directory.Exists(OrbitalElementsPath))
            {
                try
                {
                    Directory.CreateDirectory(OrbitalElementsPath);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Unable to create directory for orbital elements: {OrbitalElementsPath}, Details: {ex}");
                }
            }
        }

        internal List<GenericMoonData> Load()
        {
            List<GenericMoonData> orbits = null;
            JsonSerializer serializer = new JsonSerializer();
            string cachedFilePath = Path.Combine(OrbitalElementsPath, "SatellitesOrbits.dat");

            if (File.Exists(cachedFilePath))
            {
                Debug.WriteLine("Read cached orbital elements file...");
                try
                {
                    using (StreamReader file = File.OpenText(cachedFilePath))
                    {
                        orbits = (List<GenericMoonData>)serializer.Deserialize(file, typeof(List<GenericMoonData>));
                    }
                    Debug.WriteLine($"Reading cached orbital elements done. {orbits.Count} records read.");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Unable to read cached orbital elements file, Details: {ex}");
                }
            }
            else
            {
                Debug.WriteLine("No cached orbital elements found.");
            }

            if (orbits == null)
            {
                Debug.WriteLine("Read default orbital elements...");
                try
                {
                    using (StreamReader file = File.OpenText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/SatellitesOrbits.dat")))
                    {
                        orbits = (List<GenericMoonData>)serializer.Deserialize(file, typeof(List<GenericMoonData>));
                    }
                    Debug.WriteLine($"Reading default orbital elements done. {orbits.Count} records read.");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Unable to read default orbital elements file, Details: {ex}");
                }
            }

            if (settings.Get("GenericMoonsAutoUpdate"))
            {
                DateTime today = DateTime.Today;
                double jdToday = new Date(today).ToJulianDay();

                // check if any orbit data is obsolete
                // TODO: move validityPeriod to settings
                int validityPeriod = 30;

                var obsoleteOrbits = orbits.Where(orbit => Math.Abs(orbit.jd - jdToday) > validityPeriod);

                int obsoleteOrbitsCount = obsoleteOrbits.Count();

                if (obsoleteOrbitsCount > 0)
                {
                    Task.Run(() =>
                    {
                        Debug.WriteLine($"Found {obsoleteOrbitsCount} obsolete orbital elements, downloading from web.");

                        string startDate = today.ToString("yyyy-MM-dd");
                        string endDate = today.AddDays(2).ToString("yyyy-MM-dd");

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                        foreach (var orbit in obsoleteOrbits)
                        {
                            string url = $"https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1&COMMAND='{(orbit.planet * 100 + orbit.satellite)}'&CENTER='500@{(orbit.planet * 100 + 99)}'&MAKE_EPHEM='YES'&TABLE_TYPE='ELEMENTS'&START_TIME='{startDate}'&STOP_TIME='{endDate}'&STEP_SIZE='1 d'&OUT_UNITS='AU-D'&REF_PLANE='ECLIPTIC'&REF_SYSTEM='J2000'&TP_TYPE='ABSOLUTE'&CSV_FORMAT='YES'&OBJ_DATA='YES'";
                            try
                            {
                                var request = WebRequest.Create(url);
                                using (var response = (HttpWebResponse)(request.GetResponse()))
                                using (var receiveStream = response.GetResponseStream())
                                using (var reader = new StreamReader(receiveStream))
                                {
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        ParseOrbit(orbit, reader.ReadToEnd());
                                    }
                                    else
                                    {
                                        Trace.TraceError($"Unable to download orbital elements from url: {url}, status code: {response.StatusCode}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError($"Unable to download orbital elements from url: {url}, Details: {ex}");
                            }
                        }

                        Debug.WriteLine($"{orbits.Count} orbital elements downloaded.");
                        Debug.WriteLine("Saving orbital elements to cache...");

                        try
                        {
                            using (StreamWriter file = File.CreateText(cachedFilePath))
                            {
                                serializer.Formatting = Formatting.Indented;
                                serializer.Serialize(file, orbits);
                            }
                            Debug.WriteLine("Orbital elements saved to cache.");
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError($"Unable to save orbital elements to cache, Details: {ex}");
                        }
                    });
                }
                else
                {
                    Debug.WriteLine($"All orbital elements are up to date.");
                }
            }

            return orbits;
        }

        private void ParseOrbit(GenericMoonData orbit, string response)
        {
            List<string> lines = response.Split('\n').ToList();
            string soeMarker = lines.FirstOrDefault(ln => ln == "$$SOE");

            if (soeMarker == null) 
                throw new Exception($"Unable to parse ephemeris data for satellite {orbit.names.First().Value}");

            int soeMarkerIndex = lines.IndexOf(soeMarker);

            string header = lines[soeMarkerIndex - 2];

            List<string> headerItems = header.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();

            int dateIndex = headerItems.IndexOf(headerItems.FirstOrDefault(item => item.StartsWith("Calendar Date")));

            List<double> day1 = lines[soeMarkerIndex + 1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select((item, ind) => ind != dateIndex ? double.Parse(item.Trim(), CultureInfo.InvariantCulture) : 0).ToList();
            List<double> day2 = lines[soeMarkerIndex + 2].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select((item, ind) => ind != dateIndex ? double.Parse(item.Trim(), CultureInfo.InvariantCulture) : 0).ToList();

            orbit.jd = day1[headerItems.IndexOf("JDTDB")];
            orbit.e = day1[headerItems.IndexOf("EC")];
            orbit.i = day1[headerItems.IndexOf("IN")];
            orbit.Om = day1[headerItems.IndexOf("OM")];
            orbit.w = day1[headerItems.IndexOf("W")];
            orbit.n = day1[headerItems.IndexOf("N")];
            orbit.M = day1[headerItems.IndexOf("MA")];
            orbit.a = day1[headerItems.IndexOf("A")];

            double[] Omega = new double[] { orbit.Om, day2[headerItems.IndexOf("OM")] };
            double[] w = new double[] { orbit.w, day2[headerItems.IndexOf("W")] };
            double[] MA = new double[] { orbit.M, day2[headerItems.IndexOf("MA")] };

            Angle.Align(Omega);
            Angle.Align(w);

            orbit.POm = 360.0 / (Omega[1] - Omega[0]) / 365.25;
            orbit.Pw = 360.0 / (w[1] - w[0]) / 365.25;

            MA[0] = MA[0] + orbit.n % 360;

            // add correction to mean motion
            Angle.Align(MA);
            double dn = MA[1] - MA[0];
            if (orbit.n + dn > 0)
            {
                orbit.n += dn;
            }

            string magLine = lines.FirstOrDefault(ln => ln.Contains("V(1,0)"));
            if (orbit.mag == 0 && !string.IsNullOrEmpty(magLine))
            {
                magLine = magLine.Substring(magLine.IndexOf("V(1,0)"));
                List<string> magItems = magLine.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();
                string mag = magItems.ElementAt(1).Split(' ').First();
                orbit.mag = double.Parse(mag, CultureInfo.InvariantCulture);
            }

            string radiusLine = lines.FirstOrDefault(ln => ln.Contains("Radius"));
            if (orbit.radius == 0 && !string.IsNullOrEmpty(radiusLine))
            {
                radiusLine = radiusLine.Substring(radiusLine.IndexOf("Radius"));
                List<string> radiusItems = radiusLine.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();
                string radius = radiusItems.ElementAt(1).Split(' ').First().Split('x').First();
                orbit.radius = double.Parse(radius, CultureInfo.InvariantCulture);
            }
        }
    }
}
