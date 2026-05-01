using System;

namespace MonoLogic.GameLogic
{
    public class Dice
    {
        private Random random = new Random();

        public int Roll()
        {
            return random.Next(1, 7);
        }
    }
}