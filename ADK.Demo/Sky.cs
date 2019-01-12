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

        private CalculationContext context = new CalculationContext();



        


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
            context.ClearCache();

            NutationElements = Nutation.NutationElements(JulianDay);
            AberrationElements = Aberration.AberrationElements(JulianDay);

            Epsilon = Date.TrueObliquity(JulianDay, NutationElements.deltaEpsilon);
            SiderealTime = Date.ApparentSiderealTime(JulianDay, NutationElements.deltaPsi, Epsilon);

            foreach (var calc in Calculators)
            {
                calc.Calculate(context);
            }
        }
    }

    public class CalculationContext
    {
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
    }
}
