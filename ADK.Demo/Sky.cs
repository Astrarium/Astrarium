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
        /// <summary>
        /// Julian ephemeris day
        /// </summary>
        public double JulianDay { get; set; }

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

        private Dictionary<string, object> formulae = new Dictionary<string, object>();
        private Dictionary<string, object> cacheSingle = new Dictionary<string, object>();
        private Dictionary<string, Dictionary<int, object>> cacheCollection = new Dictionary<string, Dictionary<int, object>>();

        public void AddFormula<TResult>(string key, Func<TResult> func)
        {
            formulae[key] = func;
        }

        public void AddFormula<TObject, TResult>(string formulaName, Func<TObject, TResult> formula)
        {
            string key = $"{typeof(TObject).Name}.{formulaName}";
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
                switch (formulae[formulaName])
                {
                    case Func<TResult> func:
                        TResult result = func.Invoke();
                        cacheSingle[formulaName] = result;
                        return result;

                    default:
                        throw new ArgumentException("Formula return value does not match with requested result type.");
                }
            }
            else
            {
                throw new ArgumentException("Formula with specified key wan not found");
            }
        }

        public TResult Formula<TObject, TResult>(string formulaName, TObject obj) where TObject : CelestialObject
        {
            string key = $"{typeof(TObject).Name}.{formulaName}";

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
                switch (formulae[key])
                {
                    case Func<TObject, TResult> func:
                        var result = func.Invoke(obj);
                        cacheCollection[key][obj.Id] = result;
                        return result;

                    default:
                        throw new ArgumentException("Formula return value does not match with requested result type.");                        
                }
            }
            else
            {
                throw new ArgumentException("Formula with specified key wan not found");
            }
        }

        public void ClearCache()
        {
            cacheSingle.Clear();
            cacheCollection.Clear();
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
            JulianDay = new Date(DateTime.Now).ToJulianEphemerisDay();
            GeoLocation = new CrdsGeographical(56.3333, -44);
        }

        public void Calculate()
        {
            ClearCache();

            NutationElements = Nutation.NutationElements(JulianDay);
            AberrationElements = Aberration.AberrationElements(JulianDay);

            Epsilon = Date.TrueObliquity(JulianDay, NutationElements.deltaEpsilon);
            SiderealTime = Date.ApparentSiderealTime(JulianDay, NutationElements.deltaPsi, Epsilon);

            foreach (var calc in Calculators)
            {
                calc.Calculate();
            }
        }
    }
}
