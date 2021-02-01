using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class LocalContactsTableItem
    {
        public string Point { get; private set; }
        public string Time { get; private set; }
        public string Altitude { get; private set; }
        public string PAngle { get; private set; }
        public string ZAngle { get; private set; }

        public LocalContactsTableItem(string text, SolarEclipseLocalCircumstancesContactPoint contact)
        {
            Point = text;
            if (contact != null)
            {
                Time = !double.IsNaN(contact.JulianDay) ? $"{Format.Time.Format(new Date(contact.JulianDay, 0))} UTC" : "—";
                Altitude = Format.Alt.Format(contact.SolarAltitude);
                PAngle = Format.Angle.Format(contact.PAngle);
                ZAngle = Format.Angle.Format(contact.ZAngle);
            }
        }
    }
}
