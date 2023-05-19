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
        //public List<List<CelestialPoint>> ConstBorders { get; private set; } = new List<List<CelestialPoint>>();

        public List<List<Vec3>> Borders { get; private set; } = new List<List<Vec3>>();

        /// <summary>
        /// List of constellation lines (traditional)
        /// </summary>
        public List<Tuple<int, int>> ConstLinesTraditional { get; private set; } = new List<Tuple<int, int>>();

        /// <summary>
        /// List of constellation lines (H.A.Rey, "The Stars: A New Way To See Them")
        /// </summary>
        public List<Tuple<int, int>> ConstLinesRey { get; private set; } = new List<Tuple<int, int>>();

        public Mat4 MatPrecession { get; private set; }
        public Mat4 MatPrecession0 { get; private set; }

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
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            // precessional elements from current epoch to J2000
            var p0 = Precession.ElementsFK5(context.JulianDay, Date.EPOCH_J2000);

            MatPrecession =
                Mat4.ZRotation(Angle.ToRadians(p.z)) *
                Mat4.YRotation(Angle.ToRadians(-p.theta)) *
                Mat4.ZRotation(Angle.ToRadians(p.zeta));

            MatPrecession0 =
                Mat4.ZRotation(Angle.ToRadians(p0.z)) *
                Mat4.YRotation(Angle.ToRadians(-p0.theta)) *
                Mat4.ZRotation(Angle.ToRadians(p0.zeta));
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
                List<Vec3> block = null;
                while (sr.BaseStream.Position != sr.BaseStream.Length)
                {
                    bool start = sr.ReadBoolean();
                    if (start)
                    {
                        block = new List<Vec3>();
                        Borders.Add(block);
                    }

                    double lon = Angle.ToRadians(sr.ReadDouble());
                    double lat = Angle.ToRadians(sr.ReadDouble());

                    var vec = Projection.SphericalToCartesian(lon, lat);

                    block.Add(vec);
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
                        Cartesian = Projection.SphericalToCartesian(Angle.ToRadians(sr.ReadSingle()), Angle.ToRadians(sr.ReadSingle()))
                    });
                }
            }
        }

        private List<Tuple<int, int>> LoadLinesData(string fileName)
        {
            List<Tuple<int, int>> lines = new List<Tuple<int, int>>();
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Data/{fileName}");
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
