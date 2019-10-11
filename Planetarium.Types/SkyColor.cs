using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Types
{
    /// <summary>
    /// Holds 3 colors used to draw objects on sky map.
    /// Each color corresponds to own color schema (see <see cref="ColorSchema"/>).
    /// Current value of color schema can be obtained from <see cref="IMapContext.Schema"/>.
    /// </summary>
    public class SkyColor
    {
        /// <summary>
        /// Color for <see cref="ColorSchema.Night"/>
        /// </summary>
        public Color Night { get; set; }

        /// <summary>
        /// Color for <see cref="ColorSchema.Day"/>
        /// </summary>
        public Color Day { get; set; }

        /// <summary>
        /// Color for <see cref="ColorSchema.White"/>
        /// </summary>
        public Color White { get; set; }

        /// <summary>
        /// Creates new <see cref="SkyColor"/> instance.
        /// </summary>
        public SkyColor() { }

        /// <summary>
        /// Creates new <see cref="SkyColor"/> instance based on existing <see cref="SkyColor"/> 
        /// but with specified alpha-channel applied to all colors.
        /// </summary>
        /// <param name="alpha">Alpha-channel value to be applied to all colors.</param>
        /// <param name="other">Existing <see cref="SkyColor"/> instance used as a base.</param>
        public SkyColor(int alpha, SkyColor other)
        {
            Night = Color.FromArgb(alpha, other.Night);
            Day = Color.FromArgb(alpha, other.Day);
            White = Color.FromArgb(alpha, other.White);
        }
    }
}
