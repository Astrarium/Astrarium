using Astrarium.Types;
using Astrarium.Types.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Satellites
{
    [Singleton(typeof(IOrbitalElementsUpdater))]
    public class OrbitalElementsUpdater : IOrbitalElementsUpdater
    {
        public event Action<TLESource> OrbitalElementsUpdated;

        public async Task<bool> UpdateOrbitalElements(TLESource tleSource, bool silent)
        {
            var tokenSource = new CancellationTokenSource();

            if (!silent)
            {
                ViewManager.ShowProgress("$Satellites.Downloader.WaitTitle", "$Satellites.Downloader.WaitText", tokenSource);
            }

            bool result = await UpdateOrbitalElements(tokenSource, tleSource);

            if (!silent)
            {
                if (result)
                {
                    ViewManager.ShowMessageBox("$Success", "$Satellites.Downloader.Success");
                }
                else
                {
                    ViewManager.ShowMessageBox("$Error", "$Satellites.Downloader.Fail");
                }
            }

            if (result)
            {
                OrbitalElementsUpdated?.Invoke(tleSource);
            }

            return result;
        }

        private Task<bool> UpdateOrbitalElements(CancellationTokenSource tokenSource, TLESource tleSource)
        {
            return Task.Run(() =>
            {
                string tempFile = Path.GetTempFileName();

                try
                {
                    // download to temp dir
                    Downloader.Download(new Uri(tleSource.Url), tempFile, tokenSource);

                    // use app data path to satellites data (downloaded by user)
                    string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Satellites");
                    string targetPath = Path.Combine(directory, tleSource.FileName + ".tle");

                    // move to cache folder
                    Directory.CreateDirectory(directory);
                    File.Copy(tempFile, targetPath, overwrite: true);

                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    tokenSource.Cancel();

                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            });
        }
    }
}
