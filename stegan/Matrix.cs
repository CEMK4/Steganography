using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stegan
{
    internal class Matrix
    {
        public int[,] array;
        int row, column;

        public int Row { get { return row; } }
        public int Column { get { return column; } }

        public Matrix(int row, int colunm)
        {
            this.row = row;
            this.column = colunm;
            array = new int[row, column];
        }

        public Matrix(int[,] array)
        {
            this.row = array.GetLength(0);
            this.column = array.GetLength(1);
            this.array = array;
        }

        public Matrix(int[] array)
        {
            this.row = 1;
            this.column = array.Length;
            this.array = new int[row, column];
            for (int i = 0; i < column; i++)
            {
                this.array[0,i] = array[i];
            }
        }

        public Matrix Transpose()
        {
            Matrix m = new Matrix(column, row);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    m.array[j, i] = array[i, j];
                }
            }

            return m;
        }

        public void TransposeMyself()
        {
            array = Transpose().array;
        }
            
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            if (m1.row != m2.row || m1.column != m2.column)
            {
                throw new Exception("Сложение невозможно");
            }

            Matrix m = new Matrix(m1.row, m1.column);

            for (int i = 0; i < m1.row; i++)
            {
                for (int j = 0; j < m1.column; j++)
                {
                    m.array[i, j] = m1.array[i, j] + m2.array[i, j];
                }
            }

            return m;
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            if (m1.row != m2.row || m1.column != m2.column)
            {
                throw new Exception("Вычитание невозможно");
            }

            Matrix m = new Matrix(m1.row, m1.column);

            for (int i = 0; i < m1.row; i++)
            {
                for (int j = 0; j < m1.column; j++)
                {
                    m.array[i, j] = m1.array[i, j] - m2.array[i, j];
                }
            }

            return m;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.column != m2.row)
            {
                throw new Exception("Умножение невозможно");
            }

            Matrix m = new Matrix(m1.row, m2.column);

            for (int i = 0; i < m1.row; i++)
            {
                for (int j = 0; j < m2.column; j++)
                {
                    int sum = 0;

                    for (int k = 0; k < m1.column; k++)
                    {
                        sum += m1.array[i, k] * m2.array[k, j];
                    }

                    m.array[i, j] = sum;
                }
            }

            return m;
        }

        public override string ToString()
        {
            string str = "";

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    str += array[i, j] + "\t";
                }
                str += "\n";
            }

            return str;
        }
    }
}
