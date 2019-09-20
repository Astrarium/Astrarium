using System;
using System.Collections.Generic;
using System.Linq;

namespace Planetarium
{
    public static class Extensions
    {
        /// <summary>Splits an enumeration based on a predicate.</summary>
        /// <remarks>
        /// This method drops partitioning elements.
        /// </remarks>
        public static IEnumerable<List<TSource>> Split<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> partitionBy,
            bool removeEmptyEntries = false,
            int count = -1)
        {
            int yielded = 0;
            var items = new List<TSource>();
            foreach (var item in source)
            {
                if (!partitionBy(item))
                    items.Add(item);
                else if (!removeEmptyEntries || items.Count > 0)
                {
                    yield return items.ToList();
                    items.Clear();

                    if (count > 0 && ++yielded == count) yield break;
                }
            }

            if (items.Count > 0) yield return items.ToList();
        }

        public static T Prev<T>(this IList<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index > 0) return list[index - 1];
            else return default(T);
        }

        public static T Next<T>(this IList<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index < list.Count - 1) return list[index + 1];
            else return default(T);
        }
    }
}
