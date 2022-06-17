using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class Attachment : PropertyChangedBase
    {
        public string Id { get; set; }

        public string FilePath
        {
            get => GetValue(nameof(FilePath), "");
            set => SetValue(nameof(FilePath), value);
        }

        public string Title
        {
            get => GetValue(nameof(Title), "");
            set => SetValue(nameof(Title), value);
        }

        public string Comments
        {
            get => GetValue(nameof(Comments), "");
            set => SetValue(nameof(Comments), value);
        }
    }
}
