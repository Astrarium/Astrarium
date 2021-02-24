using Astrarium.Algorithms;
using System;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class LunarEclipseCitiesListTableItem
    {
        public CrdsGeographical Location { get; private set; }

        public string LocationName { get; private set; }

        public double TimeZone { get; private set; }
        public string TimeZoneString { get; private set; }

        public CrdsGeographical Coordinates { get; private set; }
        public string CoordinatesString { get; private set; }

        public string P1String { get; private set; }
        public double? P1 { get; private set; }

        public string U1String { get; private set; }
        public double? U1 { get; private set; }

        public string U2String { get; private set; }
        public double? U2 { get; private set; }

        public string MaxString { get; private set; }
        public double? Max { get; private set; }

        public string U3String { get; private set; }
        public double? U3 { get; private set; }

        public string U4String { get; private set; }
        public double? U4 { get; private set; }

        public string P4String { get; private set; }
        public double? P4 { get; private set; }

        public string Visibility { get; private set; }

        public LunarEclipseCitiesListTableItem(LunarEclipseLocalCircumstances local, string visibility)
        {
            Location = local.Location;

            const string empty = "—";

            LocationName = local.Location.LocationName;
            Coordinates = local.Location;
            CoordinatesString = Format.Geo.Format(local.Location);

            double offset = local.Location.UtcOffset;
            string tz = local.Location.UtcOffset != 0 ? $"UTC{(offset < 0 ? "-" : "+")}{TimeSpan.FromHours(offset):h\\:mm}" : "UTC";

            TimeZone = offset;
            TimeZoneString = tz;

            P1 = local.PenumbralBegin?.JulianDay;
            P1String = local.PenumbralBegin != null ? $"{Format.Time.Format(new Date(local.PenumbralBegin.JulianDay, offset))}" : empty;

            U1 = local.PartialBegin?.JulianDay;
            U1String = local.PartialBegin != null ? $"{Format.Time.Format(new Date(local.PartialBegin.JulianDay, offset))}" : empty;

            U2 = local.TotalBegin?.JulianDay;
            U2String = local.TotalBegin != null ? $"{Format.Time.Format(new Date(local.TotalBegin.JulianDay, offset))}" : empty;

            Max = local.Maximum?.JulianDay;
            MaxString = local.Maximum != null ? $"{Format.Time.Format(new Date(local.Maximum.JulianDay, offset))}" : empty;

            U3 = local.TotalEnd?.JulianDay;
            U3String = local.TotalEnd != null ? $"{Format.Time.Format(new Date(local.TotalEnd.JulianDay, offset))}" : empty;

            U4 = local.PartialEnd?.JulianDay;
            U4String = local.PartialEnd != null ? $"{Format.Time.Format(new Date(local.PartialEnd.JulianDay, offset))}" : empty;

            P4 = local.PenumbralEnd?.JulianDay;
            P4String = local.PenumbralEnd != null ? $"{Format.Time.Format(new Date(local.PenumbralEnd.JulianDay, offset))}" : empty;

            Visibility = visibility;
        }
    }
}
