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
        private delegate ICollection<SearchResultItem> SearchDelegate(string searchString, int maxCount = 50);
        private delegate CelestialObjectInfo GetInfoDelegate<T>(SkyContext context, T body) where T : CelestialObject;

        private List<BaseCalc> Calculators = new List<BaseCalc>();
        private Dictionary<Type, EphemerisConfig> EphemConfigs = new Dictionary<Type, EphemerisConfig>();
        private Dictionary<Type, Delegate> InfoProviders = new Dictionary<Type, Delegate>();
        private Dictionary<Type, SearchDelegate> SearchProviders = new Dictionary<Type, SearchDelegate>();
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

            var item = config.FirstOrDefault(c => c.Key == ephemKey);

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

        public List<Dictionary<string, object>> GetEphemeris(CelestialObject body, double from, double to, double step, ICollection<string> keys)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            var config = EphemConfigs[body.GetType()];

            var itemsToBeCalled = config.Filter(keys);

            for (double jd = from; jd < to; jd += step)
            {
                var context = new SkyContext(jd, Context.GeoLocation);

                Dictionary<string, object> ephemeris = new Dictionary<string, object>();

                foreach (var item in itemsToBeCalled)
                {
                    object value = item.Formula.DynamicInvoke(context, body);
                    ephemeris.Add(item.Key, value);
                }

                result.Add(ephemeris);
            }

            return result;
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

        public ICollection<AstroEvent> GetEvents(double jdFrom, double jdTo)
        {            
            var context = new AstroEventsContext()
            {
                From = jdFrom,
                To = jdTo,
                GeoLocation = Context.GeoLocation
            };

            var events = new List<AstroEvent>();
            foreach (var item in EventConfigs.SelectMany(c => c.Items))
            {
                events.AddRange(item.Formula.Invoke(context));
            }
            
            return events
                .OrderBy(e => e.JulianDay)
                .Where(e => e.JulianDay >= context.From && e.JulianDay < context.To)
                .ToArray();
        }

        public ICollection<SearchResultItem> Search(string searchString, int maxCount = 50)
        {
            var results = new List<SearchResultItem>();
            if (!string.IsNullOrWhiteSpace(searchString))
            {               
                foreach (var searchProvider in SearchProviders.Values)
                {
                    if (results.Count < maxCount)
                    {
                        results.AddRange(searchProvider(searchString, maxCount));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return results.Take(maxCount).OrderBy(r => r.Name).ToList();
        }

        public ICollection<CelestialObject> CelestialObjects<T>(Func<T, bool> search = null) where T : CelestialObject
        {
            return null;
        }

        //public void AddTrack(Track track)
        //{
        //    if (!(track.Body is IMovingObject))
        //        throw new Exception($"The '{track.Body.GetType()}' class should implement '{nameof(IMovingObject)}' interface.");

        //    var positions = GetEphemeris<CrdsEquatorial>(track.Body, track.From, track.To, track.Step, "Equatorial");
        //    foreach (var eq in positions)
        //    {
        //        track.Points.Add(new CelestialPoint() { Equatorial0 = eq });
        //    }

        //    Tracks.Add(track);
        //    Calculate();
        //}
    }
}
