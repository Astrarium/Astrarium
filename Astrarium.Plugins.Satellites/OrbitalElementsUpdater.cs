using Astrarium.Types;
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
        private const int BUFFER_SIZE = 1024;

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
                    ViewManager.ShowMessageBox("$Error", $"Satellites.Downloader.Fail");
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
                bool result = true;
                string tempFile = Path.GetTempFileName();

                try
                {
                    ServicePointManager.SecurityProtocol =
                                    SecurityProtocolType.Tls |
                                    SecurityProtocolType.Tls11 |
                                    SecurityProtocolType.Tls12 |
                                    SecurityProtocolType.Ssl3;

                    // use app data path to satellites data (downloaded by user)
                    string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Satellites");
                    string targetPath = Path.Combine(directory, tleSource.FileName + ".tle");

                    WebRequest request = WebRequest.Create(tleSource.Url);
                    WebResponse response = request.GetResponse();
                    using (Stream responseStream = response.GetResponseStream())
                    using (Stream fileStream = new FileStream(tempFile, FileMode.OpenOrCreate))
                    using (BinaryWriter streamWriter = new BinaryWriter(fileStream))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead = 0;
                        StringBuilder remainder = new StringBuilder();

                        do
                        {
                            if (tokenSource.IsCancellationRequested) return false;
                            bytesRead = responseStream.Read(buffer, 0, BUFFER_SIZE);
                            streamWriter.Write(buffer, 0, bytesRead);
                        }
                        while (bytesRead > 0);
                    }

                    Directory.CreateDirectory(directory);
                    File.Copy(tempFile, targetPath, overwrite: true);

                    tokenSource.Cancel();
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to download orbital elements of satellites ({tleSource.FileName}). Reason: {ex}");
                    result = false;
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }


                }

                return result;
            });
        }
    }
}
