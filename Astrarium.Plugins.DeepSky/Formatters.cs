using Astrarium.Types;
using System;

namespace Astrarium.Plugins.DeepSky
{
    internal class DeepSkyAngularSizeFormatter : IEphemFormatter
    {
        public string Format(object value)
        {
            if (value == null)
                return null;
            else
                return Formatters.Angle.Format(Convert.ToSingle(value) / 3600);
        }
    }

    internal class DeepSkySurfaceBrightnessFormatter : Formatters.SignedDoubleFormatter
    {
        internal DeepSkySurfaceBrightnessFormatter() : base(2, " mag/sq.arcsec") { }
    }

    internal class DeepSkyPositionAngleFormatter : Formatters.UnsignedDoubleFormatter
    {
        internal DeepSkyPositionAngleFormatter() : base(0, "\u00B0") { }
    }
}