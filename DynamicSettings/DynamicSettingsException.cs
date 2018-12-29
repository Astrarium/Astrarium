using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSettings
{
    public class DynamicSettingsException : Exception
    {
        public DynamicSettingsException(string message) : base(message) { }
    }
}
