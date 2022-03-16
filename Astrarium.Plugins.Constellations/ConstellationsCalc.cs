using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.Constellations
{   
    public class ConstellationsCalc : BaseCalc
    {
        /// <summary>
        /// Constellations
        /// </summary>
        public List<ConstellationLabel> ConstLabels { get; private set; } = new List<ConstellationLabel>();

        /// <summary>
        /// Constellations borders coordinates
        /// </summary>
        public List<List<CelestialPoint>> ConstBorders { get; private set; } = new List<List<CelestialPoint>>();

        /// <summary>
        /// List of constellation lines (traditional)
        /// </summary>
        public List<Tuple<int, int>> ConstLinesTraditional { get; private set; } = new List<Tuple<int, int>>();

        private ISky sky;

        public ConstellationsCalc(ISky sky)
        {
            this.sky = sky;
        }

        public override void Calculate(SkyContext context)
        {
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            foreach (var b in ConstBorders)
            {
                foreach (var bp in b)
                {
                    // Equatorial coordinates for the mean equinox and epoch of the target date
                    var eq = Precession.GetEquatorialCoordinates(bp.Equatorial0, p);

                    // Apparent horizontal coordinates
                    bp.Horizontal = eq.ToHorizontal(context.GeoLocation, context.SiderealTime);
                }
            }

            foreach (var c in ConstLabels)
            {
                // Equatorial coordinates for the mean equinox and epoch of the target date
                var eq = Precession.GetEquatorialCoordinates(c.Equatorial0, p);

                // Apparent horizontal coordinates
                c.Horizontal = eq.ToHorizontal(context.GeoLocation, context.SiderealTime);
            }
        }

        public override void Initialize()
        {
            LoadBordersData();
            LoadLabelsData();
            LoadLinesData();
        }

        /// <summary>
        /// Loads constellation borders data
        /// </summary>
        private void LoadBordersData()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Borders.dat");

            using (var sr = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                List<CelestialPoint> block = null;
                while (sr.BaseStream.Position != sr.BaseStream.Length)
                {
                    bool start = sr.ReadBoolean();
                    if (start)
                    {
                        block = new List<CelestialPoint>();
                        ConstBorders.Add(block);
                    }

                    block.Add(new CelestialPoint()
                    {
                        Equatorial0 = new CrdsEquatorial(sr.ReadDouble(), sr.ReadDouble())
                    });
                }
            }
        }

        /// <summary>
        /// Loads constellations labels data
        /// </summary>
        private void LoadLabelsData()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Conlabels.dat");
            using (var sr = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                while (sr.BaseStream.Position != sr.BaseStream.Length)
                {
                    string code = sr.ReadString();                
                    ConstLabels.Add(new ConstellationLabel()
                    {
                        Code = code.Substring(0, 3),
                        Equatorial0 = new CrdsEquatorial(sr.ReadSingle(), sr.ReadSingle())
                    });
                }
            }
        }

        private void LoadLinesData()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/ConLines.dat");
            string[] chunks = null;
            int from, to;
            string line = "";

            using (var sr = new StreamReader(file, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    chunks = line.Split(',');
                    from = Convert.ToInt32(chunks[0]) - 1;
                    to = Convert.ToInt32(chunks[1]) - 1;

                    ConstLinesTraditional.Add(new Tuple<int, int>(from, to));
                }

                sky.ConstellationLines = ConstLinesTraditional;
            }
        }
    }
}
