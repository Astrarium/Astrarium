using ADK;
using Planetarium.Objects;

namespace Planetarium.Plugins.Tycho2
{
    /// <summary>
    /// Represents single star from Tycho2 star catalog
    /// </summary>
    public class Tycho2Star : CelestialObject
    {
        /// <summary>
        /// Equatorial coordinates of the star at current epoch
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Equatorial coordinates of the at J2000.0 epoch
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; }

        public float PmRA { get; set; }

        public float PmDec { get; set; }

        /// <summary>
        /// Tycho2 star designation number, first part
        /// </summary>
        public short Tyc1 { get; set; }

        /// <summary>
        /// Tycho2 star designation number, second part
        /// </summary>
        public short Tyc2 { get; set; }

        /// <summary>
        /// Tycho2 star designation number, third part
        /// </summary>
        public char Tyc3 { get; set; }

        /// <summary>
        /// Magnitude
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Gets star names
        /// </summary>
        public override string[] Names => new[] { ToString() };

        /// <summary>
        /// Gets Tycho2 star designation name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("TYC {0}-{1}-{2}", Tyc1, Tyc2, Tyc3);
        }

        public override bool Equals(object obj)
        {
            if (obj is Tycho2Star)
            {
                Tycho2Star star = obj as Tycho2Star;
                return 
                    star.Tyc1 == Tyc1 &&
                    star.Tyc2 == Tyc2 &&
                    star.Tyc3 == Tyc3;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Tyc1.GetHashCode();
            hash = hash * 23 + Tyc2.GetHashCode();
            hash = hash * 23 + Tyc3.GetHashCode();
            return hash;
        }
    }
}
