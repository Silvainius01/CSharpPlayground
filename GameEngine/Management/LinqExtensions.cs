using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public static class LinqExtensions
    {
        /// <returns>Returns the last object in <paramref name="array"/>. Throws an exception if <paramref name="array"/> is null or empty</returns>
        public static T Last<T>(this T[] array) => array[array.Length - 1];
        /// <returns>Returns the last object in <paramref name="list"/>. Throws an exception if <paramref name="list"/> is null or empty</returns>
        public static T Last<T>(this IList<T> list) => list[list.Count - 1];
        /// <returns>Returns the last index for <paramref name="array"/></returns>
        public static int LastIndex<T>(this T[] array) => array.Length - 1;
        /// <returns>Returns the last index for <paramref name="list"/></returns>
        public static int LastIndex<T>(this IList<T> list) => list.Count - 1;
        /// <summary>Sets the last index for <paramref name="array"/> to <paramref name="value"/></summary>
        public static void SetLast<T>(this T[] array, T value)
        {
            array[array.Length - 1] = value;
        }
        /// <summary>Sets the last index for <paramref name="list"/> to <paramref name="value"/></summary>
        public static void SetLast<T>(this IList<T> list, T value)
        {
            list[list.Count - 1] = value;
        }

        public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> kvp)
        {
            dict.Add(kvp.Key, kvp.Value);
        }

        public static T RandomItem<T>(this T[] array) => array[Mathc.Random.NextInt(array.Length)];
        public static T RandomItem<T>(this IList<T> list) => list[Mathc.Random.NextInt(list.Count)];
        public static T RandomItem<T>(this IEnumerable<T> enumerable) => enumerable.ElementAt(Mathc.Random.NextInt(enumerable.Count()));
        public static T RandomItem<T>(this IEnumerable<T> enumerabe, Func<T, bool> predicate) => enumerabe.Where(predicate).RandomItem();

        /// <summary>Create a string with a <paramref name="seperator"/> between the items./// </summary>
        /// <returns>A string with <paramref name="seperator"/> between the items present in <paramref name="enumerable"/></returns>
        public static string ToString<T>(this IEnumerable<T> enumerable, string seperator)
        {
            int count = enumerable.Count();
            string first = enumerable.FirstOrDefault().ToString();
            // Since we cant know the total length we need, just assume that first is the average, and use it to allocate capacity.
            StringBuilder builder = new StringBuilder((count * seperator.Length) + (first.Length * count));

            builder.Append(first);
            foreach(var item in enumerable.Skip(1))
            {
                builder.Append($"{seperator}{item}");
            }
            return builder.ToString();
        }
        /// <summary>Create a string with a <paramref name="seperator"/> between the items./// </summary>
        /// <param name="toString">Function used to convert items to strings.</param>
        /// <returns>A string with <paramref name="seperator"/> between the items present in <paramref name="enumerable"/></returns>
        public static string ToString<T>(this IEnumerable<T> enumerable, Func<T, string> toString, string seperator)
        {
            int count = enumerable.Count();
            string first = toString(enumerable.FirstOrDefault());
            // Since we cant know the total length we need, just assume that first is the average, and use it to allocate capacity.
            StringBuilder builder = new StringBuilder((count * seperator.Length) + (first.Length * count));

            builder.Append(first);
            foreach (var item in enumerable.Skip(1))
            {
                builder.Append($"{seperator}{toString(item)}");
            }
            return builder.ToString();
        }

        public static void EnqueueAll<T>(this Queue<T> q, IEnumerable<T> enumerable)
        {
            foreach (var v in enumerable)
                q.Enqueue(v);
        }
    }
}