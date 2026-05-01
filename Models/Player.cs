using System.Windows.Controls;

namespace MonoLogic.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int Position { get; set; }
        public int Money { get; set; }
        public bool IsAI { get; set; }

        public int JailTurns { get; set; } = 0;
        public string LastAction { get; set; } = "";
        public TextBlock UI_Money { get; set; }
        public TextBlock UI_Action { get; set; }
        public TextBlock UI_Properties { get; set; }
        public Player(string name, bool isAI = false)
        {
            Name = name;
            Position = 0;
            Money = 1500;
            IsAI = isAI;
        }
    }
}