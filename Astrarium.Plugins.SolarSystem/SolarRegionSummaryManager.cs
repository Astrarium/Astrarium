using Astrarium.Algorithms;
using Astrarium.Types;
using Astrarium.Types.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// This class is responsible for downloading SRS (solar region summary) data from NOAA Space Weather Prediction Center
    /// </summary>
    [Singleton]
    public class SolarRegionSummaryManager
    {
        /// <summary>
        /// Base FTP directory of warehouse: place where SRS archive is located
        /// </summary>
        private const string ftpRootDir = "ftp://ftp.swpc.noaa.gov:21/pub/warehouse";

        /// <summary>
        /// Local cache folder
        /// </summary>
        private string localCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "SRS");

        /// <summary>
        /// Requests queue.
        /// </summary>
        private ConcurrentQueue<DateTime> requests = new ConcurrentQueue<DateTime>();

        /// <summary>
        /// Reset event for processing requests
        /// </summary>
        private AutoResetEvent requestEvent = new AutoResetEvent(false);

        /// <summary>
        /// Locker to access requests
        /// </summary>
        private object locker = new object();

        /// <summary>
        /// Cached SRS data, for faster access
        /// </summary>
        private SolarRegionSummary cachedSummary = null;

        /// <summary>
        /// Fired on request complete
        /// </summary>
        internal event Action OnRequestComplete;

        /// <summary>
        /// Creates new instance of the SRS manager
        /// </summary>
        public SolarRegionSummaryManager()
        {
            if (!Directory.Exists(localCacheDir))
            {
                try
                {
                    Directory.CreateDirectory(localCacheDir);
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to create directory for SRS data: {localCacheDir}, Details: {ex}");
                }
            }

            new Thread(RequestWorker) { IsBackground = true }.Start();
        }

        /// <summary>
        /// Gets SRS data for sp
        /// </summary>
        /// <param name="jd"></param>
        /// <param name="utcOffset"></param>
        /// <returns></returns>
        public SolarRegionSummary GetSRSForJulianDate(double jd, double utcOffset)
        {
            Date date = new Date(jd, utcOffset);
            DateTime dt = new DateTime(date.Year, date.Month, (int)date.Day, 0, 0, 0, DateTimeKind.Utc);

            // no SRS data for future dates
            if (dt.Date.Ticks > DateTime.UtcNow.Date.Ticks)
            {
                return null;
            }

            string fileName = $"{date.Year:0000}{date.Month:00}{(int)date.Day:00}SRS.txt";
            string fullPath = Path.Combine(localCacheDir, fileName);

            if (cachedSummary != null && cachedSummary.FilePath == fullPath)
            {
                return cachedSummary;
            }

            if (File.Exists(fullPath))
            {
                cachedSummary = new SolarRegionSummary(fullPath);
                return cachedSummary;
            }
            else
            {
                lock (locker)
                {
                    if (!requests.Contains(dt))
                    {
                        requests.Enqueue(dt);
                        requestEvent.Set();
                    }
                }
            }

            return null;
        }

        private DateTime lastFailedDate;

        private void RequestWorker()
        {
            while (true)
            {
                requestEvent.WaitOne();

                while (requests.Any())
                {
                    if (requests.TryPeek(out DateTime date))
                    {
                        if (date.Equals(lastFailedDate))
                        {
                            Thread.Sleep(10000);
                        }

                        if (!RequestSolarRegionSumary(date))
                        {
                            lastFailedDate = date;
                        }

                        requests.TryDequeue(out date);
                    }
                }

                OnRequestComplete?.Invoke();
                Thread.Sleep(200);
            }
        }

        private bool RequestSolarRegionSumary(DateTime date)
        {
            string fileName = $"{date.Year:0000}{date.Month:00}{date.Day:00}SRS.txt";
            string fileUrl = $"{ftpRootDir}/{date.Year}/SRS/{fileName}";
            string fullPath = Path.Combine(localCacheDir, fileName);
            // STEP 1: try to load non-archived file 

            try
            {
                string tempFile = Path.GetTempFileName();

                Downloader.Download(new Uri(fileUrl), tempFile);
                
                File.Move(tempFile, fullPath);

                return true;
            }
            catch { }

            // STEP 2: assume the SRS data file can be downloaded from archive

            string archName = $"{date.Year:0000}_SRS.tar.gz";
            string archUrl = $"{ftpRootDir}/{date.Year}/{archName}";
            string tempArchivePath = Path.GetTempFileName();
            string tempFolder = $"{tempArchivePath}_extracted";

            try
            {
                // download archive from FTP
                Downloader.Download(new Uri(archUrl), tempArchivePath);

                // extract it to temp folder
                Archiver.ExtractTarGz(tempArchivePath, tempFolder);

                // enumerate files and move them to cache
                var files = Directory.GetFiles(Path.Combine(tempFolder, $"{date.Year:0000}_SRS"));
                foreach (var file in files)
                {
                    File.Move(file, Path.Combine(localCacheDir, Path.GetFileName(file)));
                }
            }
            catch { }
            finally
            {
                FileSystem.DeleteDirectory(tempFolder);
                FileSystem.DeleteFile(tempArchivePath);
            }

            return File.Exists(fullPath);
        }
    }
}
