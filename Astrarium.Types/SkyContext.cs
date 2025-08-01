using Astrarium.Algorithms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public class SkyContext : Memoizer<SkyContext>
    {
        public SkyContext(double jd, CrdsGeographical location) : this(jd, location, false) { }

        public event Action ContextChanged;
        public event Action LocationChanged;
        public event Action JulianDayChanged;

        public SkyContext(double jd, CrdsGeographical location, bool preferFast)
        {
            PreferFastCalculation = preferFast;
            GeoLocation = location;
            JulianDay = jd;
        }

        /// <summary>
        /// Copies the context, or preserves the exising one if julian day not changed.
        /// </summary>
        /// <param name="jd"></param>
        /// <returns></returns>
        public SkyContext Copy(double jd)
        {
            if (Math.Abs(jd - _JulianDay) < 1e-6)
                return this;
            else
                return new SkyContext(jd, GeoLocation, PreferFastCalculation)
                {
                    MinBodyAltitudeForVisibilityCalculations = MinBodyAltitudeForVisibilityCalculations,
                    MaxSunAltitudeForVisibilityCalculations = MaxSunAltitudeForVisibilityCalculations
                };
        }

        public void Set(double jd, CrdsGeographical geoLocation)
        {
            _JulianDay = jd;
            _GeoLocation = geoLocation;
            UpdateContextVariables();
            ClearCache();
            ContextChanged?.Invoke();
            LocationChanged?.Invoke();
            JulianDayChanged?.Invoke();
        }

        private void UpdateContextVariables()
        {
            NutationElements = Nutation.NutationElements(_JulianDay);
            AberrationElements = Aberration.AberrationElements(_JulianDay);
            Epsilon = Date.TrueObliquity(_JulianDay, NutationElements.deltaEpsilon);
            SiderealTime = Date.ApparentSiderealTime(_JulianDay, NutationElements.deltaPsi, Epsilon);
            PrecessionElements = Precession.ElementsFK5(Date.EPOCH_J2000, _JulianDay);
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
                UpdateContextVariables();
                ClearCache();
                ContextChanged?.Invoke();
                JulianDayChanged?.Invoke();
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
                ContextChanged?.Invoke();
                LocationChanged?.Invoke();
            }
        }

        /// <summary>
        /// Minimal Sun altitude taken into account for calculating visibility conditions, expressed in degrees.
        /// Default value is null (not set).
        /// </summary>
        public double? MaxSunAltitudeForVisibilityCalculations { get; set; }

        /// <summary>
        /// Minimal Sun altitude taken into account for calculating visibility conditions, expressed in degrees.
        /// Default value is null (not set).
        /// </summary>
        public double? MinBodyAltitudeForVisibilityCalculations { get; set; }

        /// <summary>
        /// Flag indicating fast calculation (low precision) is preferred
        /// </summary>
        public bool PreferFastCalculation { get; private set; }

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
        /// Precession elements for converting coordinates of J2000 epoch to current epoch.
        /// </summary>
        public PrecessionalElements PrecessionElements { get; private set; }

        /// <summary>
        /// True obliquity of the ecliptic, in degrees
        /// </summary>
        public double Epsilon { get; private set; }

        /// <summary>
        /// Value indicating degree of lightness, from 0 to 1 inclusive.
        /// 0 value means totally dark sky, 1 is a day, values between are different dusk degrees.
        /// </summary>
        public float DayLightFactor { get; set; }

        /// <summary>
        /// Gets date corresponding to specified Julian Day
        /// </summary>
        /// <param name="jd">Julian Day to convert to the date</param>
        /// <returns><see cref="Date"/> instance</returns>
        public Date GetDate(double jd)
        {
            return new Date(jd, GeoLocation.UtcOffset);
        }

        /// <summary>
        /// Gets date corresponding to specified time of a day, expressed by fractions of a day (0 means noon, 0.5 is afternoon). 
        /// </summary>
        /// <param name="jd">Julian Day to convert to the date</param>
        /// <returns><see cref="Date"/> instance</returns>
        public Date GetDateFromTime(double dayFraction)
        {
            return new Date(JulianDayMidnight + dayFraction, GeoLocation.UtcOffset);
        }
    }

    /// <summary>
    /// Base class that implements memoization logic <see href="https://en.wikipedia.org/wiki/Memoization"/>.
    /// </summary>
    /// <typeparam name="TClass">Class that inherits the memoizer.</typeparam>
    public abstract class Memoizer<TClass>
    {
        private ConcurrentDictionary<IntPtr, object> resultsCache = new ConcurrentDictionary<IntPtr, object>();
        private ConcurrentDictionary<IntPtr, object>[] argsCache = new ConcurrentDictionary<IntPtr, object>[6];

        protected Memoizer()
        {
            for (int i = 0; i < argsCache.Length; i++)
            {
                argsCache[i] = new ConcurrentDictionary<IntPtr, object>();
            }
        }

        /// <summary>
        /// Calls the method with memoization logic. 
        /// If the method has been called already with the same arguments values, 
        /// cached method result will be returned instead of calling the method again.
        /// </summary>
        /// <typeparam name="R">Resulting type of the method.</typeparam>
        /// <param name="formula">Method to be called.</param>
        public R Get<R>(Func<TClass, R> formula)
        {
            return InvokeWithCache<R>(formula, this);
        }

        /// <summary>
        /// Calls the method with memoization logic. 
        /// If the method has been called already with the same arguments values, 
        /// cached method result will be returned instead of calling the method again.
        /// </summary>
        /// <typeparam name="R">Resulting type of the method.</typeparam>
        /// <typeparam name="T1">Type of the first argument of the method.</typeparam>
        /// <param name="formula">Method to be called.</param>
        /// <param name="arg">first argument of the method.</param>
        public R Get<T1, R>(Func<TClass, T1, R> formula, T1 arg)
        {
            return InvokeWithCache<R>(formula, this, arg);
        }

        /// <summary>
        /// Calls the method with memoization logic. 
        /// If the method has been called already with the same arguments values, 
        /// cached method result will be returned instead of calling the method again.
        /// </summary>
        /// <typeparam name="R">Resulting type of the method.</typeparam>
        /// <typeparam name="T1">Type of the first argument of the method.</typeparam>
        /// <typeparam name="T2">Type of the second argument of the method.</typeparam>
        /// <param name="formula">Method to be called.</param>
        /// <param name="arg1">first argument of the method.</param>
        /// <param name="arg2">Second argument of the method.</param>
        public R Get<T1, T2, R>(Func<TClass, T1, T2, R> formula, T1 arg1, T2 arg2)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2);
        }

        public R Get<T1, T2, T3, R>(Func<TClass, T1, T2, T3, R> formula, T1 arg1, T2 arg2, T3 arg3)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2, arg3);
        }

        public R Get<T1, T2, T3, T4, R>(Func<TClass, T1, T2, T3, T4, R> formula, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2, arg3, arg4);
        }

        public R Get<T1, T2, T3, T4, T5, R>(Func<TClass, T1, T2, T3, T4, R> formula, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return InvokeWithCache<R>(formula, this, arg1, arg2, arg3, arg4, arg5);
        }

        public R Get<T1, T2, T3, T4, T5, T6, R>(Func<TClass, T1, T2, T3, T4, R> formula, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
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
                    if (!argsCache[i - 1].ContainsKey(key) || !argsCache[i - 1][key].Equals(args[i]))
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

        protected void ClearCache()
        {
            resultsCache.Clear();
            for (int i = 0; i < argsCache.Length; i++)
            {
                argsCache[i].Clear();
            }
        }
    }
}
