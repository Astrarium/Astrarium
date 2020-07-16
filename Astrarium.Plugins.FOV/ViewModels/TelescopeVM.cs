using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    public class TelescopeVM : ViewModelBase
    {
        public ICollection<Telescope> Telescopes { get; set; } = new List<Telescope>();
        public Telescope Telescope { get; set; } = new Telescope() { Id = Guid.NewGuid(), Aperture = 50, FocalLength = 500 };

        public Command OkCommand { get; }
        public Command CancelCommand { get; }

        public TelescopeVM()
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }
    
        private void Ok()
        {
            if (string.IsNullOrWhiteSpace(Telescope.Name))
            {
                ViewManager.ShowMessageBox(Text.Get("TelescopeWindow.WarningTitle"), Text.Get("TelescopeWindow.EmptyNameWarningMessage"));
            }
            else if (Telescopes.Any(t => t.Name == Telescope.Name && t.Id != Telescope.Id))
            {
                ViewManager.ShowMessageBox(Text.Get("TelescopeWindow.WarningTitle"), Text.Get("TelescopeWindow.NameAlreadyExistsWarningMessage"));
            }
            else
            {
                Close(true);
            }
        }
    }
}
