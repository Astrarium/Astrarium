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

        public override void Calculate(SkyContext context)
        {
            MatEcliptic = Mat4.XRotation(Angle.ToRadians(context.Epsilon));

            MatGalactic = Mat4.ZRotation(Angle.ToRadians(-123.5)) * Mat4.XRotation(Angle.ToRadians(27.4)) * Mat4.ZRotation(Angle.ToRadians(12.25));
        }
    }
}
