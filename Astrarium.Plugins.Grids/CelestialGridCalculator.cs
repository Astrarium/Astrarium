using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override void Calculate(SkyContext context)
        {
            MatEcliptic = Mat4.XRotation(Angle.ToRadians(context.Epsilon));

            // J2000.0 galactical reference points
            // 192.855 = 12h 51.4m (alpha0)
            // 27.12825 (delta0)
            // 122.93314 (lon)
            MatGalactic = Mat4.ZRotation(Angle.ToRadians(192.855)) * Mat4.YRotation(Angle.ToRadians(90 - 27.12825)) * Mat4.ZRotation(Angle.ToRadians(180 - 122.93314));

            MatMeridian = Mat4.XRotation(Math.PI / 2);
        }
    }
}
