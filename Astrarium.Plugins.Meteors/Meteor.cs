using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Meteors
{
    public class Meteor : CelestialObject, IMovingObject
    {
        /// <inheritdoc />
        public override string Type => "Meteor";

        public override string[] Names => new[] { Name, Code };
        public override string[] DisplaySettingNames => new[] { "Meteors" };

        public string Code { get; set; }
        public string Name { get; set; }

        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();
        public CrdsEquatorial Drift { get; set; } = new CrdsEquatorial();
        
        public short Begin { get; set; }
        public short End { get; set; }
        public short Max { get; set; }

        public bool IsActive { get; set; }
        public double AverageDailyMotion => 1;
        public string ZHR { get; set; }
        public int ZHRNumeric
        {
            get
            {
                switch (ZHR)
                {
                    case "var": return 0;
                    case "<2": return 1;
                    default: return int.Parse(ZHR);
                };
            }
        }
        public int ActivityClass { get; set; }
    }
}
