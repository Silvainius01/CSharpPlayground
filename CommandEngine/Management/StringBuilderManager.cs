using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace CommandEngine
{
    public static class StringBuilderManager
    {
        public const string DefaultTabString = "\t";
        public const string DefaultNewlineString = "\n";
        public const ConsoleColor DefaultColor = ConsoleColor.Gray;

        static Queue<SmartStringBuilder> UnlockedBuilders = new Queue<SmartStringBuilder>();
        static Dictionary<string, SmartStringBuilder> LockedBuilders = new Dictionary<string, SmartStringBuilder>();

        static Queue<ColorStringBuilder> UnlockedColorBuilders = new Queue<ColorStringBuilder>();
        static Dictionary<string, ColorStringBuilder> LockedColorBuilders = new Dictionary<string, ColorStringBuilder>();

        static T GetBuilder<T>(string key, Queue<T> Unlocked, Dictionary<string, T> Locked, Func<T> GetNew) where T : class
        {
            if (Locked.ContainsKey(key))
                throw new ArgumentException($"Builder with key '{key}' is already in use.");

            T builder = Unlocked.Any()
                ? Unlocked.Dequeue()
                : GetNew();

            Locked.Add(key, builder);
            return builder;
        }
        public static void ReturnBuilder<T>(string key, Queue<T> Unlocked, Dictionary<string, T> Locked, Action<T> Reset) where T : class
        {
            if (!Locked.ContainsKey(key))
                throw new InvalidOperationException($"Builder with key '{key}' does is not in use.");

            T builder = Locked[key];
            Reset(builder);

            // Unlock, and add to pool.
            Locked.Remove(key);
            Unlocked.Enqueue(builder);
        }

        public static ColorStringBuilder GetColorBuilder(string key)
        {
            return GetBuilder(key, UnlockedColorBuilders, LockedColorBuilders, () => new ColorStringBuilder());
        }
        public static void ReturnColorBuilder(string key)
        {
            ReturnBuilder(key, UnlockedColorBuilders, LockedColorBuilders, (builder) =>
            {
                builder.Clear();
                builder.TabString = DefaultTabString;
                builder.NewlineString = DefaultNewlineString;
                builder.SetColor(DefaultColor);
            });
        }

        public static bool ContainsKey(string key) => LockedBuilders.ContainsKey(key);
    }

    public class ManagedStringBuilder : IDisposable
    {
        private bool _disposed = false;
        private string key = string.Empty;

        public ColorStringBuilder Builder { get; private set; }

        public ManagedStringBuilder(string key)
        {
            CheckKey();
            Builder = StringBuilderManager.GetColorBuilder(key);
        }
        public ManagedStringBuilder(string key, string tabString)
        {
            CheckKey();
            Builder = StringBuilderManager.GetColorBuilder(key);
            Builder.TabString = tabString;
        }
        public ManagedStringBuilder(string key, string startString, ConsoleColor startColor)
        {
            CheckKey();
            Builder = StringBuilderManager.GetColorBuilder(key);
            Builder.Append(startString);
            Builder.SetColor(startColor);
        }
        public ManagedStringBuilder(string key, string tabString, string startString, ConsoleColor startColor)
        {
            CheckKey();
            Builder = StringBuilderManager.GetColorBuilder(key);
            Builder.TabString = tabString;
            Builder.Append(startString);
            Builder.SetColor(startColor);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            StringBuilderManager.ReturnColorBuilder(key);
            this.Builder = null;

            _disposed = true;
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            Dispose();
        }

        private bool CheckKey()
        {
            if (StringBuilderManager.ContainsKey(key))
                throw new ArgumentException($"Builder with key '{key}' already in use.");
            return true;
        }
    }
}
