using System;

namespace MonoLogic.Mathematics
{
    public class MarkovChain
    {
        private int size = 40;

        public double[] PowerIteration(double[,] matrix, int iterations = 1000)
        {
            double[] vector = new double[size];

            // start uniform distribution
            for (int i = 0; i < size; i++)
                vector[i] = 1.0 / size;

            for (int iter = 0; iter < iterations; iter++)
            {
                double[] newVector = new double[size];

                for (int j = 0; j < size; j++)
                {
                    for (int i = 0; i < size; i++)
                    {
                        newVector[j] += vector[i] * matrix[i, j];
                    }
                }

                vector = newVector;
            }

            return vector;
        }
    }
}