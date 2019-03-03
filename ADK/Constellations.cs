using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Contains methods to find constellation by coordinates of a point on celestial sphere.
    /// </summary>
    public class Constellations
    {
        /// <summary>
        /// Constellations borders data
        /// </summary>
        private static List<Border> Borders = null;

        /// <summary>
        /// Represents constellation border segment.
        /// </summary>
        private struct Border
        {
            /// <summary>
            /// Right Ascension point 1 
            /// </summary>
            public float RA1 { get; private set; }

            /// <summary>
            /// Right Ascension point 2 
            /// </summary>
            public float RA2 { get; private set; }

            /// <summary>
            /// Declination
            /// </summary>
            public float Dec { get; private set; }

            /// <summary>
            /// Constallation name (3-letter code).
            /// </summary>
            public string ConstName { get; private set; }

            /// <summary>
            /// Creates new constellation border segment.
            /// </summary>
            public Border(float ra1, float ra2, float dec, string constName)
            {
                this.RA1 = ra2;
                this.RA2 = ra1;
                this.Dec = dec;
                this.ConstName = constName;
            }
        }

        /// <summary>
        /// Finds constellation name by point with specified equatorial coordinates for any epoch.
        /// </summary>
        /// <param name="eq">Equatorial coordinates of the point for any epoch.</param>
        /// <param name="epoch">Epoch value, in Julian Days.</param>
        /// <returns>International 3-letter code of a constellation.</returns>
        /// <remarks>
        /// Implementation is based on <see href="ftp://cdsarc.u-strasbg.fr/pub/cats/VI/42/"/>.
        /// </remarks>
        public static string FindConstellation(CrdsEquatorial eq, double epoch)
        {
            var pe = Precession.ElementsFK5(epoch, Date.EPOCH_B1875);
            var eq1875 = Precession.GetEquatorialCoordinates(eq, pe);
            return FindConstellation(eq1875);
        }

        /// <summary>
        /// Finds constellation name by point with specified equatorial coordinates for epoch B1875.
        /// </summary>
        /// <param name="eq1875">Equatorial coordinates of the point for epoch B1875.</param>
        /// <returns>International 3-letter code of a constellation.</returns>
        /// <remarks>
        /// Implementation is based on <see href="ftp://cdsarc.u-strasbg.fr/pub/cats/VI/42/"/>.
        /// </remarks>
        public static string FindConstellation(CrdsEquatorial eq1875)
        {
            // Load borders data if needed
            if (Borders == null)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ADK.Data.Constells.dat"))
                using (var reader = new StreamReader(stream))
                {
                    Borders = new List<Border>();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        float ra1 = float.Parse(line.Substring(0, 8), CultureInfo.InvariantCulture);
                        float ra2 = float.Parse(line.Substring(9, 7), CultureInfo.InvariantCulture);
                        float dec = float.Parse(line.Substring(17, 8), CultureInfo.InvariantCulture);
                        string constName = line.Substring(26, 3);

                        Borders.Add(new Border(ra1, ra2, dec, constName));
                    }
                }
            }

            double alpha = eq1875.Alpha / 15.0;
            double delta = eq1875.Delta;

            for (int i = 0; i < Borders.Count; i++)
            {
                if (Borders[i].Dec > delta)
                    continue;

                if (Borders[i].RA1 <= alpha)
                    continue;

                if (Borders[i].RA2 > alpha)
                    continue;

                if (alpha >= Borders[i].RA2 &&
                    alpha < Borders[i].RA1 &&
                    Borders[i].Dec <= delta)
                {
                    return Borders[i].ConstName;
                }
                else if (Borders[i].RA1 < alpha)
                {
                    continue;
                }
            }

            return "";
        }
    }
}
