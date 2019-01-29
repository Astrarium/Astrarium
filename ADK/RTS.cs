namespace ADK
{
    /// <summary>
    /// Describes conditions of celestial object visibility:
    /// instants of rising, transit and setting.
    /// </summary>
    public class RTS
    {
        /// <summary>
        /// Instant of rising, in fractions of a day.
        /// 0 means local midnight, 0.5 is a noon.
        /// </summary>
        public double Rise { get; set; } = None;

        /// <summary>
        /// Instant of trasnit, in fractions of a day.
        /// 0 means local midnight, 0.5 is a noon.
        /// </summary>
        public double Transit { get; set; } = None;

        /// Instant of setting, in fractions of a day.
        /// 0 means local midnight, 0.5 is a noon.
        /// </summary>
        public double Set { get; set; } = None;

        /// <summary>
        /// Azimuth of the body at the instant of rising.
        /// </summary>
        public double RiseAzimuth { get; set; } = None;

        /// <summary>
        /// Azimuth of the body at the instant of setting.
        /// </summary>
        public double SetAzimuth { get; set; } = None;

        /// <summary>
        /// Altitude of the body at the instant of transit.
        /// </summary>
        public double TransitAltitude { get; set; } = None;

        /// <summary>
        /// Gets duration of visibility (time when the body is above the horizon), in fractions of a day.
        /// </summary>
        public double Duration
        {
            get
            {
                if (Rise.Equals(None) && Set.Equals(None))
                {
                    if (TransitAltitude > 0)
                        return 1;
                    else
                        return 0;
                }
                else if (Rise.Equals(None))
                {
                    return Set;
                }
                else if (Set.Equals(None))
                {
                    return 1 - Rise;
                }
                else
                {
                    if (Rise < Set)
                        return Set - Rise;
                    else
                        return 1 - Rise + Set;
                }
            }
        }

        /// <summary>
        /// This value means no event took place for the day.
        /// </summary>
        public const double None = double.NaN;
    }
}
