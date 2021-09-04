using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class Vector3
    {
        private double[] data;
        public ref double X => ref data[0];
        public ref double Y => ref data[1];
        public ref double Z => ref data[2];

        public double Magnitude => Math.Sqrt(SqrMagnitude);
        public double SqrMagnitude => X * X + Y * Y + Z * Z;

        public static Vector3 Zero      => new Vector3(0, 0, 0);
        public static Vector3 Right     => new Vector3(1, 0, 0);
        public static Vector3 Up        => new Vector3(0, 1, 0);
        public static Vector3 Forward   => new Vector3(0, 0, 1);

        public Vector3(double x, double y, double z)
        {
            data = new double[3] { x, y, z };
        }
        public Vector3(double[] values)
        {
            if (values.Length != 3)
            {
                data = new double[3];
                for (int i = 0; i < data.Length; ++i)
                    data[i] = i < values.Length ? values[i] : 0.0;
            }
            else data = values;
        }

        public void Normalize()
        {
            var mag = Magnitude;
            X /= mag;
            Y /= mag;
            Z /= mag;
        }
        public Vector3 Normal()
        {
            return this / Magnitude;
        }

        public double Dot(Vector3 vector) => (X * vector.X) + (Y * vector.Y);
        public double CosDot(Vector3 vector) => Dot(vector) / (Magnitude * vector.Magnitude);
        public Vector3 Cross(Vector3 other) => new Vector3(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y + Y * other.X);

        /// <summary>
        /// Returns the perpindicular of this vector that is pointed closer to the given vector
        /// </summary>
        //public Vector3 BestPerp(Vector3 vector)
        //{
        //    Vector3 perp = new Vector3(-Y, X, Z);
        //    if (perp.CosDot(vector) > 0)
        //        return perp;
        //    return -perp;
        //}


        #region Operators
        public static Vector3 operator +(Vector3 v3a, Vector3 v3b)
        {
            return new Vector3(v3a.X + v3b.X, v3a.Y + v3b.Y, v3a.Z + v3b.Z);
        }

        public static Vector3 operator -(Vector3 v3a, Vector3 v3b)
        {
            return new Vector3(v3a.X - v3b.X, v3a.Y - v3b.Y, v3a.Z - v3b.Z);
        }
        public static Vector3 operator -(Vector3 v3a)
        {
            return new Vector3(-v3a.X, -v3a.Y, -v3a.Z);
        }

        public static Vector3 operator *(Vector3 v3a, Vector3 v3b)
        {
            return new Vector3(v3a.X * v3b.X, v3a.Y * v3b.Y, v3a.Z * v3b.Z);
        }
        public static Vector3 operator *(double d, Vector3 v3b)
        {
            return new Vector3(d * v3b.X, d * v3b.Y, d * v3b.Z);
        }
        public static Vector3 operator *(Vector3 v3a, double d)
        {
            return new Vector3(d * v3a.X, d * v3a.Y, v3a.Z * d);
        }

        public static Vector3 operator /(Vector3 v3a, Vector3 v3b)
        {
            return new Vector3(v3a.X / v3b.X, v3a.Y / v3b.Y, v3a.Z / v3b.Z);
        }
        public static Vector3 operator /(double d, Vector3 v3b)
        {
            return new Vector3(d / v3b.X, d / v3b.Y, d / v3b.Z);
        }
        public static Vector3 operator /(Vector3 v3a, double d)
        {
            return new Vector3(v3a.X / d, v3a.Y / d, v3a.Z / d);
        }

        public double this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }
        #endregion

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
