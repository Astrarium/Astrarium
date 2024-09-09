using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class EyepieceVM : ViewModelBase
    {
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private IDatabaseManager dbManager;

        public EyepieceVM(IDatabaseManager dbManager)
        {
            this.dbManager = dbManager;
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public Eyepiece Eyepiece
        {
            get => GetValue<Eyepiece>(nameof(Eyepiece));
            set => SetValue(nameof(Eyepiece), value);
        }

        private async void Ok()
        {
            if (string.IsNullOrWhiteSpace(Eyepiece.Model))
            {
                ViewManager.ShowMessageBox("$Warning", "Model should be specified");
            }
            else if (string.IsNullOrWhiteSpace(Eyepiece.Vendor))
            {
                ViewManager.ShowMessageBox("$Warning", "Vendor should be specified");
            }
            else
            {
                await dbManager.SaveEyepiece(Eyepiece);
                Close(true);
            }
        }
    }
}
