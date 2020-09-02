using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    ///  Represents filled closed area on the map.
    /// </summary>
    public class Polygon : List<GeoPoint>
    {
        /// <summary>
        /// Gets or sets polygon style.
        /// </summary>
        public PolygonStyle Style { get; set; }


        /// <summary>
        /// Creates new polygon without assigned style
        /// </summary>
        public Polygon()
        {

        }

        /// <summary>
        /// Creates new polygon with specified style.
        /// </summary>
        /// <param name="style"></param>
        public Polygon(PolygonStyle style)
        {
            Style = style;
        }
    }
}
