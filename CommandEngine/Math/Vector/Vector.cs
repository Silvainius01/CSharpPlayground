using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine 
{
    public struct VectorDouble
    {
        double[] data;

        public int Length => data.Length;
        public double Magnitude => Math.Sqrt(SquareMagnitude());
        public double SqrMagnitude => SquareMagnitude();
        public double this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public VectorDouble(int length) => this.data = new double[length];
        public VectorDouble(double[] data) => this.data = data;
                
        private double SquareMagnitude()
        {
            double m = 0;
            for (int i = 0; i < data.Length; ++i)
                m += data[i] * data[i];
            return m;
        }

        public VectorDouble Normal()
        {
            double m = this.Magnitude;
            VectorDouble rv = new VectorDouble(this.data);
            for (int i = 0; i < data.Length; ++i)
                rv[i] /= m;
            return rv;
        }

        #region Operators
        public static VectorDouble operator +(VectorDouble first, VectorDouble second)
        {
            if (first.Length != second.Length)
                throw new Exception("Vectors must be of same length!");

            VectorDouble result = new VectorDouble(first.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] += second[i];
            return result;
        }

        public static VectorDouble operator -(VectorDouble first, VectorDouble second)
        {
            VectorDouble result = new VectorDouble(first.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] -= second[i];
            return result;
        }
        public static VectorDouble operator -(VectorDouble first)
        {
            VectorDouble result = new VectorDouble(first.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] *= -1;
            return result;
        }

        public static VectorDouble operator *(VectorDouble first, VectorDouble second)
        {
            VectorDouble result = new VectorDouble(first.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] *= second[i];
            return result;
        }
        public static VectorDouble operator *(float value, VectorDouble vector)
        {
            VectorDouble result = new VectorDouble(vector.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] *= value;
            return result;
        }
        public static VectorDouble operator *(VectorDouble vector, float value)
        {
            VectorDouble result = new VectorDouble(vector.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] *= value;
            return result;
        }

        public static VectorDouble operator /(VectorDouble first, VectorDouble second)
        {
            VectorDouble result = new VectorDouble(first.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] /= second[i];
            return result;
        }
        public static VectorDouble operator /(float value, VectorDouble vector)
        {
            VectorDouble result = new VectorDouble(vector.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] /= value;
            return result;
        }
        public static VectorDouble operator /(VectorDouble first, float value)
        {
            VectorDouble result = new VectorDouble(first.data);
            for (int i = 0; i < result.Length; ++i)
                result[i] += value;
            return result;
        }
        #endregion
    }
}
