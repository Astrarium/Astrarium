using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    /// <summary>
    /// Base class for all physical objects that can be displayed on the sky map
    /// </summary>
    public abstract class CelestialObject
    {
        /// <summary>
        /// Collection of object names
        /// </summary>
        public ICollection<string> Names { get; set; }

        /// <summary>
        /// Local horizontal coordinates of the object
        /// </summary>
        public CrdsHorizontal Horizontal { get; set; }
    }
}
