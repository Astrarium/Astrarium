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
            float? magLimit = filter.MagLimit;
            double timeFrom = filter.TimeFrom / 24.0;
            double obsDuration = filter.TimeTo / 24.0;

            // Planned observation range, expressed as circular sector (angles)
            // It represents a timeframe to be compared with each celestial body visibility conditions.
            AngleRange obsRange = new AngleRange(timeFrom * 360, obsDuration * 360);

            // Resulting list of ephemerides. Each item is a planned observation of a celestial body.
            List<Ephemerides> ephemerides = new List<Ephemerides>();

            // Context used for calculations. We use local midnight as a time reference to obtain 
            // general conditions of visibility of celestial bodies,
            // and after that apply desired timeframe to find intersections.
            var context = new SkyContext(filter.JulianDayMidnight, filter.ObserverLocation, true);

            context.MinimalBodyAltitudeForVisibilityCalculations = filter.MinBodyAltitude ?? 0;
            context.MinimalSunAltitudeForVisibilityCalculations = filter.MinSunAltitude ?? 0;

            // Total objects count
            double objectsCount = sky.CelestialObjects.Count();

            int counter = 0;

            foreach (CelestialObject body in sky.CelestialObjects)
            {
                if (token.HasValue && token.Value.IsCancellationRequested)
                {
                    ephemerides.Clear();
                    break;
                }

                progress?.Report(counter++ / objectsCount * 100);

                // TODO: filter by celestial object type!

                // Ephemerides for particular celestial body
                Ephemerides bodyEphemerides = new Ephemerides(body);

                // Ephemerides available for the body
                var categories = sky.GetEphemerisCategories(body);

                // If body has visibility ephemerides, it can be planned.
                if (categories.Contains("Visibility.Duration"))
                {
                    // Apply magnitude filter
                    if (magLimit != null && categories.Contains("Magnitude"))
                    {
                        var magEphem = sky.GetEphemerides(body, context, new[] { "Magnitude" }).ElementAt(0);
                        float? mag = magEphem.GetValue<float?>();

                        // filter by magnitude, if required
                        if (mag > magLimit) continue;

                        bodyEphemerides.Add(magEphem);
                    }

                    // magnitude filted is not required
                    if (magLimit == null && categories.Contains("Magnitude"))
                    {
                        var magEphem = sky.GetEphemerides(body, context, new[] { "Magnitude" }).ElementAt(0);
                        bodyEphemerides.Add(magEphem);
                    }

                    // Body does not have magnitude, but add it anyway (empty) as it required by planner
                    if (!categories.Contains("Magnitude"))
                    {
                        bodyEphemerides.Add(new Ephemeris() { Key = "Magnitude", Value = null, Formatter = Formatters.Magnitude });
                    }

                    var visibilityEphems = sky.GetEphemerides(body, context, new[] {
                        "Visibility.Begin",
                        "Visibility.End",
                        "Visibility.Duration",
                        "RTS.Rise",
                        "RTS.Transit",
                        "RTS.Set",
                        "RTS.RiseAzimuth",
                        "RTS.TransitAltitude",
                        "RTS.SetAzimuth"
                    });

                    double visDuration = visibilityEphems.GetValue<double>("Visibility.Duration");
                    if (visDuration > 0)
                    {
                        Date visBegin = visibilityEphems.GetValue<Date>("Visibility.Begin");

                        AngleRange visRange = new AngleRange(visBegin.Time * 360, visDuration / 24 * 360);

                        var ranges = visRange.Overlaps(obsRange);

                        if (ranges.Any())
                        {
                            // Begin of body observation, limited by desired observation begin and end time,
                            // and expressed in Date
                            Date bodyObsBegin = new Date(context.JulianDayMidnight + ranges.First().Start / 360, context.GeoLocation.UtcOffset);

                            // Real duration of body observation, limited by desired observation begin and end time,
                            // and expressed in hours (convert from angle range expressed in degrees):
                            double bodyObsDuration = ranges.First().Range / 360 * 24;

                            // End of body observation, limited by desired observation begin and end time,
                            // and expressed in Date
                            Date bodyObsEnd = new Date(context.JulianDayMidnight + ranges.First().Start / 360 + ranges.First().Range / 360, context.GeoLocation.UtcOffset);

                            bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Begin", Value = bodyObsBegin, Formatter = Formatters.Time });
                            bodyEphemerides.Add(new Ephemeris() { Key = "Observation.Duration", Value = bodyObsDuration, Formatter = Formatters.VisibilityDuration });
                            bodyEphemerides.Add(new Ephemeris() { Key = "Observation.End", Value = bodyObsEnd, Formatter = Formatters.Time });

                            bodyEphemerides.AddRange(visibilityEphems);
                            ephemerides.Add(bodyEphemerides);
                        }
                    }
                }
            }

            return ephemerides;
        }
    }
}
