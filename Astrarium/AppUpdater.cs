using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;

namespace Astrarium
{
    public interface IAppUpdater
    {
        void CheckUpdates(Action<LastRelease> onUpdateFound, Action<Exception> onError = null);
    }

    public class AppUpdater : IAppUpdater
    {
        public void CheckUpdates(Action<LastRelease> onUpdateFound, Action<Exception> onError = null)
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

                return new LastRelease()
                {
                    Version = Version.Parse(releaseName),
                    ReleaseNotes = releaseNotes,
                    PublishDate = publishDate
                };
            }
        }
    }

    public class LastRelease
    {
        public Version Version { get; set; }
        public DateTime PublishDate { get; set; }
        public string ReleaseNotes { get; set; }
    }
}
