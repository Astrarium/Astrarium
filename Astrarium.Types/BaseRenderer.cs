using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Renderers
{
    /// <summary>
    /// Base class for all renderer classes which implement drawing logic of sky map.
    /// </summary>
    public abstract class BaseRenderer : PropertyChangedBase
    {
        public abstract void Render(IMapContext map);
        public virtual void Initialize() { }
        public abstract RendererOrder Order { get; }

        /// <summary>
        /// The function is called each time when position of mouse is changed.
        /// Mouse position, converted to horizontal coordinates on map, is passed as parameter.
        /// The function should return true if repaint of map is required, 
        /// otherwise it should return false (default behaviour).
        /// </summary>
        /// <param name="mouse">Current mouse position on sky map</param>
        /// <returns>True if repaint of map is required, otherwise false.</returns>
        public virtual bool OnMouseMove(CrdsHorizontal mouse, MouseButton mouseButton) { return false; }
    }

    public enum RendererOrder
    {
        Background = 0,
        Grids = 1,
        DeepSpace = 2,
        Stars = 3,
        SolarSystem = 4,
        EarthOrbit = 5,
        Terrestrial = 6,
        Foreground = 7
    }
}
