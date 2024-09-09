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
using Astrarium.Types.Utils;
using System.Threading;

namespace Astrarium.Plugins.SolarSystem
{
    [Singleton]
    public class OrbitalElementsManager
    {
        private static readonly string OrbitalElementsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "OrbitalElements");

        private static readonly string OrbitalElementsFilePath = Path.Combine(OrbitalElementsDirectory, "SatellitesOrbits.dat");

        private ISettings settings;

        public OrbitalElementsManager(ISettings settings)
        {
            this.settings = settings;

            if (!Directory.Exists(OrbitalElementsDirectory))
            {
                try
                {
                    Directory.CreateDirectory(OrbitalElementsDirectory);
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to create directory for orbital elements: {OrbitalElementsDirectory}, Details: {ex}");
                }
            }
        }

        internal List<GenericMoonData> Load()
        {
            List<GenericMoonData> orbits = null;
            JsonSerializer serializer = new JsonSerializer();

            if (File.Exists(OrbitalElementsFilePath))
            {
                Log.Debug("Read cached orbital elements file...");
                try
                {
                    using (StreamReader file = File.OpenText(OrbitalElementsFilePath))
                    {
                        orbits = (List<GenericMoonData>)serializer.Deserialize(file, typeof(List<GenericMoonData>));
                    }
                    Log.Debug($"Reading cached orbital elements done. {orbits.Count} records read.");
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to read cached orbital elements file, Details: {ex}");
                }
            }
            else
            {
                Log.Debug("No cached orbital elements found.");
            }

            if (orbits == null)
            {
                Log.Debug("Read default orbital elements...");
                try
                {
                    using (StreamReader file = File.OpenText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/SatellitesOrbits.dat")))
                    {
                        orbits = (List<GenericMoonData>)serializer.Deserialize(file, typeof(List<GenericMoonData>));
                    }
                    Log.Debug($"Reading default orbital elements done. {orbits.Count} records read.");
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to read default orbital elements file, Details: {ex}");
                }
            }

            if (settings.Get("GenericMoonsAutoUpdate"))
            {
                DateTime today = DateTime.Today;
                double jdToday = new Date(today).ToJulianDay();

                // check if any orbit data is obsolete
                decimal validityPeriod = settings.Get<decimal>("GenericMoonsOrbitalElementsValidity");

                var obsoleteOrbits = orbits.Where(orbit => Math.Abs(orbit.jd - jdToday) > (double)validityPeriod);

                int obsoleteOrbitsCount = obsoleteOrbits.Count();

                if (obsoleteOrbitsCount > 0)
                {
                    Task.Run(() =>
                    {
                        Log.Debug($"Found {obsoleteOrbitsCount} obsolete orbital elements, downloading from web.");

                        DoUpdate(obsoleteOrbits);
                    });
                }
                else
                {
                    Log.Debug($"All orbital elements are up to date.");
                }
            }

            return orbits;
        }

        /// <summary>
        /// Updates orbital elements with showing progress dialog
        /// </summary>
        public async void Update(IEnumerable<GenericMoonData> orbits, Action onBefore, Action onAfter)
        {
            onAfter?.Invoke();

            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();

            ViewManager.ShowProgress("$OrbitalElementsDownloader.WaitTitle", "$OrbitalElementsDownloader.WaitText", tokenSource, progress);

            int updated = await Task.Run(() => DoUpdate(orbits, progress, tokenSource));

            if (!tokenSource.IsCancellationRequested)
            {
                tokenSource.Cancel();

                if (updated > 0)
                {
                    ViewManager.ShowMessageBox("$OrbitalElementsDownloader.CompletedTitle", Text.Get("OrbitalElementsDownloader.CompletedText", ("count", updated.ToString())));
                }
            }

            onAfter?.Invoke();
        }

        private int DoUpdate(IEnumerable<GenericMoonData> orbits, IProgress<double> progress = null, CancellationTokenSource token = null)
        {
            DateTime today = DateTime.Today;

            string startDate = today.ToString("yyyy-MM-dd");
            string endDate = today.AddDays(2).ToString("yyyy-MM-dd");

            int processed = 0;
            int updated = 0;
            double totalCount = orbits.Count();

            Log.Debug($"Updating orbital elements...");

            foreach (var orbit in orbits)
            {
                if (token != null && token.IsCancellationRequested)
                    return updated;

                if (progress != null)
                    progress.Report(processed / totalCount * 100);

                Uri url = new Uri($"https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1&COMMAND='{(orbit.planet * 100 + orbit.satellite)}'&CENTER='500@{(orbit.planet * 100 + 99)}'&MAKE_EPHEM='YES'&TABLE_TYPE='ELEMENTS'&START_TIME='{startDate}'&STOP_TIME='{endDate}'&STEP_SIZE='1 d'&OUT_UNITS='AU-D'&REF_PLANE='ECLIPTIC'&REF_SYSTEM='J2000'&TP_TYPE='ABSOLUTE'&CSV_FORMAT='YES'&OBJ_DATA='YES'");
                string tempPath = Path.GetTempFileName();

                try
                {
                    Downloader.Download(url, tempPath);
                    ParseOrbit(orbit, File.ReadAllText(tempPath));
                    updated++;
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to download orbital elements from url: {url}, Details: {ex}");
                }
                finally
                {
                    FileSystem.DeleteFile(tempPath);
                }

                processed++;
            }

            Log.Debug($"{updated} orbital elements updated.");

            if (processed > 0)
            {
                SaveToCache(orbits);
            }

            return updated;
        }

        private void SaveToCache(IEnumerable<GenericMoonData> orbits)
        {
            Log.Debug("Saving orbital elements to cache...");

            JsonSerializer serializer = new JsonSerializer();
            try
            {
                using (StreamWriter file = File.CreateText(OrbitalElementsFilePath))
                {
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, orbits);
                }

                settings.SetAndSave("GenericMoonsOrbitalElementsLastUpdated", DateTime.Now);

                Log.Debug("Orbital elements saved to cache.");
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to save orbital elements to cache, Details: {ex}");
            }
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
