using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.UCAC4.ViewModels
{
    public class UCAC4SettingsVM : SettingsViewModel
    {
        public UCAC4Catalog Catalog { get; private set; }

        public ICommand OpenCatalogUrlCommand { get; private set; }

        public ICommand DownloadCatalogCommand { get; private set; }

        public Func<string, bool> ValidateCatalogPath { get; private set; }

        public bool IsEnabled
        {
            get => GetValue(nameof(IsEnabled), true);
            set => SetValue(nameof(IsEnabled), value);
        }

        public UCAC4SettingsVM(ISettings settings, UCAC4Catalog catalog) : base(settings)
        {
            Catalog = catalog;
            OpenCatalogUrlCommand = new Command(OpenCatalogUrl);
            DownloadCatalogCommand = new Command(DownloadCatalog);
            ValidateCatalogPath = (string path) =>
            {
                return Catalog.Validate(path, verbose: true);
            };
        }

        private void OpenCatalogUrl()
        {
            try
            {
                Process.Start("https://cdsarc.cds.unistra.fr/ftp/I/322A/UCAC4/");
            }
            catch { }
        }

        private async void DownloadCatalog()
        {
            const string FTP_ROOT = "ftp://cdsarc.cds.unistra.fr:21/0/cats/I/322A/UCAC4";
            string catalogLocalPath = ViewManager.ShowSelectFolderDialog(Text.Get("UCAC4.Downloader.ChooseText"), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            if (catalogLocalPath != null)
            {
                // check directory is not empty and warn about it
                if (Directory.EnumerateFileSystemEntries(catalogLocalPath).Any())
                {
                    if (ViewManager.ShowMessageBox("$Warning", "$UCAC4.Downloader.DirectoryNotEmptyWarning", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes) return;
                }

                try
                {
                    IsEnabled = false;

                    // Anonymous FTP creds (empty)
                    var creds = new NetworkCredential();

                    // download zones
                    {
                        string u4bPath = Path.Combine(catalogLocalPath, "u4b");
                        var context = new FtpDownloadContext();

                        try
                        {
                            Directory.CreateDirectory(u4bPath);
                            ViewManager.ShowProgress("$UCAC4.Downloader.WaitTitle", "$UCAC4.Downloader.WaitText.DownloadingZones", context.CancelTokenSource, context.Progress);
                            await DownloadFtpDirectory($"{FTP_ROOT}/u4b/", creds, u4bPath, context);
                        }
                        catch (Exception ex)
                        {
                            context.CancelTokenSource.Cancel();
                            ViewManager.ShowMessageBox("$Error", $"{Text.Get("UCAC4.Downloader.Error.DownloadingZones")}\r\n{ex.Message}");
                        }

                        if (!context.CancelTokenSource.IsCancellationRequested)
                        {
                            context.CancelTokenSource.Cancel();
                        }

                        if (!context.DownloadComplete) return;
                    }

                    // download index files
                    {
                        string u4iPath = Path.Combine(catalogLocalPath, "u4i");

                        var context = new FtpDownloadContext();

                        try
                        {
                            Directory.CreateDirectory(u4iPath);
                            ViewManager.ShowProgress("$UCAC4.Downloader.WaitTitle", "$UCAC4.Downloader.WaitText.DownloadingIndex", context.CancelTokenSource, context.Progress);
                            await DownloadFtpDirectory($"{FTP_ROOT}/u4i/", creds, u4iPath, context);
                        }
                        catch (Exception ex)
                        {
                            context.CancelTokenSource.Cancel();
                            ViewManager.ShowMessageBox("$Error", $"{Text.Get("UCAC4.Downloader.Error.DownloadingIndex")}\r\n{ex.Message}");
                        }

                        if (!context.CancelTokenSource.IsCancellationRequested)
                        {
                            context.CancelTokenSource.Cancel();
                        }

                        if (!context.DownloadComplete) return;
                    }
                }
                finally
                {
                    IsEnabled = true;
                }

                // validate catalog and save path in settings
                if (Catalog.Validate(catalogLocalPath, verbose: true))
                {
                    Settings.SetAndSave("UCAC4RootDir", catalogLocalPath);
                    ViewManager.ShowMessageBox("$UCAC4.Downloader.DownloadCompleteTitle", "$UCAC4.Downloader.DownloadCompleteText");
                }
            }
        }

        private Task DownloadFtpDirectory(string url, NetworkCredential credentials, string localPath, FtpDownloadContext context)
        {
            return Task.Run(() =>
            {
                FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(url);
                listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                listRequest.Credentials = credentials;

                var lines = new List<string>();
                bool topLevel = context.TotalSize == 0;

                // list files and directories and get sizes
                using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
                using (var listStream = listResponse.GetResponseStream())
                using (var listReader = new StreamReader(listStream))
                {
                    while (!listReader.EndOfStream)
                    {
                        if (context.CancelTokenSource.IsCancellationRequested) return;

                        string line = listReader.ReadLine();
                        lines.Add(line);

                        if (topLevel)
                        {
                            string[] tokens = line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                            string size = tokens[4];
                            context.TotalSize += long.Parse(size);
                        }
                    }
                }

                // download files and subdurectories
                foreach (string line in lines)
                {
                    string[] tokens = line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                    string name = tokens[8];
                    string permissions = tokens[0];
                    string localFilePath = Path.Combine(localPath, name);
                    string fileUrl = url + name;

                    if (permissions[0] == 'd')
                    {
                        Directory.CreateDirectory(localFilePath);
                        DownloadFtpDirectory(fileUrl + "/", credentials, localFilePath, context);
                    }
                    else
                    {
                        string size = line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries)[4];
                        long fileSize = long.Parse(size);

                        if (File.Exists(localFilePath) && new FileInfo(localFilePath).Length == fileSize)
                        {
                            context.AddDownloadedCount(fileSize);
                            continue;
                        }

                        FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        downloadRequest.Credentials = credentials;

                        using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                        using (Stream sourceStream = downloadResponse.GetResponseStream())
                        using (Stream targetStream = File.Create(localFilePath))
                        {
                            byte[] buffer = new byte[1024 * 10];
                            int read;
                            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (context.CancelTokenSource.IsCancellationRequested)
                                {
                                    return;
                                }

                                targetStream.Write(buffer, 0, read);
                                context.AddDownloadedCount(read);
                            }
                        }
                    }
                }

                if (topLevel)
                {
                    context.DownloadComplete = true;
                }
            });
        }

        /// <summary>
        /// Helper class to store FTP downloading metadata
        /// </summary>
        private class FtpDownloadContext
        {
            /// <summary>
            /// Current count of downloaded bytes
            /// </summary>
            private long downloaded;

            /// <summary>
            /// Progress instance
            /// </summary>
            public Progress<double> Progress { get; private set; }

            /// <summary>
            /// Cancellation token source
            /// </summary>
            public CancellationTokenSource CancelTokenSource { get; private set; }

            /// <summary>
            /// Total size to be downloaded, in bytes
            /// </summary>
            public long TotalSize { get; set; }

            /// <summary>
            /// Flag indicating download complete
            /// </summary>
            public bool DownloadComplete { get; set; }

            /// <summary>
            /// Adds number of downloaded bytes and reports progress
            /// </summary>
            /// <param name="count"></param>
            public void AddDownloadedCount(long count)
            {
                downloaded += count;
                (Progress as IProgress<double>).Report((double)downloaded / TotalSize * 100);
            }

            /// <summary>
            /// Creates new context instance.
            /// </summary>
            public FtpDownloadContext()
            {
                CancelTokenSource = new CancellationTokenSource();
                Progress = new Progress<double>();
            }
        }
    }
}
