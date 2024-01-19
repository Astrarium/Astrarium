using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.Plugins.DeepSky.ViewModels
{
    public class DeepSkySettingsViewModel : SettingsViewModel
    {
        public bool IsEnabled
        {
            get => GetValue(nameof(IsEnabled), true);
            set => SetValue(nameof(IsEnabled), value);
        }

        public string ImagesState
        {
            get
            {
                int count = GetDownloadedImagesCount(Settings.Get("DeepSkyImagesFolder", ""));
                if (count > 0)
                    return Text.Get("DeepSkyImages.Found", ("count", count.ToString()));
                else
                    return Text.Get("DeepSkyImages.NotFound");
            }
        }

        public bool ImagesFound => GetDownloadedImagesCount(Settings.Get("DeepSkyImagesFolder", "")) > 0;

        public ICommand OpenImagesUrlCommand { get; private set; }
        public ICommand DownloadImagesCommand { get; private set; }

        public DeepSkySettingsViewModel(ISettings settings) : base(settings)
        {
            OpenImagesUrlCommand = new Command(OpenImagesUrl);
            DownloadImagesCommand = new Command(DownloadImages);
        }

        private void OpenImagesUrl()
        {
            try
            {
                Process.Start("https://github.com/Astrarium/DeepSkyImages");
            }
            catch { }
        }

        private async void DownloadImages()
        {
            Uri imagesUri = new Uri("https://github.com/Astrarium/DeepSkyImages/releases/download/v1.0/DeepSkyImages.zip");
            string imagesLocalPath = ViewManager.ShowSelectFolderDialog(Text.Get("DeepSkyImages.Downloader.ChooseText"), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            if (imagesLocalPath != null)
            {
                // check directory is not empty and warn about it
                if (Directory.EnumerateFileSystemEntries(imagesLocalPath).Any())
                {
                    if (ViewManager.ShowMessageBox("$Warning", "$DeepSkyImages.Downloader.DirectoryNotEmptyWarning", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                }

                string zipPath = Path.Combine(imagesLocalPath, "DeepSkyImages.zip");

                try
                {
                    IsEnabled = false;
                    var progress = new Progress<double>();

                    // download zip archive with images 
                    {
                        var cancelTokenSource = new CancellationTokenSource();
                        ViewManager.ShowProgress("$DeepSkyImages.Downloader.WaitTitle", "$DeepSkyImages.Downloader.WaitText.Downloading", cancelTokenSource, progress);
                        try
                        {
                            await DownloadFile(imagesUri, zipPath, cancelTokenSource, progress);
                        }
                        catch { }
                        if (cancelTokenSource.IsCancellationRequested) return;
                        cancelTokenSource.Cancel();
                    }

                    // extracting images from zip
                    {
                        var cancelTokenSource = new CancellationTokenSource();
                        ViewManager.ShowProgress("$DeepSkyImages.Downloader.WaitTitle", "$DeepSkyImages.Downloader.WaitText.Extracting", cancelTokenSource, progress);
                        try
                        {
                            await ExtractZip(zipPath, imagesLocalPath, cancelTokenSource, progress);
                        }
                        catch { }
                        if (cancelTokenSource.IsCancellationRequested) return;
                        cancelTokenSource.Cancel();
                    }
                }
                finally
                {
                    IsEnabled = true;
                }

                // validate images path and save path in settings
                if (Validate(imagesLocalPath))
                {
                    Settings.SetAndSave("DeepSkyImagesFolder", imagesLocalPath);                    
                    ViewManager.ShowMessageBox("$DeepSkyImages.Downloader.DownloadCompleteTitle", "$DeepSkyImages.Downloader.DownloadCompleteText");
                }

                NotifyPropertyChanged(nameof(ImagesFound), nameof(ImagesState));
            }
        }

        private Task DownloadFile(Uri uri, string localPath, CancellationTokenSource cancelTokenSource, Progress<double> progress)
        {
            using (WebClient wc = new WebClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                wc.DownloadProgressChanged += (s, e) =>
                {
                    if (cancelTokenSource.IsCancellationRequested)
                    {
                        wc.CancelAsync();
                    }
                    (progress as IProgress<double>).Report(e.ProgressPercentage);
                };
                return wc.DownloadFileTaskAsync(uri, localPath);
            }
        }

        private Task ExtractZip(string zipPath, string destinationFolder, CancellationTokenSource cancelTokenSource, Progress<double> progress)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {
                        FileInfo fi = new FileInfo(zipPath);
                        long compressedLength = fi.Length;
                        long extractedLength = 0;
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            entry.ExtractToFile(Path.Combine(destinationFolder, entry.FullName), true);
                            extractedLength += entry.CompressedLength;
                            (progress as IProgress<double>).Report(100.0 * extractedLength / compressedLength);
                            if (cancelTokenSource.IsCancellationRequested) return;
                        }
                    }
                }
                finally
                {
                    File.Delete(zipPath);
                }
            });
        }

        private int GetDownloadedImagesCount(string imagesDir)
        {
            return
                Directory.Exists(imagesDir) ?
                    Directory.EnumerateFiles(imagesDir, "NGC *.jpg").Count() +
                    Directory.EnumerateFiles(imagesDir, "IC *.jpg").Count() : 0;
        }

        private bool Validate(string imagesDir)
        {
            return GetDownloadedImagesCount(imagesDir) > 0;
        }
    }
}
