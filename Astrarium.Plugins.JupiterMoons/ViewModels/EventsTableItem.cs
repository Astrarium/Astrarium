using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.JupiterMoons
{
    public class EventsTableItem
    {
        public string BeginDate { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string Text { get; set; }
        public string Code { get; set; }    
        public string Notes { get; set; }        
        public string JupiterAltBegin { get; set; }
        public string JupiterAltEnd { get; set; }
        public string SunAltBegin { get; set; }
        public string SunAltEnd { get; set; }

        public JovianEvent Event { get; set; }
    }
}
