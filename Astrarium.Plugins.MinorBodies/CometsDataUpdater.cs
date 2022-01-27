using Astrarium.Types;
using System;

namespace Astrarium.Plugins.MinorBodies
{
    public class CometsDataUpdater : DataUpdater<Comet>
    {
        public CometsDataUpdater(ISettings settings, CometsReader reader, OrbitalElementsDownloader downloader) : 
            base(settings, reader, downloader) { }

        protected override string DownloadUrl => Settings.Get<string>("CometsDownloadOrbitalElementsUrl");
        protected override int MaxCount => (int)Settings.Get<decimal>("CometsDownloadOrbitalElementsCount");
        protected override Func<string, bool> Matcher => (line) => line.Length >= 168;
        protected override string FileName => "Comets.dat";
        protected override string TimeStampKey => "CometsDownloadOrbitalElementsTimestamp";
    }
}
