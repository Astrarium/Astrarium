using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public List<List<CrdsEquatorial>> Borders { get; private set; } = new List<List<CrdsEquatorial>>();

        /// <summary>
        /// List of constellation lines (traditional)
        /// </summary>
        public List<Tuple<int, int>> ConstLinesTraditional { get; private set; } = new List<Tuple<int, int>>();

        /// <summary>
        /// List of constellation lines (H.A.Rey, "The Stars: A New Way To See Them")
        /// </summary>
        public List<Tuple<int, int>> ConstLinesRey { get; private set; } = new List<Tuple<int, int>>();

        /// <summary>
        /// Precession elements for conversion Current Epoch -> J2000.0
        /// </summary>
        public PrecessionalElements PrecessionElementsCurrentToJ2000 { get; private set; }


        /// <summary>
        /// Precession elements for conversion Current Epoch -> B1875.0
        /// </summary>
        public PrecessionalElements PrecessionElementsCurrentToB1875 { get; private set; }

        /// <summary>
        /// Precession elements for conversion J2000.0 -> Current Epoch
        /// </summary>
        public PrecessionalElements PrecessionElementsJ2000ToCurrent { get; private set; }

        private readonly ISky sky;
        private readonly ISettings settings;

        public ConstellationsCalc(ISky sky, ISettings settings)
        {
            this.sky = sky;
            this.settings = settings;
            this.settings.SettingValueChanged += Settings_SettingValueChanged;
        }

        private void Settings_SettingValueChanged(string settingName, object settingValue)
        {
            if (settingName == "ConstLinesType")
            {
                SetConstellationLinesType();
            }
        }

        public override void Calculate(SkyContext context)
        {
            // precessional elements from J2000 to current epoch
            PrecessionElementsJ2000ToCurrent = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            // precessional elements from current epoch to J2000
            PrecessionElementsCurrentToJ2000 = Precession.ElementsFK5(context.JulianDay, Date.EPOCH_J2000);

            PrecessionElementsCurrentToB1875 = Precession.ElementsFK5(context.JulianDay, Date.EPOCH_B1875);
        }

        public override void Initialize()
        {
            LoadBordersData();
            LoadLabelsData();
            ConstLinesTraditional = LoadLinesData("ConLines-Traditional.dat");
            ConstLinesRey = LoadLinesData("ConLines-Rey.dat");
            SetConstellationLinesType();
        }

        private void SetConstellationLinesType()
        {
            switch (settings.Get<LineType>("ConstLinesType"))
            {
                default:
                case LineType.Traditional:
                    sky.ConstellationLines = ConstLinesTraditional;
                    break;
                case LineType.Rey:
                    sky.ConstellationLines = ConstLinesRey;
                    break;
            }
        }

        /// <summary>
        /// Loads constellation borders data
        /// </summary>
        private void LoadBordersData()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Borders.dat");
            
            using (var sr = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                List<CrdsEquatorial> block = null;
                while (sr.BaseStream.Position != sr.BaseStream.Length)
                {
                    bool start = sr.ReadBoolean();
                    if (start)
                    {
                        block = new List<CrdsEquatorial>();
                        Borders.Add(block);
                    }

                    double ra = sr.ReadDouble();
                    double dec = sr.ReadDouble();
                    block.Add(new CrdsEquatorial(ra, dec));
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

        private List<Tuple<int, int>> LoadLinesData(string fileName)
        {
            List<Tuple<int, int>> lines = new List<Tuple<int, int>>();
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Data/{fileName}");
            string[] chunks;
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

                    lines.Add(new Tuple<int, int>(from, to));
                }
            }

            return lines;
        }

        /// <summary>
        /// Type of constellation line
        /// </summary>
        public enum LineType
        {
            [Description("Settings.ConstLinesType.Traditional")]
            Traditional = 0,

            [Description("Settings.ConstLinesType.Rey")]
            Rey = 1,
        }
    }
}
