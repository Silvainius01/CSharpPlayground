using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    /// <summary>
    /// Enum extensions! Get an array of all the value, or even a random one!
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    public class EnumExt<T> where T : struct, Enum
    {
        public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
        public static readonly int[] IntegerValues = ConvertToInts();
        public static readonly string[] Names = Enum.GetNames(typeof(T));

        public static int Count { get => Values.Length; }
        public static T RandomValue { get => Values.RandomItem(); }
        public static EnumCommandModule<T> GetCommandModule(bool allowIntegerShortcuts)
        {
            if (commandModule == null)
                commandModule = new EnumCommandModule<T>(allowIntegerShortcuts);
            else commandModule.AllowsIntegerShortcuts = allowIntegerShortcuts;

            return commandModule;
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
