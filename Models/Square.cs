namespace MonoLogic.Models
{
    public class Square
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public SquareType Type { get; set; }

        public int Price { get; set; }
        public int Rent { get; set; }

        public Player Owner { get; set; }
        public int Houses { get; set; } = 0;

        public Square(int position, string name, SquareType type)
        {
            Position = position;
            Name = name;
            Type = type;
        }
    }
}