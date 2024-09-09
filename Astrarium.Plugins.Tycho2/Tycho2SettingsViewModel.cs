using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.Plugins.Tycho2
{
    public class Tycho2SettingsViewModel : SettingsViewModel
    {
        public Tycho2Calc Catalog { get; private set; }

        public ICommand OpenCatalogUrlCommand { get; private set; }

        public ICommand DownloadCatalogCommand { get; private set; }

        public Func<string, bool> ValidateCatalogPath { get; private set; }

        public bool IsEnabled
        {
            get => GetValue(nameof(IsEnabled), true);
            set => SetValue(nameof(IsEnabled), value);
        }

        public Tycho2SettingsViewModel(ISettings settings, Tycho2Calc catalog) : base(settings)
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
                Process.Start("https://github.com/Astrarium/Tycho2");
            }
            catch { }
        }

        private async void DownloadCatalog()
        {
            Uri baseUri = new Uri("https://github.com/Astrarium/Tycho2/raw/main/");
            string catalogLocalPath = ViewManager.ShowSelectFolderDialog(Text.Get("Tycho2.Downloader.ChooseText"), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            if (catalogLocalPath != null)
            {
                // check directory is not empty and warn about it
                if (Directory.EnumerateFileSystemEntries(catalogLocalPath).Any())
                {
                    if (ViewManager.ShowMessageBox("$Warning", "$Tycho2.Downloader.DirectoryNotEmptyWarning", MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes) return;
                }

                var cancelTokenSource = new CancellationTokenSource();

                try
                {
                    IsEnabled = false;

                    var files = new string[] { "tycho2.dat", "tycho2.idx", "tycho2.ref" };
                    var progress = new Progress<double>();
                    ViewManager.ShowProgress("$Tycho2.Downloader.WaitTitle", "$Tycho2.Downloader.WaitText", cancelTokenSource, progress);

                    foreach (var file in files)
                    {
                        try
                        {
                            await DownloadFile(new Uri(baseUri, file), Path.Combine(catalogLocalPath, file), cancelTokenSource, progress);
                        }
                        catch { return; }
                        if (cancelTokenSource.IsCancellationRequested) return;
                    }
                }
                finally
                {
                    IsEnabled = true;
                }

                // need to hide progress dialog
                cancelTokenSource.Cancel();

                // validate catalog and save path in settings
                if (Catalog.Validate(catalogLocalPath, verbose: true))
                {
                    Settings.SetAndSave("Tycho2RootDir", catalogLocalPath);
                    ViewManager.ShowMessageBox("$Tycho2.Downloader.DownloadCompleteTitle", "$Tycho2.Downloader.DownloadCompleteText");
                }
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
    }
}
