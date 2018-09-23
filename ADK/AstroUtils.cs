using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace ADK
{
    public static partial class AstroUtils
    {
        /// <summary>
        /// 1 radian in degrees 
        /// </summary>
        private const double RAD = 180.0 / Math.PI;

        /// <summary>
        /// Converts angle value expressed in degrees to radians
        /// </summary>
        /// <param name="angle">Angle value in degrees</param>
        /// <returns>Angle value expressed in radians</returns>
        public static double ToRadian(double angle)
        {
            return angle / RAD;
        }

        public static double ToDegree(double angle)
        {
            return angle * RAD;
        }

        /// <summary>
        /// Normalizes angle value expressed in degrees to value in range from 0 to 360.
        /// </summary>
        /// <param name="angle">Angle value expressed in degrees.</param>
        /// <returns>Value expressed in degrees in range from 0 to 360</returns>
        public static double To360(double angle)
        {
            return angle - 360 * (long)(angle / 360.0) + (angle < 0 ? 360 : 0);
        }
    }
}
