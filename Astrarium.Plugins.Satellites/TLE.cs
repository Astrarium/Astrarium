using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Astrarium.Algorithms;

namespace Astrarium.Plugins.Satellites
{
    public class TLE
    {
        /// <summary>
        /// [n0] Mean motion at epoch, in revolutions per day
        /// </summary>
        public double MeanMotion { get; private set; }

        /// <summary>
        /// [i0] Mean inclination at epoch, in degrees
        /// </summary>
        public double Inclination { get; private set; }

        /// <summary>
        /// [e0] Eccentricity
        /// </summary>
        public double Eccentricity { get; private set; }

        /// <summary>
        /// [BStar] BStar drag term
        /// </summary>
        public double BStar { get; private set; }

        /// <summary>
        /// [w0] Argument of perigee at epoch, in degrees
        /// </summary>
        public double ArgumentOfPerigee { get; private set; }

        /// <summary>
        /// [M0] Mean anomaly at epoch, in degrees
        /// </summary>
        public double MeanAnomaly { get; private set; }

        /// <summary>
        /// [OMEGA0] Longitude of ascending node, in degrees
        /// </summary>
        public double LongitudeAscNode { get; private set; }

        /// <summary>
        /// Number of satellite in SATCAT database (Satellite Catalog Number)
        /// </summary>
        public string SatelliteNumber { get; private set; }

        /// <summary>
        /// International Designator of satellite (COSPAR ID)
        /// </summary>
        public string InternationalDesignator { get; private set; }

        /// <summary>
        /// Epoch of elements, in julian days
        /// </summary>
        public double Epoch { get; private set; }

        /// <summary>
        /// Constructs new satellite orbit by TLE notation:
        /// [http://celestrak.com/NORAD/documentation/tle-fmt.asp]
        /// [http://celestrak.com/columns/v04n03/]
        /// </summary>
        /// <param name="line1">Line 1</param>
        /// <param name="line2">Line 2</param>
        public TLE(string line1, string line2)
        {
            try
            {
                SatelliteNumber = line1.Substring(2, 5).Trim();
                InternationalDesignator = line1.Substring(9, 8).Trim();

                // 57-99 correspond to 1957-1999 and those from 
                // 00-56 correspond to 2000-2056
                int year = Convert.ToInt32(line1.Substring(18, 2));
                double day = Convert.ToDouble(line1.Substring(20, 12).Trim(), CultureInfo.InvariantCulture);

                if (year >= 57 && year <= 99) year += 1900;
                else year += 2000;

                Epoch = Date.JulianDay0(year) + day;

                string bstar = line1.Substring(53, 8);
                int bstar_sgn = (bstar[0] == '-') ? -1 : 1;
                double bstar_mnt = Convert.ToDouble("0." + bstar.Substring(1, 5).Trim(), CultureInfo.InvariantCulture);
                double bstar_exp = Convert.ToDouble(bstar.Substring(6, 2).Trim().Replace("+", ""));
                BStar = bstar_sgn * bstar_mnt * Math.Pow(10, bstar_exp);

                Inclination = Convert.ToDouble(line2.Substring(8, 8).Trim(), CultureInfo.InvariantCulture);
                LongitudeAscNode = Convert.ToDouble(line2.Substring(17, 8).Trim(), CultureInfo.InvariantCulture);
                Eccentricity = Convert.ToDouble("0." + line2.Substring(26, 7).Trim(), CultureInfo.InvariantCulture);
                ArgumentOfPerigee = Convert.ToDouble(line2.Substring(34, 8).Trim(), CultureInfo.InvariantCulture);
                MeanAnomaly = Convert.ToDouble(line2.Substring(43, 8).Trim(), CultureInfo.InvariantCulture);
                MeanMotion = Convert.ToDouble(line2.Substring(52, 11).Trim(), CultureInfo.InvariantCulture);
            }
            catch { }
        }

        /// <summary>
        /// Gets period of revolution, in minutes
        /// </summary>
        public double Period => 1440.0 / MeanMotion;
    }
}
