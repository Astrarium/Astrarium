using Astrarium.Types;
using Astrarium.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Workers
{
    public class AppUpdater : IWorker
    {
        private readonly ISettings settings;

        public AppUpdater(ISettings settings)
        {
            this.settings = settings;
            ViewManager.RegisterMessageHandler("CheckForUpdates", new Command(CheckForUpdates));
        }

        public void Run()
        {
            if (settings.Get("CheckUpdatesOnStart"))
            {
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    CheckUpdates(OnAppUpdateFound);
                });
            }
        }

        private async void CheckForUpdates()
        {
            await Task.Run(() => CheckUpdates(OnAppUpdateFound, OnAppUpdateNotFound, OnAppUpdateError));
        }

        private void CheckUpdates(Action<LastRelease> onUpdateFound, Action onUpdateNotFound = null, Action<Exception> onError = null)
        {
            try
            {
                var lastRelease = GetLatestRelease("Astrarium", "Astrarium");
                Assembly assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                Version currentVersion = Version.Parse(fileVersionInfo.ProductVersion);
                if (lastRelease.Version.CompareTo(currentVersion) > 0)
                {
                    onUpdateFound?.Invoke(lastRelease);
                }
                else
                {
                    onUpdateNotFound?.Invoke();
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        private LastRelease GetLatestRelease(string owner, string repo)
        {
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Ssl3;

            Uri uri = new Uri($"https://api.github.com/repos/{owner}/{repo}/releases/latest");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = "CheckGitHubUpdates/1.0";
            WebResponse response = request.GetResponse();
            var serializer = new JsonSerializer();
            using (var responseStream = response.GetResponseStream())
            using (var sr = new StreamReader(responseStream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var obj = serializer.Deserialize(jsonTextReader);
                var jObject = (JObject)obj;

                string releaseName = jObject["name"].ToString();
                string releaseNotes = jObject["body"].ToString();
                DateTime publishDate = jObject["published_at"].Value<DateTime>();
                string releaseVersion = GetNumericVersion(releaseName);

                return new LastRelease()
                {
                    Version = Version.Parse(releaseVersion),
                    ReleaseNotes = releaseNotes,
                    PublishDate = publishDate
                };
            }
        }

        public string GetNumericVersion(string version)
        {
            return new string(version.Where(p => char.IsDigit(p) || p == '.').ToArray());
        }

        private void OnAppUpdateFound(LastRelease lastRelease)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var vm = ViewManager.CreateViewModel<AppUpdateVM>();
                vm.SetReleaseInfo(lastRelease);
                ViewManager.ShowDialog(vm);
            });
        }

        private void OnAppUpdateNotFound()
        {
            Application.Current.Dispatcher.Invoke(() => ViewManager.ShowMessageBox("$Information", "$AppUpdateWindow.OnAppUpdateNotFound"));
        }

        private void OnAppUpdateError(Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() => ViewManager.ShowMessageBox("$Error", $"{Text.Get("AppUpdateWindow.OnAppUpdateError")}: {ex.Message}"));
        }
    }
}
