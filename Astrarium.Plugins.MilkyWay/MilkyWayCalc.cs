using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Astrarium.Plugins.MilkyWay
{
    /// <summary>
    /// Calculates coordinates of Milky Way outline points to be rendered on map
    /// </summary>
    public class MilkyWayCalc : BaseCalc
    {
        /// <summary>
        /// Altitude of the Sun above horizon
        /// </summary>
        public double SunAltitude { get; private set; }
        
        public Mat4 MatGalactic { get; private set; }

        private readonly ISky sky;

        public MilkyWayCalc(ISky sky)
        {
            this.sky = sky;
        }

        public override void Calculate(SkyContext context)
        {
            // solar altitude, in degrees
            SunAltitude = context.Get(sky.SunEquatorial).ToHorizontal(context.GeoLocation, context.SiderealTime).Altitude;

            // precessional elements from J2000 to current epoch
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            // precession matrix used to convert J2000.0 galactical coordinates to current epoch
            var matPrecession =
                Mat4.ZRotation(Angle.ToRadians(p.z)) *
                Mat4.YRotation(Angle.ToRadians(-p.theta)) *
                Mat4.ZRotation(Angle.ToRadians(p.zeta));

            // J2000.0 galactical reference points
            // 192.855 = 12h 51.4m (alpha0)
            // 27.12825 (delta0)
            // 122.93314 (lon)
            MatGalactic = matPrecession * Mat4.ZRotation(Angle.ToRadians(192.855)) * Mat4.YRotation(Angle.ToRadians(90 - 27.12825)) * Mat4.ZRotation(Angle.ToRadians(180 - 122.93314));

        }
    }
}
