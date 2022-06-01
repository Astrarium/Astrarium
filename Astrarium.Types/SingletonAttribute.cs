using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    /// <summary>
    /// Indicates that instance of the class should be a singleton
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SingletonAttribute : Attribute 
    {
        /// <summary>
        /// Type of interface
        /// </summary>
        public Type InterfaceType { get; set; }

        public SingletonAttribute() { }

        public SingletonAttribute(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"The {interfaceType} type should be an interface.");
            }

            InterfaceType = interfaceType;
        }
    }
}
