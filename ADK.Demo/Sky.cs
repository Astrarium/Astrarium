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

            var itemsToBeCalled = config.Filter(keys);

            List<Delegate> calledDelegates = new List<Delegate>();

            for (double jd = from; jd < to; jd++)
            {
                var context = new SkyContext(jd, Context.GeoLocation);
                calledDelegates.Clear();

                provider.Calculate(context, copy);

                Dictionary<string, object> ephemeris = new Dictionary<string, object>();

                foreach (var item in itemsToBeCalled)
                {
                    foreach (var action in item.Actions)
                    {
                        if (!calledDelegates.Contains(action))
                        {
                            action.DynamicInvoke(context, copy);
                            calledDelegates.Add(action);
                        }
                    }

                    object value = item.Formula.DynamicInvoke(context, copy);

                    ephemeris.Add(item.Key, value);
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

        /// <summary>
        /// Extra data to store within the context
        /// </summary>
        public dynamic Data { get; } = new ExpandoObject();
    }
}
