using Astrarium.Plugins.Journal.Database.Entities;
using Newtonsoft.Json;

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
        /// <param name="dbo">DB entity to map to.</param>
        /// <returns>DB entity</returns>
        public static CameraDB ToDBO(this Camera camera, CameraDB dbo = null)
        {
            if (dbo == null)
            {
                dbo = new CameraDB();
            }

            dbo.Id = camera.Id;
            dbo.Vendor = camera.Vendor;
            dbo.Model = camera.Model;
            dbo.PixelsX = camera.PixelsX;
            dbo.PixelsY = camera.PixelsY;
            dbo.PixelXSize = camera.PixelXSize;
            dbo.PixelYSize = camera.PixelYSize;
            dbo.Binning = camera.Binning;
            dbo.Remarks = camera.Remarks;

            return dbo;
        }

        /// <summary>
        /// Map DB Camera entity to Camera model
        /// </summary>
        /// <param name="dbo">DB entity to map from</param>
        /// <returns>Camera model</returns>
        public static Camera FromDBO(this CameraDB dbo)
        {
            return new Camera()
            {
                Id = dbo.Id,
                Vendor = dbo.Vendor,
                Model = dbo.Model,
                PixelsX = dbo.PixelsX,
                PixelsY = dbo.PixelsY,
                PixelXSize = dbo.PixelXSize,
                PixelYSize = dbo.PixelYSize,
                Binning = dbo.Binning,
                Remarks = dbo.Remarks
            };
        }

        public static EyepieceDB ToDBO(this Eyepiece eyepiece, EyepieceDB dbo = null)
        {
            if (dbo == null)
            {
                dbo = new EyepieceDB();
            }

            dbo.Id = eyepiece.Id;
            dbo.Vendor = eyepiece.Vendor;
            dbo.Model = eyepiece.Model;
            dbo.FocalLength = eyepiece.FocalLength;
            dbo.FocalLengthMax = eyepiece.IsZoomEyepiece ? eyepiece.MaxFocalLength : (double?)null;
            dbo.ApparentFOV = eyepiece.ApparentFOVSpecified ? eyepiece.ApparentFOV : (double?)null;

            return dbo;
        }

        public static Eyepiece FromDBO(this EyepieceDB eyepieceDb)
        {
            return new Eyepiece()
            {
                Id = eyepieceDb.Id,
                Vendor = eyepieceDb.Vendor,
                Model = eyepieceDb.Model,
                FocalLength = eyepieceDb.FocalLength,
                MaxFocalLength = eyepieceDb.FocalLengthMax.HasValue ? eyepieceDb.FocalLengthMax.Value : 10,
                IsZoomEyepiece = eyepieceDb.FocalLengthMax.HasValue,
                ApparentFOV = eyepieceDb.ApparentFOV.HasValue ? eyepieceDb.ApparentFOV.Value : 50,
                ApparentFOVSpecified = eyepieceDb.ApparentFOV.HasValue
            };
        }

        public static SiteDB ToDBO(this Site site, SiteDB dbo = null)
        {
            if (dbo == null)
            {
                dbo = new SiteDB();
            }

            dbo.Id = site.Id;
            dbo.Name = site.Name;
            dbo.Elevation = site.Elevation;
            dbo.IAUCode = site.IAUCode;
            dbo.Latitude = site.Latitude;
            dbo.Longitude = site.Longitude;
            dbo.Timezone = site.Timezone;

            return dbo;
        }

        public static Site FromDBO(this SiteDB dbo)
        {
            return new Site()
            {
                Id = dbo.Id,
                Name = dbo.Name,
                Elevation = dbo.Elevation ?? 0,
                IAUCode = dbo.IAUCode,
                Latitude = dbo.Latitude,
                Longitude = dbo.Longitude,
                Timezone = dbo.Timezone,
            };
        }

        public static OpticsDB ToDBO(this Optics optics, OpticsDB dbo = null)
        {
            if (dbo == null)
            {
                dbo = new OpticsDB();
            }

            dbo.Id = optics.Id;
            dbo.Vendor = optics.Vendor;
            dbo.Model = optics.Model;
            dbo.Scheme = optics.Scheme;
            dbo.Type = optics.Type;
            dbo.Aperture = optics.Aperture;
            dbo.OrientationErect = optics.OrientationErect;
            dbo.OrientationTrueSided = optics.OrientationTrueSided;

            if (optics.Type == "Telescope")
            {
                dbo.Details = JsonConvert.SerializeObject(new ScopeDetails() { FocalLength = optics.FocalLength });
            }
            else if (optics.Type == "Fixed")
            {
                dbo.Details = JsonConvert.SerializeObject(new FixedOpticsDetails() { Magnification = optics.Magnification, TrueField = optics.TrueFieldSpecified ? optics.TrueField : (double?)null });
            }

            return dbo;
        }

        public static Optics FromDBO(this OpticsDB dbo)
        {
            var optics = new Optics()
            {
                Id = dbo.Id,
                Vendor = dbo.Vendor,
                Model = dbo.Model,
                Scheme = dbo.Scheme,
                Type = dbo.Type,
                Aperture = dbo.Aperture,
                OrientationErect = dbo.OrientationErect,
                OrientationTrueSided = dbo.OrientationTrueSided
            };

            if (optics.Type == "Telescope")
            {
                var details = JsonConvert.DeserializeObject<ScopeDetails>(dbo.Details);
                optics.FocalLength = details.FocalLength;
            }
            else if (optics.Type == "Fixed")
            {
                var details = JsonConvert.DeserializeObject<FixedOpticsDetails>(dbo.Details);
                optics.Magnification = details.Magnification;
                optics.TrueField = details.TrueField ?? 0;
                optics.TrueFieldSpecified = details.TrueField != null;
            }

            return optics;
        }

        public static LensDB ToDBO(this Lens lens, LensDB dbo = null)
        {
            if (dbo == null)
            {
                dbo = new LensDB();
            }

            dbo.Id = lens.Id;
            dbo.Vendor = lens.Vendor;
            dbo.Model = lens.Model;
            dbo.Factor = lens.Factor;

            return dbo;
        }

        public static Lens FromDBO(this LensDB dbo)
        {
            return new Lens()
            {
                Id = dbo.Id,
                Vendor = dbo.Vendor,
                Model = dbo.Model,
                Factor = dbo.Factor
            };
        }

        public static FilterDB ToDBO(this Filter filter, FilterDB dbo = null)
        {
            if (dbo == null)
            {
                dbo = new FilterDB();
            }

            dbo.Id = filter.Id;
            dbo.Vendor = filter.Vendor;
            dbo.Model = filter.Model;
            dbo.Type = filter.Type;
            if (filter.Type == "color")
            {
                dbo.Color = filter.Color;
                dbo.Wratten = filter.Wratten;
            }
            else
            {
                dbo.Color = null;
                dbo.Wratten = null;
            }

            return dbo;
        }

        public static Filter FromDBO(this FilterDB dbo)
        {
            return new Filter()
            {
                Id = dbo.Id,
                Vendor = dbo.Vendor,
                Model = dbo.Model,
                Type = dbo.Type,
                Color = dbo.Type == "color" ? dbo.Color : null,
                Wratten = dbo.Type == "color" ? dbo.Wratten : null,
            };
        }

        // TODO: move other mapping logic here
    }
}
