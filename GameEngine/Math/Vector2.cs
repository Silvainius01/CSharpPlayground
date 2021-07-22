using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine 
{
    public struct Vector2
    {
        private float[] data;
        public float X
        {
            get { return data[0]; }
            set { data[0] = value; }
        }
        public float Y
        {
            get { return data[1]; }
            set { data[1] = value; }
        }
        public float Magnitude { get { return (float)Mathc.Pythag(X, Y); } }
        public float SqrMagnitude { get { return (float)Mathc.PythagSqr(X, Y); } }
        public float AngleRadians
        {
            get
            {
                var direction = this.Normal();
                return (float)Math.Atan2(direction.Y, direction.X);
            }
        }
        public float AngleDegrees { get { return AngleRadians * (float)Mathc.RAD2DEG; } }
        public float this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public static Vector2 Zero { get { return new Vector2(0, 0); } }

        public Vector2(float x, float y)
        {
            data = new float[2] { x, y };
        }

        public void Normalize()
        {
            var mag = Magnitude;
            X /= mag;
            Y /= mag;
        }
        public Vector2 Normal()
        {
            return this / Magnitude;
        }

        public float Dot(Vector2 vector)
        {
            return (X * vector.X) + (Y * vector.Y);
        }
        public float CosDot(Vector2 vector)
        {
            return Dot(vector) / (Magnitude * vector.Magnitude);
        }

        public static Vector2 FromAngle(float radians)
        {
            return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }

        /// <summary>
        /// Returns the perpindicular of this vector that is pointed closer to the give vector
        /// </summary>
        public Vector2 BestPerp(Vector2 vector)
        {
            Vector2 perp = new Vector2(-Y, X);
            if (perp.CosDot(vector) > 0)
                return perp;
            return -perp;
        }

        #region Operators
        public static Vector2 operator +(Vector2 v2a, Vector2 v2b)
        {
            return new Vector2(v2a.X + v2b.X, v2a.Y + v2b.Y);
        }

        public static Vector2 operator -(Vector2 v2a, Vector2 v2b)
        {
            return new Vector2(v2a.X - v2b.X, v2a.Y - v2b.Y);
        }
        public static Vector2 operator -(Vector2 v2a)
        {
            return new Vector2(-v2a.X, -v2a.Y);
        }
        public static Vector2 operator *(Vector2 v2a, Vector2 v2b)
        {
            return new Vector2(v2a.X * v2b.X, v2a.Y * v2b.Y);
        }
        public static Vector2 operator *(float d, Vector2 v2b)
        {
            return new Vector2(d * v2b.X, d * v2b.Y);
        }
        public static Vector2 operator *(Vector2 v2a, float d)
        {
            return new Vector2(d * v2a.X, d * v2a.Y);
        }

        public static Vector2 operator /(Vector2 v2a, Vector2 v2b)
        {
            return new Vector2(v2a.X / v2b.X, v2a.Y / v2b.Y);
        }
        public static Vector2 operator /(float d, Vector2 v2b)
        {
            return new Vector2(d / v2b.X, d / v2b.Y);
        }
        public static Vector2 operator /(Vector2 v2a, float d)
        {
            return new Vector2(v2a.X / d, v2a.Y / d);
        }
        #endregion

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
        public string ToString(string format)
        {
            return $"({X.ToString(format)}, {Y.ToString(format)})";
        }
    }

    public struct Vector2_64
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
        public double Magnitude { get { return Mathc.Pythag(X, Y); } }
        public double SqrMagnitude { get { return Mathc.PythagSqr(X, Y); } }
        public double AngleRadians
        {
            get
            {
                var direction = this.Normal();
                return Math.Atan2(direction.Y, direction.X);
            }
        }
        public double AngleDegrees { get { return AngleRadians * Mathc.RAD2DEG; } }

        public static Vector2_64 Zero { get { return new Vector2_64(0, 0); } }

        public Vector2_64(double x, double y)
        {
            data = new double[2] { x, y };
        }
        
        public void Normalize()
        {
            var mag = Magnitude;
            X /= mag;
            Y /= mag;
        }
        public Vector2_64 Normal()
        {
            return this / Magnitude;
        }

        public double Dot(Vector2_64 vector)
        {
            return (X * vector.X) + (Y * vector.Y);
        }
        public double CosDot(Vector2_64 vector)
        {
            return Dot(vector) / (Magnitude * vector.Magnitude);
        }

        public static Vector2_64 FromAngle(double radians)
        {
            return new Vector2_64(Math.Cos(radians), Math.Sin(radians));
        }

        /// <summary>
        /// Returns the perpindicular of this vector that is pointed closer to the give vector
        /// </summary>
        public Vector2_64 BestPerp(Vector2_64 vector)
        {
            Vector2_64 perp = new Vector2_64(-Y, X);
            if (perp.CosDot(vector) > 0)
                return perp;
            return -perp;
        }


        #region Operators
        public static Vector2_64 operator +(Vector2_64 v2a, Vector2_64 v2b)
        {
            return new Vector2_64(v2a.X + v2b.X, v2a.Y + v2b.Y);
        }

        public static Vector2_64 operator -(Vector2_64 v2a, Vector2_64 v2b)
        {
            return new Vector2_64(v2a.X - v2b.X, v2a.Y - v2b.Y);
        }
        public static Vector2_64 operator -(Vector2_64 v2a)
        {
            return new Vector2_64(-v2a.X, -v2a.Y);
        }
        public static Vector2_64 operator *(Vector2_64 v2a, Vector2_64 v2b)
        {
            return new Vector2_64(v2a.X * v2b.X, v2a.Y * v2b.Y);
        }
        public static Vector2_64 operator *(double d, Vector2_64 v2b)
        {
            return new Vector2_64(d * v2b.X, d * v2b.Y);
        }
        public static Vector2_64 operator *(Vector2_64 v2a, double d)
        {
            return new Vector2_64(d * v2a.X, d * v2a.Y);
        }

        public static Vector2_64 operator /(Vector2_64 v2a, Vector2_64 v2b)
        {
            return new Vector2_64(v2a.X / v2b.X, v2a.Y / v2b.Y);
        }
        public static Vector2_64 operator /(double d, Vector2_64 v2b)
        {
            return new Vector2_64(d / v2b.X, d / v2b.Y);
        }
        public static Vector2_64 operator /(Vector2_64 v2a, double d)
        {
            return new Vector2_64(v2a.X / d, v2a.Y / d);
        }
        #endregion

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }

    public struct Vector2Int
    {
        private int[] data;
        public int X
        {
            get { return data[0]; }
            set { data[0] = value; }
        }
        public int Y
        {
            get { return data[1]; }
            set { data[1] = value; }
        }
        public static Vector2Int Zero { get { return new Vector2Int(0, 0); } }

        public Vector2Int(int x, int y)
        {
            data = new int[2] { x, y };
        }

        #region Operators
        public static Vector2Int operator +(Vector2Int v2a, Vector2Int v2b)
        {
            return new Vector2Int(v2a.X + v2b.X, v2a.Y + v2b.Y);
        }
        public static Vector2Int operator -(Vector2Int v2a, Vector2Int v2b)
        {
            return new Vector2Int(v2a.X - v2b.X, v2a.Y - v2b.Y);
        }
        public static Vector2Int operator *(Vector2Int v2a, Vector2Int v2b)
        {
            return new Vector2Int(v2a.X * v2b.X, v2a.Y * v2b.Y);
        }
        public static Vector2Int operator /(Vector2Int v2a, Vector2Int v2b)
        {
            return new Vector2Int(v2a.X / v2b.X, v2a.Y / v2b.Y);
        }
        #endregion
    }
}
