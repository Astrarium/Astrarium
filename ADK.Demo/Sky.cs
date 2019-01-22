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

        private List<Type> CelestialObjectTypes = new List<Type>();
        private Dictionary<Type, Delegate> InfoProviders = new Dictionary<Type, Delegate>();
        private Dictionary<Type, EphemerisConfig> EphemConfigs = new Dictionary<Type, EphemerisConfig>();

        public void Initialize()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            CelestialObjectTypes = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && typeof(CelestialObject).IsAssignableFrom(t))
                .ToList();

            Type ephemProviderType = typeof(IEphemProvider<>);
            Type infoProviderType = typeof(IInfoProvider<>);

            string configureEphemerisMethodName = nameof(IEphemProvider<CelestialObject>.ConfigureEphemeris);

            foreach (var calc in Calculators)
            {
                calc.Initialize();

                foreach (Type bodyType in CelestialObjectTypes)
                {
                    Type genericEphemProviderType = ephemProviderType.MakeGenericType(bodyType);
                    Type genericInfoProviderType = infoProviderType.MakeGenericType(bodyType);

                    if (genericEphemProviderType.IsAssignableFrom(calc.GetType()))
                    {
                        EphemerisConfig config = Activator.CreateInstance(typeof(EphemerisConfig<>).MakeGenericType(bodyType)) as EphemerisConfig;
                        genericEphemProviderType.GetMethod(configureEphemerisMethodName).Invoke(calc, new object[] { config });
                        EphemConfigs[bodyType] = config;
                    }

                    if (genericInfoProviderType.IsAssignableFrom(calc.GetType()))
                    {
                        Type funcType = typeof(Func<,,>);
                        Type genericFuncType = funcType.MakeGenericType(typeof(SkyContext), bodyType, typeof(string));
                        InfoProviders[bodyType] = genericInfoProviderType.GetMethod(nameof(IInfoProvider<CelestialObject>.GetInfo)).CreateDelegate(genericFuncType, calc);
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

            var config = EphemConfigs[obj.GetType()];

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

        public string GetInfo(CelestialObject body)
        {
            Type bodyType = body.GetType();
            if (InfoProviders.ContainsKey(bodyType))
            {
                return InfoProviders[bodyType].DynamicInvoke(Context, body).ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
