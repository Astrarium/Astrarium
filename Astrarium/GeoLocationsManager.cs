using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium
{
    /// <inheritdoc/>
    public class GeoLocationsManager : IGeoLocationsManager
    {
        /// <summary>
        /// List of all locations
        /// </summary>
        private List<GeoLocation> allLocations = new List<GeoLocation>();

        /// <summary>
        /// Locker to access elements from different threads
        /// </summary>
        private object locker = new object();

        /// <summary>
        /// Flag indicating locations list has been loaded
        /// </summary>
        private bool isLoaded = false;

        /// <inheritdoc/>
        public ICollection<GeoLocation> Search(CrdsGeographical center, float radius)
        {
            if (isLoaded)
            {
                lock (locker)
                {
                    return allLocations.Where(c => c.DistanceTo(center) <= radius).ToArray();
                }
            }
            else
            {
                return new GeoLocation[0];
            }
        }

        public ICollection<GeoLocation> Search(string searchString, int maxCount)
        {
            if (isLoaded)
            {
                lock (locker)
                {
                    return allLocations
                        .Where(c => c.Names.Any(n => n.Replace("\'", "").StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                        .Take(maxCount)
                        .ToArray();
                }
            }
            else
            {
                return new GeoLocation[0];
            }
        }

        /// <inheritdoc/>
        public void Load()
        {
            Task.Run(() =>
            {
                LoadLocations();
                isLoaded = true;
            });
        }

        /// <inheritdoc/>
        public void Unload()
        {
            lock (locker)
            {
                allLocations.Clear();
                allLocations = null;
                isLoaded = false;
            }
        }

        /// <inheritdoc/>
        public event Action LocationsLoaded;

        private Dictionary<string, float> LoadTimeZones()
        {
            var timeZones = new Dictionary<string, float>();
            string line;
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "TimeZones.dat");
            using (StreamReader file = new StreamReader(filePath))
            {
                while ((line = file.ReadLine()) != null)
                {
                    // skip first and empty lines
                    if (line.StartsWith("CountryCode") || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    string[] chunks = line.Split('\t');
                    timeZones[chunks[1]] = float.Parse(chunks[4], CultureInfo.InvariantCulture);
                }
                file.Close();
            }

            return timeZones;
        }

        /// <summary>
        /// Loads cities from file
        /// </summary>
        private void LoadLocations()
        {
            lock (locker)
            {
                // Do not read cities again
                if (allLocations != null && allLocations.Count > 0)
                {
                    return;
                }

                var timeZoneAbbrs = LoadTimeZones();
                allLocations = new List<GeoLocation>();
                FileStream fileStream = null;
                try
                {
                    string stringPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Cities.dat");
                    fileStream = File.OpenRead(stringPath);

                    using (var fileReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        var timeZones = TimeZoneInfo.GetSystemTimeZones();

                        string line = null;
                        while ((line = fileReader.ReadLine()) != null)
                        {
                            try
                            {
                                string[] chunks = line.Split('\t');
                                float latitude = float.Parse(chunks[4], CultureInfo.InvariantCulture);
                                float longitude = float.Parse(chunks[5], CultureInfo.InvariantCulture);
                                float elevation = float.Parse(string.IsNullOrWhiteSpace(chunks[15]) ? "0" : chunks[15], CultureInfo.InvariantCulture);
                                float utcOffset = timeZoneAbbrs[chunks[17]];

                                var names = new List<string>();
                                names.Add(chunks[1]);
                                names.AddRange(chunks[3].Split(','));

                                allLocations.Add(new GeoLocation()
                                {
                                    Names = names.ToArray(),
                                    Country = chunks[8],
                                    Elevation = elevation,
                                    Latitude = latitude,
                                    Longitude = -longitude,
                                    UtcOffset = utcOffset,
                                });
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"Unable to parse geographical location, line = {line}, error: {ex}");
                            }
                        }
                    }
                    fileStream.Close();
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to load locations list, error: {ex}");
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Close();
                    }
                }
            }
            LocationsLoaded?.Invoke();
        }
    }
}
