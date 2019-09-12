using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public interface ISearcher
    {
        ICollection<SearchResultItem> Search(string searchString, Func<CelestialObject, bool> filter);
        string GetObjectName(CelestialObject body);
    }

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
