using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Satellites
{
    public class TLESource
    {
        public string Url { get; set; }
        public bool IsEnabled { get; set; }
        public string FileName { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
