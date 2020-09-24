using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    public class EyepieceVM : ViewModelBase
    {
        public ICollection<Eyepiece> Eyepieces { get; set; } = new List<Eyepiece>();
        public Eyepiece Eyepiece { get; set; } = new Eyepiece() { Id = Guid.NewGuid(), FieldOfView = 50, FocalLength = 10 };

        public Command OkCommand { get; }
        public Command CancelCommand { get; }

        public EyepieceVM()
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }
    
        private void Ok()
        {
            if (string.IsNullOrWhiteSpace(Eyepiece.Name))
            {
                ViewManager.ShowMessageBox("$EyepieceWindow.WarningTitle", "$EyepieceWindow.EmptyNameWarningMessage");
            }
            else if (Eyepieces.Any(t => t.Name == Eyepiece.Name && t.Id != Eyepiece.Id))
            {
                ViewManager.ShowMessageBox("$EyepieceWindow.WarningTitle", "$EyepieceWindow.NameAlreadyExistsWarningMessage");
            }
            else
            {
                Close(true);
            }
        }
    }
}
