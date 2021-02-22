using Astrarium.Algorithms;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class LunarEclipseLocalContactsTableItem
    {
        public string Point { get; private set; }
        public string Time { get; private set; }
        public string Altitude { get; private set; }
        public string PAngle { get; private set; }
        public string ZAngle { get; private set; }

        public LunarEclipseLocalContactsTableItem(string text, LunarEclipseLocalCircumstancesContactPoint contact)
        {
            Point = text;
            if (contact != null)
            {
                Time = !double.IsNaN(contact.JulianDay) ? $"{Format.Time.Format(new Date(contact.JulianDay, 0))} UTC" : "—";
                Altitude = Format.Alt.Format(contact.LunarAltitude);
                PAngle = Format.Angle.Format(contact.PAngle);
                ZAngle = Format.Angle.Format(contact.ZAngle);
            }
        }
    }
}
