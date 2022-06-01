using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Astrarium.Plugins.Planner
{
    public class ObservationPlanSmartFilter : SmartFilter<Ephemerides>
    {
        protected override Expression<Predicate<Ephemerides>> BuildPredicate(SmartFilterItem item)
        {
            switch (item.Property)
            {
                case "name":
                    if (item.Operator == "=")
                        return x => x.CelestialObject.Names.Any(n => n.StartsWith(item.Value, StringComparison.OrdinalIgnoreCase));
                    else
                        throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");

                case "type":
                    if (item.Operator == "=")
                        return x => x.CelestialObject.Type.Equals(item.Value, StringComparison.OrdinalIgnoreCase);
                    else
                        throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");

                case "con":
                case "const":
                case "constellation":
                    if (item.Operator == "=")
                        return x => x.GetValue<string>("Constellation").Equals(item.Value, StringComparison.OrdinalIgnoreCase);
                    else
                        throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");

                case "mag":
                case "magnitude":
                    if (item.Operator == "=")
                        return x => x.GetValue<float?>("Magnitude") != null ? Math.Round(x.GetValue<float?>("Magnitude").Value, 2) == float.Parse(item.Value, CultureInfo.InvariantCulture) : false;
                    else if (item.Operator == ">")
                        return x => x.GetValue<float?>("Magnitude") > float.Parse(item.Value, CultureInfo.InvariantCulture);
                    else if (item.Operator == "<")
                        return x => x.GetValue<float?>("Magnitude") < float.Parse(item.Value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");

                case "begin":
                    return GetTimePredicate("Observation.Begin", item);

                case "best":
                    return GetTimePredicate("Observation.Best", item);

                case "end":
                    return GetTimePredicate("Observation.End", item);

                case "alt":
                    if (item.Operator == "=")
                        return x => x.GetValue<double?>("Observation.BestAltitude") != null ? Math.Round(x.GetValue<double?>("Observation.BestAltitude").Value) == double.Parse(item.Value, CultureInfo.InvariantCulture) : false;
                    else if (item.Operator == ">")
                        return x => x.GetValue<double?>("Observation.BestAltitude") > double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else if (item.Operator == "<")
                        return x => x.GetValue<double?>("Observation.BestAltitude") < double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");

                case "dur":
                case "duration":
                    if (item.Operator == "=")
                        return x => Math.Round(x.GetValue<double>("Observation.Duration"), 1) == double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else if (item.Operator == ">")
                        return x => x.GetValue<double?>("Observation.Duration") > double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else if (item.Operator == "<")
                        return x => x.GetValue<double?>("Observation.Duration") < double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");

                case "rise":
                    return GetTimePredicate("RTS.Rise", item);

                case "transit":
                    return GetTimePredicate("RTS.Transit", item);

                case "set":
                    return GetTimePredicate("RTS.Set", item);

                case "ra":
                    if (item.Operator == "=")
                        return x => Math.Round(x.GetValue<double>("Equatorial.Alpha")) / 15 == double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else if (item.Operator == ">")
                        return x => x.GetValue<double>("Equatorial.Alpha") / 15 > double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else if (item.Operator == "<")
                        return x => x.GetValue<double>("Equatorial.Alpha") / 15 < double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");

                case "dec":
                    if (item.Operator == "=")
                        return x => Math.Round(x.GetValue<double>("Equatorial.Delta")) == double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else if (item.Operator == ">")
                        return x => x.GetValue<double>("Equatorial.Delta") > double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else if (item.Operator == "<")
                        return x => x.GetValue<double>("Equatorial.Delta") < double.Parse(item.Value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");

                default:
                    throw new ArgumentException($"Unknown property {item.Property}.");
            }
        }

        private Expression<Predicate<Ephemerides>> GetTimePredicate(string ephemerisKey, SmartFilterItem item)
        {
            if (item.Operator == "=")
                return x => !double.IsNaN(x.GetValue<Date>(ephemerisKey).Day) && TimeSpan.FromDays(x.GetValue<Date>(ephemerisKey).Time).ToString(@"hh\:mm") == TimeSpan.Parse(item.Value).ToString(@"hh\:mm");
            if (item.Operator == ">")
                return x => !double.IsNaN(x.GetValue<Date>(ephemerisKey).Day) && TimeSpan.FromDays(x.GetValue<Date>(ephemerisKey).Time) > TimeSpan.Parse(item.Value);
            if (item.Operator == "<")
                return x => !double.IsNaN(x.GetValue<Date>(ephemerisKey).Day) && TimeSpan.FromDays(x.GetValue<Date>(ephemerisKey).Time) < TimeSpan.Parse(item.Value);
            else
                throw new ArgumentException($"Unknown operator {item.Operator} for {item.Property} property.");
        }
    }
}
