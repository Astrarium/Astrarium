using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    throw new Exception("Title is missing");

                if (!string.IsNullOrWhiteSpace(URL) && Uri.TryCreate(URL, UriKind.Absolute, out Uri uriResult) && 
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                    throw new Exception("Invalid URL");
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
