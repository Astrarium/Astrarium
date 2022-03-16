using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
{
    public interface IOrbitalElementsReader<TCelesitalBody>
    {
        ICollection<TCelesitalBody> Read(string orbitalElementsFile);
    }
}
