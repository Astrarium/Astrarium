using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class SkyContext
    {
        private Dictionary<IntPtr, object> resultsCache = new Dictionary<IntPtr, object>();
        private Dictionary<IntPtr, object>[] argsCache = new Dictionary<IntPtr, object>[6];

        public SkyContext(double jd, CrdsGeographical location)
        {
            for (int i = 0; i < argsCache.Length; i++)
            {
                argsCache[i] = new Dictionary<IntPtr, object>();
            }

            GeoLocation = location;
            JulianDay = jd;
        }

        private double _JulianDay;

        /// <summary>
        /// Julian ephemeris day for the context time instant
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
                ClearCache();
            }
        }

        public double JulianDayMidnight
        {
            get
            {
                Date date = new Date(_JulianDay, _GeoLocation.UtcOffset);
                return _JulianDay - (date.Day - Math.Truncate(date.Day));
            }
        }

        private CrdsGeographical _GeoLocation;

        /// <summary>
        /// Geographical coordinates of the observer
        /// </summary>
        public CrdsGeographical GeoLocation
        {
            get
            {
                return _GeoLocation;
            }
            set
            {
                _GeoLocation = value;
                ClearCache();
            }
        }

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

        public R Get<R>(Func<SkyContext, R> formula)
        {
            return InvokeWithCache<R>(formula, this);
        }

        public R Get<T1, R>(Func<SkyContext, T1, R> formula, T1 arg)
        {
            return InvokeWithCache<R>(formula, this, arg);
        }

        public R Get<T1, T2, R>(Func<SkyContext, T1, T2, R> formula, T1 arg1, T2 arg2)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2);
        }

        public R Get<T1, T2, T3, R>(Func<SkyContext, T1, T2, T3, R> formula, T1 arg1, T2 arg2, T3 arg3)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2, arg3);
        }

        public R Get<T1, T2, T3, T4, R>(Func<SkyContext, T1, T2, T3, T4, R> formula, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2, arg3, arg4);
        }

        public R Get<T1, T2, T3, T4, T5, R>(Func<SkyContext, T1, T2, T3, T4, R> formula, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2, arg3, arg4, arg5);
        }

        public R Get<T1, T2, T3, T4, T5, T6, R>(Func<SkyContext, T1, T2, T3, T4, R> formula, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        private R InvokeWithCache<R>(Delegate formula, params object[] args)
        {
            IntPtr key = formula.Method.MethodHandle.Value;

            bool needInvoke = false;
            if (resultsCache.ContainsKey(key))
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (!argsCache[i - 1][key].Equals(args[i]))
                    {
                        needInvoke = true;
                        break;
                    }
                }
            }
            else
            {
                needInvoke = true;
            }

            if (needInvoke)
            {
                R result = (R)formula.DynamicInvoke(args);
                resultsCache[key] = result;
                for (int i = 1; i < args.Length; i++)
                {
                    argsCache[i - 1][key] = args[i];
                }
                return result;
            }
            else
            {
                return (R)resultsCache[key];
            }
        }

        private void ClearCache()
        {
            resultsCache.Clear();
            for (int i = 0; i < argsCache.Length; i++)
            {
                argsCache[i].Clear();
            }
        }
    }
}
