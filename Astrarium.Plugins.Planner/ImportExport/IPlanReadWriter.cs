using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public interface IPlanReadWriter
    {
        ICollection<CelestialObject> Read(string filePath, CancellationToken? token = null, IProgress<double> progress = null);
        void Write(ICollection<Ephemerides> plan, string filePath);
    }
}