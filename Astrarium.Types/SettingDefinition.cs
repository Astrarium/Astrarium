using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public class SettingDefinition
    {
        public string Name { get; private set; }
        public object DefaultValue { get; private set; }
        public bool IsPermanent { get; private set; }

        public SettingDefinition(string name, object defaultValue, bool isPermanent = false)
        {
            Name = name;
            DefaultValue = defaultValue;
            IsPermanent = isPermanent;
        }
    }

    public class SettingSectionDefinition
    {
        public Type ViewType { get; private set; }
        public Type ViewModelType { get; private set; }
        public SettingSectionDefinition(Type viewType, Type viewModelType)
        {
            ViewType = viewType;
            ViewModelType = viewModelType;
        }
    }
}
