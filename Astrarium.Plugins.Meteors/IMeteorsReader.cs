using System.Collections.Generic;

namespace Astrarium.Plugins.Meteors
{
    public interface IMeteorsReader
    {
        ICollection<Meteor> Read(string filePath);
    }
}