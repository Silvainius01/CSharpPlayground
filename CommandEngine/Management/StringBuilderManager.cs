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
        static void ReturnBuilder<T>(string key, Queue<T> Unlocked, Dictionary<string, T> Locked, Action<T> Reset) where T : class
        {
            if (!Locked.ContainsKey(key))
                throw new InvalidOperationException($"Builder with key '{key}' does is not in use.");

            T builder = Locked[key];
            Reset(builder);

            // Unlock, and add to pool.
            Locked.Remove(key);
            Unlocked.Enqueue(builder);
        }

        public static SmartStringBuilder GetSmartBuilder(string key)
        {
            return GetBuilder(key, UnlockedBuilders, LockedBuilders, () => new SmartStringBuilder());
        }
        public static void ReturnSmartBuilder(string key)
        {
            ReturnBuilder(key, UnlockedBuilders, LockedBuilders, (builder) =>
            {
                builder.Clear();
                builder.TabString = DefaultTabString;
                builder.NewlineString = DefaultNewlineString;
            });
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

        public static bool ContainsSmartKey(string key) => LockedBuilders.ContainsKey(key);
        public static bool ContainsColorKey(string key) => LockedColorBuilders.ContainsKey(key);
    }

    public class ManagedStringBuilder : IDisposable
    {
        private bool _disposed = false;
        private string key = string.Empty;

        public SmartStringBuilder Builder { get; private set; }

        public ManagedStringBuilder(string key)
        {
            CheckKey();
            this.key = key;
            Builder = StringBuilderManager.GetSmartBuilder(key);
        }
        public ManagedStringBuilder(string key, string startString) : this(key)
        {
            Builder = StringBuilderManager.GetSmartBuilder(key);
            Builder.Append(startString);
        }
        public ManagedStringBuilder(string key, string startString, string tabString) : this(key, startString)
        {
            Builder.TabString = tabString;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            StringBuilderManager.ReturnSmartBuilder(key);
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
            if (StringBuilderManager.ContainsSmartKey(key))
                throw new ArgumentException($"Builder with key '{key}' already in use.");
            return true;
        }
    }

    public class ManagedColorBuilder : IDisposable
    {
        private bool _disposed = false;
        private string key = string.Empty;

        public ColorStringBuilder Builder { get; private set; }

        public ManagedColorBuilder(string key)
        {
            CheckKey();
            this.key = key;
            Builder = StringBuilderManager.GetColorBuilder(key);
        }
        public ManagedColorBuilder(string key, string startString) : this(key)
        {
            Builder.Append(startString);
        }
        public ManagedColorBuilder(string key, string startString, ConsoleColor startColor) : this(key, startString)
        {
            Builder.SetColor(startColor);
        }
        public ManagedColorBuilder(string key, string startString, string tabString, ConsoleColor startColor) : this(key, startString, startColor)
        {
            Builder.TabString = tabString;
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
            if (StringBuilderManager.ContainsColorKey(key))
                throw new ArgumentException($"Builder with key '{key}' already in use.");
            return true;
        }
    }
}
