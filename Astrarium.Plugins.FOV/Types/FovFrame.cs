using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Represents FOV frame
    /// </summary>
    [JsonConverter(typeof(FovFrameJsonConverter))]
    public abstract class FovFrame
    {
        /// <summary>
        /// Id of the FOV frame
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Flag indicating FOV frame is enabled (visible)
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Frame color
        /// </summary>
        [JsonConverter(typeof(FovFrameColorJsonConverter))]
        public SkyColor Color { get; set; }

        /// <summary>
        /// Shading level, from 0 to 100 percent
        /// </summary>
        public short Shading { get; set; }

        /// <summary>
        /// Frame label, to be shown on sky map
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Makes a copy of FOV frame
        /// </summary>
        /// <returns></returns>
        public FovFrame Copy()
        {
            var copy = JsonConvert.DeserializeObject<FovFrame>(JsonConvert.SerializeObject(this));
            copy.Id = Guid.NewGuid();
            copy.Label = null;
            return copy;
        }
    }

    /// <summary>
    /// Represents circular FOV frame
    /// </summary>
    public abstract class CircularFovFrame : FovFrame
    {
        /// <summary>
        /// Frame diameter, in degrees of arc
        /// </summary>
        public float Size { get; set; }
    }

    /// <summary>
    /// Represents telescope FOV frame
    /// </summary>
    public class TelescopeFovFrame : CircularFovFrame
    {
        /// <summary>
        /// Telescope id
        /// </summary>
        public Guid TelescopeId { get; set; }

        /// <summary>
        /// Eyepiece id
        /// </summary>
        public Guid EyepieceId { get; set; }

        // <summary>
        /// Optional Barlow lens/Reducer id
        /// </summary>
        public Guid? LensId { get; set; }
    }

    /// <summary>
    /// Represents binocular FOV frame
    /// </summary>
    public class BinocularFovFrame : CircularFovFrame
    {
        /// <summary>
        /// Binocular id
        /// </summary>
        public Guid BinocularId { get; set; }
    }

    /// <summary>
    /// Represents frame of a finder
    /// </summary>
    public class FinderFovFrame : FovFrame
    {
        public float[] Sizes { get; set; } = new float[1];
        public bool Crosslines { get; set; }
    }

    /// <summary>
    /// Represents camera FOV frame
    /// </summary>
    public class CameraFovFrame : FovFrame
    {
        /// <summary>
        /// Telescope id
        /// </summary>
        public Guid TelescopeId { get; set; }

        /// <summary>
        /// Camera id
        /// </summary>
        public Guid CameraId { get; set; }

        /// <summary>
        /// Optional Barlow lens/Reducer id
        /// </summary>
        public Guid? LensId { get; set; }

        /// <summary>
        /// Frame width, in degrees of arc
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Frame height, in degrees of arc
        /// </summary>
        public float Height { get; set; }
        
        /// <summary>
        /// Camera rotation
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Rotation origin of the frame
        /// </summary>
        public FovFrameRotateOrigin RotateOrigin { get; set; }

        /// <summary>
        /// Camera binning
        /// </summary>
        public float Binning { get; set; }
    }

    /// <summary>
    /// Rotation origin of the FOV frame
    /// </summary>
    public enum FovFrameRotateOrigin
    {
        /// <summary>
        /// Frame is rotated around equatorial grid
        /// </summary>
        Equatorial = 0,

        /// <summary>
        /// Frame is rotated around horizontal grid
        /// </summary>
        Horizontal = 1,
    }
}
