using Planetarium.Types;

namespace Planetarium.Renderers
{
    /// <summary>
    /// Base class for all renderer classes which implement drawing logic of sky map.
    /// </summary>
    public abstract class BaseRenderer
    {
        public abstract void Render(IMapContext map);
        public virtual void Initialize() { }
        public virtual bool NeedRenderOnMouseMove => false;
        public abstract RendererOrder Order { get; }
        public abstract string Name { get; }
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
