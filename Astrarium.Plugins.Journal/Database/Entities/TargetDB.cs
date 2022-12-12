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
        /// JSON-serialized list of alias names
        /// </summary>
        public string Aliases { get; set; }

        /// <summary>
        /// Source of data, for example application name the position and details are taken from
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Details of the target, in JSON form
        /// </summary>
        public string Details { get; set; }
    }
}