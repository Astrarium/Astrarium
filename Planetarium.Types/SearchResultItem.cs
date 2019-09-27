using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Types
{
    public class SearchResultItem
    {
        public string Name { get; private set; }
        public CelestialObject Body { get; private set; }

        public SearchResultItem(CelestialObject body, string name)
        {
            Body = body;
            Name = name;
        }
    }
}
