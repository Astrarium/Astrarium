using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
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

        private DynamicFormulae Formulae { get; set; }



        /// <summary>
        /// Extra data to store within the context
        /// </summary>
        public dynamic Data { get; } = new ExpandoObject();

        public R Formula<T, R>(FormulaDelegate<T, R> formula, T obj)
        {
            return formula.Invoke(this, obj);
        }
    }

    public delegate R FormulaDelegate<T, R>(SkyContext context, T obj);

    public class FormulaDefinitions<T, R>
    {
        public FormulaDelegate<T, R> this[string formulaName]
        {
            set
            {

            }
        }
    }

    public class DynamicFormulae : DynamicObject
    {
        private SkyContext context;

        

        public void SetContext(SkyContext context)
        {
            this.context = context;
        }

        private Dictionary<string, Delegate> formulae = new Dictionary<string, Delegate>();
        private Dictionary<string, object> cachedResults = new Dictionary<string, object>();

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value.GetType() == typeof(Func<,,>) &&
                value.GetType().GetGenericArguments()[0] == typeof(SkyContext) &&
                typeof(CelestialObject).IsAssignableFrom(value.GetType().GetGenericArguments()[1]) &&
                value.GetType().GetGenericArguments()[2] == typeof(object))
            {
                formulae.Add(binder.Name, value as Delegate);
            }
            else
            {
                throw new Exception("Wrong assignment.");
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (formulae.ContainsKey(binder.Name))
            {
                if (cachedResults.ContainsKey(binder.Name))
                {
                    result = cachedResults[binder.Name];
                    return true;
                }
                else
                {

                    result = formulae[binder.Name].DynamicInvoke(context, null);
                    return true;
                }
            }
            else
            {
                throw new Exception("Formula not set.");
            }
        }
    }
}
