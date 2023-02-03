using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace CommandEngine
{
    public class Matrix
    {
        public double[] data;
        public int width;
        public int height;

        public Matrix(int width, int height)
        {
            this.width = width;
            this.height = height;
            data = new double[width * height];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = 0;
            }
        }
        public Matrix(double[] values, int width, int height)
        {
            this.width = width;
            this.height = height;
            data = new double[width * height];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = i < values.Length ? values[i] : 0;
            }
        }

        // public Matrix(double[,] values) { }
        // public Matrix(double[][] values) { }

        public double[] GetRow(int row)
        {
            double[] r = new double[width];

            for (int i = row; i < data.Length && i < row + width; i += width)
                r[i / width] = data[i];

            return r;
        }

        public double[] GetColumn(int col)
        {
            double[] c = new double[height];

            for (int i = col * width; i < data.Length; i += width)
                c[i / width] = data[i];

            return c;
        }

        public static double Dot(double[] set1, double[] set2)
        {
            if (set1.Length != set2.Length)
                throw new Exception("Sets must be of the same length!");

            double v = 0;
            for (int i = 0; i < set1.Length; ++i)
                v += set1[i] * set2[i];
            return v;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                    sb.Append($"{data[x + y]} ");
                sb.Append('\n');
            }
            return sb.ToString();
        }

        #region Operators
        //public static VectorDouble operator *(Matrix m, VectorDouble vd)
        //{
        //    if (m.width != vd.Length)
        //        throw new ArgumentException($"Matrix does not have the required rows to form a product with Vector length {vd.Length}");

        //    VectorDouble rv = new VectorDouble(vd.Length);

        //    for (int i = 0; i < vd.Length; ++i)
        //        for (int j = 0; j < m.width; ++j)
        //            rv[i]+=

        //            return rv;
        //}
        #endregion
    }
}
