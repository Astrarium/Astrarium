using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace ADK
{
    public class Constellations
    {
        private static List<Border> Borders = null;

        private struct Border
        {
            public float ra_upper { get; private set; }
            public float ra_lower { get; private set; }
            public float dec { get; private set; }
            public string name { get; private set; }

            public Border(float ra_upper, float ra_lower, float dec, string name)
            {
                this.ra_upper = ra_upper;
                this.ra_lower = ra_lower;
                this.dec = dec;
                this.name = name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eq0"></param>
        /// <param name="jd"></param>
        /// <returns></returns>
        /// <remarks>
        /// Implementation is based on <see href="ftp://cdsarc.u-strasbg.fr/pub/cats/VI/42/"/>.
        /// </remarks>
        public static string GetConstellationByCoordinates(CrdsEquatorial eq0, double jd)
        {
            // Load borders data if needed
            if (Borders == null)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ADK.Data.Constells.dat"))
                using (var reader = new StreamReader(stream))
                {
                    Borders = new List<Border>();

                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        string line = reader.ReadLine();
                        float ra_lower = float.Parse(line.Substring(0, 8), CultureInfo.InvariantCulture);
                        float ra_upper = float.Parse(line.Substring(9, 7), CultureInfo.InvariantCulture);
                        float dec = float.Parse(line.Substring(18, 7), CultureInfo.InvariantCulture);
                        string name = line.Substring(26, 3);

                        Borders.Add(new Border(ra_upper, ra_lower, dec, name));
                    }
                }
            }

            // precessional elements for B1950 epoch
            var p = Precession.ElementsFK4(jd, Date.EPOCH_B1950);

            // Equatorial coordinates for B1950 epoch 
            CrdsEquatorial eq = Precession.GetEquatorialCoordinatesOfEpoch(eq0, p);

            double alpha = eq.Alpha / 15.0;
            double delta = eq.Delta;

            for (int i = 0; i < Borders.Count; i++)
            {
                if (Borders[i].dec > delta) continue;
                if (Borders[i].ra_upper <= alpha) continue;
                if (Borders[i].ra_lower > alpha) continue;

                if (alpha >= Borders[i].ra_lower && alpha < Borders[i].ra_upper && Borders[i].dec <= delta) return Borders[i].name;
                else if (Borders[i].ra_upper < alpha) continue;
            }
            return "";
        }
    }
}
