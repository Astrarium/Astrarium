using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Marker interface for all field of views
    /// </summary>
    public interface IFieldOfView { }

    /// <summary>
    /// Finder frame of view
    /// </summary>
    public class FinderFieldOfView : IFieldOfView { }

    /// <summary>
    /// Abstract field of view
    /// </summary>
    public abstract class FieldOfView : IFieldOfView
    {
        /// <summary>
        /// Dawes limit
        /// </summary>
        public float DawesLimit { get; set; }
    }

    /// <summary>
    /// Circular field of view
    /// </summary>
    public abstract class CircularFieldOfView : FieldOfView
    {
        /// <summary>
        /// Field of view, in degrees of arc
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Exit pupil, in mm
        /// </summary>
        public float ExitPupil { get; set; }

        /// <summary>
        /// Magnification
        /// </summary>
        public float Magnification { get; set; }

        /// <summary>
        /// Visual magnitude limit
        /// </summary>
        public float VisualMagnitudeLimit { get; set; }
    }

    /// <summary>
    /// Binocular field of view
    /// </summary>
    public class BinocularFieldOfView : CircularFieldOfView
    {

    }

    /// <summary>
    /// Telescope field of view
    /// </summary>
    public class TelescopeFieldOfView : CircularFieldOfView
    {
        /// <summary>
        /// Focal ratio of telescope
        /// </summary>
        public float FocalRatio { get; set; }
    }

    /// <summary>
    /// Camera field of view
    /// </summary>
    public class CameraFieldOfView : FieldOfView
    {
        /// <summary>
        /// Size, in degrees of arc
        /// </summary>
        public SizeF Size { get; set; }

        /// <summary>
        /// Camera rotation angle
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Camera binning
        /// </summary>
        public int Binning { get; set; }

        /// <summary>
        /// Resolution, arcseconds per pixel
        /// </summary>
        public SizeF Resolution { get; set; }

        /// <summary>
        /// Focal ratio of telescope
        /// </summary>
        public float FocalRatio { get; set; }
    }
}
