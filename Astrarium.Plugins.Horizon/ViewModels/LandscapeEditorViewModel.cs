using Astrarium.Types;
using System;
using System.Windows.Input;

namespace Astrarium.Plugins.Horizon.ViewModels
{
    public class LandscapeEditorViewModel : ViewModelBase
    {
        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        public string Title { get; set; }
        public decimal AzimuthShift { get; set; }
        public string Description { get; set; }

        public LandscapeEditorViewModel()
        {
            OKCommand = new Command(OK);
            CancelCommand = new Command(() => Close(false));
        }

        public void OK()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title))
                    throw new Exception(Text.Get("LandscapeEditorView.Validator.EmptyTitle"));
            }
            catch (Exception ex)
            {
                ViewManager.ShowMessageBox("$Warning", ex.Message);
                return;
            }

            Close(true);
        }
    }
}
