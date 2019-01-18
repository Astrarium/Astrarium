using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class Sky
    {
        public SkyContext Context { get; set; }

        public ICollection<BaseSkyCalc> Calculators { get; private set; } = new List<BaseSkyCalc>();

        private Dictionary<string, object> DataProviders = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void AddDataProvider<T>(string key, Func<T> provider)
        {
            DataProviders.Add(key, provider);
        }

        public T Get<T>(string key)
        {
            if (DataProviders.ContainsKey(key))
            {
                return (DataProviders[key] as Func<T>).Invoke();
            }
            else
            {
                throw new ArgumentException($"There is no data provider with name `{key}`.");
            }
        }

        private List<Type> celestialObjectTypes = new List<Type>();
        private Dictionary<Type, BaseSkyCalc> ephemProviders = new Dictionary<Type, BaseSkyCalc>();
        private Dictionary<Type, EphemerisConfig> ephemConfigs = new Dictionary<Type, EphemerisConfig>();

        public void Initialize()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            celestialObjectTypes = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && typeof(CelestialObject).IsAssignableFrom(t))
                .ToList();

            Type providerType = typeof(IEphemProvider<>);

            string configureEphemerisMethodName = nameof(IEphemProvider<CelestialObject>.ConfigureEphemeris);

            foreach (var calc in Calculators)
            {
                calc.Initialize();

                foreach (Type celestialObjectType in celestialObjectTypes)
                {
                    Type genericProviderType = providerType.MakeGenericType(celestialObjectType);

                    if (genericProviderType.IsAssignableFrom(calc.GetType()))
                    {
                        EphemerisConfig config = Activator.CreateInstance(typeof(EphemerisConfig<>).MakeGenericType(celestialObjectType)) as EphemerisConfig;
                        genericProviderType.GetMethod(configureEphemerisMethodName).Invoke(calc, new object[] { config });
                        ephemConfigs[celestialObjectType] = config;
                        ephemProviders[celestialObjectType] = calc;
                    }
                }
            }
        }

        public Sky()
        {
            Context = new SkyContext(
                new Date(DateTime.Now).ToJulianEphemerisDay(),
                new CrdsGeographical(56.3333, -44));
        }

        public void Calculate()
        {
            foreach (var calc in Calculators)
            {
                calc.Calculate(Context);
            }
        }

        public List<Dictionary<string, object>> GetEphemeris(CelestialObject obj, double from, double to, ICollection<string> keys)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            var config = ephemConfigs[obj.GetType()];

            var itemsToBeCalled = config.Filter(keys);

            for (double jd = from; jd < to; jd++)
            {
                var context = new SkyContext(jd, Context.GeoLocation);

                Dictionary<string, object> ephemeris = new Dictionary<string, object>();

                foreach (var item in itemsToBeCalled)
                {
                    object value = item.Formula.DynamicInvoke(context, obj);

                    if (item.Formatter != null)
                    {
                        ephemeris.Add(item.Key, item.Formatter.Format(value));
                    }
                    else
                    {
                        ephemeris.Add(item.Key, value);
                    }
                }

                result.Add(ephemeris);
            }

            return result;
        }
    }
}
