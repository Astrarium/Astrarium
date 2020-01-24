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

namespace Planetarium.ViewModels
{
    public class SelectFolderVM : ViewModelBase
    {
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }
    }
}
