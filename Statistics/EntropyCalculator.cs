using System;

namespace MonoLogic.Statistics
{
    public class EntropyCalculator
    {
        public double CalculateEntropy(double[] probabilities)
        {
            double entropy = 0;

            foreach (var p in probabilities)
            {
                if (p > 0)
                    entropy -= p * Math.Log(p, 2);
            }

            return entropy;
        }
    }
}