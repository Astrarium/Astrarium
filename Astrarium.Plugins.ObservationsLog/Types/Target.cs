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

    public class StarTarget : Target
    {
        public double? ApparentMag { get; set; }
        public string Classification { get; set; }
    }

    public abstract class DeepSkyTarget : Target 
    { 
        public double? SmallDiameter { get; set; }
        public double? LargeDiameter { get; set; }
        public double? VisualMag { get; set; }

        /// <summary>
        /// Surface brightness in mags per sq. arcsec
        /// </summary>
        public double? SurfaceBrightness { get; set; }
    }

    public class MultipleStarTarget : Target
    {
        public List<StarTarget> Components { get; set; }
    }

    public class GalaxyTarget : DeepSkyTarget
    {
        public string HubbleType { get; set; }
        public string PosAngle { get; set; }
    }

    public class GalaxyNebulaTarget : DeepSkyTarget
    {
        public string NebulaType { get; set; }
        public string PosAngle { get; set; }
    }

    public class AsterismTarget : DeepSkyTarget
    {
        public string PosAngle { get; set; }
    }

    public class ClusterOfGalaxiesTarget : DeepSkyTarget
    {
        public double? Mag10 { get; set; }
    }

    public class DarkNebulaTarget : DeepSkyTarget
    {
        public string PosAngle { get; set; }
        public int Opacity { get; set; }
    }

    public class DoubleStarTarget : DeepSkyTarget
    {
        public string PosAngle { get; set; }
        public double? Separation { get; set; }
        public double? MagComp { get; set; }
    }

    public abstract class SolarSystemTarget : Target { }
    public class CometTarget : SolarSystemTarget { }
    public class MinorPlanetTarget : SolarSystemTarget { }
    public class MoonTarget : SolarSystemTarget { }
    public class PlanetTarget : SolarSystemTarget { }
    public class SunTarget : SolarSystemTarget { }
}
