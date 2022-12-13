using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CelestialObjectTypeAttribute : Attribute
    {
        public string CelestialObjectType { get; private set; }

        public CelestialObjectTypeAttribute(string celestialObjectType)
        {
            CelestialObjectType = celestialObjectType;
        }
    }
}
