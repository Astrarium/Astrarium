using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Represents Barlow lens/reducer parameters
    /// </summary>
    public class Lens
    {
        /// <summary>
        /// Equipment id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Barlow lens/reducer value
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Barlow/reducer name, manufacturer, model, etc.
        /// </summary>
        public string Name { get; set; }
    }
}
