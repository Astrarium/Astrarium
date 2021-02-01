using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class ContactsTableItem
    {
        public string Point { get; set; }
        public string Coordinates { get; set; }
        public string Time { get; set; }

        public ContactsTableItem(string text, SolarEclipseMapPoint p)
        {
            Point = text;
            Coordinates = Format.Geo.Format(p);
            Time = $"{Format.Time.Format(new Date(p.JulianDay, 0))} UTC";
        }
    }
}
