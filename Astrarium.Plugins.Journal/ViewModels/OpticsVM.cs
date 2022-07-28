using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class OpticsVM : ViewModelBase
    {
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public OpticsVM()
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public Optics Optics
        {
            get => GetValue<Optics>(nameof(Optics));
            set => SetValue(nameof(Optics), value);
        }

        private async void Ok()
        {
            if (string.IsNullOrWhiteSpace(Optics.Model))
            {
                ViewManager.ShowMessageBox("$Warning", "Model should be specified");
            }
            else if (string.IsNullOrWhiteSpace(Optics.Vendor))
            {
                ViewManager.ShowMessageBox("$Warning", "Vendor should be specified");
            }
            else
            {
                await DatabaseManager.SaveOptics(Optics);
                Close(true);
            }
        }
    }
}
