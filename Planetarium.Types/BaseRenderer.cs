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
        public virtual int ZOrder => 0;
        public virtual string Name => GetType().Name;
    }
}
