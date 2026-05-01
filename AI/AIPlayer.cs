using MonoLogic.Models;

namespace MonoLogic.AI
{
    public class AIPlayer
    {
        public bool ShouldBuyProperty(Player player, Square square)
        {
            // Basic strategy: keep safety money
            int safetyThreshold = 200;

            if (player.Money > square.Price + safetyThreshold)
                return true;

            return false;
        }
    }
}