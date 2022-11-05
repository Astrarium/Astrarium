using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class CameraDB : IEntity
    {
        /// <summary>
        /// Empty element (equals to "Not selected")
        /// </summary>
        public static CameraDB Empty = new CameraDB() { Id = null };

        public string Id { get; set; }
        public string Model { get; set; }
        public string Vendor { get; set; }
        public string Remarks { get; set; }

        public int PixelsX { get; set; }
        public int PixelsY { get; set; }
        public double? PixelXSize { get; set; }
        public double? PixelYSize { get; set; }
        public int Binning { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Vendor} {Model}".Trim();
        }
    }
}
