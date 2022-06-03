using Astrarium.Types;
using System.Windows.Input;

namespace Astrarium.ViewModels
{
    public class AppUpdateVM : ViewModelBase
    {
        public ICommand GoToDownloadPageCommand { get; private set; }

        public AppUpdateVM()
        {
            GoToDownloadPageCommand = new Command(GoToDownloadPage);
        }

        public string ReleaseNotes { get; private set; }

        public void SetReleaseInfo(LastRelease lastRelease)
        {
            ReleaseNotes = $"New version of **Astarium {lastRelease.Version}** is available!\r\n What's new:\r\n\r\n" + lastRelease.ReleaseNotes;
        }

        private void GoToDownloadPage()
        {
            System.Diagnostics.Process.Start("https://github.com/Astrarium/Astrarium/releases/latest/download/Astrarium-setup.exe");
        }
    }
}
