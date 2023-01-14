﻿using Astrarium.Algorithms;
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
        public ICollection<CrdsGeographical> Search(CrdsGeographical center, float radius)
        {
            if (isLoaded)
            {
                lock (locker)
                {
                    return allLocations.Where(c => c.DistanceTo(center) <= radius).Select(c =>
                    {
                        // TODO: constructor
                        return new CrdsGeographical()
                        {
                            Elevation = c.Elevation,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude,
                            LocationName = c.Names.FirstOrDefault(),
                            UtcOffset = c.TimeZone?.BaseUtcOffset.TotalHours ?? 0
                        };
                    }).ToArray();
                }
            }
            else
            {
                return new CrdsGeographical[0];
            }
        }

        public ICollection<CrdsGeographical> Search(string searchString, int maxCount)
        {
            if (isLoaded)
            {
                lock (locker)
                {
                    return allLocations
                        .Where(c => c.Names.Any(n => n.Replace("\'", "").StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                        .Take(maxCount)
                        .Select(c =>
                        {
                            string name = c.Names.First(n => n.Replace("\'", "").StartsWith(searchString, StringComparison.OrdinalIgnoreCase));
                            return new CrdsGeographical()
                            {
                                //CountryCode = c.Country,
                                Elevation = c.Elevation,
                                Latitude = c.Latitude,
                                Longitude = c.Longitude,
                                LocationName = name,
                                //OtherNames = c.Names.Distinct().Except(new[] { name }).ToArray(),
                                UtcOffset = c.TimeZone?.BaseUtcOffset.TotalHours ?? 0
                            };
                        })
                        .OrderBy(c => c.LocationName)
                        .ToArray();
                }
            }
            else
            {
                return new CrdsGeographical[0];
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
                                TimeZoneInfo timeZone = timeZones.FirstOrDefault(tz => tz.Id.Equals(chunks[17], StringComparison.InvariantCultureIgnoreCase));

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
                                    TimeZone = timeZone,
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

        private class GeoLocation
        {
            public string[] Names { get; set; }
            public string Country { get; set; }
            public float Latitude { get; set; }
            public float Longitude { get; set; }
            public float Elevation { get; set; }
            public TimeZoneInfo TimeZone { get; set; }

            public double DistanceTo(CrdsGeographical g)
            {
                return g.DistanceTo(new CrdsGeographical(Longitude, Latitude));
            }
        }       

        /// <summary>
        /// Represents time zone information
        /// </summary>
        private class TimeZoneItem
        {
            /// <summary>
            /// Unique time zone id, like "Europe/Moscow"
            /// </summary>
            public string TimeZoneId { get; set; }

            /// <summary>
            /// UTC offset of the zone, in hours
            /// </summary>
            public double UtcOffset { get; set; }

            /// <summary>
            /// Gets displayable name of the time zone
            /// </summary>
            public string Name
            {
                get
                {
                    return $"UTC{(UtcOffset >= 0 ? "+" : "-")}{TimeSpan.FromHours(UtcOffset):hh\\:mm} ({TimeZoneId.Replace('_', ' ')})";
                }
            }
        }
    }
}
