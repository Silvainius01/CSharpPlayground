using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace GameEngine
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
    }

    public class SquareMatrix : Matrix
    {
        public int sideLength;

        /// <summary>
        /// Construct a square matrix populated with values. 
        /// If there are not enough values, the remaining slots will be filled with the identity matrix.
        /// </summary>
        public SquareMatrix(double[] values, int sideLength) : base(values, sideLength, sideLength)
        {
            this.sideLength = sideLength;
            if (values.Length < data.Length)
            {
                for (int i = 0; i < sideLength; ++i)
                {
                    for (int j = 0; j < sideLength; ++j)
                    {
                        int index = i * sideLength + j;
                        if (index >= values.Length && i == j)
                            data[index] = 1;
                    }
                }
            }
        }

        /// <summary>Constructs a square identity matrix</summary>
        public SquareMatrix(int sideLength) : base(sideLength, sideLength)
        {
            this.sideLength = sideLength;
            for (int i = 0; i < sideLength; ++i)
            {
                for (int j = 0; j < sideLength; ++j)
                {
                    if (i == j)
                        data[i * sideLength + j] = 1;
                }
            }
        }

        public SquareMatrix(SquareMatrix other) : base(other.data, other.width, other.height) { this.sideLength = other.sideLength; }

        public SquareMatrix GetMinor(int rowElim, int colElim)
        {
            SquareMatrix minor = new SquareMatrix(sideLength - 1);

            int count = 0;
            for (int i = 0; i < sideLength; ++i)
                for (int j = 0; j < sideLength; ++j)
                {
                    int index = i * sideLength + j;
                    if (i == rowElim || j == colElim)
                        continue;
                    minor.data[count++] = data[index];
                }

            return minor;
        }

        public SquareMatrix Cofactor()
        {
            SquareMatrix cf = new SquareMatrix(sideLength);

            for (int i = 0; i < sideLength; ++i)
                for (int j = 0; j < sideLength; ++j)
                {
                    int index = i * sideLength + j;
                    cf.data[index] = GetMinor(i, j).Determinate() * (index % 2 == 0 ? 1 : -1);
                }

            return cf;
        }

        /// <summary>
        /// Get the determiniate via expansion of minors
        /// </summary>
        /// <returns></returns>
        public double Determinate()
        {
            // Return the determinate for 1x1 or 2x2 matricies.
            switch(sideLength)
            {
                case 1:
                    return data[0];
                case 2:
                    return data[0] * data[3] - data[1] * data[2];
            }

            // Use the first row of the matrix for the determinate, and recurse over their minors.
            double determinate = 0;
            for (int index = 0; index < sideLength; ++index)
            {
                double md = data[index] * GetMinor(0, index).Determinate();
                determinate = index % 2 == 0 ? determinate + md : determinate - md;
            }

            return determinate;
        }

        public SquareMatrix Transpose()
        {
            SquareMatrix transpose = new SquareMatrix(this);

            for (int i = 0; i < sideLength; ++i)
                for (int j = 0; j < sideLength; ++j)
                {
                    if (i > j)
                    {
                        int index1 = i * sideLength + j;
                        int index2 = j * sideLength + i;
                        double save = transpose.data[index1];
                        transpose.data[index2] = transpose.data[index1];
                        transpose.data[index1] = save;
                    }
                }

            return transpose;
        }

        public SquareMatrix Adjugate()
        {
            return Cofactor().Transpose();
        }

        public bool Inverse(out SquareMatrix inverse)
        {
            double det = Determinate();
            if (det > 0)
            {
                inverse = Adjugate();
                inverse *= 1 / det;
                return true;
            }
            inverse = null;
            return false;
        }

        #region Operators
        public static SquareMatrix operator +(in SquareMatrix m1, in SquareMatrix m2)
        {
            if (m1.sideLength != m2.sideLength)
                return null;

            int sideLength = m1.sideLength;
            SquareMatrix retval = new SquareMatrix(sideLength);

            for (int i = 0; i < retval.data.Length; ++i)
                retval.data[i] = m1.data[i] + m2.data[i];

            return retval;
        }

        public static SquareMatrix operator -(in SquareMatrix m1, in SquareMatrix m2)
        {
            if (m1.sideLength != m2.sideLength)
                return null;

            int sideLength = m1.sideLength;
            SquareMatrix retval = new SquareMatrix(sideLength);

            for (int i = 0; i < retval.data.Length; ++i)
                retval.data[i] = m1.data[i] - m2.data[i];

            return retval;
        }

        public static SquareMatrix operator *(in SquareMatrix m1, in SquareMatrix m2)
        {
            if (m1.sideLength != m2.sideLength)
                return null;

            int sideLength = m1.sideLength;
            SquareMatrix retval = new SquareMatrix(sideLength);

            for (int i = 0; i < sideLength; i++)
            {
                //Gets Column data from first
                var colData = m1.GetColumn(i);

                for (int j = 0; j < sideLength; j++)
                {
                    //Gets row data from second
                    int index = i * sideLength + j;
                    var rowData = m2.GetRow(j);
                    retval.data[index] = Matrix.Dot(rowData, colData);
                }
            }

            return retval;
        }

        public static SquareMatrix operator *(in SquareMatrix m, double v)
        {
            SquareMatrix rv = new SquareMatrix(m);
            for (int i = 0; i < rv.data.Length; ++i)
                rv.data[i] *= v;
            return rv;
        }
        #endregion
    }
}
