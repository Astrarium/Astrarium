using Planetarium.Calculators;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public interface ISearcher
    {
        ICollection<SearchResultItem> Search(string searchString, Func<CelestialObject, bool> filter);
        string GetObjectName(CelestialObject body);
    }

    public class Sky : ISearcher, IEphemerisProvider
    {
        private delegate string GetNameDelegate<T>(T body) where T : CelestialObject;
        private delegate ICollection<SearchResultItem> SearchDelegate(string searchString, int maxCount = 50);
        private delegate CelestialObjectInfo GetInfoDelegate<T>(SkyContext context, T body) where T : CelestialObject;

        private List<BaseCalc> Calculators = new List<BaseCalc>();
        private Dictionary<Type, EphemerisConfig> EphemConfigs = new Dictionary<Type, EphemerisConfig>();
        private Dictionary<Type, Delegate> InfoProviders = new Dictionary<Type, Delegate>();
        private Dictionary<Type, SearchDelegate> SearchProviders = new Dictionary<Type, SearchDelegate>();
        private Dictionary<Type, Delegate> NameProviders = new Dictionary<Type, Delegate>();
        private List<BaseAstroEventsProvider> EventProviders = new List<BaseAstroEventsProvider>();
        private List<AstroEventsConfig> EventConfigs = new List<AstroEventsConfig>();

        public SkyContext Context { get; private set; }

        public void Initialize()
        {
            foreach (var calc in Calculators)
            {
                calc.Initialize();

                Type calcType = calc.GetType();
                Type calcBaseType = calcType.BaseType;

                if (calcBaseType.IsGenericType && calcBaseType.GetGenericTypeDefinition() == typeof(BaseCalc<>))
                {
                    Type bodyType = calcBaseType.GetGenericArguments().First();

                    // Ephemeris configs
                    EphemerisConfig config = Activator.CreateInstance(typeof(EphemerisConfig<>).MakeGenericType(bodyType)) as EphemerisConfig;
                    calcType.GetMethod(nameof(BaseCalc<CelestialObject>.ConfigureEphemeris)).Invoke(calc, new object[] { config });
                    if (config.Any())
                    {
                        EphemConfigs[bodyType] = config;
                    }

                    // Info provider
                    Type genericGetInfoFuncType = typeof(GetInfoDelegate<>).MakeGenericType(bodyType);
                    InfoProviders[bodyType] = calcType.GetMethod(nameof(BaseCalc<CelestialObject>.GetInfo)).CreateDelegate(genericGetInfoFuncType, calc) as Delegate;

                    // Name provider
                    Type genericGetNameFuncType = typeof(GetNameDelegate<>).MakeGenericType(bodyType);
                    NameProviders[bodyType] = calcType.GetMethod(nameof(BaseCalc<CelestialObject>.GetName)).CreateDelegate(genericGetNameFuncType, calc) as Delegate;

                    // Search provider
                    var searchFunc = calcType.GetMethod(nameof(BaseCalc<CelestialObject>.Search)).CreateDelegate(typeof(SearchDelegate), calc) as SearchDelegate;
                    SearchProviders[bodyType] = searchFunc;
                }
            }

            foreach (var eventProvider in EventProviders)
            {
                // Astro events provider
                AstroEventsConfig eventsConfig = new AstroEventsConfig();
                eventProvider.ConfigureAstroEvents(eventsConfig);
                if (eventsConfig.Any())
                {
                    EventConfigs.Add(eventsConfig);
                }     
            }
        }

        public Sky(SkyContext context, ICollection<BaseCalc> calculators, ICollection<BaseAstroEventsProvider> eventProviders)
        {
            Calculators.AddRange(calculators);
            EventProviders.AddRange(eventProviders);
            Context = context;
        }

        public void Calculate()
        {
            foreach (var calc in Calculators)
            {
                calc.Calculate(Context);
            }
        }

        public T GetEphemeris<T>(CelestialObject body, double jd, string ephemKey)
        {
            return GetEphemeris<T>(body, jd, jd + 1, 2, ephemKey).First();
        }

        public ICollection<T> GetEphemeris<T>(CelestialObject body, double from, double to, double step, string ephemKey)
        {
            Type bodyType = body.GetType();

            if (body == null)
                throw new ArgumentNullException("Celestial body should not be null.", nameof(body));

            if (string.IsNullOrWhiteSpace(ephemKey))
                throw new ArgumentException("Ephemeris key should not be null or empty.", nameof(body));

            if (!EphemConfigs.ContainsKey(bodyType))
                throw new ArgumentException($"Object of type '{bodyType}' does not have configured ephemeris provider.", nameof(body));

            var config = EphemConfigs[body.GetType()];

            var item = config.FirstOrDefault(c => c.Category == ephemKey);

            if (item == null)
                throw new Exception($"Unknown ephemeris key '{ephemKey}'. Check that corresponding ephemeris provider has required ephemeris key.");

            List<T> result = new List<T>();

            for (double jd = from; jd <= to; jd += step)
            {
                var context = new SkyContext(jd, Context.GeoLocation);

                object value = item.Formula.DynamicInvoke(context, body);

                if (!(value is T))
                    throw new Exception($"Ephemeris with key '{ephemKey}' has type {value.GetType()} but requested cast to {typeof(T)}.");

                result.Add((T)value);
            }

            return result;
        }

        public List<List<Ephemeris>> GetEphemerides(CelestialObject body, double from, double to, double step, IEnumerable<string> categories)
        {
            List<List<Ephemeris>> ephemerides = new List<List<Ephemeris>>();

            var config = EphemConfigs[body.GetType()];

            var itemsToBeCalled = config.Filter(categories);

            for (double jd = from; jd < to; jd += step)
            {
                var context = new SkyContext(jd, Context.GeoLocation);

                List<Ephemeris> ephemeris = new List<Ephemeris>();

                foreach (var item in itemsToBeCalled)
                {
                    ephemeris.Add(new Ephemeris()
                    {
                        Key = item.Category,
                        Value = item.Formula.DynamicInvoke(context, body),
                        Formatter = item.Formatter ?? Formatters.GetDefault(item.Category)
                    });
                }

                ephemerides.Add(ephemeris);
            }

            return ephemerides;
        }

        public CelestialObjectInfo GetInfo(CelestialObject body)
        {
            Type bodyType = body.GetType();

            if (InfoProviders.ContainsKey(bodyType))
            {
                return (CelestialObjectInfo)InfoProviders[bodyType].DynamicInvoke(Context, body);
            }
            else
            {
                return null;
            }
        }

        public ICollection<string> GetEventsCategories()
        {
            return EventConfigs.SelectMany(c => c.Items).Select(i => i.Key).Distinct().ToArray();
        }

        public ICollection<string> GetEphemerisCategories(CelestialObject body)
        {
            return EphemConfigs[body.GetType()].Where(c => c.IsAvailable?.Invoke(body) ?? true).Select(c => c.Category).Distinct().ToArray();
        }

        public ICollection<AstroEvent> GetEvents(double jdFrom, double jdTo, ICollection<string> categories)
        {            
            var context = new AstroEventsContext()
            {
                From = jdFrom,
                To = jdTo,
                GeoLocation = Context.GeoLocation
            };

            var events = new List<AstroEvent>();
            foreach (var item in EventConfigs.SelectMany(c => c.Items).Where(i => categories.Contains(i.Key)))
            {
                events.AddRange(item.Formula.Invoke(context));
            }
            
            return events
                .OrderBy(e => e.NoExactTime)
                .ThenBy(e => e.JulianDay)
                .Where(e => e.JulianDay >= context.From && e.JulianDay < context.To)
                .ToArray();
        }

        public ICollection<SearchResultItem> Search(string searchString, Func<CelestialObject, bool> filter)
        {
            int maxCount = 50;
            var filterFunc = filter ?? ((b) => true);
            var results = new List<SearchResultItem>();
            if (!string.IsNullOrWhiteSpace(searchString))
            {               
                foreach (var searchProvider in SearchProviders.Values)
                {
                    if (results.Count < maxCount)
                    {
                        results.AddRange(searchProvider(searchString, maxCount).Where(r => filterFunc(r.Body)));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return results.Take(maxCount).OrderBy(r => r.Name).ToList();
        }

        public string GetObjectName(CelestialObject body)
        {
            Type bodyType = body.GetType();

            if (NameProviders.ContainsKey(bodyType))
            {
                return (string)NameProviders[bodyType].DynamicInvoke(body);
            }
            else
            {
                return body.ToString();
            }
        }
    }
}
