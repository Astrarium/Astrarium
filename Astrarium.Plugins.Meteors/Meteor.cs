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
        public int ActivityClass { get; set; }
    }
}
