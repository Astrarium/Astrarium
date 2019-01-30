using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    public class Star : CelestialObject
    {
        /// <summary>
        /// Greek alphabet abbreviations
        /// </summary>
        private static IDictionary<string, string> Alphabet = new Dictionary<string, string>() {
            {"Alp", "α"}, {"Bet", "β"}, {"Gam", "γ"}, {"Del", "δ"}, {"Eps", "ε"}, {"Zet", "ζ"}, {"Eta", "η"}, {"The", "θ"}, {"Iot", "ι"}, {"Kap", "κ"}, {"Lam", "λ"}, {"Mu", "μ"}, {"Nu", "ν"}, {"Xi", "ξ"}, {"Omi", "ο"}, {"Pi", "π"}, {"Rho", "ρ"}, {"Sig", "σ"}, {"Tau", "τ"}, {"Ups", "υ"}, {"Phi", "φ"}, {"Chi", "χ"}, {"Psi", "ψ"}, {"Ome", "ω"}
        };

        /// <summary>
        /// Superscript digits
        /// </summary>
        private static string[] SuperscrriptDigits = new string[] {
            "⁰", "¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹"
        };

        /// <summary>
        /// Star number in BSC catalogue (= HR number = Harvard Revised Number = Bright Star Number) 
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Star name, generally Bayer and/or Flamsteed name
        /// </summary>
        public string Name { get; set; }

        public string BayerName
        {
            get
            {
                if (Name[2] != ' ')
                {
                    string letter = Name.Substring(3, 3).Trim();  
                    
                    if (Alphabet.ContainsKey(letter))
                    {
                        letter = Alphabet[letter];
                    }

                    if (Name[6] != ' ')
                    {
                        int digit = int.Parse(Name[6].ToString());
                        return $"{letter}\u2006{SuperscrriptDigits[digit]}";
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

        public string FlamsteedNumber
        {
            get
            {
                string name = Name.Substring(0, 3).Trim();
                return string.IsNullOrEmpty(name) ? null : name;
            }
        }

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
