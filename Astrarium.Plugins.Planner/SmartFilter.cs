using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner
{
    public abstract class SmartFilter<T> where T : class
    {
        private static Regex smartFilterRegex = new Regex(@"\s*(\s*\S+\s*[><=]\s*[\sA-Za-z0-9-\.:]+\s*)\s*", RegexOptions.Multiline);
        private static Regex queryParameterRegex = new Regex(@"^\s*(\S+)\s*([><=])\s*([\sA-Za-z0-9-\.:]+)\s*$");

        public Predicate<T> CreateFromString(string filter)
        {
            var matches = smartFilterRegex.Matches(filter);
            var groups = new List<string>();
            if (matches.Count == 1 || (matches.Count > 1 && smartFilterRegex.Replace(filter, "").Any(c => c == ',')))
            {
                foreach (Match match in matches)
                {
                    groups.Add(match.Groups[1].Value);
                }
            }

            if (groups.Any())
            {
                var predicates = new List<Expression<Predicate<T>>>();
                foreach (string group in groups)
                {
                    var match = queryParameterRegex.Match(group.Trim());
                    string property = match.Groups[1].Value.ToLower();
                    string @operator = match.Groups[2].Value;
                    string value = match.Groups[3].Value;
                    predicates.Add(BuildPredicate(property, @operator, value));
                }

                ParameterExpression x = Expression.Parameter(typeof(T), "x");
                Expression body = predicates.Select(exp => exp.Body)
                     .Select(exp => ParameterReplacer.Replace(x, exp))
                     .Aggregate((left, right) => Expression.AndAlso(left, right));

                return Expression.Lambda<Predicate<T>>(body, x).Compile();
            }

            throw new ArgumentException($"Unable to parse filter string: {filter}");
        }

        public abstract Expression<Predicate<T>> BuildPredicate(string property, string @operator, string value);

        class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _param;

            private ParameterReplacer(ParameterExpression param)
            {
                _param = param;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node.Type == _param.Type ? // if types match on both of ends
                  base.VisitParameter(_param) : // replace
                  node; // ignore
            }

            public static E Replace<E>(ParameterExpression param, E exp) where E : Expression
            {
                return (E)new ParameterReplacer(param).Visit(exp);
            }
        }
    }

    public class ObservationPlanSmartFilter : SmartFilter<Ephemerides>
    {
        public override Expression<Predicate<Ephemerides>> BuildPredicate(string property, string @operator, string value)
        {
            switch (property)
            {
                case "name":
                    if (@operator == "=")
                        return x => x.CelestialObject.Names.Any(n => n.StartsWith(value, StringComparison.OrdinalIgnoreCase));
                    else
                        throw new ArgumentException($"Unknown operator {@operator} for {property} property.");

                case "type":
                    if (@operator == "=")
                        return x => x.CelestialObject.Type.Equals(value, StringComparison.OrdinalIgnoreCase);
                    else
                        throw new ArgumentException($"Unknown operator {@operator} for {property} property.");

                case "con":
                case "const":
                case "constellation":
                    if (@operator == "=")
                        return x => x.GetValue<string>("Constellation").Equals(value, StringComparison.OrdinalIgnoreCase);
                    else
                        throw new ArgumentException($"Unknown operator {@operator} for {property} property.");

                case "mag":
                case "magnitude":
                    if (@operator == "=")
                        return x => x.GetValue<float?>("Magnitude") != null ? Math.Round(x.GetValue<float?>("Magnitude").Value, 2) == float.Parse(value, CultureInfo.InvariantCulture) : false;
                    else if (@operator == ">")
                        return x => x.GetValue<float?>("Magnitude") > float.Parse(value, CultureInfo.InvariantCulture);
                    else if (@operator == "<")
                        return x => x.GetValue<float?>("Magnitude") < float.Parse(value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {@operator} for {property} property.");

                case "begin":
                    return GetTimePredicate("Observation.Begin", property, @operator, value);

                case "best":
                    return GetTimePredicate("Observation.Best", property, @operator, value);

                case "end":
                    return GetTimePredicate("Observation.End", property, @operator, value);

                case "alt":
                    if (@operator == "=")
                        return x => x.GetValue<double?>("Observation.BestAltitude") != null ? Math.Round(x.GetValue<double?>("Observation.BestAltitude").Value) == double.Parse(value, CultureInfo.InvariantCulture) : false;
                    else if (@operator == ">")
                        return x => x.GetValue<double?>("Observation.BestAltitude") > double.Parse(value, CultureInfo.InvariantCulture);
                    else if (@operator == "<")
                        return x => x.GetValue<double?>("Observation.BestAltitude") < double.Parse(value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {@operator} for {property} property.");

                case "dur":
                case "duration":
                    if (@operator == "=")
                        return x => Math.Round(x.GetValue<double>("Observation.Duration"), 1) == double.Parse(value, CultureInfo.InvariantCulture);
                    else if (@operator == ">")
                        return x => x.GetValue<double?>("Observation.Duration") > double.Parse(value, CultureInfo.InvariantCulture);
                    else if (@operator == "<")
                        return x => x.GetValue<double?>("Observation.Duration") < double.Parse(value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {@operator} for {property} property.");

                case "rise":
                    return GetTimePredicate("RTS.Rise", property, @operator, value);

                case "transit":
                    return GetTimePredicate("RTS.Transit", property, @operator, value);

                case "set":
                    return GetTimePredicate("RTS.Set", property, @operator, value);

                case "ra":
                    if (@operator == "=")
                        return x => Math.Round(x.GetValue<double>("Equatorial.Alpha")) / 15 == double.Parse(value, CultureInfo.InvariantCulture);
                    else if (@operator == ">")
                        return x => x.GetValue<double>("Equatorial.Alpha") / 15 > double.Parse(value, CultureInfo.InvariantCulture);
                    else if (@operator == "<")
                        return x => x.GetValue<double>("Equatorial.Alpha") / 15 < double.Parse(value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {@operator} for {property} property.");

                case "dec":
                    if (@operator == "=")
                        return x => Math.Round(x.GetValue<double>("Equatorial.Delta")) == double.Parse(value, CultureInfo.InvariantCulture);
                    else if (@operator == ">")
                        return x => x.GetValue<double>("Equatorial.Delta") > double.Parse(value, CultureInfo.InvariantCulture);
                    else if (@operator == "<")
                        return x => x.GetValue<double>("Equatorial.Delta") < double.Parse(value, CultureInfo.InvariantCulture);
                    else
                        throw new ArgumentException($"Unknown operator {@operator} for {property} property.");

                default:
                    throw new ArgumentException($"Unknown property {property}.");
            }
        }

        private Expression<Predicate<Ephemerides>> GetTimePredicate(string ephemerisKey, string property, string @operator, string value)
        {
            if (@operator == "=")
                return x => !double.IsNaN(x.GetValue<Date>(ephemerisKey).Day) && TimeSpan.FromDays(x.GetValue<Date>(ephemerisKey).Time).ToString(@"hh\:mm") == TimeSpan.Parse(value).ToString(@"hh\:mm");
            if (@operator == ">")
                return x => !double.IsNaN(x.GetValue<Date>(ephemerisKey).Day) && TimeSpan.FromDays(x.GetValue<Date>(ephemerisKey).Time) > TimeSpan.Parse(value);
            if (@operator == "<")
                return x => !double.IsNaN(x.GetValue<Date>(ephemerisKey).Day) && TimeSpan.FromDays(x.GetValue<Date>(ephemerisKey).Time) < TimeSpan.Parse(value);
            else
                throw new ArgumentException($"Unknown operator {@operator} for {property} property.");
        }
    }
}
