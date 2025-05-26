using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
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
            int sign = rInt % 2;
            int exponent = rInt % ((1 << 8) - 1); // do not generate 0xFF (infinities and NaN) (255)
            int mantissa = rInt % (1 << 23); // (8 388 608)

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
            return NextFloat() * maxValue;
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
            spareMarsagliaRand = Mathc.NormalizeBetween(y * s, -nf, nf) * 2 - 1;

            if (negOne2One)
                return Mathc.NormalizeBetween(x * s, -nf, nf) * 2 - 1;
            return Math.Abs(Mathc.NormalizeBetween(x * s, -nf, nf) * 2 - 1);
        }

        /// <summary>Get a normal distribution between 0 and <paramref name="range"/></summary>
        public static double MarsagliaRange(double range, bool neg = false)
        {
            if (neg)
                return Marsaglia(true) * range;
            return Marsaglia(false) * range;
        }
        /// <summary>Get a normal distribution between 0 and <paramref name="range"/></summary>
        public static float MarsagliaRange(float range, bool neg = false)
        {
            if (neg)
                return (float)Marsaglia(true) * range;
            return (float)Marsaglia(false) * range;
        }

        /// <summary>Get a normal distribution centered between two numbers</summary>
        public static double GetMarsagliaBetween(double min, double max)
        {
            Mathc.Swap(ref min, ref max);
            double mid = Mathc.GetMidValue(min, max);
            return MarsagliaRange(mid / 2, true) + mid;
        }
        /// <summary>Get a normal distribution centered between two numbers</summary>
        public static float GetMarsagliaBetween(float min, float max)
        {
            Mathc.Swap(ref min, ref max);
            float range = (max - min) / 2;
            float middle = Mathc.GetMidValue(min, max);
            return MarsagliaRange(range, true) + middle;
        }

        public static float GetMarsagliaBetween(Vector2Int range)
            => GetMarsagliaBetween(range.X, range.Y);
    }
}
