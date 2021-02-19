using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class SolarEclipseCitiesListTableItem
    {
        public CrdsGeographical Location { get; private set; }

        public string LocationName { get; private set; }

        public double TimeZone { get; private set; }
        public string TimeZoneString { get; private set; }

        public CrdsGeographical Coordinates { get; private set; }
        public string CoordinatesString { get; private set; }

        public string MaxMagString { get; private set; }
        public double MaxMag { get; private set; }

        public string MoonSunRatioString { get; private set; }
        public double MoonSunRatio { get; private set; }

        public string PartialDurString { get; private set; }
        public double? PartialDur { get; private set; }

        public string TotalDurString { get; private set; }
        public double? TotalDur { get; private set; }

        public string ShadowWidthString { get; private set; }
        public double? ShadowWidth { get; private set; }

        public string C1TimeString { get; private set; }
        public double? C1Time { get; private set; }

        public string C2TimeString { get; private set; }
        public double? C2Time { get; private set; }

        public string MaxTimeString { get; private set; }
        public double? MaxTime { get; private set; }

        public string C3TimeString { get; private set; }
        public double? C3Time { get; private set; }

        public string C4TimeString { get; private set; }
        public double? C4Time { get; private set; }

        public string Visibility { get; private set; }

        public SolarEclipseCitiesListTableItem(SolarEclipseLocalCircumstances local, string visibility)
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

            MaxMagString = local.MaxMagnitude > 0 ? Format.Mag.Format(local.MaxMagnitude) : empty;
            MaxMag = local.MaxMagnitude;

            MoonSunRatioString = local.MoonToSunDiameterRatio > 0 ? Format.Ratio.Format(local.MoonToSunDiameterRatio) : empty;
            MoonSunRatio = local.MoonToSunDiameterRatio;

            PartialDurString = local.PartialDuration > 0 ? Format.Time.Format(local.PartialDuration) : empty;
            PartialDur = local.PartialDuration;

            TotalDurString = local.TotalDuration > 0 ? Format.Time.Format(local.TotalDuration) : empty;
            TotalDur = local.TotalDuration;

            ShadowWidthString = local.PathWidth > 0 ? Format.PathWidth.Format(local.PathWidth) : empty;
            ShadowWidth = local.PathWidth;

            C1TimeString = local.PartialBegin != null ? $"{Format.Time.Format(new Date(local.PartialBegin.JulianDay, offset))}" : empty;
            C1Time = local.PartialBegin?.JulianDay;

            C2TimeString = local.TotalBegin != null ? $"{Format.Time.Format(new Date(local.TotalBegin.JulianDay, offset))}" : empty;
            C2Time = local.TotalBegin?.JulianDay;

            MaxTimeString = local.Maximum?.JulianDay != null ? $"{Format.Time.Format(new Date(local.Maximum.JulianDay, offset))}" : empty;
            MaxTime = local.Maximum?.JulianDay;

            C3TimeString = local.TotalEnd != null ? $"{Format.Time.Format(new Date(local.TotalEnd.JulianDay, offset))}" : empty;
            C3Time = local.TotalEnd?.JulianDay;

            C4TimeString = local.PartialEnd != null ? $"{Format.Time.Format(new Date(local.PartialEnd.JulianDay, offset)) }" : empty;
            C4Time = local.PartialEnd?.JulianDay;

            Visibility = visibility;
        }
    }
}
