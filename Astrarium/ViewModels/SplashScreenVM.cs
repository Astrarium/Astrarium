﻿using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class SplashScreenVM : ViewModelBase, IProgress<string>
    {
        public string Progress { get; private set; } = "Initializing";

        public override bool Loggable => false;

        public void Report(string value)
        {
            Progress = value;
            NotifyPropertyChanged(nameof(Progress));
        }
    }
}
