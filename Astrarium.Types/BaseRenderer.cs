using Astrarium.Algorithms;
using Astrarium.Types;
using System.Drawing;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all renderer classes which implement drawing logic of sky map.
    /// </summary>
    public abstract class BaseRenderer : PropertyChangedBase
    {
        /// <summary>
        /// Does the rendering logic
        /// </summary>
        /// <param name="map"></param>
        public abstract void Render(ISkyMap map);

        /// <summary>
        /// Intitialization logic should be placed here.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Gets rendering order for the renderer.
        /// </summary>
        public abstract RendererOrder Order { get; }

        public virtual void OnMouseMove(ISkyMap map, MouseButton mouseButton) { }
        public virtual void OnMouseDown(ISkyMap map, MouseButton mouseButton) { }
        public virtual void OnMouseUp(ISkyMap map, MouseButton mouseButton) { }
    }

    /// <summary>
    /// Defines default rendering layer
    /// </summary>
    public enum RendererOrder
    {
        /// <summary>
        /// Rendering layer for sky background
        /// </summary>
        Background = 0,

        /// <summary>
        /// Rendering layer for displaying celestial grids and other lines
        /// </summary>
        Grids = 1,

        /// <summary>
        /// Rendering layer for deep sky objects
        /// </summary>
        DeepSpace = 2,

        /// <summary>
        /// Rendering layer for stars
        /// </summary>
        Stars = 3,

        /// <summary>
        /// Rendering layer for solar system objects
        /// </summary>
        SolarSystem = 4,

        /// <summary>
        /// Rendering layer for objects on Earth orbit
        /// </summary>
        EarthOrbit = 5,

        /// <summary>
        /// Rendering layer for atmosphere
        /// </summary>
        Atmosphere = 6,

        /// <summary>
        /// Rendering layer for terrestrial objects
        /// </summary>
        Terrestrial = 7,

        /// <summary>
        /// Rendering layer for surrounding objects, like fog, solar rays flares and etc.
        /// </summary>
        Surround = 8,

        /// <summary>
        /// Rendering layer for foreground objects which overlap all previous layers
        /// </summary>
        Foreground = 9
    }
}
