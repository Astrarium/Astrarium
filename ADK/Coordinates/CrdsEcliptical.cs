using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Represents a pair of ecliptical coordinates, 
    /// optionally complemented with distance from the Sun.
    /// </summary>
    public class CrdsEcliptical
    {
        /// <summary>
        /// Ecliptical longitude, in degrees. 
        /// Measured from the vernal equinox along the ecliptic.
        /// </summary>
        public double Lambda { get; set; }

        /// <summary>
        /// Ecliptical latitude, in degrees. 
        /// Positive if north of the ecliptic, negative if south.
        /// </summary>
        public double Beta { get; set; }
        
        /// <summary>
        /// Distance.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Creates new ecliptical coordinates with default values.
        /// </summary>
        public CrdsEcliptical() { }

        /// <summary>
        /// Creates new ecliptical coordinates.
        /// </summary>
        /// <param name="lambda">Ecliptical longitude, in degrees. Measured from the vernal equinox along the ecliptic.</param>
        /// <param name="beta">Ecliptical latitude, in degrees. Positive if north of the ecliptic, negative if south.</param>
        /// <param name="distance">Distance from the Sun, in astronomical units.</param>
        public CrdsEcliptical(double lambda, double beta, double distance = 0)
        {
            Lambda = lambda;
            Beta = beta;
            Distance = distance;
        }

        /// <summary>
        /// Creates new ecliptical coordinates.
        /// </summary>
        /// <param name="lambda">Ecliptical longitude, in degrees. Measured from the vernal equinox along the ecliptic.</param>
        /// <param name="beta">Ecliptical latitude, in degrees. Positive if north of the ecliptic, negative if south.</param>
        /// <param name="distance">Distance from the Sun, in astronomical units.</param>
        public CrdsEcliptical(DMS lambda, DMS beta, double distance = 0)
        {
            Lambda = lambda.ToDecimalAngle();
            Beta = beta.ToDecimalAngle();
            Distance = distance;
        }

        /// <summary>
        /// Sets new values of ecliptical coordinates.
        /// </summary>
        /// <param name="lambda">Ecliptical longitude, in degrees. Measured from the vernal equinox along the ecliptic.</param>
        /// <param name="beta">Ecliptical latitude, in degrees. Positive if north of the ecliptic, negative if south.</param>
        /// <param name="distance">Distance from the Sun, in astronomical units.</param>
        public void Set(double lambda, double beta, double distance = 0)
        {
            Lambda = lambda;
            Beta = beta;
            Distance = distance;
        }
        
        /// <summary>
        /// Adds corrections to ecliptical coordinates
        /// </summary>
        public static CrdsEcliptical operator +(CrdsEcliptical lhs, CrdsEcliptical rhs)
        {
            CrdsEcliptical ecl = new CrdsEcliptical();
            ecl.Lambda = Angle.To360(lhs.Lambda + rhs.Lambda);
            ecl.Beta = lhs.Beta + rhs.Beta;
            ecl.Distance = lhs.Distance;
            return ecl;
        }

        /// <summary>
        /// Subtracts corrections to ecliptical coordinates
        /// </summary>
        public static CrdsEcliptical operator -(CrdsEcliptical lhs, CrdsEcliptical rhs)
        {
            CrdsEcliptical ecl = new CrdsEcliptical();
            ecl.Lambda = Angle.To360(lhs.Lambda - rhs.Lambda);
            ecl.Beta = lhs.Beta - rhs.Beta;
            ecl.Distance = lhs.Distance;
            return ecl;
        }
    }
}
