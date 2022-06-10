using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservationPlannerDatabase.Database.Entities
{
    public class TargetDB : IEntity
    {
        public string Id { get; set; }

        /// <summary>
        /// Type of object
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Common name of the object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Comma-separated list of alias names
        /// </summary>
        public string Aliases { get; set; }

        /// <summary>
        /// Right Ascension at the moment of observation, if available, in degrees, J2000
        /// </summary>
        public double? RightAscension { get; set; }

        /// <summary>
        /// Declination at the moment of observation, if available, in degrees, J2000
        /// </summary>
        public double? Declination { get; set; }

        /// <summary>
        /// Local altitude of the target at the moment of observation, if available, in degrees
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Local azimuth of the target at the moment of observation, if available, in degrees
        /// </summary>
        public double? Azimuth { get; set; }

        /// <summary>
        /// Constellation of the object location at the moment of observation, if available
        /// </summary>
        public string Constellation { get; set; }

        /// <summary>
        /// Source of data, for example application name the position and details are taken from
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Details of the target
        /// </summary>
        public string Details { get; set; }
    }

    public class StarTargetDetails
    {
        public double? Magnitude { get; set; }
        public string Classification { get; set; }
    }

    public abstract class DeepSkyTargetDetails
    {
        public double? SmallDiameter { get; set; }
        public double? LargeDiameter { get; set; }
        public double? Magnitude { get; set; }
        public double? Brightness { get; set; }
    }

    public class DeepSkyAsterismTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle of axis, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }
    }

    public class DeepSkyGlobularClusterTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Magnitude of brightest stars in [mag]
        /// </summary>
        public double? MagStars { get; set; }

        /// <summary>
        /// Degree of concentration [I..XII]
        /// </summary>
        public string Concentration { get; set; }
    }

    public class DeepSkyClusterOfGalaxiesTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Magnitude of the 10th brightest member in [mag] 
        /// </summary>
        public double? Mag10 { get; set; }
    }

    public class DeepSkyDarkNebulaTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle of axis, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Opacity acc. to Lynds (1: min, 6:max)
        /// </summary>
        public int? Opacity { get; set; }
    }

    public class DeepSkyDoubleStarTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Separation between components
        /// </summary>
        public double? Separation { get; set; }

        /// <summary>
        /// Magnitude of companion star
        /// </summary>
        public double? CompanionMagnitude { get; set; }
    }

    public class DeepSkyGalaxyTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Hubble type of galaxy
        /// </summary>
        public string HubbleType { get; set; }
    }

    public class DeepSkyGalaxyNebulaTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Indicates emission, reflection or dark nebula not restricted to an enum to cover exotic objects
        /// </summary>
        public string NebulaType { get; set; }
    }

    public class DeepSkyOpenClusterTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Number of stars
        /// </summary>
        public int? StarsCount { get; set; }

        /// <summary>
        /// Magnitude of brightest star in [mag]
        /// </summary>
        public double? BrightestStarMagnitude { get; set; }

        /// <summary>
        /// Classification according to Trumpler
        /// </summary>
        public string TrumplerClass { get; set; }
    }

    public class DeepSkyPlanetaryNebulaTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Magnitude of central star
        /// </summary>
        public double? CentralStarMagnitude { get; set; }
    }

    public class DeepSkyQuasarTargetDetails : DeepSkyTargetDetails
    {

    }

    public class DeepSkyStarCloudTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle of axis, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }
    }

    public class DeepSkyUnspecifiedTargetDetails : DeepSkyTargetDetails
    {

    }

    public class VariableStarTargetDetails : StarTargetDetails
    {
        /// <summary>
        /// Variable star type or subtype like Delta Cepheid, Mira, Eruptive, Semiregular, Supernovae
        /// </summary>
        public string VarStarType { get; set; }

        /// <summary>
        /// Maximal apparent magnitude. The derived <see cref="StarTargetDetails.Magnitude"/> will be used for minimal apparent magnitude
        /// </summary>
        public double? MaxMagnitude { get; set; }

        /// <summary>
        /// Pperiod of variable star (if any) in days 
        /// </summary>
        public double? Period { get; set; }
    }
}