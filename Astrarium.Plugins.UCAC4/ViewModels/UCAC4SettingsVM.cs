using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            string localPath = ViewManager.ShowSelectFolderDialog("Choose detination", "C:\\");
            if (localPath != null)
            {
                NetworkCredential creds = new NetworkCredential();

                string u4bPath = Path.Combine(localPath, "u4b");
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                var progress = new Progress<double>();

                Directory.CreateDirectory(u4bPath);

                ViewManager.ShowProgress("Please, wait...", "Downloading catalog zones", tokenSource, progress);
                await DownloadFtpDirectory("ftp://cdsarc.cds.unistra.fr:21/0/cats/I/322A/UCAC4/u4b/", creds, u4bPath, tokenSource.Token, progress);
            }
        }

        private Task DownloadFtpDirectory(string url, NetworkCredential credentials, string localPath, CancellationToken token, IProgress<double> progress, bool topLevel = true)
        {
            return Task.Run(() =>
            {
                FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(url);
                listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                listRequest.Credentials = credentials;

                List<string> lines = new List<string>();

                using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
                using (Stream listStream = listResponse.GetResponseStream())
                using (var listReader = new StreamReader(listStream))
                {
                    while (!listReader.EndOfStream)
                    {
                        if (token.IsCancellationRequested)
                        {
                            listRequest.Abort();
                            return;
                        }
                        lines.Add(listReader.ReadLine());
                    }
                }

                long totalSize = 0;

                foreach (string line in lines)
                {
                    if (token.IsCancellationRequested)
                    {
                        listRequest.Abort();
                        return;
                    }

                    string[] tokens = line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                    string name = tokens[8];
                    string permissions = tokens[0];
                    string size = tokens[4];
                    string localFilePath = Path.Combine(localPath, name);
                    string fileUrl = url + name;

                    if (topLevel)
                    {
                        totalSize += long.Parse(size);
                    }

                    if (permissions[0] == 'd')
                    {
                        if (!Directory.Exists(localFilePath))
                        {
                            Directory.CreateDirectory(localFilePath);
                        }

                        DownloadFtpDirectory(fileUrl + "/", credentials, localFilePath, token, progress, topLevel: false);
                    }
                    else
                    {
                        FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        downloadRequest.Credentials = credentials;

                        using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                        using (Stream sourceStream = downloadResponse.GetResponseStream())
                        using (Stream targetStream = File.Create(localFilePath))
                        {
                            byte[] buffer = new byte[10240];
                            int read;
                            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                targetStream.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            });
        }
    }
}
