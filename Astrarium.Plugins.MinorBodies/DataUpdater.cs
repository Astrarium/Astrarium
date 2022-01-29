using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
{
    public abstract class DataUpdater<TCelestialBody>
    {
        protected ISettings Settings;
        protected IOrbitalElementsReader<TCelestialBody> Reader;
        protected OrbitalElementsDownloader Downloader;

        protected abstract string DownloadUrl { get; }
        protected abstract int MaxCount { get; }
        protected abstract Func<string, bool> Matcher { get; }
        protected abstract string FileName { get; }
        protected abstract string TimeStampKey { get; }

        public DataUpdater(ISettings settings, IOrbitalElementsReader<TCelestialBody> reader, OrbitalElementsDownloader downloader)
        {
            Settings = settings;
            Reader = reader;
            Downloader = downloader;
        }

        public async Task<ICollection<TCelestialBody>> Update(bool silent)
        {
            string tempFile = Path.GetTempFileName();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Progress<double> progress = new Progress<double>();

            if (!silent)
            {
                ViewManager.ShowProgress("$DataUpdater.ProgressTitle", "$DataUpdater.ProgressText", tokenSource, progress);
            }

            Exception error = null;
            int totalCount = 0;
            bool userCanceled = false;
            try
            {
                totalCount = await Task.Run(() => Downloader.Download(DownloadUrl, tempFile, MaxCount, Matcher, tokenSource, progress));
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                userCanceled = tokenSource.IsCancellationRequested;
                tokenSource.Cancel();
            }

            if (userCanceled)
            {
                DeleteTempFile(tempFile);
                return null;
            }

            if (error != null)
            {
                Trace.TraceError($"Unable to update orbital elements. Error: {error}");
                if (!silent)
                {
                    ViewManager.ShowMessageBox("$Error", $"{Text.Get("DataUpdater.DownloadError")}: {error.Message}");
                }
                DeleteTempFile(tempFile);
                return null;
            }

            if (totalCount == 0)
            {
                Trace.TraceWarning("Unable to download orbital elements. File does not contain data.");
                if (!silent)
                {
                    ViewManager.ShowMessageBox("$Error", "$DataUpdater.NoData");
                }
                DeleteTempFile(tempFile);
                return null;
            }

            ICollection<TCelestialBody> orbitalElements = new TCelestialBody[0];
            try
            {
                orbitalElements = await Task.Run(() => Reader.Read(tempFile));
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                Trace.TraceError($"Unable to read orbital elements. Error: {error}");
                if (!silent)
                {
                    ViewManager.ShowMessageBox("$Error", $"{Text.Get("DataUpdater.ParseError")}: {error.Message}");
                }
                DeleteTempFile(tempFile);
                return null;
            }

            if (!orbitalElements.Any())
            {
                Trace.TraceError($"Unable to read orbital elements.");
                if (!silent)
                {
                    ViewManager.ShowMessageBox("$Error", Text.Get("DataUpdater.ParseError"));
                }
                DeleteTempFile(tempFile);
                return null;
            }

            try
            {
                string orbitalElementsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "OrbitalElements");
                Directory.CreateDirectory(orbitalElementsPath);
                string targetPath = Path.Combine(orbitalElementsPath, FileName);
                File.Copy(tempFile, targetPath, true);
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                DeleteTempFile(tempFile);
            }

            if (error != null)
            {
                Trace.TraceError($"Unable to copy elements file. Error: {error}");
                if (!silent)
                {
                    ViewManager.ShowMessageBox("$Error", $"{Text.Get("DataUpdater.CopyError")}: {error.Message}");
                }
            }

            if (error == null)
            {
                Settings.SetAndSave(TimeStampKey, DateTime.Now);
                if (!silent)
                {
                    ViewManager.ShowMessageBox("$Success", "$DataUpdater.Success");
                }
            }

            return orbitalElements;
        }

        private void DeleteTempFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Unable to delete temp file {file}: {ex.Message}");
            }
        }
    }
}
