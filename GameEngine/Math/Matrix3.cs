using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    class Matrix3
    {
        public double[,] data;

        public static Matrix3 Identity { get { return new Matrix3(new double[9]); } }
        
        public double Determinant { get { return GetDeterminant(); } }
        public Matrix3 Transpose { get { return GetTranspose(); } }
        public Matrix3 Inverse { get { return GetInverse(); } }


        public Matrix3()
        {
            data = new double[3, 3];
        }
        public Matrix3(Vector3_64 v3a, Vector3_64 v3b, Vector3_64 v3c, bool asColumns) : this()
        {
            if (asColumns)
            {
                SetColumn(0, v3a);
                SetColumn(1, v3b);
                SetColumn(2, v3c);
            }
            else
            {
                SetRow(0, v3a);
                SetRow(1, v3b);
                SetRow(2, v3c);
            } 
        }
        public Matrix3(Matrix3 m)
        {
            data = m.data;
        }
        public Matrix3(double[] values)
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (i + j < values.Length)
                        data[i, j] = values[i + j];
                    else data[i, j] = 0.0;
                }
            }
        }
        public Matrix3(double[,] values)
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (i < values.GetLength(0) && j < values.GetLength(1))
                        data[i, j] = values[i, j];
                    else data[i, j] = 0.0;
                }
            }
        }
        public Matrix3(double[][] values)
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (i < values.GetLength(0) && j < values.GetLength(1))
                        data[i, j] = values[i][j];
                    else data[i, j] = 0.0;
                }
            }
        }

        private Matrix3 GetTranspose()
        {
            Matrix3 transpose = new Matrix3(this);

            for (int a = 0; a < 3; a++)
            {
                for (int b = 0; b < 3; b++)
                {
                    if (a > b)
                    {
                        double save = transpose.data[b, a];
                        transpose.data[b, a] = transpose.data[a, b];
                        transpose.data[a, b] = save;
                    }
                }
            }

            return transpose;
        }
        
        private double GetDeterminant()
        {
            return ((data[0, 0] * ((data[1, 1] * data[2, 2]) - (data[1, 2] * data[2, 1]))) -
                (data[1, 0] * ((data[0, 1] * data[2, 2]) - (data[0, 2] * data[2, 1]))) +
                (data[2, 0] * ((data[0, 1] * data[1, 2]) - (data[0, 2] * data[1, 1]))));
        }

        private Matrix3 GetInverse()
        {
            int count = 0;
            double[] e = new double[4];
            Matrix3 adjugate = new Matrix3();

            for (int col = 0, row = 0; row < 3; col++)
            {
                                for (int a = 0; a < 3; a++)                        
                {

                    for (int b = 0; b < 3; b++)
                    {
                        if (a != col && b != row)
                        {
                            e[count] = this.data[a, b];
                            count++;
                        }
                    }
                }

                count = 0;
                adjugate.data[col, row] = (e[0] * e[3]) - (e[1] * e[2]);
                
                if (col == 2)
                {
                    col = -1;
                    row++;
                }
            }

            for (int a = 0; a < 3; a++)
            {
                for (int b = 0; b < 3; b++)
                {
                    if (a - 1 == b)
                    {
                        adjugate.data[a, b] = -adjugate.data[a, b];
                        adjugate.data[b, a] = -adjugate.data[b, a];
                    }
                }
            }

            adjugate = adjugate.GetTranspose();
            return adjugate * (1 / GetDeterminant());
        }

        public Vector3_64 GetRow(int row) 
        {
            return new Vector3_64(data[0, row], data[1, row], data[2, row]); 
        }
        public Vector3_64 GetColumn(int col)
        {
            return new Vector3_64(data[col, 0], data[col, 1], data[col, 2]);
        }

        public void SetRow(int row, Vector3_64 v3) 
        {
            for (int a = 0; a < 3; a++)
            {
                data[a, row] = v3[a];
            } 
        }
        public void SetColumn(int col, Vector3_64 v3)
        {
            for (int i = 0; i < 3; i++)
            {
                data[col, i] = v3[i];
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int x = 0; x < 3; x++)
            { 
                for (int y = 0; y < 3; y++)
                    sb.Append($"{data[y, x]} ");
                sb.Append('\n');
            }
            return sb.ToString();
        }

        #region Operators
        public static Matrix3 operator +(in Matrix3 m1, in Matrix3 m2)
        {
            Matrix3 retval = new Matrix3();

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    retval.data[x, y] = m1.data[x,y]+ m2.data[x, y];

            return retval;
        }

        public static Matrix3 operator -(in Matrix3 m1, in Matrix3 m2)
        {

            Matrix3 retval = new Matrix3();

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    retval.data[x, y] = m1.data[x, y] - m2.data[x, y];

            return retval;
        }

        public static Matrix3 operator *(in Matrix3 m1, in Matrix3 m2)
        {
            Vector3_64 rowData = Vector3_64.Zero;
            Vector3_64 colData = Vector3_64.Zero;
            Matrix3 retval = new Matrix3();

            for (int x = 0; x < 3; x++)
            {
                //Gets Column data from first
                colData = m1.GetColumn(x);
                
                for (int y = 0; y < 3; y++)
                {
                    //Gets row data from second
                    rowData = m2.GetRow(y);
                    retval.data[x, y] = rowData.Dot(colData);
                }
            }           

            return retval;
            
            //Vector3_64 rowData;
            //Vector3_64 colData;
            //Matrix3 retval;

            //for (int a = 0; a < 3; a++)
            //{
            //    //Gets Column data from first
            //    for (int c = 0; c < 3; c++) { colData[c] = data[a, c]; }

            //    for (int b = 0; b < 3; b++)
            //    {
            //        //Gets row data from second
            //        for (int c = 0; c < 3; c++) { rowData[c] = m.data[c, b]; }
            //        retval.data[a, b] = rowData.getDot(colData);
            //    }
            //}

            //return retval;
        }

        public static Matrix3 operator *(in double f, in Matrix3 m)
        {
            Matrix3 retval = new Matrix3(m);

            for (int a = 0; a < 3; a++)
                for (int b = 0; b < 3; b++)
                    retval.data[a, b] *= f;

            return retval;
        }

        public static Matrix3 operator *(in Matrix3 m, in double f)
        {
            Matrix3 retval = new Matrix3(m);

            for (int a = 0; a < 3; a++)
                for (int b = 0; b < 3; b++)
                    retval.data[a, b] *= f;

            return retval;
        }
        #endregion
    }
}
