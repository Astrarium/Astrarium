using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Astrarium
{
    /// <inheritdoc/>
    public class GeoLocationsManager : IGeoLocationsManager
    {
        /// <summary>
        /// App settings instance
        /// </summary>
        private readonly ISettings settings;

        /// <summary>
        /// Locker to access elements from different threads
        /// </summary>
        private object locker = new object();

        /// <summary>
        /// StreamReader instance used for reading cities
        /// </summary>
        private StreamReader fileReader;

        /// <summary>
        /// Creates new instance of <see cref="GeoLocationsManager"/>
        /// </summary>
        public GeoLocationsManager(ISettings settings)
        {
            this.settings = settings;
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Cities.dat");
            fileReader = new StreamReader(File.OpenRead(filePath), Encoding.UTF8);
        }

        /// <inheritdoc/>
        public ICollection<CrdsGeographical> Search(CrdsGeographical center, float radius)
        {
            var locations = new List<CrdsGeographical>();

            lock (locker)
            {
                try
                {
                    string line = null;
                    fileReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    fileReader.DiscardBufferedData();

                    while ((line = fileReader.ReadLine()) != null)
                    {
                        string[] chunks = line.Split('\t');

                        double longitude = double.Parse(chunks[3], CultureInfo.InvariantCulture);
                        double latitude = double.Parse(chunks[4], CultureInfo.InvariantCulture);

                        if (new CrdsGeographical(longitude, latitude).DistanceTo(center) <= radius)
                        {
                            locations.Add(Parse(chunks));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error on searching geo locations: {ex}");
                }
            }

            return locations;
        }

        /// <inheritdoc/>
        public ICollection<CrdsGeographical> Search(string searchString, int maxCount)
        {
            var locations = new List<CrdsGeographical>();

            lock (locker)
            {
                try
                {
                    string line = null;
                    fileReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    fileReader.DiscardBufferedData();

                    while ((line = fileReader.ReadLine()) != null)
                    {
                        string[] chunks = line.Split('\t');
                        string name = chunks[1];
                        string[] names = chunks[2].Split(',');

                        if (name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase) ||
                            names.Any(x => x.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                        {
                            locations.Add(Parse(chunks));

                            if (locations.Count >= maxCount)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error on searching geo locations: {ex}");
                }

                return locations;
            }
        }

        /// <inheritdoc />
        public void AddToFavorites(CrdsGeographical location)
        {
            var favorites = settings.Get("FavoriteLocations", new List<CrdsGeographical>());
            var existing = favorites.FirstOrDefault(x => x.Equals(location));
            if (existing == null)
            {
                favorites.Add(location);
                settings.SetAndSave("FavoriteLocations", favorites);
            }
        }

        /// <summary>
        /// Parses single city record
        /// </summary>
        /// <param name="chunks"></param>
        /// <returns></returns>
        private CrdsGeographical Parse(string[] chunks)
        {
            string country = chunks[0];
            string name = chunks[1];
            string[] names = chunks[2].Split(',');
            double longitude = double.Parse(chunks[3], CultureInfo.InvariantCulture);
            double latitude = double.Parse(chunks[4], CultureInfo.InvariantCulture);
            float elevation = int.Parse(chunks[5]);
            float utcOffset = float.Parse(chunks[6], CultureInfo.InvariantCulture);
            return new CrdsGeographical()
            {
                Name = name,
                Names = names,
                Country = country,
                Elevation = elevation,
                Latitude = latitude,
                Longitude = longitude,
                UtcOffset = utcOffset,
            };
        }

        private void CreateCitiesFile()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium");
            string rawCitiesFilePath = Path.Combine(appDataFolder, "cities5000.txt");
            string timezonesFilePath = Path.Combine(appDataFolder, "timeZones.txt");
            string targetFilePath = Path.Combine(appDataFolder, "Cities.dat");

            StreamWriter targetFile = new StreamWriter(targetFilePath);

            var timezones = new Dictionary<string, float>();

            // read timezones
            foreach (string line in File.ReadLines(timezonesFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("CountryCode")) continue;
                string[] chunks = line.Split('\t');
                string name = chunks[1];
                string utcoffset = chunks[4];
                timezones[name] = float.Parse(utcoffset, CultureInfo.InvariantCulture);
            }

            // read cities
            foreach (string line in File.ReadLines(rawCitiesFilePath))
            {
                /*
                    0  = geonameid         : integer id of record in geonames database
                    1  = name              : name of geographical point (utf8) varchar(200)
                    2  = asciiname         : name of geographical point in plain ascii characters, varchar(200)
                    3  = alternatenames    : alternatenames, comma separated, ascii names automatically transliterated, convenience attribute from alternatename table, varchar(10000)
                    4  = latitude          : latitude in decimal degrees (wgs84)
                    5  = longitude         : longitude in decimal degrees (wgs84)
                    6  = feature class     : see http://www.geonames.org/export/codes.html, char(1)
                    7  = feature code      : see http://www.geonames.org/export/codes.html, varchar(10)
                    8  = country code      : ISO-3166 2-letter country code, 2 characters
                    9  = cc2               : alternate country codes, comma separated, ISO-3166 2-letter country code, 200 characters
                    10 = admin1 code       : fipscode (subject to change to iso code), see exceptions below, see file admin1Codes.txt for display names of this code; varchar(20)
                    11 = admin2 code       : code for the second administrative division, a county in the US, see file admin2Codes.txt; varchar(80) 
                    12 = admin3 code       : code for third level administrative division, varchar(20)
                    13 = admin4 code       : code for fourth level administrative division, varchar(20)
                    14 = population        : bigint (8 byte int) 
                    15 = elevation         : in meters, integer
                    16 = dem               : digital elevation model, srtm3 or gtopo30, average elevation of 3''x3'' (ca 90mx90m) or 30''x30'' (ca 900mx900m) area in meters, integer. srtm processed by cgiar/ciat.
                    17 = timezone          : the iana timezone id (see file timeZone.txt) varchar(40)
                    18 = modification date : date of last modification in yyyy-MM-dd format
                 */

                string[] chunks = line.Split('\t');
                string country = chunks[8];
                string name = chunks[1];
                string[] names = chunks[3].Split(',');
                double longitude = -double.Parse(chunks[5], CultureInfo.InvariantCulture);
                double latitude = double.Parse(chunks[4], CultureInfo.InvariantCulture);
                float elevation = string.IsNullOrEmpty(chunks[15]) ? 0 : int.Parse(chunks[15]);
                float utcOffset = timezones[chunks[17]];

                targetFile.WriteLine($"{country}\t{name}\t{string.Join(",", names)}\t{longitude.ToString(CultureInfo.InvariantCulture)}\t{latitude.ToString(CultureInfo.InvariantCulture)}\t{elevation}\t{utcOffset}");
            }

            targetFile.Flush();
        }
    }
}
