using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    /// <summary>
    /// Represents single star from Tycho2 star catalog
    /// </summary>
    public class Tycho2Star :  CelestialObject
    {
        /// <summary>
        /// Equatorial coordinates of the star at current epoch
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

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
        /// Gets Tycho2 star designation name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("TYC {0}-{1}-{2}", Tyc1, Tyc2, Tyc3);
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj is Tycho2Star)
        //    {
        //        Tycho2Star star = obj as Tycho2Star;
        //        return star.ToString() == ToString();
        //    }
        //    return false;
        //}
    }
}
