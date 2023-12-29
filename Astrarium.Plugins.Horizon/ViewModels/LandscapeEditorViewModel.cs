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
        public string Author { get; set; }
        public decimal AzimuthShift { get; set; }
        public string Copyright { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }

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

                Uri uriResult;
                if (!string.IsNullOrWhiteSpace(URL) && !(Uri.TryCreate(URL, UriKind.Absolute, out uriResult) && 
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
                    throw new Exception(Text.Get("LandscapeEditorView.Validator.InvalidUrl"));
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
