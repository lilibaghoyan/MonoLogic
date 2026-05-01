using System;

namespace MonoLogic.Mathematics
{
    public class TransitionMatrix
    {
        public double[,] Matrix { get; private set; }
        private int size = 40;

        public TransitionMatrix()
        {
            Matrix = new double[size, size];
            BuildMatrix();
        }

        private void BuildMatrix()
        {
            // One 6-sided die → equal probabilities
            for (int i = 0; i < size; i++)
            {
                for (int dice = 1; dice <= 6; dice++)
                {
                    int next = (i + dice) % size;

                    // Handle Go To Jail
                    if (next == 30)
                        next = 10;

                    Matrix[i, next] += 1.0 / 6.0;
                }
            }
        }
    }
}