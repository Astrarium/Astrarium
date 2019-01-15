using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
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

        public void Initialize()
        {
            foreach (var calc in Calculators)
            {
                calc.Initialize();
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

            var copy = obj.CreateCopy();
            EphemerisConfig config = new EphemerisConfig<Planet>();
            Type providerType = typeof(IEphemProvider<>).MakeGenericType(obj.GetType());

            var provider = Calculators.OfType<IEphemProvider>().FirstOrDefault(c => typeof(IEphemProvider).IsAssignableFrom(c.GetType()));

            providerType.GetMethod("ConfigureEphemeris").Invoke(provider, new object[] { config });

            List<EphemerisConfigItem> itemsToBeCalled = config.Items.Where(i =>
                keys.Contains(i.Key) ||
                keys.Intersect(i.Formulae.Select(f => f.Key)).Count() > 0).ToList();

            for (double jd = from; jd < to; jd++)
            {
                var context = new SkyContext(jd, Context.GeoLocation);
                provider.Calculate(context, copy);

                Dictionary<string, object> ephemeris = new Dictionary<string, object>();

                foreach (var item in itemsToBeCalled)
                {
                    object value = item.Formula.DynamicInvoke(context, copy);

                    if (keys.Contains(item.Key))
                    {
                        if (item.Formulae.Count == 0)
                        {
                            ephemeris.Add(item.Key, value);
                        }
                        else
                        {
                            foreach (string key in item.Formulae.Keys)
                            {
                                object subvalue = item.Formulae[key].DynamicInvoke(value);
                                ephemeris.Add(key, subvalue);
                            }
                        }
                    }

                    foreach (string key in item.Formulae.Keys)
                    {
                        if (keys.Contains(key))
                        {
                            object subvalue = item.Formulae[key].DynamicInvoke(value);
                            ephemeris.Add(key, subvalue);
                        }
                    }
                }

                result.Add(ephemeris);
            }

            return result;
        }
    }

    public class SkyContext
    {    
        public SkyContext(double jd, CrdsGeographical location)
        {
            GeoLocation = location;
            JulianDay = jd;
        }

        private double _JulianDay;

        /// <summary>
        /// Julian ephemeris day
        /// </summary>
        public double JulianDay
        {
            get { return _JulianDay; }
            set
            {
                _JulianDay = value;
                NutationElements = Nutation.NutationElements(_JulianDay);
                AberrationElements = Aberration.AberrationElements(_JulianDay);
                Epsilon = Date.TrueObliquity(_JulianDay, NutationElements.deltaEpsilon);
                SiderealTime = Date.ApparentSiderealTime(_JulianDay, NutationElements.deltaPsi, Epsilon);
            }
        }

        /// <summary>
        /// Geographical coordinates of the observer
        /// </summary>
        public CrdsGeographical GeoLocation { get; set; }

        /// <summary>
        /// Apparent sidereal time at Greenwich (theta0), in degrees
        /// </summary>
        public double SiderealTime { get; private set; }

        /// <summary>
        /// Elements to calculate nutation effect
        /// </summary>
        public NutationElements NutationElements { get; private set; }

        /// <summary>
        /// Elements to calculate aberration effect
        /// </summary>
        public AberrationElements AberrationElements { get; private set; }

        /// <summary>
        /// True obliquity of the ecliptic, in degrees
        /// </summary>
        public double Epsilon { get; private set; }


        /*
        private static Dictionary<string, Delegate> formulae = new Dictionary<string, Delegate>();
        private Dictionary<string, object> cacheSingle = new Dictionary<string, object>();
        private Dictionary<string, Dictionary<int, object>> cacheCollection = new Dictionary<string, Dictionary<int, object>>();

        public static void AddFormula<TResult>(string key, Func<CalculationContext, TResult> func)
        {
            formulae[key] = func;
        }

        public static void AddFormula<TObject, TResult>(string key, Func<CalculationContext, TObject, TResult> formula) where TObject : CelestialObject
        {
            formulae[key] = formula;
        }

        public TResult Formula<TResult>(string formulaName)
        {
            if (cacheSingle.ContainsKey(formulaName))
            {
                switch (cacheSingle[formulaName])
                {
                    case TResult result:
                        return result;
                    default:
                        throw new ArgumentException("Cached value has different type.");
                }
            }

            if (formulae.ContainsKey(formulaName))
            {
                return (TResult)formulae[formulaName].DynamicInvoke(this);               
            }
            else
            {
                throw new ArgumentException("Formula with specified key wan not found");
            }
            
        }

        public TResult Formula<TResult>(string key, CelestialObject obj)
        {
            if (cacheCollection.ContainsKey(key))
            {
                var dict = cacheCollection[key];

                if (dict.ContainsKey(obj.Id))
                {
                    switch (dict[obj.Id])
                    {
                        case TResult result:
                            return result;

                        default:
                            throw new ArgumentException("Cached value has different type.");
                    }
                }
            }
            else
            {
                cacheCollection[key] = new Dictionary<int, object>();
            }

            if (formulae.ContainsKey(key))
            {
                return (TResult)formulae[key].DynamicInvoke(this, obj);
            }
            else
            {
                throw new ArgumentException($"Formula with name \"{key}\" was not found");
            }
        }

        public void ClearCache()
        {
            cacheSingle.Clear();
            cacheCollection.Clear();
        }
        */
    }
}
