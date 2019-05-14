using Planetarium.Calculators;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private List<SearchDelegate> SearchProviders = new List<SearchDelegate>();
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

                IEnumerable<Type> genericCalcTypes = calcType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICelestialObjectCalc<>));

                foreach (Type genericCalcType in genericCalcTypes)
                {
                    Type bodyType = genericCalcType.GetGenericArguments().First();
                    Type concreteCalc = typeof(ICelestialObjectCalc<>).MakeGenericType(bodyType);

                    // Ephemeris configs
                    EphemerisConfig config = Activator.CreateInstance(typeof(EphemerisConfig<>).MakeGenericType(bodyType)) as EphemerisConfig;
                    concreteCalc.GetMethod(nameof(ICelestialObjectCalc<CelestialObject>.ConfigureEphemeris)).Invoke(calc, new object[] { config });
                    if (config.Any())
                    {
                        EphemConfigs[bodyType] = config;
                    }

                    // Info provider
                    Type genericGetInfoFuncType = typeof(GetInfoDelegate<>).MakeGenericType(bodyType);
                    InfoProviders[bodyType] = concreteCalc.GetMethod(nameof(ICelestialObjectCalc<CelestialObject>.GetInfo)).CreateDelegate(genericGetInfoFuncType, calc) as Delegate;

                    // Name provider
                    Type genericGetNameFuncType = typeof(GetNameDelegate<>).MakeGenericType(bodyType);
                    NameProviders[bodyType] = concreteCalc.GetMethod(nameof(ICelestialObjectCalc<CelestialObject>.GetName)).CreateDelegate(genericGetNameFuncType, calc) as Delegate;

                    // Search provider
                    var searchFunc = concreteCalc.GetMethod(nameof(ICelestialObjectCalc<CelestialObject>.Search)).CreateDelegate(typeof(SearchDelegate), calc) as SearchDelegate;
                    if (!SearchProviders.Contains(searchFunc))
                    {
                        SearchProviders.Add(searchFunc);
                    }
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

        public List<List<Ephemeris>> GetEphemerides(CelestialObject body, double from, double to, double step, IEnumerable<string> categories, CancellationToken? cancelToken = null, IProgress<double> progress = null)
        {
            List<List<Ephemeris>> ephemerides = new List<List<Ephemeris>>();

            var config = EphemConfigs[body.GetType()];

            var itemsToBeCalled = config.Filter(categories);

            for (double jd = from; jd < to; jd += step)
            {
                if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                {
                    break;
                }
                else
                {
                    progress?.Report((jd - from) / (to - from) * 100);
                }

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
            var type = body.GetType();
            if (EphemConfigs.ContainsKey(type))
            {
                return EphemConfigs[type].Where(c => c.IsAvailable?.Invoke(body) ?? true).Select(c => c.Category).Distinct().ToArray();
            }
            else
            {
                return new string[0];
            }
            
        }

        public ICollection<AstroEvent> GetEvents(double jdFrom, double jdTo, IEnumerable<string> categories, CancellationToken? cancelToken = null)
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
                if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                {
                    break;
                }
                else
                {
                    events.AddRange(item.Formula.Invoke(context));
                }
            }
            
            return events
                .OrderBy(e => e.NoExactTime ? e.JulianDay + 1 : e.JulianDay)
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
                foreach (var searchProvider in SearchProviders)
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
