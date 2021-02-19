using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.ImportExport
{
    /// <summary>
    /// Class to read geographical locations from CSV file
    /// </summary>
    public class CsvLocationsReader
    {
        /// <summary>
        /// Line parsing regexes.
        /// </summary>
        private readonly Regex[] regexp = new[] {
            // comma-separated file with optional quotes
            new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))"),
            // semicolon-separated file with optional quotes
            new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))") 
        };

        /// <summary>
        /// Reads geographical locations from a CSV file.
        /// </summary>
        /// <param name="fileName">Full path to a file to be read.</param>
        /// <returns>Collection of geographical locations.</returns>
        public ICollection<CrdsGeographical> ReadFromFile(string fileName)
        {
            Exception error = null;
            foreach (var regex in regexp)
            {
                try
                {
                    return ReadFromFile(regex, fileName);
                }
                catch (Exception ex)
                {
                    error = ex;                    
                }
            }
            throw error;
        }

        /// Reads geographical locations from a CSV file with specified regex.
        /// </summary>
        /// <param name="fileName">Full path to a file to be read.</param>
        /// <returns>Collection of geographical locations.</returns>
        private ICollection<CrdsGeographical> ReadFromFile(Regex regex, string fileName)
        {
            var locations = new List<CrdsGeographical>();

            using (var reader = new StreamReader(fileName, true))
            {
                string line;

                int row = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    row++;

                    string[] chunks = regex.Split(line);

                    // At least 3 columns should be in the CSV file
                    if (chunks.Length > 2)
                    {
                        string name = TrimQuotes(chunks[0]);
                        string latitudeStr = TrimQuotes(chunks[1]).Replace(',', '.');
                        double latitude = 0;
                        if (!double.TryParse(latitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude))
                            throw new Exception($"Incorrect value at row {row}, column 2. Expected decimal latitude value, actual = {(string.IsNullOrWhiteSpace(latitudeStr) ? "<empty>" : latitudeStr)}.");

                        if (Math.Abs(latitude) > 90)
                            throw new Exception($"Incorrect value at row {row}, column 3. Expected decimal latitude value in range -90...+90, actual = {latitude.ToString(CultureInfo.InvariantCulture)}.");

                        string longitudeStr = TrimQuotes(chunks[2]).Replace(',', '.');
                        double longitude = 0;
                        if (!double.TryParse(longitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude))
                            throw new Exception($"Incorrect value at row {row}, column 3. Expected decimal longitude value, actual = {(string.IsNullOrWhiteSpace(longitudeStr) ? "<empty>" : longitudeStr)}.");

                        double timeZone = 0;
                        if (chunks.Length > 3 && !string.IsNullOrWhiteSpace(chunks[3]))
                        {
                            string timeZoneStr = TrimQuotes(chunks[3]).Replace(',', '.');
                            if (!double.TryParse(timeZoneStr, NumberStyles.Float, CultureInfo.InvariantCulture, out timeZone))
                                throw new Exception($"Incorrect value at row {row}, column 4. Expected decimal UTC offset value, actual = {(string.IsNullOrWhiteSpace(timeZoneStr) ? "<empty>" : timeZoneStr)}.");
                        }

                        locations.Add(new CrdsGeographical(-longitude, latitude, timeZone, 0, null, name));
                    }
                    else
                    {
                        throw new Exception($"Incorrect value at row {row}: incompete data. Expected at least 3 columns:\n\n- Location name (string)\n- Latitude in decimal degrees (float, in range -90...90, positive north, negative south)\n- longitude in decimal degrees (float, in range -180...180, positive east, negative west)\n- UTC offset in hours (float, optional)\n");
                    }

                }
            }

            return locations;
        }

        /// <summary>
        /// Removes starting and ending quotes symbols for a string value, if required.
        /// </summary>
        /// <param name="str">String value to be processed.</param>
        /// <returns>String value without starting and ending quotes.</returns>
        private string TrimQuotes(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1 && str.First() == '"' && str.Last() == '"')
                return str.Trim('"');
            else
                return str;
        }
    }
}
