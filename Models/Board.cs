using System.Collections.Generic;

namespace MonoLogic.Models
{
    public class Board
    {
        public List<Square> Squares { get; set; }

        public Board()
        {
            Squares = CreateBoard();
        }

        private List<Square> CreateBoard()
        {
            var squares = new List<Square>
            {
                new Square(0, "GO", SquareType.Go),
                new Square(1, "Mediterranean Avenue", SquareType.Property){ Price = 60, Rent = 2 },
                new Square(2, "Community Chest", SquareType.CommunityChest),
                new Square(3, "Baltic Avenue", SquareType.Property){ Price = 60, Rent = 4 },
                new Square(4, "Income Tax", SquareType.Tax),
                new Square(5, "Reading Railroad", SquareType.Railroad){ Price = 200, Rent = 25 },
                new Square(6, "Oriental Avenue", SquareType.Property){ Price = 100, Rent = 6 },
                new Square(7, "Chance", SquareType.Chance),
                new Square(8, "Vermont Avenue", SquareType.Property){ Price = 100, Rent = 6 },
                new Square(9, "Connecticut Avenue", SquareType.Property){ Price = 120, Rent = 8 },
                new Square(10, "Jail", SquareType.Jail),
                new Square(11, "St. Charles Place", SquareType.Property){ Price = 140, Rent = 10 },
                new Square(12, "Electric Company", SquareType.Utility){ Price = 150, Rent = 10 },
                new Square(13, "States Avenue", SquareType.Property){ Price = 140, Rent = 10 },
                new Square(14, "Virginia Avenue", SquareType.Property){ Price = 160, Rent = 12 },
                new Square(15, "Pennsylvania Railroad", SquareType.Railroad){ Price = 200, Rent = 25 },
                new Square(16, "St. James Place", SquareType.Property){ Price = 180, Rent = 14 },
                new Square(17, "Community Chest", SquareType.CommunityChest),
                new Square(18, "Tennessee Avenue", SquareType.Property){ Price = 180, Rent = 14 },
                new Square(19, "New York Avenue", SquareType.Property){ Price = 200, Rent = 16 },
                new Square(20, "Free Parking", SquareType.FreeParking),
                new Square(21, "Kentucky Avenue", SquareType.Property){ Price = 220, Rent = 18 },
                new Square(22, "Chance", SquareType.Chance),
                new Square(23, "Indiana Avenue", SquareType.Property){ Price = 220, Rent = 18 },
                new Square(24, "Illinois Avenue", SquareType.Property){ Price = 240, Rent = 20 },
                new Square(25, "B&O Railroad", SquareType.Railroad){ Price = 200, Rent = 25 },
                new Square(26, "Atlantic Avenue", SquareType.Property){ Price = 260, Rent = 22 },
                new Square(27, "Ventnor Avenue", SquareType.Property){ Price = 260, Rent = 22 },
                new Square(28, "Water Works", SquareType.Utility){ Price = 150, Rent = 10 },
                new Square(29, "Marvin Gardens", SquareType.Property){ Price = 280, Rent = 24 },
                new Square(30, "Go To Jail", SquareType.GoToJail),
                new Square(31, "Pacific Avenue", SquareType.Property){ Price = 300, Rent = 26 },
                new Square(32, "North Carolina Avenue", SquareType.Property){ Price = 300, Rent = 26 },
                new Square(33, "Community Chest", SquareType.CommunityChest),
                new Square(34, "Pennsylvania Avenue", SquareType.Property){ Price = 320, Rent = 28 },
                new Square(35, "Short Line Railroad", SquareType.Railroad){ Price = 200, Rent = 25 },
                new Square(36, "Chance", SquareType.Chance),
                new Square(37, "Park Place", SquareType.Property){ Price = 350, Rent = 35 },
                new Square(38, "Luxury Tax", SquareType.Tax),
                new Square(39, "Boardwalk", SquareType.Property){ Price = 400, Rent = 50 }
            };

            return squares;
        }
    }
}