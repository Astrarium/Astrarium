using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class FilterVM : ViewModelBase
    {
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private IDatabaseManager dbManager;

        public FilterVM(IDatabaseManager dbManager)
        {
            this.dbManager = dbManager;
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public Filter Filter
        {
            get => GetValue<Filter>(nameof(Filter));
            set => SetValue(nameof(Filter), value);
        }

        private async void Ok()
        {
            if (string.IsNullOrWhiteSpace(Filter.Model))
            {
                ViewManager.ShowMessageBox("$Warning", "Model should be specified");
            }
            else if (string.IsNullOrWhiteSpace(Filter.Vendor))
            {
                ViewManager.ShowMessageBox("$Warning", "Vendor should be specified");
            }
            else
            {
                await dbManager.SaveFilter(Filter);
                Close(true);
            }
        }
    }
}
