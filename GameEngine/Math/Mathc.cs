using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public static class Mathc
    {
        public class Random
        {

            /// <summary> Normalizing factor </summary>
            private static readonly double nf = 3f;
            private static System.Random rng = new System.Random();
            private static bool hasSpareRand = false;
            private static double spareMarsagliaRand;

            public static System.Random RNG { get { return rng; } }

            public static int Int { get { return rng.Next(); } }
            public static int NextInt(int maxVal) { return rng.Next(maxVal); }
            public static int NextInt(int minVal, int maxVal) { return rng.Next(minVal, maxVal); }
            public static int NextInt(Vector2Int range, bool inclusive = false)
            {

                return inclusive ? rng.Next(range.X, range.Y + 1) : rng.Next(range.X, range.Y); 
            }

            public static double Double { get { return NextDouble(); } }
            public static double NormalDouble { get { return rng.NextDouble(); } }
            public static double NextDouble()
            {
                var byte8 = new byte[8];
                rng.NextBytes(byte8);
                return BitConverter.ToDouble(byte8, 0);
            }
            
            public static float NextFloatFromBytes()
            {
                var byte4 = new byte[4];
                rng.NextBytes(byte4);
                return BitConverter.ToSingle(byte4, 0);
            }
            // https://stackoverflow.com/a/3365388
            private static float NextFloatFromDiscrete()
            {
                double mantissa = (rng.NextDouble() * 2.0) - 1.0;
                // choose -149 instead of -126 to also generate subnormal floats (*)
                double exponent = Math.Pow(2.0, rng.Next(-126, 127));
                return (float)(mantissa * exponent);
            }
            private static float NextFloatFromRandInt()
            {
                // Orginal version uses three random ints.
                // But testing seems to reveal that using just one with bit shifting is identical.
                int rInt = rng.Next();
                int sign = rInt%2;
                int exponent = rInt%((1 << 8) - 1); // do not generate 0xFF (infinities and NaN) (255)
                int mantissa = rInt%(1 << 23); // (8 388 608)

                int bits = (sign << 31) + (exponent << 23) + mantissa;
                return IntBitsToFloat(bits);
            }
            private static float IntBitsToFloat(int bits)
            {
                unsafe
                {
                    return *(float*)&bits;
                }
            }


            public static float NextFloat() => (float)rng.NextDouble();
            /// <summary>
            /// Generate random float between 0 and maxValue
            /// </summary>
            public static float NextFloat(float maxValue)
            {
                return NextFloat()*maxValue;
                //return (float)((rng.NextDouble() * 2.0 - 1.0) * (double)float.MaxValue);
            }
            public static float NextFloat(Vector2 range)
            {
                float min = Math.Abs(range.X), max = Math.Abs(range.Y);
                float rFloat = NextFloat(min+max);
                Mathc.Swap(ref min, ref max);
                return rFloat - max;
            }

            /// <summary> Return a random, normally distributed number that is between 0 and 1 </summary>
            /// <param name="negOne2One"> Make the range -1 to 1 instead </param>
            /// <returns></returns>
            public static double Marsaglia(bool negOne2One)
            {
                if (hasSpareRand)
                {
                    hasSpareRand = false;
                    if (negOne2One)
                        return spareMarsagliaRand;
                    return Math.Abs(spareMarsagliaRand);
                }
                double x, y, s;
                do
                {
                    x = Random.NormalDouble * 2 - 1;
                    y = Random.NormalDouble * 2 - 1;
                    s = x * x + y * y;
                } while (s >= 1 || s == 0.0f);

                hasSpareRand = true;
                s = Math.Sqrt((-2 * Math.Log(s)) / s);
                spareMarsagliaRand = NormalizeBetween(y * s, -nf, nf) * 2 - 1;

                if (negOne2One)
                    return NormalizeBetween(x * s, -nf, nf) * 2 - 1;
                return Math.Abs(NormalizeBetween(x * s, -nf, nf) * 2 - 1);
            }
            
            /// <summary>
            /// Get a value that is equivalent to the mid value, and shifted a bit to a random side.
            /// </summary>
            public static double GetMarsagliaBetween(double min, double max)
            {
                if (min > max)
                {
                    var t = max;
                    max = min;
                    min = t;
                }
                return min + ((max - min) * 0.5f * Marsaglia(true));
            }
        }

        /// <summary> I.E. 90 degrees </summary>
        public const double HALF_PI = Math.PI / 2;
        /// <summary> I.E. 45 degrees </summary>
        public const double QUARTER_PI = Math.PI / 4;
        /// <summary> I.E. 22.5 degrees </summary>
        public const double EIGTH_PI = Math.PI / 8;
        /// <summary> I.E. 360 degrees  </summary>
        public const double TWO_PI = Math.PI * 2;
        public const double E = 2.7182818284f;
        public const double ONE_THIRD = 1 / 3;

        public const double RAD2DEG = 360 / (Math.PI * 2);
        public const double DEG2RAD = (Math.PI * 2) / 360;



        public static T GetRandomItemFromEnumerable<T>(IEnumerable<T> list)
        {
            int count = list.Count();
            return count > 1 ? list.ElementAt(Random.NextInt(count)) : list.First();
        }

        public static T GetRandomItemFromList<T>(List<T> list)
        {
            return list.Count > 1 ? list[Random.NextInt(list.Count)] : list[0];
        }

        // This solution looks lazy, but according to this Stack Overflow answer: https://stackoverflow.com/a/51099524
        // The if-chain solution is actually faster than anyother method in .Net Framework.
        // I know, it's pretty weird.
        public static int NumDigits(this int n)
        {
            if (n >= 0)
            {
                if (n < 10) return 1;
                if (n < 100) return 2;
                if (n < 1000) return 3;
                if (n < 10000) return 4;
                if (n < 100000) return 5;
                if (n < 1000000) return 6;
                if (n < 10000000) return 7;
                if (n < 100000000) return 8;
                if (n < 1000000000) return 9;
                return 10;
            }
            else
            {
                if (n > -10) return 2;
                if (n > -100) return 3;
                if (n > -1000) return 4;
                if (n > -10000) return 5;
                if (n > -100000) return 6;
                if (n > -1000000) return 7;
                if (n > -10000000) return 8;
                if (n > -100000000) return 9;
                if (n > -1000000000) return 10;
                return 11;
            }
        }

        public static int Clamp(int v, int min, int max)
        {
            return Min(max, Max(v, min));
        }
        public static float Clamp(float v, float min, float max)
        {
            return Min(max, Max(v, min));
        }

        public static float Min(float value1, float value2)
        {
            if (value1 < value2)
                return value1;
            return value2;
        }
        public static float Max(float value1, float value2)
        {
            if (value1 > value2)
                return value1;
            return value2;
        }

        public static int Min(int value1, int value2)
        {
            if (value1 < value2)
                return value1;
            return value2;
        }
        public static int Max(int value1, int value2)
        {
            if (value1 > value2)
                return value1;
            return value2;
        }

        public static double QuadraticFormula(double a, double b, double c, bool neg)
        {
            if (neg)
                return (-b - Math.Sqrt(Math.Pow(b, 2) - (4 * a * c))) / (2 * a);
            return (-b + Math.Sqrt(Math.Pow(b, 2) - (4 * a * c))) / (2 * a);
        }

        public static double Pythag(double a, double b)
        {
            return Math.Sqrt(PythagSqr(a, b));
        }

        public static double PythagSqr(double a, double b)
        {
            return Math.Pow(a, 2) + Math.Pow(b, 2);
        }

        public static double Pythag3D(double a, double b, double c)
        {
            return Math.Sqrt(PythagSqr3D(a, b, c));
        }

        public static double PythagSqr3D(double a, double b, double c)
        {
            return Math.Pow(a, 2) + Math.Pow(b, 2) + Math.Pow(c, 2);
        }


        public static double Sigmoid(double value, double weight)
        {
            return (2.0 / (1.0 - Math.Pow(Math.E, -value * weight))) - 1.0;
        }

        /// <summary> Returns a random KeyValuePair from a dicitonary. NOTE: creates a new list every time. not reccommened for large or unchanging dictionaries. </summary>
        public static KeyValuePair<TKey, TVal> GetRandomKVPFromDict<TKey, TVal>(ref Dictionary<TKey, TVal> dict)
        {
            var k = GetRandomKeyFromDict(ref dict);
            var v = dict[k];
            return new KeyValuePair<TKey, TVal>(k, v);
        }
        /// <summary> Returns a random key from a dicitonary. NOTE: creates a new list every time. not reccommened for large or unchanging dictionaries. </summary>
        public static TKey GetRandomKeyFromDict<TKey, TVal>(ref Dictionary<TKey, TVal> dict)
        {
            Random.NextInt(0, dict.Count);
            return System.Linq.Enumerable.ToList(dict.Keys)[Random.NextInt(0, dict.Count)];
        }
        /// <summary> Returns a random value from a dicitonary. NOTE: creates a new list every time. not reccommened for large or unchanging dictionaries. </summary>
        public static TVal GetRandomValueFromDict<TKey, TVal>(ref Dictionary<TKey, TVal> dict)
        {
            return System.Linq.Enumerable.ToList(dict.Values)[Random.NextInt(0, dict.Count)];
        }

        /// <summary> Returns an array filled with the declared values of an enum. </summary>
        public static T[] GetEnumValues<T>()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("Passed type is not an enum.");
            return (T[])Enum.GetValues(typeof(T));
        }

        public static double Truncate(this double val, int numPlaces)
        {
            double m = Math.Pow(10.0f, numPlaces);
            return (double)(Math.Truncate(m * val) / m);
        }

        /// <summary> Returns true if array[index].Equals() is true. </summary>
        public static bool ArrayContains<T>(ref T[] array, T value)
        {
            foreach (var val in array)
                if (val.Equals(value))
                    return true;
            return false;
        }
        /// <summary> Returns true if array[index].Equals() is true. </summary>
        public static bool ArrayContains<T>(ref T[] array, T value, out int index)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i].Equals(value))
                {
                    index = i;
                    return true;
                }
            index = -1;
            return false;
        }

        /// <summary> Swaps two values if min is greater than max. </summary>
        public static bool Swap(ref double min, ref double max)
        {
            if (min > max)
            {
                min = min + max;
                max = min - max;
                min = min - max;
                return true;
            }
            return false;
        }
        /// <summary> Swaps two values if min is greater than max. </summary>
        public static bool Swap(ref int min, ref int max)
        {
            if (min > max)
            {
                min = min + max;
                max = min - max;
                min = min - max;
                return true;
            }
            return false;
        }
        /// <summary> Swaps two values if min is greater than max. </summary>
        public static bool Swap(ref float min, ref float max)
        {
            if (min > max)
            {
                min = min + max;
                max = min - max;
                min = min - max;
                return true;
            }
            return false;
        }

        /// <summary> Returns a 0 to 1 value between two numbers. </summary>
        /// <returns> (val - min) / (max - min) </returns>
        public static double NormalizeBetween(double val, double min, double max)
        {
            if (max == min) return 0.0f;
            Swap(ref min, ref max);
            return (Clamp(val, min, max) - min) / (max - min);
        }

        public static double Clamp(double val, double min, double max)
        {
            return Math.Max(Math.Min(val, max), min);
        }

        /// <summary> Steps through any given enum either up or down. </summary>
        public static T EnumLooper<T>(T currentValue, bool stepUp, int maxValue, int minValue = 0) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("It's not an enum, fam. What the fuck?");

            int nextVal = Convert.ToInt32(currentValue) + (stepUp ? 1 : -1);
            nextVal = ReverseClamp(nextVal, minValue, maxValue);

            return (T)(object)nextVal;
        }

        /// <summary>  Will clamp a number between min and max. If value is greater than max, will return min and vice versa. </summary>
        public static int ReverseClamp(int value, int min, int max)
        {
            if (value > max) return min;
            if (value < min) return max;
            return value;
        }

        /// <summary> 
        /// Returns Mod(value + (-min), max + (-min)) + min.
        /// See more about how this function behaves: https://www.desmos.com/calculator/jgbixokd86
        /// </summary>
        public static int RemClamp(int value, int min, int max, bool maxInclusive = false)
        {
            max += maxInclusive ? 1 : 0;
            return Mod(value + -min, max + -min) + min;
        }

        /// <summary>
        /// Returns a list of type T that contains elements
        /// </summary>
        public static List<T> CreateList<T>(params T[] elements)
        {
            return new List<T>(elements);
        }

        public static float Mod(float a, float b)
        {
            return (a % b + b) % b;
        }
        public static double Mod(double a, double b)
        {
            return (a % b + b) % b;
        }
        public static int Mod(int a, int b)
        {
            return (a % b + b) % b;
        }

        /// <summary>
        /// Turn a |0 to 360| angle to a |-180 to 180| angle
        /// </summary>
        public static double Angle360ToAngle180(double angle)
        {
            return angle > 180 ? -180 + (angle - 180) : angle;
        }

        /// <summary>
        /// Turn a |-180 - 180| angle to a |0 - 360| angle
        /// </summary>
        public static double Angle180ToAngle360(double angle)
        {
            return angle < 0 ? angle + 360 : angle;
        }

        /// <summary>
        /// Turn a |0 - 2pi| angle to |-pi to pi| angle.
        /// </summary>
        public static double Angle2PiToAnglePi(double angle)
        {
            return angle > Math.PI ? -Math.PI + (angle - Math.PI) : angle;
        }
        /// <summary> Turn a |-pi - pi| to a |0 - 2pi| angle. </summary>
        public static double AnglePiToAngle2Pi(double angle)
        {
            return angle < 0 ? angle + TWO_PI : angle;
        }

        /// <summary> Returns true if a - b is less than threshold </summary>
        public static bool Approximately(double a, double b, double threshold)
        {
            return Math.Abs(a - b) <= threshold;
        }

        /// <summary>
        /// Returns the midpoint between two doubles.
        /// </summary>
        public static double GetMidValue(double min, double max)
        {
            if (min > max)
                return ((min - max) / 2) + max;
            return ((max - min) / 2) + min;
        }

        /// <summary> Returns val is between min and max, or if it is equal to them. </summary>
        /// <param name="strict">If true, will return true only if val is between </param>
        public static bool ValueIsBetween(double val, double min, double max, bool strict)
        {
            if (strict)
                return val > min && val < max;
            return val >= min && val <= max;
        }
        public static bool VectorIsBetween(Vector2_64 val, Vector2_64 min, Vector2_64 max, bool strict)
        {
            return ValueIsBetween(val.X, min.X, max.X, strict) && ValueIsBetween(val.Y, min.Y, max.Y, strict);
        }

    }
}