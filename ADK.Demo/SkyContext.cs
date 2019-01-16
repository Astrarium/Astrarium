using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class SkyContext
    {
        public SkyContext(double jd, CrdsGeographical location)
        {
            GeoLocation = location;
            JulianDay = jd;
        }

        private double _JulianDay;

        /// <summary>
        /// Julian ephemeris day
        /// </summary>
        public double JulianDay
        {
            get { return _JulianDay; }
            set
            {
                _JulianDay = value;
                NutationElements = Nutation.NutationElements(_JulianDay);
                AberrationElements = Aberration.AberrationElements(_JulianDay);
                Epsilon = Date.TrueObliquity(_JulianDay, NutationElements.deltaEpsilon);
                SiderealTime = Date.ApparentSiderealTime(_JulianDay, NutationElements.deltaPsi, Epsilon);
            }
        }

        /// <summary>
        /// Geographical coordinates of the observer
        /// </summary>
        public CrdsGeographical GeoLocation { get; set; }

        /// <summary>
        /// Apparent sidereal time at Greenwich (theta0), in degrees
        /// </summary>
        public double SiderealTime { get; private set; }

        /// <summary>
        /// Elements to calculate nutation effect
        /// </summary>
        public NutationElements NutationElements { get; private set; }

        /// <summary>
        /// Elements to calculate aberration effect
        /// </summary>
        public AberrationElements AberrationElements { get; private set; }

        /// <summary>
        /// True obliquity of the ecliptic, in degrees
        /// </summary>
        public double Epsilon { get; private set; }

        /// <summary>
        /// Extra data to store within the context
        /// </summary>
        public dynamic Data { get; } = new ExpandoObject();
    }
}
