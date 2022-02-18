using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Astrarium.Types
{
    public class SmartFilterItem
    {
        public string Property { get; }
        public string Operator { get; }
        public string Value { get; }
    
        public SmartFilterItem(string property, string @operator, string value)
        {
            Property = property;
            Operator = @operator;
            Value = value;
        }
    }

    public interface ISmartFilter
    {
        ICollection<SmartFilterItem> ParseItems(string filter);
    }

    /// <summary>
    /// Base class for smart filters
    /// </summary>
    /// <typeparam name="T">Type of elements to be filtered</typeparam>
    public abstract class SmartFilter<T> : ISmartFilter where T : class
    {
        private static Regex smartFilterRegex = new Regex(@"\s*(\s*\S+\s*[><=]\s*[\sA-Za-z0-9-\.:]+\s*)\s*", RegexOptions.Multiline);
        private static Regex queryParameterRegex = new Regex(@"^\s*(\S+)\s*([><=])\s*([\sA-Za-z0-9-\.:]+)\s*$");

        /// <summary>
        /// Creates filter predicate from filter string
        /// </summary>
        /// <param name="filter">Filter string in following format: "param1 = value1, param2 < value2, param3 < value3" etc.</param>
        /// <returns>Predicate</returns>
        public Predicate<T> CreateFromString(string filter)
        {
            var items = ParseItems(filter);
            var predicates = new List<Expression<Predicate<T>>>();
            foreach (var item in items)
            {
                predicates.Add(BuildPredicate(item));
            }

            ParameterExpression x = Expression.Parameter(typeof(T), "x");
            Expression body = predicates.Select(exp => exp.Body)
                 .Select(exp => ParameterReplacer.Replace(x, exp))
                 .Aggregate((left, right) => Expression.AndAlso(left, right));

            return Expression.Lambda<Predicate<T>>(body, x).Compile();
        }

        public ICollection<SmartFilterItem> ParseItems(string filter)
        {
            var items = new List<SmartFilterItem>();
            try
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
                    foreach (string group in groups)
                    {
                        var match = queryParameterRegex.Match(group.Trim());
                        string property = match.Groups[1].Value.ToLower();
                        string @operator = match.Groups[2].Value;
                        string value = match.Groups[3].Value;
                        items.Add(new SmartFilterItem(property, @operator, value));
                    }
                }
            }
            catch { }
            return items;
        }

        /// <summary>
        /// Builds predicate from parsed part of expression
        /// </summary>
        /// <returns>Predicate expression</returns>
        protected abstract Expression<Predicate<T>> BuildPredicate(SmartFilterItem item);

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _param;

            private ParameterReplacer(ParameterExpression param)
            {
                _param = param;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node.Type == _param.Type ? base.VisitParameter(_param) : node;
            }

            public static E Replace<E>(ParameterExpression param, E exp) where E : Expression
            {
                return (E)new ParameterReplacer(param).Visit(exp);
            }
        }
    }
}
