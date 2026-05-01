using MonoLogic.Models;
using System.Collections.Generic;

namespace MonoLogic.Statistics
{
    public class ROIResult
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }

    public class ROIAnalyzer
    {
        public List<ROIResult> Analyze(Board board, double[] probabilities)
        {
            var results = new List<ROIResult>();

            foreach (var square in board.Squares)
            {
                if (square.Type == SquareType.Property && square.Price > 0)
                {
                    double prob = probabilities[square.Position];
                    double roi = (prob * square.Rent) / square.Price;

                    results.Add(new ROIResult
                    {
                        Name = square.Name,
                        Value = roi
                    });
                }
            }

            results.Sort((a, b) => b.Value.CompareTo(a.Value));

            return results;
        }
    }
}