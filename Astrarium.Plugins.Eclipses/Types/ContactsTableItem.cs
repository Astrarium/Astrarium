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

        public ContactsTableItem(string text, double jd, CrdsGeographical g = null)
        {
            Point = text;
            Coordinates = g != null ? Format.Geo.Format(g) : null;
            Time = $"{Format.Time.Format(new Date(jd, 0))} {(!double.IsNaN(jd) ? "UTC" : "")}";
        }
    }
}
