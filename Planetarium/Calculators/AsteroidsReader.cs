using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    public class AsteroidsReader
    {
        public ICollection<Asteroid> Read(string file)
        {
            List<Asteroid> asteroids = new List<Asteroid>();

            string line = "";
            using (var sr = new StreamReader(file, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
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
                        Name = Read<string>(line, 167, 194)
                    });
                }
            }

            return asteroids;
        }

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
