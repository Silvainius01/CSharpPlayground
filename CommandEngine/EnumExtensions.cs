using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    /// <summary>
    /// Enum extensions! Get an array of all the value, or even a random one!
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    public class EnumExt<T> where T : struct, Enum
    {
        public static readonly string TypeName = typeof(T).Name;
        public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
        public static readonly int[] IntegerValues = ConvertToInts();
        public static readonly string[] Names = Enum.GetNames(typeof(T));

        public static int Count { get => Values.Length; }
        public static T RandomValue { get => Values.RandomItem(); }
        public static EnumCommandModule<T> GetCommandModule(bool allowIntegerShortcuts)
        {
            if (commandModule == null)
                commandModule = new EnumCommandModule<T>($"Enter a value of {TypeName}: ", allowIntegerShortcuts);
            else commandModule.AllowsIntegerShortcuts = allowIntegerShortcuts;

            return commandModule;
        }

        public static string GetName(T t)
        {
            string? name = Enum.GetName(typeof(T), t);
            return name is not null ? name : string.Empty;
        }

        static EnumCommandModule<T> commandModule = null;


        static int[] ConvertToInts()
        {
            int[] intValues = new int[Values.Length];
            for (int i = 0; i < Values.Length; i++)
            {
                intValues[i] = Convert.ToInt32(Values[i]);
            }
            return intValues;
        }
    }
}
