using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class DonateVM : ViewModelBase
    {
        public Command DelayCommand { get; private set; }
        public Command DonateCommand { get; private set; }
        public Command BlockCommand { get; private set; }
        public Command DismissCommand { get; private set; }

        public bool OpenedByUser { get; set; }

        public DonationResult Result { get; private set; }

        public DonateVM()
        {
            DelayCommand = new Command(() => CloseWithResult(DonationResult.Delayed));
            DonateCommand = new Command(() => CloseWithResult(DonationResult.Donated));
            BlockCommand = new Command(() => CloseWithResult(DonationResult.Blocked));
            DismissCommand = new Command(Close);
        }

        private void CloseWithResult(DonationResult result)
        {
            Result = result;
            Close();
        }
    }
}
