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
        /// List of all cities
        /// </summary>
        private List<CrdsGeographical> allCities = new List<CrdsGeographical>();

        public IEnumerable<CrdsGeographical> FindCities(CrdsGeographical center, float radius)
        {
            LoadCitiesIfRequired();
            return allCities.Where(c => c.DistanceTo(center) <= radius);
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
                            string name = null;

                            if (chunks[8] == "US")
                            {
                                name = $"{chunks[1]}, {chunks[10]}, {chunks[8]}";
                            }
                            else
                            {
                                name = $"{chunks[1]}, {chunks[8]}";
                            }

                            var location = new CrdsGeographical(-longitude, latitude, 0, elevation, null, name);
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
    }
}
