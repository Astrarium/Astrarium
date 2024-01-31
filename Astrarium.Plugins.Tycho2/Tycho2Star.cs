using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.Tycho2
{
    /// <summary>
    /// Represents single star from Tycho2 star catalog
    /// </summary>
    public class Tycho2Star : CelestialObject, IMagnitudeObject, IObservableObject
    {
        /// <inheritdoc />
        public override string Type => "Star";

        /// <inheritdoc />
        public override string CommonName => ToString();

        /// <summary>
        /// Right ascension at J2000.0 epoch, in degrees
        /// </summary>
        public double Alpha0 { get; set; }

        /// <summary>
        /// Declination at J2000.0 epoch, in degrees
        /// </summary>
        public double Delta0 { get; set; }

        /// <summary>
        /// Cartesian coordinates of the star
        /// </summary>
        //public Vec3 Cartesian { get; set; }

        /// <summary>
        /// Proper motion in RA
        /// </summary>
        public float PmRA { get; set; }

        /// <summary>
        /// Proper motion in Dec
        /// </summary>
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
        /// Approximate spectral class
        /// </summary>
        public char SpectralClass { get; set; }

        /// <summary>
        /// Proper name of the star, if exists
        /// </summary>
        public string ProperName { get; set; }

        /// <summary>
        /// Gets star names
        /// </summary>
        public override string[] Names
        {
            get 
            {
                if (ProperName != null)
                {
                    return new string[] { ProperName, ToString() };
                }
                else
                {
                    return new string[] { ToString() };
                }
            }
        }

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

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Stars", "Tycho2" };
    }
}
