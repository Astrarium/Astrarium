using Astrarium.Types;
using Astrarium.Types.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.ViewModels
{
    public class AppUpdateVM : ViewModelBase
    {
        private ISettings settings;

        public ICommand DownloadCommand { get; private set; }

        public AppUpdateVM(ISettings settings)
        {
            this.settings = settings;
            DownloadCommand = new Command(Download);
        }

        public string ReleaseNotes { get; private set; }

        public bool CheckUpdatesOnStart
        {
            get => settings.Get("CheckUpdatesOnStart");
            set
            {
                settings.SetAndSave("CheckUpdatesOnStart", value);
            }
        }

        public void SetReleaseInfo(LastRelease lastRelease)
        {
            ReleaseNotes = Text.Get("AppUpdateWindow.AppUpdateAvailable", ("version", lastRelease.Version.ToString()), ("releaseNotes", lastRelease.ReleaseNotes));
        }

        private async void Download()
        {
            Close();

            await Task.Run(() =>
            {
                string filePath = ViewManager.ShowSaveFileDialog("$Save", "Astrarium-installer", ".exe", "Application executable|*.exe|All files|*.*", out int selectedExtensionIndex);
                if (filePath != null)
                {
                    try
                    {
                        Downloader.Download(new Uri("https://github.com/Astrarium/Astrarium/releases/latest/download/Astrarium-setup.exe"), filePath);
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() => ViewManager.ShowMessageBox("$Error", $"{Text.Get("AppUpdateWindow.UnableToDownloadInstaller")}: {ex.Message}"));
                        return;
                    }

                    if (File.Exists(filePath))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var answer = ViewManager.ShowMessageBox("$Warning", "$AppUpdateWindow.InstallConfirmation", MessageBoxButton.YesNo);
                            if (answer == MessageBoxResult.Yes)
                            {
                                try
                                {
                                    ProcessStartInfo info = new ProcessStartInfo(filePath);
                                    info.UseShellExecute = true;
                                    info.Verb = "runas";
                                    Process.Start(info);
                                    Environment.Exit(0);
                                }
                                catch (Exception ex)
                                {
                                    ViewManager.ShowMessageBox("$Error", $"{Text.Get("AppUpdateWindow.UnableToStartInstaller")}: {ex.Message}");
                                }
                            }
                        });
                    }
                }
            });
        }
    }
}
