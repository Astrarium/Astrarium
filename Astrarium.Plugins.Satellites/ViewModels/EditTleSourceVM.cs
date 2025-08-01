using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Input;

namespace Astrarium.Plugins.Satellites.ViewModels
{
    public class EditTleSourceVM : ViewModelBase
    {
        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public DateTime? LastUpdated
        {
            get => GetValue<DateTime?>(nameof(LastUpdated));
            set => SetValue(nameof(LastUpdated), value);
        }

        public bool IsEnabled
        {
            get => GetValue<bool>(nameof(IsEnabled));
            set => SetValue(nameof(IsEnabled), value);
        }

        public string Url
        {
            get => GetValue<string>(nameof(Url));
            set => SetValue(nameof(Url), value);
        }

        public string FileName
        {
            get => GetValue<string>(nameof(FileName));
            set => SetValue(nameof(FileName), value);
        }

        private ICollection<TLESource> tleSources;
        private TLESource tleSource;

        public void SetTleSource(ICollection<TLESource> tleSources, TLESource tleSource)
        {
            this.tleSources = tleSources;
            this.tleSource = tleSource;
            IsEnabled = tleSource.IsEnabled;
            Url = tleSource.Url;
            FileName = tleSource.FileName;
            LastUpdated = tleSource.LastUpdated;
        }

        public TLESource GetTleSource()
        {
            return new TLESource()
            {
                IsEnabled = IsEnabled,
                FileName = FileName,
                Url = Url,
                LastUpdated = tleSource.Url == Url ? LastUpdated : null
            };
        }

        public EditTleSourceVM(ISettings settings)
        {
            OkCommand = new Command(OK);
            CancelCommand = new Command(Close);
        }

        private void OK()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FileName))
                    throw new Exception("$TLESource.Validator.EmptyName");

                if (FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    throw new Exception("$TLESource.Validator.InvalidName");

                if (tleSources.Any(x => x != tleSource && x.FileName.Equals(FileName, StringComparison.OrdinalIgnoreCase)))
                    throw new Exception("$TLESource.Validator.NameAlreadyUsed");

                if (string.IsNullOrWhiteSpace(Url))
                    throw new Exception("$TLESource.Validator.EmptyUrl");

                if (!RemoteFileExists(Url))
                    throw new Exception("$TLESource.Validator.UnavailableUrl");

                Close(true);
            }
            catch (Exception ex)
            {
                ViewManager.ShowMessageBox("$Warning", ex.Message);
            }
        }

        public override object Payload => new { File = FileName, Url = Url };

        private bool RemoteFileExists(string url)
        {
            try
            {
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Ssl3;
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                response.Close();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }
    }
}
