using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class LensVM : ViewModelBase
    {
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public LensVM()
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public Lens Lens
        {
            get => GetValue<Lens>(nameof(Lens));
            set => SetValue(nameof(Lens), value);
        }

        private async void Ok()
        {
            if (string.IsNullOrWhiteSpace(Lens.Model))
            {
                ViewManager.ShowMessageBox("$Warning", "Model should be specified");
            }
            else if (string.IsNullOrWhiteSpace(Lens.Vendor))
            {
                ViewManager.ShowMessageBox("$Warning", "Vendor should be specified");
            }
            else
            {
                await DatabaseManager.SaveLens(Lens);
                Close(true);
            }
        }
    }
}
