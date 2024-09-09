using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astrarium.Plugins.BrightStars
{
    public class Star : CelestialObject, IMagnitudeObject, IObservableObject
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

        /// <inheritdoc />
        public override string Type => "Star";

        /// <inheritdoc />
        public override string CommonName => $"HR {Number}";

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
        /// RA for the catalogue epoch, in degrees 
        /// </summary>
        public float Alpha0 { get; set; }

        /// <summary>
        /// Dec for the catalogue epoch, in degrees 
        /// </summary>
        public float Delta0 { get; set; }

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
        public float Magnitude { get; set; }

        /// <summary>
        /// Star color, i.e. spectral class
        /// </summary>
        public char Color { get; set; }

        /// <summary>
        /// Gets array of star names
        /// </summary>
        public override string[] Names
        {
            get
            {
                return GetNames(this).ToArray();
            }
        }

        /// <summary>
        /// External function to get star names
        /// </summary>
        internal static Func<Star, ICollection<string>> GetNames { get; set; }

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Stars" };
    }
}
