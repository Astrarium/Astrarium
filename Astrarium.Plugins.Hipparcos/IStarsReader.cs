using System.Collections.Generic;

namespace Astrarium.Plugins.Hipparcos
{
    public interface IStarsReader
    {
        Dictionary<string, string> ReadAlphabet();
        ICollection<Star> ReadStars();
        ICollection<Region> ReadIndex();
    }
}