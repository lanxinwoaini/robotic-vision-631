using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSim
{
    class Vector
    {
        private double[] array;
        public Vector(double[] vectorElements)
        {
            array = vectorElements;
        }

        public static double operator*(Vector a, Vector b)
        {
            double retValue = 0;
            if (a.array.Length == b.array.Length)
            {
                for (int arrayIndex = 0; arrayIndex < a.array.Length; ++arrayIndex)
                {
                    retValue += a.array[arrayIndex] * b.array[arrayIndex];
                }
            }
            else
                throw new IndexOutOfRangeException();
            return retValue;
        }

        public static Vector operator -(Vector a, Vector b)
        {
            double[] elements = null;
            if (a.array.Length == b.array.Length)
            {
                elements = new double[a.array.Length];
                for (int arrIndex = 0; arrIndex < a.array.Length; ++arrIndex)
                {
                    elements[arrIndex] = a.array[arrIndex] - b.array[arrIndex];
                }
            }
            else
                throw new IndexOutOfRangeException();
            return new Vector(elements);
        }
    }
}
