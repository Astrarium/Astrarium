using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium
{
    public class Sky : ISky
    {
        private delegate ICollection<CelestialObject> SearchDelegate(SkyContext context, string searchString, Func<CelestialObject, bool> filter, int maxCount = 50);
        private delegate void GetInfoDelegate<T>(CelestialObjectInfo<T> body) where T : CelestialObject;
        private delegate IEnumerable<T> GetCelestialObjectsDelegate<T>() where T : CelestialObject;

        private Dictionary<Type, Delegate> CelestialObjectsProviders = new Dictionary<Type, Delegate>();
        private Dictionary<Type, EphemerisConfig> EphemConfigs = new Dictionary<Type, EphemerisConfig>();
        private Dictionary<Type, Delegate> InfoProviders = new Dictionary<Type, Delegate>();
        private List<SearchDelegate> SearchProviders = new List<SearchDelegate>();
        private List<AstroEventsConfig> EventConfigs = new List<AstroEventsConfig>();
        private Dictionary<string, Constellation> Constellations = new Dictionary<string, Constellation>();

        private List<BaseCalc> Calculators = new List<BaseCalc>();
        private List<BaseAstroEventsProvider> EventProviders = new List<BaseAstroEventsProvider>();
        public SkyContext Context { get; private set; }

        public event Action Calculated;
        public event Action DateTimeSyncChanged;

        private ManualResetEvent dateTimeSyncResetEvent = new ManualResetEvent(false);
        private bool dateTimeSync = false;
        public bool DateTimeSync
        {
            get { return dateTimeSync; }
            set
            {
                if (dateTimeSync != value)
                {
                    dateTimeSync = value;
                    if (dateTimeSync)
                    {
                        dateTimeSyncResetEvent.Set();
                    }
                    else
                    {
                        dateTimeSyncResetEvent.Reset();
                    }
                    DateTimeSyncChanged?.Invoke();
                }
            }
        }

        /// <inheritdoc />
        public ICollection<Tuple<int, int>> ConstellationLines { get; set; } = new Tuple<int, int>[0];

        public IEnumerable<CelestialObject> CelestialObjects
        {
            get
            {
                return CelestialObjectsProviders.SelectMany(p => p.Value.DynamicInvoke() as IEnumerable<CelestialObject>);
            }
        }

        /// <inheritdoc />
        public Func<SkyContext, CrdsEquatorial> SunEquatorial { get; set; } = (c) => new CrdsEquatorial(0, 0);

        public Sky()
        {
            new Thread(() =>
            {
                do
                {
                    dateTimeSyncResetEvent.WaitOne();
                    Context.JulianDay = new Date(DateTime.Now).ToJulianEphemerisDay();
                    Calculate();
                    Thread.Sleep(1000);
                }
                while (true);
            })
            { IsBackground = true }.Start();
        }

        public void Initialize(SkyContext context, ICollection<BaseCalc> calculators, ICollection<BaseAstroEventsProvider> eventProviders)
        {
            Context = context;
            Calculators.AddRange(calculators);
            EventProviders.AddRange(eventProviders);
            
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
                    if (!config.IsEmpty)
                    {
                        EphemConfigs[bodyType] = config;
                    }

                    // Celestial objects providers
                    Type genericGetCelestialObjectsFuncType = typeof(GetCelestialObjectsDelegate<>).MakeGenericType(bodyType);
                    CelestialObjectsProviders[bodyType] = concreteCalc.GetMethod(nameof(ICelestialObjectCalc<CelestialObject>.GetCelestialObjects)).CreateDelegate(genericGetCelestialObjectsFuncType, calc);

                    // Info provider
                    Type genericGetInfoFuncType = typeof(GetInfoDelegate<>).MakeGenericType(bodyType);
                    InfoProviders[bodyType] = concreteCalc.GetMethod(nameof(ICelestialObjectCalc<CelestialObject>.GetInfo)).CreateDelegate(genericGetInfoFuncType, calc);

                    // Search provider
                    Type[] searchParameters = typeof(SearchDelegate).GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();
                    var searchFunc = concreteCalc.GetMethod(nameof(ICelestialObjectCalc<CelestialObject>.Search), searchParameters).CreateDelegate(typeof(SearchDelegate), calc) as SearchDelegate;
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

            LoadConstNames();
        }

        /// <summary>
        /// Loads constellation labels data
        /// </summary>
        // TODO: move to reader
        private void LoadConstNames()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/ConNames.dat");
            string line = "";
            using (var sr = new StreamReader(file, Encoding.Default))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    var chunks = line.Split(';');
                    string code = chunks[0].Trim().ToUpper();                    
                    string name = chunks[1].Trim();
                    string genitive = chunks[2].Trim();

                    Constellations.Add(code, new Constellation()
                    {
                        Code = code,
                        LatinName = name,
                        LatinGenitiveName = genitive
                    });
                }
            }
        }

        public void SetDate(double jd)
        {
            DateTimeSync = false;
            Context.JulianDay = jd;
            Calculate();
        }

        public void Calculate()
        {
            foreach (var calc in Calculators)
            {
                calc.Calculate(Context);
            }
            Calculated?.Invoke();
        }

        // TODO: add ability to specify location and preferFast flag
        public List<Ephemerides> GetEphemerides(CelestialObject body, double from, double to, double step, IEnumerable<string> categories, CancellationToken? cancelToken = null, IProgress<double> progress = null)
        {
            List<Ephemerides> all = new List<Ephemerides>();

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

                Ephemerides ephemerides = new Ephemerides(body);

                foreach (var item in itemsToBeCalled)
                {
                    ephemerides.Add(new Ephemeris(item.Category, item.Formula.DynamicInvoke(context, body), item.Formatter ?? Formatters.GetDefault(item.Category)));
                }

                all.Add(ephemerides);
            }

            return all;
        }

        public Ephemerides GetEphemerides(CelestialObject body, SkyContext context, IEnumerable<string> categories)
        {
            var config = EphemConfigs[body.GetType()];
            var itemsToBeCalled = config.Filter(categories);
            Ephemerides ephemerides = new Ephemerides(body);
            foreach (var item in itemsToBeCalled)
            {
                ephemerides.Add(new Ephemeris(item.Category, item.Formula.DynamicInvoke(context, body), item.Formatter ?? Formatters.GetDefault(item.Category)));
            }
            return ephemerides;
        }

        public CelestialObjectInfo GetInfo(CelestialObject body)
        {
            Type bodyType = body.GetType();

            if (InfoProviders.ContainsKey(bodyType))
            {
                var ephem = GetEphemerides(body, Context.JulianDay, Context.JulianDay + 1, 1, GetEphemerisCategories(body)).First();

                var infoType = typeof(CelestialObjectInfo<>).MakeGenericType(bodyType);
                var info = (CelestialObjectInfo)Activator.CreateInstance(infoType, Context, body, ephem);

                InfoProviders[bodyType].DynamicInvoke(info);

                return info;
            }
            else
            {
                return null;
            }
        }

        public ICollection<string> GetEventsCategories()
        {
            return EventConfigs.SelectMany(c => c).Select(i => i.Key).Distinct().ToArray();
        }

        public ICollection<string> GetEphemerisCategories(CelestialObject body)
        {
            var type = body.GetType();
            if (EphemConfigs.ContainsKey(type))
            {
                return EphemConfigs[type].GetCategories(body);
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
                GeoLocation = Context.GeoLocation,
                CancelToken = cancelToken
            };

            var events = new List<AstroEvent>();
            foreach (var item in EventConfigs.SelectMany(c => c).Where(i => categories.Contains(i.Key)))
            {
                try
                {
                    if (cancelToken?.IsCancellationRequested == true)
                    {
                        break;
                    }
                    else
                    {
                        events.AddRange(item.Formula.Invoke(context));
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return events
                .OrderBy(e => e.NoExactTime ? e.JulianDay + 1 : e.JulianDay)
                .Where(e => e.JulianDay >= context.From && e.JulianDay < context.To)
                .ToArray();
        }

        /// <inheritdoc />
        public ICollection<CelestialObject> Search(string searchString, Func<CelestialObject, bool> filter, int maxCount = 50)
        {
            var filterFunc = filter ?? ((b) => true);
            var results = new List<CelestialObject>();
            foreach (var searchProvider in SearchProviders)
            {
                if (results.Count < maxCount)
                {
                    results.AddRange(searchProvider(Context, searchString, filterFunc, maxCount));
                }
                else
                {
                    break;
                }
            }
            return results.OrderBy(b => string.Join(", ", b.Names)).Take(maxCount).ToList();
        }

        /// <inheritdoc />
        public CelestialObject Search(string objectType, string commonName = null)
        {
            return Search(commonName ?? "", x => x.Type == objectType && (!string.IsNullOrEmpty(commonName) ? x.CommonName == commonName : true), 1).FirstOrDefault();
        }

        public Constellation GetConstellation(string code)
        {
            code = code.ToUpper();
            if (Constellations.ContainsKey(code))
            {
                return Constellations[code];
            }
            else
            {
                return null;
            }
        }
    }
}
