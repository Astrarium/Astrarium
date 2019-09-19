using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class SplashScreenVM : ViewModelBase, IProgress<string>
    {
        public string Progress { get; private set; }

        public void Report(string value)
        {
            Progress = value;
            NotifyPropertyChanged(nameof(Progress));
        }
    }
}
