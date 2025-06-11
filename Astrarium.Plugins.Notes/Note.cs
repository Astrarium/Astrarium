using Astrarium.Types;

namespace Astrarium.Plugins.Notes
{
    public class Note
    {
        public CelestialObject Body { get; set; }
        public double Date { get; set; }
        public string Title { get; set; }
        public bool Markdown { get; set; }
        public string Description { get; set; }
    }
}
