using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class CameraVM : ViewModelBase
    {
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private IDatabaseManager dbManager;

        public CameraVM(IDatabaseManager dbManager)
        {
            this.dbManager = dbManager;
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public Camera Camera
        {
            get => GetValue<Camera>(nameof(Camera));
            set => SetValue(nameof(Camera), value);
        }

        private async void Ok()
        {
            if (string.IsNullOrWhiteSpace(Camera.Model))
            {
                ViewManager.ShowMessageBox("$Warning", "Model should be specified");
            }
            else if (string.IsNullOrWhiteSpace(Camera.Vendor))
            {
                ViewManager.ShowMessageBox("$Warning", "Vendor should be specified");
            }
            else
            {
                await dbManager.SaveCamera(Camera);
                Close(true);
            }
        }
    }
}
