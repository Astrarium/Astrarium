using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astrarium.Plugins.Journal.Database.Entities;

namespace Astrarium.Plugins.Journal.Types
{
    /// <summary>
    /// Maps database types to model types and vice versa.
    /// </summary>
    internal static class Mapper
    {
        /// <summary>
        /// Maps Camera model to DB entity. 
        /// </summary>
        /// <param name="camera">Camera model</param>
        /// <param name="db">DB entity to map to.</param>
        /// <returns>DB entity</returns>
        public static CameraDB ToDB(this Camera camera, CameraDB db = null)
        {
            if (db == null)
            {
                db = new CameraDB();
            }

            db.Id = camera.Id;
            db.Vendor = camera.Vendor;
            db.Model = camera.Model;
            db.PixelsX = camera.PixelsX;
            db.PixelsY = camera.PixelsY;
            db.PixelXSize = camera.PixelXSize;
            db.PixelYSize = camera.PixelYSize;
            db.Binning = camera.Binning;
            db.Remarks = camera.Remarks;

            return db;
        }

        // TODO: move other mapping logic here
    }
}
