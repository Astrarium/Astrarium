﻿using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Astrarium.Plugins.BrightStars
{
    [Singleton(typeof(IStarsReader))]
    public class StarsReader : IStarsReader
    {
        /// <summary>
        /// Length of single record in data file
        /// </summary>
        private int RecordLength = 0;

        /// <summary>
        /// File path to the stars data file
        /// </summary>
        private readonly string STARS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Stars.dat");

        /// <summary>
        /// File with greek alphabet letters abbreviations and full names
        /// </summary>
        private readonly string ALPHABET_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Alphabet.dat");

        /// <summary>
        /// Sky instance
        /// </summary>
        private readonly ISky sky;

        public StarsReader(ISky sky)
        {
            this.sky = sky;
        }

        /// <summary>
        /// Reads stars data
        /// </summary>
        public ICollection<Star> ReadStars()
        {
            List<Star> stars = new List<Star>();

            string line = "";

            using (var sr = new StreamReader(STARS_FILE, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();

                    Star star = null;

                    if (line[94] != ' ')
                    {
                        star = new Star();
                        star.Number = ushort.Parse(line.Substring(0, 4).Trim());
                        star.Name = line.Substring(4, 10);

                        string hdNumber = line.Substring(25, 6).Trim();
                        if (!string.IsNullOrEmpty(hdNumber))
                        {
                            star.HDNumber = uint.Parse(hdNumber);
                        }

                        string saoNumber = line.Substring(31, 6).Trim();
                        if (!string.IsNullOrEmpty(saoNumber))
                        {
                            star.SAONumber = uint.Parse(saoNumber);
                        }

                        string fk5Number = line.Substring(37, 4).Trim();
                        if (!string.IsNullOrEmpty(fk5Number))
                        {
                            star.FK5Number = ushort.Parse(fk5Number);
                        }

                        string varName = line.Substring(51, 9).Trim();
                        if (!string.IsNullOrEmpty(varName) &&
                            !varName.Equals("Var?") &&
                            !varName.Equals("Var") &&
                            !line.Substring(51, 3).Trim().Equals(star.Name.Substring(3, 3).Trim()))
                        {
                            star.VariableName = varName;
                        }

                        star.Alpha0 = (float)new HMS(
                                            Convert.ToUInt32(line.Substring(75, 2)),
                                            Convert.ToUInt32(line.Substring(77, 2)),
                                            Convert.ToDouble(line.Substring(79, 4), CultureInfo.InvariantCulture)
                                        ).ToDecimalAngle();

                        star.Delta0 = (line[83] == '-' ? -1 : 1) * (float)new DMS(
                                                    Convert.ToUInt32(line.Substring(84, 2)),
                                                    Convert.ToUInt32(line.Substring(86, 2)),
                                                    Convert.ToUInt32(line.Substring(88, 2))
                                                ).ToDecimalAngle();

                        if (line[148] != ' ')
                        {
                            star.PmAlpha = Convert.ToSingle(line.Substring(148, 6), CultureInfo.InvariantCulture);
                        }
                        if (line[154] != ' ')
                        {
                            star.PmDelta = Convert.ToSingle(line.Substring(154, 6), CultureInfo.InvariantCulture);
                        }

                        star.Magnitude = Convert.ToSingle(line.Substring(102, 5), CultureInfo.InvariantCulture);
                        star.Color = line[129];

                        string identifier = star.Names.FirstOrDefault(n => sky.StarNames.ContainsKey(n));
                        if (identifier != null)
                        {
                            star.ProperName = sky.StarNames[identifier];
                        }
                    }

                    stars.Add(star);
                }

                RecordLength = (int)Math.Round(sr.BaseStream.Length / 9110.0);
            }

            return stars;
        }

        public StarDetails GetStarDetails(ushort hrNumber)
        {
            var details = new StarDetails();

            using (var sr = new StreamReader(STARS_FILE, Encoding.Default))
            {
                sr.BaseStream.Seek((hrNumber - 1) * RecordLength, SeekOrigin.Begin);
                string line = sr.ReadLine();

                details.IsInfraredSource = line[41] == 'I';
                details.SpectralClass = line.Substring(127, 20).Trim();
                details.Pecularity = line.Substring(147, 1).Trim();

                string radialVelocity = line.Substring(166, 4).Trim();

                details.RadialVelocity = string.IsNullOrEmpty(radialVelocity) ? (int?)null : int.Parse(radialVelocity);
            }

            return details;
        }

        public Dictionary<string, string> ReadAlphabet()
        {
            Dictionary<string, string> alphabet = new Dictionary<string, string>();
            using (var sr = new StreamReader(ALPHABET_FILE, Encoding.Default))
            {
                string line = "";
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    string[] chunks = line.Split('=');
                    alphabet.Add(chunks[0].Trim(), chunks[1].Trim());
                }
            }
            return alphabet;
        }
    }

    public class StarDetails
    {
        public int? RadialVelocity { get; set; }
        public bool IsInfraredSource { get; set; }
        public string SpectralClass { get; set; }
        public string Pecularity { get; set; }
    }
}
