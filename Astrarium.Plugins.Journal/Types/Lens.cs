using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class Lens : PropertyChangedBase
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

        public double Factor
        {
            get => GetValue(nameof(Factor), 2.0);
            set => SetValue(nameof(Factor), value);
        }
    }
}
