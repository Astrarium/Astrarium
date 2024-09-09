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

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CelestialObjectTypeDiscriminatorAttribute: Attribute
    {
        public Type Discriminator { get; private set; }

        public CelestialObjectTypeDiscriminatorAttribute(Type discriminator)
        {
            if (typeof(ICelestialObjectTypeDiscriminator).IsAssignableFrom(discriminator))
            {
                Discriminator = discriminator;
            }
            else
            {
                throw new ArgumentException($"{discriminator.Name} should implement {nameof(ICelestialObjectTypeDiscriminator)} interface.");
            }
        }
    }

    public interface ICelestialObjectTypeDiscriminator
    {
        string Discriminate(object dataObject);
    }
}
