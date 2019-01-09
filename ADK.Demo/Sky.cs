using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
        private Dictionary<string, object> values = new Dictionary<string, object>();

        public void AddFormula<T>(string key, Func<T> formula)
        {
            formulae[key] = formula;
            values[key] = null;
        }

        public void AddFormula<T>(string key, Func<int, T> formula)
        {
            formulae[key] = formula;
            values[key] = new Dictionary<int, object>();
        }

        public T Formula<T>(string key)
        {
            object cached = values[key];

            if (cached != null)
            {
                return (T)cached;
            }

            if (formulae.ContainsKey(key))
            {
                var func = formulae[key] as Func<T>;
                if (func != null)
                {
                    var result = func.Invoke();
                    values[key] = result;
                    return result;
                }
                else
                {
                    throw new ArgumentException("Formula return value does not match with requested result type.");
                }
            }
            else
            {
                throw new ArgumentException("Formula with specified key wan not found");
            }
        }

        public T Formula<T>(string key, int index)
        {            
            var dict = values[key] as Dictionary<int, object>;
           
            if (dict.ContainsKey(index))
            {
                return (T)dict[index];
            }            
            else if (formulae.ContainsKey(key))
            {
                var func = formulae[key] as Func<int, T>;
                if (func != null)
                {
                    var result = func.Invoke(index);                    
                    dict[index] = result;
                    return result;
                }
                else
                {
                    throw new ArgumentException("Formula return value does not match with requested result type.");
                }
            }
            else
            {
                throw new ArgumentException($"Formula with key `{key}` is not defined.");
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
            JulianDay = new Date(DateTime.Now).ToJulianEphemerisDay();
            GeoLocation = new CrdsGeographical(56.3333, -44);
        }

        public void Calculate()
        {
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
