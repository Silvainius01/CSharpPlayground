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

        static Queue<ColorStringBuilder> UnlockedBuilders = new Queue<ColorStringBuilder>();
        static Dictionary<string, ColorStringBuilder> LockedBuilders = new Dictionary<string, ColorStringBuilder>();

        public static ColorStringBuilder GetBuilder(string key)
        {
            if (LockedBuilders.ContainsKey(key))
                throw new InvalidOperationException($"StringBuilder with key '{key}' is already in use.");

            ColorStringBuilder sb = UnlockedBuilders.Any()
                ? UnlockedBuilders.Dequeue()
                : new ColorStringBuilder();

            LockedBuilders.Add(key, sb);
            return sb;
        }

        public static void ReturnBuilder(string key)
        {
            if (!LockedBuilders.ContainsKey(key))
                throw new InvalidOperationException($"StringBuilder with key '{key}' does is not in use.");

            ColorStringBuilder builder = LockedBuilders[key];

            // Reset ColorBuilder to default
            builder.Clear();
            builder.TabString = DefaultTabString;
            builder.NewlineString = DefaultNewlineString;
            builder.SetColor(DefaultColor);

            // Unlock, and add to pool.
            LockedBuilders.Remove(key);
            UnlockedBuilders.Enqueue(builder);
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
            Builder = StringBuilderManager.GetBuilder(key);
        }
        public ManagedStringBuilder(string key, string tabString)
        {
            CheckKey();
            Builder = StringBuilderManager.GetBuilder(key);
            Builder.TabString = tabString;
        }
        public ManagedStringBuilder(string key, string startString, ConsoleColor startColor)
        {
            CheckKey();
            Builder = StringBuilderManager.GetBuilder(key);
            Builder.Append(startString);
            Builder.SetColor(startColor);
        }
        public ManagedStringBuilder(string key, string tabString, string startString, ConsoleColor startColor)
        {
            CheckKey();
            Builder = StringBuilderManager.GetBuilder(key);
            Builder.TabString = tabString;
            Builder.Append(startString);
            Builder.SetColor(startColor);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            StringBuilderManager.ReturnBuilder(key);
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
