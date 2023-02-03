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
            public static int NextInt(int maxVal, bool inclusive = false) 
                => rng.Next(maxVal + (inclusive ? 1 : 0));
            public static int NextInt(int minVal, int maxVal, bool inclusive = false) 
                => rng.Next(minVal, maxVal + (inclusive ? 1 : 0));
            public static int NextInt(Vector2Int range, bool inclusive = false) 
                => inclusive ? rng.Next(range.X, range.Y + 1) : rng.Next(range.X, range.Y);

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
            }
            public static float NextFloat(Vector2 range)
            {
                range.SortSelf();
                float rFloat = NextFloat(Math.Abs(range.X - range.Y));
                return rFloat + range.X;
            }

            /// <summary> Return a random, normally distributed number that is between 0 and 1 </summary>
            /// <param name="negOne2One"> Make the range -1 to 1 instead </param>
            /// <returns></returns>
            public static double Marsaglia(bool negOne2One = false)
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

            /// <summary>Get a normal distribution between 0 and <paramref name="range"/></summary>
            public static double MarsagliaRange(double range, bool neg = false)
            {
                if (neg)
                    return Marsaglia(true) * range;
                return Marsaglia(false) * range;
            }
            /// <summary>Get a normal distribution between 0 and <paramref name="range"/></summary>
            public static float MarsagliaRange(float range, bool neg= false)
            {
                if (neg)
                    return (float)Marsaglia(true) * range;
                return (float)Marsaglia(false) * range;
            }

            /// <summary>Get a normal distribution between two numbers</summary>
            public static double GetMarsagliaBetween(double min, double max)
            {
                Mathc.Swap(ref min, ref max);
                double mid = GetMidValue(min, max);
                return MarsagliaRange(mid, true) + mid;
            }
            /// <summary>Get a normal distribution between two numbers</summary>
            public static float GetMarsagliaBetween(float min, float max)
            {
                Mathc.Swap(ref min, ref max);
                float mid = GetMidValue(min, max);
                return MarsagliaRange(mid, true) + mid;
            }
            /// <summary>Get a normal distribution between two numbers</summary>
            /// <param name="bias">Bias multiplier. >1.0f biases towards <paramref name="min"/>, <1.0f biases toward <paramref name="max"/></param>
            /// <returns></returns>
            public static float GetMarsagliaBetween(float min, float max, float bias)
            {
                Mathc.Swap(ref min, ref max);
                float mid = GetMidValue(min, max);
                return Mathc.Clamp((float)MarsagliaRange(mid, true) + (bias * mid), min, max);
            }
            public static float GetMarsagliaBetween(Vector2Int range, float bias)
                => GetMarsagliaBetween(range.X, range.Y, bias);
        }

        /// <summary> I.E. 90 degrees </summary>
        public const double HalfPi = Math.PI / 2;
        /// <summary> I.E. 45 degrees </summary>
        public const double QuarterPi = Math.PI / 4;
        /// <summary> I.E. 22.5 degrees </summary>
        public const double EigthPi = Math.PI / 8;
        /// <summary> I.E. 360 degrees  </summary>
        public const double TwoPi = Math.PI * 2;
        public const double E = 2.7182818284f;
        public const double OneThird = 1 / 3;

        public const double RAD2DEG = 360 / (Math.PI * 2);
        public const double DEG2RAD = (Math.PI * 2) / 360;


        public static float Frac(float min, float max, int num, int den)
        {
           // Swap(ref min, ref max);
            return (max - min * (num / den)) + min;
        }

        public static Vector2Int Clamp(Vector2Int value, Vector2Int clamp)
        {
            clamp.SortSelf();
            return new Vector2Int(Mathc.Clamp(value.X, clamp.X, clamp.Y), Mathc.Clamp(value.Y, clamp.X, clamp.Y));
        }
        public static Vector2Int Clamp(Vector2Int value, int min, int max)
        {
            Mathc.Swap(ref min, ref max);
            return new Vector2Int(Math.Max(value.X, min), Math.Min(value.Y, max));
        }

        public static Vector2Int Min(Vector2Int v, int minValue) 
            => new Vector2Int(Mathc.Min(v.X, minValue), Mathc.Min(v.Y, minValue));
        public static Vector2Int Min(Vector2Int vFirst, Vector2Int vSecond)
            => vFirst.SqrMagnitude < vSecond.SqrMagnitude ? vFirst : vSecond;
        public static Vector2Int MinValue(Vector2Int vFirst, Vector2Int vSecond)
            => new Vector2Int(Mathc.Min(vFirst.X, vSecond.X), Mathc.Min(vFirst.Y, vSecond.Y));
        
        public static Vector2Int Max(Vector2Int v2, int maxValue) 
            => new Vector2Int(Mathc.Max(v2.X, maxValue), Mathc.Max(v2.Y, maxValue));
        public static Vector2Int Max(Vector2Int vFirst, Vector2Int vSecond)
            => vFirst.SqrMagnitude > vSecond.SqrMagnitude ? vFirst : vSecond;
        public static Vector2Int MaxValue(Vector2Int vFirst, Vector2Int vSecond)
            => new Vector2Int(Mathc.Max(vFirst.X, vSecond.X), Mathc.Max(vFirst.Y, vSecond.Y));

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

        public static double Truncate(this double val, int numPlaces)
        {
            double m = Math.Pow(10, numPlaces);
            return (Math.Truncate(m * val) / m);
        }
        public static float Truncate(this float val, int numPlaces)
        {
            double m = Math.Pow(10, numPlaces);
            return (float)(Math.Truncate(m * val) / m);
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
            return angle < 0 ? angle + TwoPi : angle;
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
                return ((min - max) * 0.5) + max;
            return ((max - min) * 0.5) + min;
        }
        /// <summary>
        /// Returns the midpoint between two floats.
        /// </summary>
        public static float GetMidValue(float min, float max)
        {
            if (min > max)
                return ((min - max) * 0.5f) + max;
            return ((max - min) * 0.5f) + min;
        }

        /// <summary> Returns val is between min and max, or if it is equal to them. </summary>
        /// <param name="strict">If true, will return true only if val is between </param>
        public static bool ValueIsBetween(double val, double min, double max, bool strict = false)
        {
            if (strict)
                return val > min && val < max;
            return val >= min && val <= max;
        }
        /// <summary> Returns val is between min and max, or if it is equal to them. </summary>
        /// <param name="strict">If true, will return true only if val is between </param>
        public static bool ValueIsBetween(float val, float min, float max, bool strict = false)
        {
            if (strict)
                return val > min && val < max;
            return val >= min && val <= max;
        }
        /// <summary> Returns val is between min and max, or if it is equal to them. </summary>
        /// <param name="strict">If true, will return true only if val is between </param>
        public static bool ValueIsBetween(int val, int min, int max, bool strict = false)
        {
            if (strict)
                return val > min && val < max;
            return val >= min && val <= max;
        }
        public static bool VectorIsBetween(Vector2_64 val, Vector2_64 min, Vector2_64 max, bool strict = false)
        {
            return ValueIsBetween(val.X, min.X, max.X, strict) && ValueIsBetween(val.Y, min.Y, max.Y, strict);
        }

    }
}