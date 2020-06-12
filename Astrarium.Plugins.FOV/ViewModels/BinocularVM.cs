using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    public class BinocularVM : ViewModelBase
    {
        public ICollection<Binocular> Binoculars { get; set; } = new List<Binocular>();
        public Binocular Binocular { get; set; } = new Binocular() { Id = Guid.NewGuid(), Aperture = 50, FieldOfView = 10, Magnification = 50 };

        public Command OkCommand { get; }
        public Command CancelCommand { get; }

        public BinocularVM()
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }
    
        private void Ok()
        {
            if (string.IsNullOrWhiteSpace(Binocular.Name))
            {
                ViewManager.ShowMessageBox(Text.Get("BinocularWindow.WarningTitle"), Text.Get("BinocularWindow.EmptyNameWarningMessage"));
            }
            else if (Binoculars.Any(t => t.Name == Binocular.Name && t.Id != Binocular.Id))
            {
                ViewManager.ShowMessageBox(Text.Get("BinocularWindow.WarningTitle"), Text.Get("BinocularWindow.NameAlreadyExistsWarningMessage")); 
            }
            else
            {
                Close(true);
            }
        }
    }
}
