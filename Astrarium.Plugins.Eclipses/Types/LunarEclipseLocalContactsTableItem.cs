﻿using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class LunarEclipseLocalContactsTableItem
    {
        public string Point { get; private set; }
        public string Time { get; private set; }
        public string Altitude { get; private set; }

        public LunarEclipseLocalContactsTableItem(string text, LunarEclipseLocalCircumstancesContactPoint contact)
        {
            Point = text;
            if (contact != null)
            {
                Time = !double.IsNaN(contact.JulianDay) ? $"{Format.Time.Format(new Date(contact.JulianDay, 0))} UTC" : "—";
                Altitude = Format.Alt.Format(contact.LunarAltitude);
            }
        }
    }
}