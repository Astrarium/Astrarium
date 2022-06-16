using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class ImagerDB : IEntity
    {
        public string Id { get; set; }
        public string Model { get; set; }
        public string Vendor { get; set; }
        public string Remarks { get; set; }

        /// <summary>
        /// Imager type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Detailed information related to particular imager type, serialized as JSON
        /// </summary>
        public string Details { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Vendor} {Model}".Trim();
        }
    }

    public class CameraImagerDetails
    {
        public int PixelsX { get; set; }
        public int PixelsY { get; set; }
        public double? PixelsXSize { get; set; }
        public double? PixelsYSize { get; set; }
        public int Binning { get; set; }
    }
}
