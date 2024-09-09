using System.Threading;

namespace Astrarium.Plugins.Journal.Types
{
    public interface IOALExporter
    {
        void ExportToOAL(string file, CancellationToken? token = null);
    }
}