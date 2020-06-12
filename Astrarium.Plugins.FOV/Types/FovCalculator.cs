using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Calculates Field Of View depending on equipment
    /// </summary>
    public static class FovCalculator
    {
        /// <summary>
        /// Gets Field Of View parameters as seen by eye with telescope equipped by eyepiece and Barlow lens or reducer
        /// </summary>
        /// <param name="telescope">Telesope parameters</param>
        /// <param name="eyepiece">Eyepiece parameters</param>
        /// <param name="lens">Barlow lens or reducer parameters</param>
        /// <returns>
        /// Field Of View parameters as seen by eye with equipment provided
        /// </returns>
        public static TelescopeFieldOfView GetTelescopeView(Telescope telescope, Eyepiece eyepiece, Lens lens)
        {
            float focalLength = telescope.FocalLength * (lens != null ? lens.Value : 1);
            float focalRatio = telescope.FocalLength / telescope.Aperture;
            float dawesLimit = GetDawesLimit(telescope.Aperture);
            float magnitudeLimit = GetManitudeLimit(telescope.Aperture);
            float magnification = focalLength / eyepiece.FocalLength;
            float fov = eyepiece.FieldOfView / magnification;
            float exitPupil = telescope.Aperture / magnification;
            
            return new TelescopeFieldOfView()
            {
                FocalRatio = focalRatio,
                ExitPupil = exitPupil,
                DawesLimit = dawesLimit,
                Magnification = magnification,
                Size = fov,
                VisualMagnitudeLimit = magnitudeLimit
            };
        }

        /// <summary>
        /// Gets Field Of View parameters as seen by eye with binocular or optical finder
        /// </summary>
        /// <param name="binocular">Binocular or optical finder parameters</param>
        /// <returns>
        /// Field Of View parameters as seen by eye with equipment provided
        /// </returns>
        public static BinocularFieldOfView GetBinocularView(Binocular binocular)
        {
            float exitPupil = binocular.Aperture / binocular.Magnification;
            float dawesLimit = GetDawesLimit(binocular.Aperture);
            float magnitudeLimit = GetManitudeLimit(binocular.Aperture);

            return new BinocularFieldOfView()
            {
                DawesLimit = dawesLimit,
                ExitPupil = exitPupil,
                Magnification = binocular.Magnification,
                Size = binocular.FieldOfView,
                VisualMagnitudeLimit = magnitudeLimit
            };
        }

        /// <summary>
        /// Gets Field Of View parameters as seen by camera with telescope equipped by Barlow lens or reducer
        /// </summary>
        /// <param name="telescope">Telesope parameters</param>
        /// <param name="camera">Camers parameters</param>
        /// <param name="lens">Barlow lens or reducer parameters</param>
        /// <param name="binning">Binning value</param>
        /// <param name="rotation">Camera rotation angle</param>
        /// <returns>
        /// Field Of View parameters as seen by camera with equipment provided
        /// </returns>
        public static CameraFieldOfView GetCameraView(Telescope telescope, Camera camera, Lens lens, int binning, int rotation)
        {
            float focalLength = telescope.FocalLength * (lens != null ? lens.Value : 1);
            float focalRatio = focalLength / telescope.Aperture;
            float dawesLimit = GetDawesLimit(telescope.Aperture);
            float resolutionHorizontal = ((camera.PixelSizeWidth * binning) / focalLength) * 206;
            float resolutionVertical = ((camera.PixelSizeHeight * binning) / focalLength) * 206;
            float fovHorizontal = (resolutionHorizontal * (camera.HorizontalResolution / binning)) / 3600;
            float fovVertical = (resolutionVertical * (camera.VerticalResolution / binning)) / 3600;

            return new CameraFieldOfView()
            {
                FocalRatio = focalRatio,
                Resolution = new SizeF(resolutionHorizontal, resolutionVertical),
                Size = new SizeF(fovHorizontal, fovVertical),
                Rotation = rotation,
                Binning = binning,
                DawesLimit = dawesLimit
            };
        }

        /// <summary>
        /// Gets Dawes limit from aperture
        /// </summary>
        /// <param name="aperture">Aperture expressed in mm</param>
        private static float GetDawesLimit(float aperture)
        {
            return 116.0f / aperture;
        }

        /// <summary>
        /// Gets visual magnitude limit from aperture
        /// </summary>
        /// <param name="aperture">Aperture expressed in mm</param>
        /// <remarks>
        /// Based on formula https://www.astronomics.com/info-library/astronomical-terms/limiting-magnitude/
        /// </remarks>
        private static float GetManitudeLimit(float aperture)
        {
            return (float)(7.5 + 5 * Math.Log10(aperture / 10));
        }
    }
}
