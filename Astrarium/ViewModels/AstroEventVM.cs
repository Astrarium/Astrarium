using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class AstroEventVM
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public double JulianDay { get; set; }
        public string Text { get; set; }
        public bool NoExactTime { get; set; }
        public CelestialObject PrimaryBody { get; set; }
        public CelestialObject SecondaryBody { get; set; }

        public AstroEventVM(AstroEvent e, double utcOffset)
        {
            var date = new Date(e.JulianDay, utcOffset);
            JulianDay = e.JulianDay;
            Text = e.Text;
            NoExactTime = e.NoExactTime;
            PrimaryBody = e.PrimaryBody;
            SecondaryBody = e.SecondaryBody;
            Date = Formatters.Date.Format(date);
            Time = Formatters.Time.Format(date);
        }
    }
}
