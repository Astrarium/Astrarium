using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string MaxMagString { get; private set; }
        public double MaxMag { get; private set; }

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

            Visibility = visibility;
        }
    }
}
