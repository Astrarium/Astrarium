using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Astrarium.Plugins.Hipparcos
{
    [Singleton(typeof(IStarsReader))]
    public class StarsReader : IStarsReader
    {
        /// <summary>
        /// File path to the Hipparcos catalogue
        /// </summary>
        private readonly string HIPPARCOS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Hipparcos.dat");

        /// <summary>
        /// Index file
        /// </summary>
        private readonly string INDEX_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Hipparcos.idx");

        /// <summary>
        /// File with greek alphabet letters abbreviations and full names
        /// </summary>
        private readonly string ALPHABET_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Alphabet.dat");

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

            using (var instream = new FileStream(HIPPARCOS_FILE, FileMode.Open))
            {
                using (var sr = new BinaryReader(instream))
                {
                    int index = 0;
                    while (instream.Position != instream.Length)
                    {
                        var star = new Star();

                        star.HIP = sr.ReadUInt32();
                        star.HD = sr.ReadUInt32();
                        star.DM = Encoding.ASCII.GetString(sr.ReadBytes(12));
                        star.GC = sr.ReadUInt16();
                        star.HR = sr.ReadUInt16();
                        star.Flamsteed = sr.ReadByte();
                        star.Bayer = Encoding.ASCII.GetString(sr.ReadBytes(5));
                        star.Con = Encoding.ASCII.GetString(sr.ReadBytes(3));

                        star.Alpha0 = sr.ReadSingle();
                        star.Delta0 = sr.ReadSingle();
                        star.PmAlpha = sr.ReadSingle();
                        star.PmDelta = sr.ReadSingle();
                        star.Magnitude = sr.ReadSingle();
                        star.Plx = sr.ReadSingle();

                        star.SpType = Encoding.ASCII.GetString(sr.ReadBytes(12));

                        star.VarFlag = sr.ReadByte();
                        star.VarType = sr.ReadByte();
                        star.VarPeriod = sr.ReadSingle();

                        star.NComps = sr.ReadByte();
                        star.Comps = Encoding.ASCII.GetString(sr.ReadBytes(2));
                        star.PosA = sr.ReadByte();
                        star.Sep = sr.ReadSingle();
                        star.MagDiff = sr.ReadSingle();

                        star.Index = index;
                        index++;

                        if (star.HR > 0 && sky.StarNames.TryGetValue($"HR {star.HR}", out string pn)) 
                        {
                            star.ProperName = pn;
                        }

                        if (star.HD > 0 && sky.StarNames.TryGetValue($"HD {star.HD}", out string pn2))
                        {
                            star.ProperName = pn2;
                        }

                        stars.Add(star);
                    }
                }
            }

            return stars;
        }

        public ICollection<Region> ReadIndex()
        {
            var regions = new List<Region>();
            const int sectors = 10;
            const int bands = 10;

            double lonStep = 360.0 / sectors;

            using (var sr = new StreamReader(INDEX_FILE, Encoding.Default))
            {
                for (int i = 0; i < bands; i++)
                {
                    double sinMin = -1.0 + 2.0 * i / bands;
                    double sinMax = -1.0 + 2.0 * (i + 1) / bands;

                    double latMin = Math.Asin(sinMin) * 180.0 / Math.PI; // от -90
                    double latMax = Math.Asin(sinMax) * 180.0 / Math.PI; // до +90

                    for (int j = 0; j < sectors; j++)
                    {
                        double lonMin = j * lonStep;
                        double lonMax = (j + 1) * lonStep;

                        string line = sr.ReadLine();

                        var chunks = line.Split(',');
                        int firstStarIndex = int.Parse(chunks[0]);
                        int starsCount = int.Parse(chunks[1]);

                        regions.Add(new Region(firstStarIndex, starsCount, latMin, latMax, lonMin, lonMax));
                    }
                }
            }

            return regions;
        }

        public Dictionary<string, string> ReadAlphabet()
        {
            var alphabet = new Dictionary<string, string>();
            using (var sr = new StreamReader(ALPHABET_FILE, Encoding.Default))
            {
                string line = "";
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    string[] chunks = line.Split('=');
                    alphabet.Add(chunks[0].Trim().ToLower(), chunks[1].Trim());
                }
            }
            return alphabet;
        }
    }
}
