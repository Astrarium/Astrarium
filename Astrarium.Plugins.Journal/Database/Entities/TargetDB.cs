using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class TargetDB : IEntity
    {
        public string Id { get; set; }

        /// <summary>
        /// Type of object
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Name of the object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Common name of the object
        /// </summary>
        public string CommonName { get; set; }

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
}