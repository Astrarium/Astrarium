using System.Collections.Generic;

namespace Astrarium.Plugins.BrightStars
{
    public interface IStarsReader
    {
        StarDetails GetStarDetails(ushort hrNumber);
        Dictionary<string, string> ReadAlphabet();
        ICollection<Star> ReadStars();
    }
}