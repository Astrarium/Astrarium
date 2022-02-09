using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner
{
    public class ObservationPlanner
    {
        private readonly ISky sky;

        public ObservationPlanner(ISky sky)
        {
            this.sky = sky;
        }

        public ICollection<Ephemerides> CreatePlan(PlanningFilter filter, CancellationToken? token = null, IProgress<double> progress = null)
        {
            double timeFrom = filter.TimeFrom / 24.0;
            double timeTo = filter.TimeTo / 24.0;
            double obsDuration = timeTo < timeFrom ? timeTo - timeFrom + 1 : timeTo - timeFrom;
            int countLimit = filter.CountLimit ?? 1000;

            // Planned observation range, expressed as circular sector (angles)
            // It represents a timeframe to be compared with each celestial body visibility conditions.
            AngleRange obsRange = new AngleRange(timeFrom * 360, obsDuration * 360);

            // Resulting list of ephemerides. Each item is a planned observation of a celestial body.
            List<Ephemerides> ephemerides = new List<Ephemerides>();

            // Context used for calculations. We use local midnight as a time reference to obtain 
            // general conditions of visibility of celestial bodies,
            // and after that apply desired timeframe to find intersections.
            var context = new SkyContext(filter.JulianDayMidnight, filter.ObserverLocation, true);

            context.MinBodyAltitudeForVisibilityCalculations = filter.MinBodyAltitude;
            context.MaxSunAltitudeForVisibilityCalculations = filter.MaxSunAltitude;

            var celesialObjects = sky.CelestialObjects.Where(b => filter.ObjectTypes.Contains(b.Type));

            // Total objects count
            double objectsCount = celesialObjects.Count();

            int counter = 0;

            foreach (CelestialObject body in celesialObjects)
            {
                if (token.HasValue && token.Value.IsCancellationRequested)
                {
                    ephemerides.Clear();
                    break;
                }

                if (ephemerides.Count >= countLimit)
                {
                    break;
                }

                progress?.Report(counter++ / objectsCount * 100);

                var e = GetObservationDetails(filter, context, body, force: false);
                if (e != null)
                {
                    ephemerides.Add(e);
                }
            }

            return ephemerides;
        }

        public Ephemerides GetObservationDetails(PlanningFilter filter, CelestialObject body)
        {
            var ctx = new SkyContext(filter.JulianDayMidnight, filter.ObserverLocation, preferFast: true);
            ctx.MinBodyAltitudeForVisibilityCalculations = filter.MinBodyAltitude;
            ctx.MaxSunAltitudeForVisibilityCalculations = filter.MaxSunAltitude;
            return GetObservationDetails(filter, ctx, body, force: true);
        }

        private Ephemerides GetObservationDetails(PlanningFilter filter, SkyContext context, CelestialObject body, bool force = false)
        {
            float? magLimit = filter.MagLimit;
            double timeFrom = filter.TimeFrom / 24.0;
            double timeTo = filter.TimeTo / 24.0;
            double obsDuration = timeTo < timeFrom ? timeTo - timeFrom + 1 : timeTo - timeFrom;
            double? durationLimit = filter.DurationLimit != null ? filter.DurationLimit / 60.0 : null;

            // Planned observation range, expressed as circular sector (angles)
            // It represents a timeframe to be compared with each celestial body visibility conditions.
            AngleRange obsRange = new AngleRange(timeFrom * 360, obsDuration * 360);

            // Ephemerides for particular celestial body
            Ephemerides bodyEphemerides = new Ephemerides(body);

            // Ephemerides available for the body
            var categories = sky.GetEphemerisCategories(body);

            var visibilityEphems = sky.GetEphemerides(body, context, new[] {
                "Visibility.Begin",
                "Visibility.End",
                "Visibility.Duration",
                "RTS.Rise",
                "RTS.Transit",
                "RTS.Set",
                "RTS.RiseAzimuth",
                "RTS.TransitAltitude",
                "RTS.SetAzimuth",
                "Magnitude"
            });

            // Body does not have magnitude 
            if (!categories.Contains("Magnitude"))
            {
                if (!force && filter.SkipUnknownMagnitude) return null;
                // Apply "skip unknown mag" filter
                bodyEphemerides.Add(new Ephemeris() { Key = "Magnitude", Value = null, Formatter = Formatters.Magnitude });
            }

            if (!force && filter.MagLimit != null)
            {
                float? mag = bodyEphemerides.GetValue<float?>("Magnitude");
                // Apply magnitude filter
                if (mag > magLimit) return null;
            }

            // If body has visibility ephemerides, it can be planned.
            if (categories.Contains("Visibility.Duration"))
            {
                double visDuration = visibilityEphems.GetValue<double>("Visibility.Duration");
                if (visDuration > 0)
                {
                    Date visBegin = visibilityEphems.GetValue<Date>("Visibility.Begin");

                    AngleRange visRange = new AngleRange(visBegin.Time * 360, visDuration / 24 * 360);

                    var ranges = visRange.Overlaps(obsRange);

                    if (ranges.Any())
                    {
                        double beginTime = ranges.First().Start / 360;

                        if (beginTime < timeFrom)
                        {
                            beginTime += 1;
                        }

                        double endTime = beginTime + ranges.First().Range / 360;

                        // Begin of body observation, limited by desired observation begin and end time,
                        // and expressed in Date
                        Date bodyObsBegin = new Date(context.JulianDayMidnight + beginTime, context.GeoLocation.UtcOffset);

                        // Real duration of body observation, limited by desired observation begin and end time,
                        // and expressed in hours (convert from angle range expressed in degrees):
                        double bodyObsDuration = ranges.First().Range / 360 * 24;

                        // Apply duration filter
                        if (!force && durationLimit > bodyObsDuration) return null;

                        // End of body observation, limited by desired observation begin and end time,
                        // and expressed in Date
                        Date bodyObsEnd = new Date(context.JulianDayMidnight + endTime, context.GeoLocation.UtcOffset);

                        bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Begin", Value = bodyObsBegin, Formatter = Formatters.Time });
                        bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Duration", Value = bodyObsDuration, Formatter = Formatters.VisibilityDuration });
                        bodyEphemerides.Add(new Ephemeris() { Key = "Observation.End", Value = bodyObsEnd, Formatter = Formatters.Time });

                        bodyEphemerides.AddRange(visibilityEphems);

                        // best time of observation (in the expected time range)
                        {
                            var transit = visibilityEphems.GetValue<Date>("RTS.Transit");
                            var transitAlt = visibilityEphems.GetValue<double>("RTS.TransitAltitude");

                            AngleRange bodyObsRange = new AngleRange(bodyObsBegin.Time * 360, bodyObsDuration / 24 * 360);
                            AngleRange tranRange = new AngleRange(transit.Time * 360, 1e-6);

                            if (bodyObsRange.Overlaps(tranRange).Any())
                            {
                                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Best", Value = transit, Formatter = Formatters.Time });
                                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.BestAltitude", Value = transitAlt, Formatter = Formatters.Altitude });
                                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.BestAzimuth", Value = 0.0, Formatter = Formatters.Azimuth });

                                var ctxBest = new SkyContext(transit.ToJulianEphemerisDay(), context.GeoLocation, preferFast: true);
                                var bestEphemerides = sky.GetEphemerides(body, ctxBest, new[] { "Equatorial.Alpha", "Equatorial.Delta", "Constellation" });
                                bodyEphemerides.AddRange(bestEphemerides);
                            }
                            else
                            {
                                var ctxBegin = new SkyContext(bodyObsBegin.ToJulianEphemerisDay(), context.GeoLocation, preferFast: true);
                                var beginEphemerides = sky.GetEphemerides(body, ctxBegin, new[] { "Horizontal.Altitude", "Horizontal.Azimuth", "Equatorial.Alpha", "Equatorial.Delta", "Constellation" });
                                double altBegin = beginEphemerides.GetValue<double>("Horizontal.Altitude");
                                double aziBegin = beginEphemerides.GetValue<double>("Horizontal.Azimuth");

                                var ctxEnd = new SkyContext(bodyObsEnd.ToJulianEphemerisDay(), context.GeoLocation, preferFast: true);
                                var endEphemerides = sky.GetEphemerides(body, ctxEnd, new[] { "Horizontal.Altitude", "Horizontal.Azimuth", "Equatorial.Alpha", "Equatorial.Delta", "Constellation" });
                                double altEnd = endEphemerides.GetValue<double>("Horizontal.Altitude");
                                double aziEnd = endEphemerides.GetValue<double>("Horizontal.Azimuth");

                                if (altBegin >= altEnd)
                                {
                                    bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Best", Value = bodyObsBegin, Formatter = Formatters.Time });
                                    bodyEphemerides.Add(new Ephemeris() { Key = "Observation.BestAltitude", Value = altBegin, Formatter = Formatters.Altitude });
                                    bodyEphemerides.Add(new Ephemeris() { Key = "Observation.BestAzimuth", Value = aziBegin, Formatter = Formatters.Azimuth });
                                    bodyEphemerides.AddRange(beginEphemerides);
                                }
                                else
                                {
                                    bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Best", Value = bodyObsEnd, Formatter = Formatters.Time });
                                    bodyEphemerides.Add(new Ephemeris() { Key = "Observation.BestAltitude", Value = altEnd, Formatter = Formatters.Altitude });
                                    bodyEphemerides.Add(new Ephemeris() { Key = "Observation.BestAzimuth", Value = aziEnd, Formatter = Formatters.Azimuth });
                                    bodyEphemerides.AddRange(endEphemerides);
                                }
                            }
                        }

                        return bodyEphemerides;
                    }
                }
            }

            // body observation duration does not match with desired time
            // or body does not observable at all,
            // or no info provided about observation
            if (force)
            {
                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Begin", Value = new Date(double.NaN), Formatter = Formatters.Time });
                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Duration", Value = double.NaN, Formatter = Formatters.VisibilityDuration });
                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.End", Value = new Date(double.NaN), Formatter = Formatters.Time });
                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Best", Value = new Date(double.NaN), Formatter = Formatters.Time });
                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.BestAltitude", Value = double.NaN, Formatter = Formatters.Altitude });
                bodyEphemerides.Add(new Ephemeris() { Key = "Observation.BestAzimuth", Value = double.NaN, Formatter = Formatters.Azimuth });
                bodyEphemerides.AddRange(visibilityEphems);
                var ctx = new SkyContext(filter.JulianDayMidnight, filter.ObserverLocation, preferFast: true);
                bodyEphemerides.AddRange(sky.GetEphemerides(body, ctx, new[] { "Horizontal.Altitude", "Horizontal.Azimuth", "Equatorial.Alpha", "Equatorial.Delta", "Constellation" }));

                // final check: add fictive/empty ephemerides as it needed by planner
                string[] existingEphems = bodyEphemerides.Select(e => e.Key).ToArray();
                if (!existingEphems.Contains("Constellation"))
                {
                    bodyEphemerides.Add(new Ephemeris() { Key = "Constellation", Value = null, Formatter = Formatters.Simple });
                }
                if (!existingEphems.Contains("RTS.RiseAzimuth"))
                {
                    bodyEphemerides.Add(new Ephemeris() { Key = "RTS.RiseAzimuth", Value = double.NaN, Formatter = Formatters.GetDefault("RTS.RiseAzimuth") });
                }
                if (!existingEphems.Contains("RTS.SetAzimuth"))
                {
                    bodyEphemerides.Add(new Ephemeris() { Key = "RTS.SetAzimuth", Value = double.NaN, Formatter = Formatters.GetDefault("RTS.SetAzimuth") });
                }
                if (!existingEphems.Contains("RTS.TransitAltitude"))
                {
                    bodyEphemerides.Add(new Ephemeris() { Key = "RTS.TransitAltitude", Value = double.NaN, Formatter = Formatters.GetDefault("RTS.TransitAltitude") });
                }

                return bodyEphemerides;
            }

            return null;
        }
    }
}
