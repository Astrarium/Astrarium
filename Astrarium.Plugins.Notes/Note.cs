using Astrarium.Types;
using Newtonsoft.Json;

namespace Astrarium.Plugins.Notes
{
    public class Note
    {
        [JsonIgnore]
        public CelestialObject Body { get; set; }

        public string BodyType { get; set; }
        public string BodyName { get; set; }
        public double Date { get; set; }
        public string Title { get; set; }
        public bool Markdown { get; set; }
        public string Description { get; set; }
    }
}
