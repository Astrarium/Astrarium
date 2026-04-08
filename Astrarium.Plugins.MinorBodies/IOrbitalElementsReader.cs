using System.Collections.Generic;

namespace Astrarium.Plugins.MinorBodies
{
    public interface IOrbitalElementsReader<TCelesitalBody>
    {
        ICollection<TCelesitalBody> Read(string orbitalElementsFile);
    }
}
