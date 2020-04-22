namespace Astrarium.Types
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
