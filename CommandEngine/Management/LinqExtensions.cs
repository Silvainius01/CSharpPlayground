using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
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

        public static T RandomItem<T>(this T[] array) => array[CommandEngine.Random.NextInt(array.Length)];
        public static T RandomItem<T>(this IList<T> list) => list[CommandEngine.Random.NextInt(list.Count)];
        public static T RandomItem<T>(this IEnumerable<T> enumerable) => enumerable.ElementAt(CommandEngine.Random.NextInt(enumerable.Count()));
        public static T RandomItem<T>(this IEnumerable<T> enumerabe, Func<T, bool> predicate) => enumerabe.Where(predicate).RandomItem();

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys, TValue value = default(TValue))
        {
            foreach (var key in keys)
                dict.Add(key, value);
        }
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            int length = keys.Count();

            if (length != values.Count())
                throw new ArgumentException("There must be equal amounts of keys and values.");

            foreach ((TKey First, TValue Second) pair in keys.Zip(values))
            {
                dict.Add(pair.First, pair.Second);
            }
        }

        /// <summary>
        /// Adds a key value pair, or updates it to the passed value.
        /// </summary>
        /// <param name="key">The key associated with the value being added or updated.</param>
        /// <param name="value">Teh value that will be added or replacing the existing one</param>
        /// <returns></returns>
        public static bool TryAddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                return dict.TryUpdate(key, value, dict[key]);
            return dict.TryAdd(key, value);
        }

        public static bool Contains<T>(this T[] array, Func<T, bool> predicate)
        {
            for (int i = 0; i < array.Length; i++)
                if (predicate.Invoke(array[i]))
                    return true;
            return false;
        }
        public static bool Contains<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; i++)
                if (predicate.Invoke(list[i]))
                    return true;
            return false;
        }
        public static bool Contains<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            foreach (var item in enumerable)
                if (predicate.Invoke(item))
                    return true;
            return false;
        }

        public static bool TryFirst<T>(this T[] array, Func<T, bool> predicate, out T firstItem)
        {
            for (int i = 0; i < array.Length; i++)
                if (predicate.Invoke(array[i]))
                {
                    firstItem = array[i];
                    return true;
                }
            firstItem = default(T);
            return false;
        }
        public static bool TryFirst<T>(this IList<T> list, Func<T, bool> predicate, out T firstItem)
        {
            for (int i = 0; i < list.Count; i++)
                if (predicate.Invoke(list[i]))
                {
                    firstItem = list[i];
                    return true;
                }
            firstItem = default(T);
            return false;
        }
        public static bool TryFirst<T>(this IEnumerable<T> enumberable, Func<T, bool> predicate, out T firstItem)
        {
            foreach(var item in enumberable)
                if(predicate.Invoke(item))
                {
                    firstItem = item;
                    return true;
                }
            firstItem = default(T);
            return false;
        }

        /// <summary>Create a string with a <paramref name="seperator"/> between the items./// </summary>
        /// <returns>A string with <paramref name="seperator"/> between the items present in <paramref name="enumerable"/></returns>
        public static string ToString<T>(this IEnumerable<T> enumerable, string seperator)
        {
            int count = enumerable.Count();
            string first = enumerable.FirstOrDefault().ToString();
            // Since we cant know the total length we need, just assume that first is the average, and use it to allocate capacity.
            StringBuilder builder = new StringBuilder((count * seperator.Length) + (first.Length * count));

            builder.Append(first);
            foreach (var item in enumerable.Skip(1))
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