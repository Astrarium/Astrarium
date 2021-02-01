using Astrarium.Types;

namespace Astrarium.Plugins.Eclipses.Types
{
    /// <summary>
    /// Utility class to store formatters used to convert data to strings
    /// </summary>
    public static class Format
    {
        /// <summary>
        /// Geographical coordinates formatter
        /// </summary>
        public static readonly IEphemFormatter Geo = new Formatters.GeoCoordinatesFormatter();
        
        /// <summary>
        /// Time values formatter
        /// </summary>
        public static readonly IEphemFormatter Time = new Formatters.TimeFormatter(withSeconds: true);
        
        /// <summary>
        /// Altitude values formatter
        /// </summary>
        public static readonly IEphemFormatter Alt = new Formatters.SignedDoubleFormatter(1, "\u00B0");
        
        /// <summary>
        /// Angle values formatter
        /// </summary>
        public static readonly IEphemFormatter Angle = new Formatters.UnsignedDoubleFormatter(1, "\u00B0");
        
        /// <summary>
        /// Solar eclipse magnitude formatter
        /// </summary>
        public static readonly IEphemFormatter Mag = new Formatters.UnsignedDoubleFormatter(3, "");
        
        /// <summary>
        /// Moon/Sun visible diameter formatter
        /// </summary>
        public static readonly IEphemFormatter Ratio = new Formatters.UnsignedDoubleFormatter(4, "");
        
        /// <summary>
        /// Total path width formatter 
        /// </summary>
        public static readonly IEphemFormatter PathWidth = new Formatters.UnsignedDoubleFormatter(0, " km");
    }
}
