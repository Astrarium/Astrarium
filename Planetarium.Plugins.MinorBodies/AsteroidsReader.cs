using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    internal class AsteroidsReader
    {
        /// <summary>
        /// Reads asteroids orbital elements written in MPC format.
        /// Description of the format can be found at <see href="https://www.minorplanetcenter.net/iau/info/MPOrbitFormat.html"/>.
        /// </summary>
        /// <param name="orbitalElementsFile">Full path to the file with orbital elements.</param>
        /// <returns>Collection of <see cref="Asteroid"/> items.</returns>
        public ICollection<Asteroid> Read(string orbitalElementsFile, string sizesFile)
        {
            List<Asteroid> asteroids = new List<Asteroid>();
            var sizes = new Dictionary<int, float>();

            string line = "";

            using (var sr = new StreamReader(sizesFile, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    string[] chunks = line.Split(',');
                    sizes.Add(int.Parse(chunks[0].Trim()), float.Parse(chunks[1].Trim(), CultureInfo.InvariantCulture));
                }
            }

            using (var sr = new StreamReader(orbitalElementsFile, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    int number = Read<int>(line, 1, 7);
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
                        PhysicalDiameter = sizes.ContainsKey(number) ? sizes[number] : 0
                    });
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
