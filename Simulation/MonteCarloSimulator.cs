using System;
using MonoLogic.Models;
using MonoLogic.GameLogic;
using MonoLogic.Statistics;

namespace MonoLogic.Simulation
{
    public class MonteCarloSimulator
    {
        private Dice dice = new Dice();
        private Board board = new Board();

        public void RunSimulation(int steps, StatisticsManager stats)
        {
            int position = 0;

            for (int i = 0; i < steps; i++)
            {
                int roll = dice.Roll();
                position = (position + roll) % 40;

                // Handle Go To Jail
                if (board.Squares[position].Type == SquareType.GoToJail)
                    position = 10;

                stats.RecordVisit(position);
            }
        }
    }
}