using Astrarium.Algorithms;
using System.Threading;

namespace Astrarium.Types
{
    public class AstroEventsContext : Memoizer<AstroEventsContext>
    {
        public CrdsGeographical GeoLocation { get; set; }
        public double From { get; set; }
        public double To { get; set; }
        public CancellationToken? CancelToken { get; set; }
    }
}
