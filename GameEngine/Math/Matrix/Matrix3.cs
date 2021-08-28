using GameEngine.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    class Matrix3 : SquareMatrix
    {
        public Matrix3() : base(3) { }
        public Matrix3(Vector3 v3a, Vector3 v3b, Vector3 v3c, bool asColumns) : this()
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

        public Vector3 GetRow(int row)
        {
            return new Vector3(data[0, row], data[1, row], data[2, row]);
        }
        public Vector3 GetColumn(int col)
        {
            return new Vector3(data[col, 0], data[col, 1], data[col, 2]);
        }

        public void SetRow(int row, Vector3 v3)
        {
            for (int i = 0; i < 3; i++)
                data[row*3+i] = v3[i];
        }
        public void SetColumn(int col, Vector3 v3)
        {
            for (int i = 0; i < 3; i++)
                data[3 * i + col] = v3[i];
        }

        #region Operators

        #endregion
    }
}
