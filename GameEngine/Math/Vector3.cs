using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    class Vector3_64
    {
        private double[] data;
        public double X
        {
            get { return data[0]; }
            set { data[0] = value; }
        }
        public double Y
        {
            get { return data[1]; }
            set { data[1] = value; }
        }
        public double Z
        {
            get { return data[2]; }
            set { data[2] = value; }
        }

        public double Magnitude { get { return Mathc.Pythag(X, Y); } }
        public double SqrMagnitude { get { return Mathc.PythagSqr(X, Y); } }

        public Vector3_64 EulerAngles
        {
            get
            {
                // z -> x -> y
                // From pos, then normalize.
                // v3 = pos.normal
                // v2 = ()

                var retval = this.Normal();


                return Zero;
            }
        }

        public static Vector3_64 Zero { get { return new Vector3_64(0, 0, 0); } }

        public Vector3_64(double x, double y, double z)
        {
            data = new double[3] { x, y, z };
        }

        public void Normalize()
        {
            var mag = Magnitude;
            X /= mag;
            Y /= mag;
            Z /= mag;
        }
        public Vector3_64 Normal()
        {
            return this / Magnitude;
        }

        public double Dot(Vector3_64 vector)
        {
            return (X * vector.X) + (Y * vector.Y);
        }
        public double CosDot(Vector3_64 vector)
        {
            return Dot(vector) / (Magnitude * vector.Magnitude);
        }

        public static Vector3_64 FromAngle(double radians)
        {
            return Vector3_64.Zero; // (Math.(radians), Math.Sin(radians));
        }

        /// <summary>
        /// Returns the perpindicular of this vector that is pointed closer to the give vector
        /// </summary>
        public Vector3_64 BestPerp(Vector3_64 vector)
        {
            Vector3_64 perp = new Vector3_64(-Y, X, Z);
            if (perp.CosDot(vector) > 0)
                return perp;
            return -perp;
        }


        #region Operators
        public static Vector3_64 operator +(Vector3_64 v3a, Vector3_64 v3b)
        {
            return new Vector3_64(v3a.X + v3b.X, v3a.Y + v3b.Y, v3a.Z + v3b.Z);
        }

        public static Vector3_64 operator -(Vector3_64 v3a, Vector3_64 v3b)
        {
            return new Vector3_64(v3a.X - v3b.X, v3a.Y - v3b.Y, v3a.Z - v3b.Z);
        }
        public static Vector3_64 operator -(Vector3_64 v3a)
        {
            return new Vector3_64(-v3a.X, -v3a.Y, -v3a.Z);
        }
        public static Vector3_64 operator *(Vector3_64 v3a, Vector3_64 v3b)
        {
            return new Vector3_64(v3a.X * v3b.X, v3a.Y * v3b.Y, v3a.Z * v3b.Z);
        }
        public static Vector3_64 operator *(double d, Vector3_64 v3b)
        {
            return new Vector3_64(d * v3b.X, d * v3b.Y, d * v3b.Z);
        }
        public static Vector3_64 operator *(Vector3_64 v3a, double d)
        {
            return new Vector3_64(d * v3a.X, d * v3a.Y, v3a.Z * d);
        }

        public static Vector3_64 operator /(Vector3_64 v3a, Vector3_64 v3b)
        {
            return new Vector3_64(v3a.X / v3b.X, v3a.Y / v3b.Y, v3a.Z / v3b.Z);
        }
        public static Vector3_64 operator /(double d, Vector3_64 v3b)
        {
            return new Vector3_64(d / v3b.X, d / v3b.Y, d / v3b.Z);
        }
        public static Vector3_64 operator /(Vector3_64 v3a, double d)
        {
            return new Vector3_64(v3a.X / d, v3a.Y / d, v3a.Z / d);
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
