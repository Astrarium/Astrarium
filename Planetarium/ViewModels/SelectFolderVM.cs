using Planetarium.Objects;
using Planetarium.Types.Themes;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Planetarium.Types.Localization;
using System.IO;

namespace Planetarium.ViewModels
{
    public class SelectFolderVM : ViewModelBase
    {
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        private string selectedPath = "C:\\";
        public string SelectedPath 
        { 
            get { return selectedPath; } 
            set
            {
                if (Directory.Exists(value))
                {
                    selectedPath = value;
                }
            }
        }

        public SelectFolderVM()
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        private void Ok()
        {
            Close(true);
        }
    }
}
