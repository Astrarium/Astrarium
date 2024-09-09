using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class Filter : PropertyChangedBase
    {
        public string Id { get; set; }

        public string Vendor
        {
            get => GetValue<string>(nameof(Vendor));
            set => SetValue(nameof(Vendor), value);
        }

        public string Model
        {
            get => GetValue<string>(nameof(Model));
            set => SetValue(nameof(Model), value);
        }

        public string Type
        {
            get => GetValue<string>(nameof(Type));
            set => SetValue(nameof(Type), value);
        }

        public string Color
        {
            get => GetValue<string>(nameof(Color));
            set => SetValue(nameof(Color), value);
        }

        public string Wratten
        {
            get => GetValue<string>(nameof(Wratten));
            set => SetValue(nameof(Wratten), value);
        }

        public string Schott
        {
            get => GetValue<string>(nameof(Schott));
            set => SetValue(nameof(Schott), value);
        }
    }
}
