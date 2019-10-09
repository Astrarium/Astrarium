using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Types
{
    public class SkyColor
    {
        public Color Night { get; set; }
        public Color Day { get; set; }
        public Color Red { get; set; }
        public Color White { get; set; }

        public SkyColor() { }

        public SkyColor(int alpha, SkyColor other)
        {
            Night = Color.FromArgb(alpha, other.Night);
            Day = Color.FromArgb(alpha, other.Day);
            Red = Color.FromArgb(alpha, other.Red);
            White = Color.FromArgb(alpha, other.White);
        }
    }
}
