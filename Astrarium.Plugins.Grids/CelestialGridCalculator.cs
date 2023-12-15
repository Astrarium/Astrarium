using Astrarium.Algorithms;
using Astrarium.Types;
using System;

namespace Astrarium.Plugins.Grids
{
    public class CelestialGridCalculator : BaseCalc
    {
        /// <summary>
        /// Matrix for conversion from Equatorial to Ecliptical
        /// </summary>
        public Mat4 MatEcliptic { get; private set; }

        /// <summary>
        /// Matrix for conversion from Equatorial to Galactical
        /// </summary>
        public Mat4 MatGalactic { get; private set; }

        /// <summary>
        /// Matrix for drawing celestial meridian
        /// </summary>
        public Mat4 MatMeridian { get; private set; }

        /// <summary>
        /// Longitude of lunar ascending node, in radians
        /// </summary>
        public double LunarAscendingNodeLongitude { get; private set; }

        public PrecessionalElements PrecessionalElementsB1950ToCurrent { get; private set; }

        public override void Calculate(SkyContext context)
        {
            MatEcliptic = Mat4.XRotation(Angle.ToRadians(context.Epsilon));

            PrecessionalElementsB1950ToCurrent = Precession.ElementsFK5(Date.EPOCH_B1950, context.JulianDay);

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

            MatMeridian = Mat4.XRotation(Math.PI / 2);

            LunarAscendingNodeLongitude = Angle.ToRadians(LunarEphem.TrueAscendingNode(context.JulianDay));
        }
    }
}
