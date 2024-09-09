using System;
using System.Threading;

namespace Astrarium.Plugins.Journal
{
    public interface IOALImporter
    {
        void ImportFromOAL(string file, CancellationToken? token = null, IProgress<double> progress = null);
        event Action OnImportBegin;
        event Action<bool> OnImportEnd;
    }
}