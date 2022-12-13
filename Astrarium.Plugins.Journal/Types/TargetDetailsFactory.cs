using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [Singleton(typeof(ITargetDetailsFactory))]
    public class TargetDetailsFactory : ITargetDetailsFactory
    {
        private readonly ISky sky;

        public TargetDetailsFactory(ISky sky)
        {
            this.sky = sky;
        }

        public TargetDetails BuildTargetDetails(CelestialObject body, SkyContext context)
        {
            // Celestial object type (unique type of object across the program and plugins)
            string bodyType = body.Type;

            // Target details class related to that celestial object type
            Type targetDetailsType = Assembly.GetAssembly(GetType()).GetTypes().FirstOrDefault(x => typeof(TargetDetails).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>().Any(a => a.CelestialObjectType == bodyType)) ?? typeof(TargetDetails);

            // Create empty target details
            TargetDetails targetDetails = (TargetDetails)Activator.CreateInstance(targetDetailsType);

            // Get all properties with Ephemeris attribute (they should be filled)
            var properties = targetDetailsType.GetProperties().Where(x => x.GetCustomAttribute<EphemerisAttribute>() != null);

            // Get unique codes of ephemerides
            string[] ephemerisCodes = properties.Select(x => x.GetCustomAttribute<EphemerisAttribute>().EphemerisCode).ToArray();

            // Get ephemerirides by codes
            var ephemerides = sky.GetEphemerides(body, context, ephemerisCodes);

            // Fill properties
            foreach (var prop in properties)
            {
                string ephemerisCode = prop.GetCustomAttribute<EphemerisAttribute>().EphemerisCode;
                object ephemeris = ephemerides.GetValueOrDefault<object>(ephemerisCode);
                Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                object safeValue = (ephemeris == null) ? null : Convert.ChangeType(ephemeris, propType);
                prop.SetValue(targetDetails, safeValue);
            }

            // Here we go!
            return targetDetails;
        }
    }
}
