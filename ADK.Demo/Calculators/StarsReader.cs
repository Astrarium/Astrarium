using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class StarsReader
    {
        /// <summary>
        /// Length of single record in data file
        /// </summary>
        private int RecordLength = 0;

        /// <summary>
        /// File path to the stars data file
        /// </summary>
        public string StarsDataFilePath { get; set; }

        /// <summary>
        /// File path to the file with stars proper names
        /// </summary>
        public string StarsNamesFilePath { get; set; }

        /// <summary>
        /// Reads stars data
        /// </summary>
        public ICollection<Star> ReadStars()
        {
            SanityCheck();

            List<Star> stars = new List<Star>();

            string line = "";

            using (var sr = new StreamReader(StarsDataFilePath, Encoding.Default))
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

                        star.Equatorial0.Alpha = new HMS(
                                                    Convert.ToUInt32(line.Substring(75, 2)),
                                                    Convert.ToUInt32(line.Substring(77, 2)),
                                                    Convert.ToDouble(line.Substring(79, 4), CultureInfo.InvariantCulture)
                                                ).ToDecimalAngle();

                        star.Equatorial0.Delta = (line[83] == '-' ? -1 : 1) * new DMS(
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

                        star.Mag = Convert.ToSingle(line.Substring(102, 5), CultureInfo.InvariantCulture);
                        star.Color = line[129];
                    }

                    stars.Add(star);
                }

                RecordLength = (int)Math.Round(sr.BaseStream.Length / 9110.0);
            }

            using (var sr = new StreamReader(StarsNamesFilePath, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    string[] parts = line.Split('=');
                    int number = int.Parse(parts[0].Trim());
                    stars[number - 1].ProperName = parts[1].Trim();
                }
            }

            return stars;
        }

        public StarDetails GetStarDetails(Star s)
        {
            SanityCheck();

            var details = new StarDetails();

            using (var sr = new StreamReader(StarsDataFilePath, Encoding.Default))
            {
                sr.BaseStream.Seek((s.Number - 1) * RecordLength, SeekOrigin.Begin);
                string line = sr.ReadLine();

                details.IsInfraredSource = line[41] == 'I';
                details.SpectralClass = line.Substring(127, 20).Trim();
                details.Pecularity = line.Substring(147, 1).Trim();

                string radialVelocity = line.Substring(166, 4).Trim();

                details.RadialVelocity = string.IsNullOrEmpty(radialVelocity) ? (int?)null : int.Parse(radialVelocity);
            }

            return details;
        }

        private void SanityCheck()
        {
            if (string.IsNullOrEmpty(StarsDataFilePath))
                throw new Exception("Stars data file path is not set.");

            if (string.IsNullOrEmpty(StarsNamesFilePath))
                throw new Exception("Stars names file path is not set.");
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
