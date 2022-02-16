using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Astrarium.Plugins.MinorBodies
{
    public class AsteroidsReader : IOrbitalElementsReader<Asteroid>
    {
        private readonly string SIZES_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/AsteroidsSizes.dat");
        private readonly string BRIGHTNESS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/AsteroidsBright.dat");

        /// <summary>
        /// Reads asteroids orbital elements written in MPC format.
        /// Description of the format can be found at <see href="https://www.minorplanetcenter.net/iau/info/MPOrbitFormat.html"/>.
        /// </summary>
        /// <returns>Collection of <see cref="Asteroid"/> items.</returns>
        public ICollection<Asteroid> Read(string orbitalElementsFile)
        {
            List<Asteroid> asteroids = new List<Asteroid>();
            var sizes = new Dictionary<int, float>();
            var brightness = new Dictionary<int, float?>();

            string line = "";

            using (var sr = new StreamReader(SIZES_FILE, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    string[] chunks = line.Split(',');
                    sizes.Add(int.Parse(chunks[0].Trim()), float.Parse(chunks[1].Trim(), CultureInfo.InvariantCulture));
                }
            }

            using (var sr = new StreamReader(BRIGHTNESS_FILE, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    string[] chunks = line.Split(',');
                    brightness.Add(int.Parse(chunks[0].Trim()), float.Parse(chunks[1].Trim(), CultureInfo.InvariantCulture));
                }
            }

            using (var sr = new StreamReader(orbitalElementsFile, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    try
                    {
                        line = sr.ReadLine();
                        string number = Read<string>(line, 1, 7);
                        float physicalDiameter = 0;
                        float? maxBrightness = null;

                        if (int.TryParse(number, out int num))
                        {
                            physicalDiameter = sizes.ContainsKey(num) ? sizes[num] : 0;
                            maxBrightness = brightness.ContainsKey(num) ? brightness[num] : null;
                        }

                        // H or G is not defined
                        if (string.IsNullOrWhiteSpace(Read<string>(line, 9, 13)) ||
                            string.IsNullOrWhiteSpace(Read<string>(line, 15, 19)))
                            continue;

                        asteroids.Add(new Asteroid()
                        {
                            H = Read<double>(line, 9, 13),
                            G = Read<double>(line, 15, 19),
                            Orbit = new OrbitalElements()
                            {
                                Epoch = ReadDate(line, 21, 25),
                                M = Read<double>(line, 27, 35),
                                omega = Read<double>(line, 38, 46),
                                Omega = Read<double>(line, 49, 57),
                                i = Read<double>(line, 60, 68),
                                e = Read<double>(line, 71, 79),
                                a = Read<double>(line, 93, 103)
                            },
                            AverageDailyMotion = Read<double>(line, 81, 91),
                            Name = Read<string>(line, 167, 194),
                            PhysicalDiameter = physicalDiameter,
                            MaxBrightness = maxBrightness
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Unable to parse asteroid data. Line:\n{line}\nError: {ex}");
                    }
                }
            }

            return asteroids;
        }

        /// <summary>
        /// Reads date written in packed form.
        /// Description of packed format can be found at <see href="https://www.minorplanetcenter.net/iau/info/PackedDes.html"/>.
        /// </summary>
        /// <param name="line">Line of orbital elements in MPC format.</param>
        /// <param name="from">Starting column (character index + 1) in the line</param>
        /// <param name="to">Ending column (character index + 1) in the line</param>
        /// <returns>Julian day corresponding to encoded date.</returns>
        private double ReadDate(string line, int from, int to)
        {
            string strValue = Read<string>(line, from, to);
            int year = ReadPackedDigit(strValue[0]) * 100 + int.Parse(strValue.Substring(1, 2));
            int month = ReadPackedDigit(strValue[3]);
            double day = ReadPackedDigit(strValue[4]);
            if (strValue.Length > 5)
            {
                day += double.Parse($"0.{strValue.Substring(5)}", CultureInfo.InvariantCulture);
            }
            return Date.JulianDay(year, month, day);
        }

        private int ReadPackedDigit(char c)
        {
            int v = (int)c;
            return (v >= 48 && v <= 57) ?
                v - 48 :
                (v >= 65 && v <= 86 ? v - 65 + 10 : 0);
        }

        private T Read<T>(string line, int from, int to)
        {
            to = Math.Min(to, line.Length);
            string strValue = line.Substring(from - 1, to - from + 1).Trim();
            if (typeof(T) == typeof(string))
            {
                return (T)(object)strValue;
            }

            return (T)Convert.ChangeType(strValue, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}
