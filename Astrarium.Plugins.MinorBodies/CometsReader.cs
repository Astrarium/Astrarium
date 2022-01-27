using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Astrarium.Plugins.MinorBodies
{
    public class CometsReader : IOrbitalElementsReader<Comet>
    {
        /// <summary>
        /// Reads comets orbital elements written in MPC format.
        /// Description of the format can be found at <see href="https://www.minorplanetcenter.net/iau/info/CometOrbitFormat.html"/>.
        /// </summary>
        /// <param name="orbitalElementsFile">Full path to the file with orbital elements.</param>
        /// <returns>Collection of <see cref="Comet"/> items.</returns>
        public ICollection<Comet> Read(string orbitalElementsFile)
        {
            List<Comet> comets = new List<Comet>();
           
            string line = "";

            using (var sr = new StreamReader(orbitalElementsFile, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    try
                    {
                        line = sr.ReadLine();
                        if (!line.Contains("****"))
                        {
                            // perihelion distance, in AU
                            double q = Read<double>(line, 31, 39);

                            // orbital eccentricity
                            double e = Read<double>(line, 42, 49);

                            // mean motion, degrees/day
                            double n = 1; // TODO: ?

                            int y = Read<int>(line, 15, 18);
                            int m = Read<int>(line, 20, 21);
                            double d = Read<double>(line, 23, 29);

                            // date of perihelion passage
                            double pp = Date.JulianDay(y, m, d);

                            comets.Add(new Comet()
                            {
                                H = Read<double>(line, 92, 95),
                                G = Read<double>(line, 97, 100),
                                Orbit = new OrbitalElements()
                                {
                                    Epoch = pp,
                                    M = 0, // since the epoch is perihelion passage date, mean anomaly is zero
                                    omega = Read<double>(line, 52, 59),
                                    Omega = Read<double>(line, 62, 69),
                                    i = Read<double>(line, 72, 79),
                                    e = e,
                                    q = q
                                },
                                AverageDailyMotion = n,
                                Name = Read<string>(line, 103, 158)
                            });
                        }
                    }
                    catch (Exception ex) 
                    {
                        Trace.TraceError($"Unable to parse comet data. Line:\n{line}\nError: {ex}");
                    }
                }
            }

            return comets;
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
