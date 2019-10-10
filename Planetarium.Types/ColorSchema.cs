using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Types
{
    /// <summary>
    /// Represents different values of color schema that can be used by sky map.
    /// </summary>
    public enum ColorSchema
    {
        /// <summary>
        /// Night schema. 
        /// Map simulates black sky at any time of a day, without taking into account light emission in atmosphere.
        /// This is a defult value.
        /// </summary>
        Night = 0,

        /// <summary>
        /// Day schema.
        /// Map simulates real sky depend on time of a day. Sky is black at night, blue at day.
        /// </summary>
        Day   = 1,

        /// <summary>
        /// Night vision schema.
        /// All colors are shades of red.
        /// </summary>
        Red   = 2,

        /// <summary>
        /// Printable map schema.
        /// Sky map background is white, all objects are black or gray.
        /// </summary>
        White = 3
    }
}
