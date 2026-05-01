using System.Collections.Generic;

namespace MonoLogic.Statistics
{
    public class StatisticsManager
    {
        public Dictionary<int, int> VisitCounts { get; private set; }
        public int TotalVisits { get; private set; }

        public StatisticsManager(int boardSize)
        {
            VisitCounts = new Dictionary<int, int>();

            for (int i = 0; i < boardSize; i++)
                VisitCounts[i] = 0;

            TotalVisits = 0;
        }

        public void RecordVisit(int position)
        {
            VisitCounts[position]++;
            TotalVisits++;
        }

        public double GetProbability(int position)
        {
            if (TotalVisits == 0)
                return 0;

            return (double)VisitCounts[position] / TotalVisits;
        }
    }
}