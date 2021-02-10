using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.Types
{
    /// <summary>
    /// Defines single row in a table of local contacts for the solar eclipses.
    /// </summary>
    public class SolarEclipseLocalContactsTableItem
    {
        /// <summary>
        /// String title of the contact, for example, "C2: beginning of total phase"
        /// </summary>
        public string Point { get; private set; }

        /// <summary>
        /// String representation of the time instant
        /// </summary>
        public string Time { get; private set; }

        /// <summary>
        /// Solar altitude, converted to string
        /// </summary>
        public string Altitude { get; private set; }

        /// <summary>
        /// Position angle of contact point, in degrees, measured CCW from celestial North pole, converted to string
        /// </summary>
        public string PAngle { get; private set; }

        /// <summary>
        /// Position angle of contact point, in degrees, measured CCW from zenith, converted to string
        /// </summary>
        public string ZAngle { get; private set; }

        /// <summary>
        /// Creates new table item
        /// </summary>
        /// <param name="text">String title of the contact, for example, "C2: beginning of total phase"</param>
        /// <param name="contact">Contact point info</param>
        public SolarEclipseLocalContactsTableItem(string text, SolarEclipseLocalCircumstancesContactPoint contact)
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
