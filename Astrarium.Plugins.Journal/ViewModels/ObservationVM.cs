using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class ObservationVM : ViewModelBase
    {
        public ObservationVM()
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public TimeSpan Begin 
        {
            get => GetValue<TimeSpan>(nameof(Begin));
            set => SetValue(nameof(Begin), value);
        }

        public TimeSpan End
        {
            get => GetValue<TimeSpan>(nameof(End));
            set => SetValue(nameof(End), value);
        }

        public DateTime Date
        {
            get => GetValue<DateTime>(nameof(Date));
            set => SetValue(nameof(Date), value);
        }

        public CelestialObject CelestialBody
        {
            get => GetValue<CelestialObject>(nameof(CelestialBody));
            set => SetValue(nameof(CelestialBody), value);
        }

        private void Ok()
        {
            if (CelestialBody == null)
            {
                ViewManager.ShowMessageBox("$Warning", "Please specifiy celestial object");
                return;
            }

            Close(true);
        }
    }
}
