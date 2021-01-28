using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class CitiesManager
    {
        /// <summary>
        /// List of timezones
        /// </summary>
        private List<TimeZoneItem> timeZones = new List<TimeZoneItem>();

        /// <summary>
        /// List of all cities
        /// </summary>
        private List<CrdsGeographical> allCities = new List<CrdsGeographical>();

        public IEnumerable<CrdsGeographical> FindCities(CrdsGeographical center, float radius)
        {
            LoadTimeZonesIfRequired();
            LoadCitiesIfRequired();
            return allCities.Where(c => c.DistanceTo(center) <= radius);
        }

        private void LoadTimeZonesIfRequired()
        {
            // Do not read cities again
            if (timeZones.Count > 0)
            {
                return;
            }

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
                    timeZones.Add(new TimeZoneItem() { TimeZoneId = chunks[1], UtcOffset = double.Parse(chunks[4], CultureInfo.InvariantCulture) });
                }
                file.Close();
            }
        }

        /// <summary>
        /// Loads cities from file
        /// </summary>
        private void LoadCitiesIfRequired()
        {
            // Do not read cities again
            if (allCities.Count > 0)
            {
                return;
            }

            FileStream fileStream = null;
            try
            {
                string stringPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Cities.dat");
                fileStream = File.OpenRead(stringPath);

                using (var fileReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line = null;
                    while ((line = fileReader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] chunks = line.Split('\t');
                            float latitude = float.Parse(chunks[4], CultureInfo.InvariantCulture);
                            float longitude = float.Parse(chunks[5], CultureInfo.InvariantCulture);
                            float elevation = float.Parse(string.IsNullOrWhiteSpace(chunks[15]) ? "0" : chunks[15], CultureInfo.InvariantCulture);
                            TimeZoneItem timeZone = timeZones.FirstOrDefault(tz => tz.TimeZoneId.Equals(chunks[17], StringComparison.InvariantCultureIgnoreCase));

                            string name = null;

                            if (chunks[8] == "US")
                            {
                                name = $"{chunks[1]}, {chunks[10]}, {chunks[8]}";
                            }
                            else
                            {
                                name = $"{chunks[1]}, {chunks[8]}";
                            }

                            var location = new CrdsGeographical(-longitude, latitude, timeZone?.UtcOffset ?? 0, elevation, timeZone?.TimeZoneId, name);
                            allCities.Add(location);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError($"Unable to parse geographical location, line = {line}, error: {ex}");
                        }
                    }
                }
                fileStream.Close();
            }
            catch (Exception ex) 
            {
                Trace.TraceError($"Unable to load locations list, error: {ex}");
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
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
