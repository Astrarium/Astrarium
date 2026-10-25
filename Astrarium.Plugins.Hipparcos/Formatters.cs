using Astrarium.Types;

namespace Astrarium.Plugins.Hipparcos
{
    /// <summary>
    /// Internal formatters
    /// </summary>
    internal static class Formatters
    {
        /// <summary>
        /// Formats double value as an angle value expressed in milliarcseconds with "mas" unit of measure
        /// </summary>
        internal static IEphemFormatter Mas = new UnsignedDoubleFormatter(3, " mas");

        /// <summary>
        /// Formats double value as a distance expressed in light years
        /// </summary>
        internal static IEphemFormatter Distance = new UnsignedDoubleFormatter(0, " ly");
        internal static IEphemFormatter VarFlag = new VarFlagFormatter();
        internal static IEphemFormatter VarType = new VarTypeFormatter();
        internal static IEphemFormatter MagDiff = new UnsignedDoubleFormatter(2, "ᵐ"); 
        internal static IEphemFormatter PosA = new UnsignedDoubleFormatter(0, "°");
        internal static IEphemFormatter Sep = new UnsignedDoubleFormatter(3, "″");
        internal static IEphemFormatter VarPeriod = new UnsignedDoubleFormatter(2, " d");
        internal static IEphemFormatter ProperMotion = new SignedDoubleFormatter(2, " mas/yr");

        private class VarFlagFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                switch (value)
                {
                    case (byte)1: return "< 0.06ᵐ";
                    case (byte)2: return "0.06ᵐ … 0.6ᵐ";
                    case (byte)3: return "> 0.6ᵐ";
                    default: return "—";
                }
            }
        }

        private class VarTypeFormatter : IEphemFormatter
        {
            public string Format(object value)
            {
                switch (value)
                {
                    case (byte)'C': return "Constant (no variability)";
                    case (byte)'D': return "Duplicity-induced variability";
                    case (byte)'M': return "Possibly micro-variable";
                    case (byte)'P': return "Periodic variable";
                    case (byte)'R': return "V-I colour index was revised due to variability analysis";
                    case (byte)'U': return "Variable of unsolved category";
                    default: return "—";
                }
            }
        }
    }
}
