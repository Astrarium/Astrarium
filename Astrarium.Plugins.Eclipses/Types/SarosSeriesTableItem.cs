using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class SarosSeriesTableItem
    {
        public string Member { get; set; }
        public int MeeusLunationNumber { get; set; }
        public double JulianDay { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public string Gamma { get; set; }
        public string Magnitude { get; set; }
        public string LocalVisibility { get; set; }
    }
}
