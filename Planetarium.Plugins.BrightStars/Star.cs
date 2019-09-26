using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.BrightStars
{
    public class Star : CelestialObject
    {
        /// <summary>
        /// Greek alphabet abbreviations
        /// </summary>
        private static List<string> Alphabet = new List<string> {
            "Alp", "Bet", "Gam", "Del", "Eps", "Zet", "Eta", "The", "Iot", "Kap", "Lam", "Mu",
            "Nu", "Xi", "Omi", "Pi", "Rho", "FSg", "Sig", "Tau", "Ups", "Phi", "Chi", "Psi", "Ome" };

        /// <summary>
        /// Superscript digits
        /// </summary>
        private static string[] SuperscriptDigits = new string[] {
            "⁰", "¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹"
        };

        /// <summary>
        /// Star number in BSC catalogue (= HR number = Harvard Revised Number = Bright Star Number) 
        /// </summary>
        public ushort Number { get; set; }

        /// <summary>
        /// Star name, generally Bayer and/or Flamsteed name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Proper name of the star
        /// </summary>
        public string ProperName { get; set; }

        /// <summary>
        /// Gets Bayer designation of the star name
        /// </summary>
        public string BayerName
        {
            get
            {
                if (Name[3] != ' ')
                {
                    string letter = Name.Substring(3, 3).Trim();

                    int i = Alphabet.IndexOf(letter);
                    if (i >= 0)
                    {
                        letter = ((char)('\u03b1' + i)).ToString();
                    }

                    if (Name[6] != ' ')
                    {
                        int digit = int.Parse(Name[6].ToString());
                        return $"{letter}\u2006{SuperscriptDigits[digit]}";
                    }
                    else
                    {
                        return letter;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets Flamesteed designation of the star name
        /// </summary>
        public string FlamsteedNumber
        {
            get
            {
                string name = Name.Substring(0, 3).Trim();
                return string.IsNullOrEmpty(name) ? null : name;
            }
        }

        /// <summary>
        /// Gets variable star designation (like RR, V399)
        /// </summary>
        public string VariableName { get; set; }

        public uint HDNumber { get; set; }

        public uint SAONumber { get; set; }

        public ushort FK5Number { get; set; }

        /// <summary>
        /// Equatorial coordinates for the catalogue epoch
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();

        /// <summary>
        /// Annual proper motion in RA J2000, FK5 system, arcsec/yr
        /// </summary>
        public float PmAlpha { get; set; }

        /// <summary>
        /// Annual proper motion in Dec J2000, FK5 system, arcsec/yr
        /// </summary>
        public float PmDelta { get; set; }

        /// <summary>
        /// Apparent magnitude of the star
        /// </summary>
        public float Mag { get; set; }

        /// <summary>
        /// Star color, i.e. spectral class
        /// </summary>
        public char Color { get; set; }
    }
}
