using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ObservationsLog.Types
{
    public class Target : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string[] OtherNames { get; set; }
        public CrdsEquatorial Position { get; set; }
        public string Constellation { get; set; }
    }

    public abstract class DeepSkyTarget : Target 
    { 
        public float? SmallDiameter { get; set; }
        public float? LargeDiameter { get; set; }
        public float? VisualMag { get; set; }
        public float? SurfaceBrightness { get; set; }
    }

    public class DeepSkyGalaxyTarget : DeepSkyTarget
    {
        public float? PosAngle { get; set; }
    }

    public class PlanetTarget : Target
    {
        
    }
}
