using MonoLogic.AI;
using MonoLogic.GameLogic;
using MonoLogic.Mathematics;
using MonoLogic.Models;
using MonoLogic.Simulation;
using MonoLogic.Statistics;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MonoLogic
{
    public partial class MainWindow : Window
    {
        private List<Player> players;
        private int currentPlayerIndex = 0;
        private readonly Dice dice = new Dice();
        private readonly List<Border> boardSquares = new List<Border>();
        private readonly Board board = new Board();
        private readonly AIPlayer ai = new AIPlayer();
        private StatisticsManager stats = new StatisticsManager(40);
        private readonly MonteCarloSimulator simulator = new MonteCarloSimulator();
        private readonly TransitionMatrix transitionMatrix = new TransitionMatrix();
        private readonly MarkovChain markov = new MarkovChain();
        private readonly EntropyCalculator entropyCalc = new EntropyCalculator();
        private readonly ROIAnalyzer roiAnalyzer = new ROIAnalyzer();
        private double[] lastMarkovResult;
        private readonly List<Border> realBoardSquares = new List<Border>();
        private readonly Random random = new Random();
        private bool gameStarted = false;
        private bool _isDarkTheme = true;

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme(_isDarkTheme);
        }

        private void ApplyTheme(bool dark)
        {
            var res = this.Resources;

            if (dark)
            {
                // ── Dark palette  
                res["BrBgRoot"] = new SolidColorBrush(Color.FromRgb(26, 26, 46));   // #1a1a2e
                res["BrBgPanel"] = new SolidColorBrush(Color.FromRgb(22, 33, 62));   // #16213e
                res["BrBgCard"] = new SolidColorBrush(Color.FromRgb(15, 52, 96));   // #0f3460
                res["BrAccent"] = new SolidColorBrush(Color.FromRgb(233, 69, 96));   // #e94560
                res["BrTxtPrim"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));  // white
                res["BrTxtMuted"] = new SolidColorBrush(Color.FromRgb(170, 170, 204));  // #aaaacc
                res["BrTxtDim"] = new SolidColorBrush(Color.FromRgb(136, 136, 170));  // #8888aa
                res["BrSep"] = new SolidColorBrush(Color.FromRgb(42, 42, 74));   // #2a2a4a
                res["BrBtnAna"] = new SolidColorBrush(Color.FromRgb(15, 52, 96));   // #0f3460

                ThemeToggleBtn.Content = "☀  Light mode";
            }
            else
            {
                // ── Light palette 
                res["BrBgRoot"] = new SolidColorBrush(Color.FromRgb(242, 242, 247));  // #f2f2f7
                res["BrBgPanel"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));  // white
                res["BrBgCard"] = new SolidColorBrush(Color.FromRgb(235, 240, 250));  // #ebf0fa
                res["BrAccent"] = new SolidColorBrush(Color.FromRgb(193, 39, 67));   // #c12743
                res["BrTxtPrim"] = new SolidColorBrush(Color.FromRgb(20, 20, 40));   // near black
                res["BrTxtMuted"] = new SolidColorBrush(Color.FromRgb(90, 90, 120));  // #5a5a78
                res["BrTxtDim"] = new SolidColorBrush(Color.FromRgb(130, 130, 160));  // #8282a0
                res["BrSep"] = new SolidColorBrush(Color.FromRgb(210, 215, 230));  // #d2d7e6
                res["BrBtnAna"] = new SolidColorBrush(Color.FromRgb(210, 220, 240));  // #d2dcf0

                ThemeToggleBtn.Content = "🌙  Dark mode";
            }
        }

        private List<Brush> playerColors = new List<Brush>
        {
            Brushes.Green,
            Brushes.Blue,
            Brushes.Red,
            Brushes.Purple,
            Brushes.Orange
        };
        public MainWindow()
        {
            InitializeComponent();
            CreateBoard();
            UpdateUI();
            CreateRealBoard();
        }


        private void InitializePlayers(int count, bool useAI)
        {
            players = new List<Player>();

            for (int i = 0; i < count; i++)
            {
                bool isAI = useAI && i != 0;

                players.Add(new Player(
                    isAI ? $"AI {i}" : $"Player {i + 1}",
                    isAI
                ));
            }
        }
        private void CreateBoard()
        {
            BoardGrid.Children.Clear();
            boardSquares.Clear();

            for (int i = 0; i < 40; i++)
            {
                Border square = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.LightGray
                };

                TextBlock text = new TextBlock
                {
                    Text = board.Squares[i].Name,
                    FontSize = 10,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                square.Child = text;

                BoardGrid.Children.Add(square);
                boardSquares.Add(square);
            }
        }
        private void RollDice_Click(object sender, RoutedEventArgs e)
        {
            if (!gameStarted)
            {
                MessageBox.Show("Click Start Game first!");
                return;
            }

            Player currentPlayer = players[currentPlayerIndex];


            if (currentPlayer.JailTurns > 0)
            {
                currentPlayer.JailTurns--;

                currentPlayer.LastAction = $"In Jail ({currentPlayer.JailTurns} turns left)";

                NextPlayer();
                UpdateUI();
                return;
            }
            int roll = dice.Roll();
            DiceResultText.Text = "Rolled: " + roll;

            MovePlayer(currentPlayer, roll);
            stats.RecordVisit(currentPlayer.Position);
            HandleSquare(currentPlayer);
            CheckBankruptcy(currentPlayer);
            if (!players.Contains(currentPlayer))
            {
                UpdateUI();
                return;
            }
            UpdatePlayerPanels();
            NextPlayer();
            UpdateUI();
        }
        private void MovePlayer(Player player, int steps)
        {
            int oldPosition = player.Position;

            player.Position = (player.Position + steps) % 40;

            // PASSED GO
            if (player.Position < oldPosition)
            {
                player.Money += 200;
                player.LastAction += (string.IsNullOrEmpty(player.LastAction) ? "" : " | ") + "+$200 Passed GO";
            }
        }
        private void HandleSquare(Player player)
        {
            // Reset action at start
            player.LastAction = "";

            Square square = board.Squares[player.Position];

            //  PROPERTY BUY
            if ((square.Type == SquareType.Property || square.Type == SquareType.Railroad || square.Type == SquareType.Utility)
    && square.Owner == null)
            {
                bool shouldBuy = true;

                if (player.IsAI)
                    shouldBuy = ai.ShouldBuyProperty(player, square);

                else
                {
                    var result = MessageBox.Show(
                        $"Do you want to buy {square.Name} for ${square.Price}?",
                        "Buy Property",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    shouldBuy = (result == MessageBoxResult.Yes);
                }
                if (shouldBuy && player.Money >= square.Price)
                {
                    square.Owner = player;
                    player.Money -= square.Price;
                    player.LastAction = $"Bought {square.Name} (-${square.Price})";
                }
                else
                {
                    player.LastAction = "Skipped buying property";
                }
            }

            //  PAY RENT

            else if ((square.Type == SquareType.Property || square.Type == SquareType.Railroad || square.Type == SquareType.Utility)
    && square.Owner != null && square.Owner != player)
            {
                player.Money -= square.Rent;
                square.Owner.Money += square.Rent;

                player.LastAction = $"Paid ${square.Rent} rent to {square.Owner.Name}";
            }

            // TAX
            else if (square.Type == SquareType.Tax)
            {
                player.Money -= 100;
                player.LastAction = "-$100 Tax";
            }

            //  CHANCE / COMMUNITY CHEST
            else if (square.Type == SquareType.Chance || square.Type == SquareType.CommunityChest)
            {
                HandleChance(player);
            }

            //  GO TO JAIL
            else if (square.Type == SquareType.GoToJail)
            {
                player.Position = 10;
                player.JailTurns = 3;
                player.LastAction = "Sent to Jail (3 turns)";
            }

            //  DEFAULT
            else
            {
                if (string.IsNullOrEmpty(player.LastAction))
                    player.LastAction = "No action";
            }

            //  BANKRUPTCY CHECK
            if (player.Money < 0)
            {
                player.LastAction += " | BANKRUPT!";
            }
        }

        private void NextPlayer()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        }



        private string GetPlayerPropertiesText(Player player)
        {
            var owned = new List<string>();

            foreach (var square in board.Squares)
            {
                if (square.Owner == player)
                    owned.Add(square.Name);
            }

            if (owned.Count == 0)
                return "No properties";

            return "Properties:\n" + string.Join(", ", owned);
        }



        private void UpdatePlayerPanels()
        {
            if (players == null) return;

            foreach (var player in players)
            {
                player.UI_Money.Text = $"Money: ${player.Money}";
                player.UI_Action.Text = player.LastAction ?? "";
                player.UI_Properties.Text = GetPlayerPropertiesText(player);
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            int count = int.Parse(((ComboBoxItem)PlayerCountBox.SelectedItem).Content.ToString());
            bool useAI = UseAIBox.IsChecked == true;

            InitializePlayers(count, useAI);
            BuildPlayerPanels();

            gameStarted = true;

            SetupPanel.Visibility = Visibility.Collapsed;

            FinishGameButton.Visibility = Visibility.Visible;

            UpdatePlayerPanels();
            UpdateUI();
        }

        private void FinishGame_Click(object sender, RoutedEventArgs e)
        {
            players = null;
            currentPlayerIndex = 0;
            gameStarted = false;

            PlayersPanel.Items.Clear();

            foreach (var square in boardSquares)
                square.Background = Brushes.LightGray;

            DiceResultText.Text = "";
            CurrentPlayerText.Text = "";

            SetupPanel.Visibility = Visibility.Visible;

            FinishGameButton.Visibility = Visibility.Collapsed;
        }



        private void UpdateUI()
        {
            if (players == null || players.Count == 0)
                return;

            foreach (var square in boardSquares)
                square.Background = Brushes.LightGray;


            for (int i = 0; i < players.Count; i++)
            {
                var color = playerColors[i % playerColors.Count];
                boardSquares[players[i].Position].Background = color;
            }

            CurrentPlayerText.Text = "Current: " + players[currentPlayerIndex].Name;
            UpdateRealBoardPlayers();
        }
        private void UpdateHeatmap()
        {

            double maxProb = 0;

            // find maximum probability
            for (int i = 0; i < 40; i++)
            {
                double p = stats.GetProbability(i);
                if (p > maxProb)
                    maxProb = p;
            }

            // avoid division by zero
            if (maxProb == 0) maxProb = 1;

            // apply normalized colors
            for (int i = 0; i < 40; i++)
            {
                double p = stats.GetProbability(i);

                double normalized = p / maxProb;

                byte red = (byte)(normalized * 255);
                byte green = (byte)((1 - normalized) * 255);

                boardSquares[i].Background =
                    new SolidColorBrush(Color.FromRgb(red, green, 0));

                ((TextBlock)boardSquares[i].Child).Text =
                    board.Squares[i].Name + $"\nP={p:F3}";
            }
        }
        private void RunSimulation_Click(object sender, RoutedEventArgs e)
        {
            stats = new StatisticsManager(40); // reset stats

            simulator.RunSimulation(10000, stats);


            MessageBox.Show("Simulation complete!");

            UpdateHeatmap();
        }
        private void ApplyMarkovHeatmap(double[] probabilities)
        {
            double max = 0;

            for (int i = 0; i < probabilities.Length; i++)
                if (probabilities[i] > max)
                    max = probabilities[i];

            if (max == 0) max = 1;

            for (int i = 0; i < probabilities.Length; i++)
            {
                double normalized = probabilities[i] / max;

                byte red = (byte)(normalized * 255);
                byte green = (byte)((1 - normalized) * 255);

                boardSquares[i].Background =
                    new SolidColorBrush(Color.FromRgb(red, green, 0));

                ((TextBlock)boardSquares[i].Child).Text =
       board.Squares[i].Name + $"\nP={probabilities[i]:F3}";
            }
        }

        private void RunMarkov_Click(object sender, RoutedEventArgs e)
        {
            lastMarkovResult = markov.PowerIteration(transitionMatrix.Matrix);

            ApplyMarkovHeatmap(lastMarkovResult);

            MessageBox.Show("Markov analysis complete!");
        }
        private void Entropy_Click(object sender, RoutedEventArgs e)
        {
            if (lastMarkovResult == null)
            {
                MessageBox.Show("Run Markov first!");
                return;
            }

            // Calculate entropy
            double entropy = entropyCalc.CalculateEntropy(lastMarkovResult);

            // Calculate maximum entropy
            int n = lastMarkovResult.Length;
            double maxEntropy = System.Math.Log(n, 2); // log2(n)

            // Calculate percentage
            double efficiency = (entropy / maxEntropy) * 100;

            // Show result
            MessageBox.Show(
                $"Entropy: {entropy:F4} bits\n" +
                $"Max Entropy: {maxEntropy:F4} bits\n" +
                $"Normalized Entropy: {efficiency:F2}%",
                "Entropy Analysis"
            );
        }


        private void ROI_Click(object sender, RoutedEventArgs e)
        {
            if (lastMarkovResult == null)
            {
                MessageBox.Show("Run Markov first!");
                return;
            }

            var results = roiAnalyzer.Analyze(board, lastMarkovResult);

            string output = "Top Properties by ROI:\n\n";

            for (int i = 0; i < results.Count && i < 10; i++)
            {
                output += $"{i + 1}. {results[i].Name} → {results[i].Value:F4}\n";
            }

            MessageBox.Show(output);
        }

        private (int, int) GetRealBoardPosition(int i)
        {
            if (i >= 0 && i <= 9)
                return (10, 10 - i);             // bottom row (right → left)

            if (i >= 10 && i <= 19)
                return (10 - (i - 10), 0);       // left side (bottom → top)

            if (i >= 20 && i <= 29)
                return (0, i - 20);              // top row (left → right)

            if (i >= 30 && i <= 39)
                return (i - 30, 10);             // right side (top → bottom)

            return (0, 0);
        }

        private void ApplyRealMonopolyStyle(Border square, Square s)
        {
            Brush color = Brushes.Beige;

            if (s.Type == SquareType.Property)
            {
                if (s.Price <= 100) color = Brushes.SaddleBrown;     // Brown
                else if (s.Price <= 140) color = Brushes.LightBlue;  // Light Blue
                else if (s.Price <= 180) color = Brushes.Magenta;    // Pink
                else if (s.Price <= 220) color = Brushes.Orange;     // Orange
                else if (s.Price <= 260) color = Brushes.Red;        // Red
                else if (s.Price <= 300) color = Brushes.Yellow;     // Yellow
                else if (s.Price <= 350) color = Brushes.Green;      // Green
                else color = Brushes.DarkBlue;                       // Dark Blue
            }
            else if (s.Type == SquareType.Railroad)
                color = Brushes.Black;
            else if (s.Type == SquareType.Utility)
                color = Brushes.LightGray;
            else if (s.Type == SquareType.Tax)
                color = Brushes.LightCoral;
            else if (s.Type == SquareType.Go)
                color = Brushes.LightGreen;
            else if (s.Type == SquareType.Jail)
                color = Brushes.OrangeRed;
            else if (s.Type == SquareType.GoToJail)
                color = Brushes.DarkRed;
            else if (s.Type == SquareType.Chance)
                color = Brushes.Gold;
            else if (s.Type == SquareType.CommunityChest)
                color = Brushes.LightSkyBlue;

            square.Background = color;
        }



        private void CheckBankruptcy(Player player)
        {
            if (player.Money < 0)
            {
                MessageBox.Show($"{player.Name} is BANKRUPT!");

                // remove player
                players.Remove(player);

                // reset index safely
                currentPlayerIndex = 0;

                // check win condition
                if (players.Count == 1)
                {
                    MessageBox.Show($"{players[0].Name} WINS!");
                    Application.Current.Shutdown();
                }
            }
        }
        private void CreateRealBoard()
        {
            RealBoardGrid.Children.Clear();
            realBoardSquares.Clear();

            // Classic Monopoly green background
            RealBoardGrid.Background = new SolidColorBrush(Color.FromRgb(200, 230, 201));

            for (int i = 0; i < 40; i++)
            {
                Square sq = board.Squares[i];
                var (row, col) = GetRealBoardPosition(i);

                bool isCorner = (i == 0 || i == 10 || i == 20 || i == 30);
                bool isLeftCol = (!isCorner && col == 0);
                bool isRightCol = (!isCorner && col == 10);
                bool isTopRow = (!isCorner && row == 0);
                bool isBottomRow = (!isCorner && row == 10);

                // ── Outer border  
                Border square = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                    BorderThickness = new Thickness(1),
                    Background = Brushes.White,
                    CornerRadius = isCorner ? new CornerRadius(6) : new CornerRadius(2),
                    Margin = new Thickness(1),
                    ClipToBounds = true
                };

                // ── Inner layout 
                Grid innerGrid = new Grid();

                // Corner squares: two rows — icon area + label
                if (isCorner)
                {
                    innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                    TextBlock icon = new TextBlock
                    {
                        Text = GetCornerIcon(sq),
                        FontSize = 22,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 0)
                    };
                    Grid.SetRow(icon, 0);

                    TextBlock label = new TextBlock
                    {
                        Text = sq.Name.ToUpper(),
                        FontFamily = new FontFamily("Georgia"),
                        FontSize = 7,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = GetCornerForeground(sq),
                        Margin = new Thickness(2)
                    };
                    Grid.SetRow(label, 1);

                    innerGrid.Children.Add(icon);
                    innerGrid.Children.Add(label);
                }
                else
                {

                    StackPanel panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

                    Brush stripColor = GetPropertyColor(sq);
                    if (stripColor != Brushes.Transparent)
                    {
                        Border strip = new Border
                        {
                            Height = 10,
                            Background = stripColor,
                            CornerRadius = new CornerRadius(1, 1, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };
                        panel.Children.Add(strip);
                    }

                    string icon = GetSquareIcon(sq);
                    if (!string.IsNullOrEmpty(icon))
                    {
                        TextBlock iconBlock = new TextBlock
                        {
                            Text = icon,
                            FontSize = 11,
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 1, 0, 0)
                        };
                        panel.Children.Add(iconBlock);
                    }

                    // Name
                    TextBlock name = new TextBlock
                    {
                        Text = FormatSquareName(sq.Name),
                        FontFamily = new FontFamily("Georgia"),
                        FontSize = isCorner ? 7 : 6,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(1, 1, 1, 0)
                    };
                    panel.Children.Add(name);

                    // Price
                    if (sq.Price > 0)
                    {
                        TextBlock price = new TextBlock
                        {
                            Text = $"${sq.Price}",
                            FontSize = 5,
                            FontStyle = FontStyles.Italic,
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80))
                        };
                        panel.Children.Add(price);
                    }

                    // Rotate side columns so text reads toward the board center
                    if (isLeftCol)
                        panel.LayoutTransform = new RotateTransform(90);
                    else if (isRightCol)
                        panel.LayoutTransform = new RotateTransform(-90);

                    innerGrid.Children.Add(panel);
                }

                // Apply special background colors
                ApplySquareBackground(square, sq);

                square.Child = innerGrid;

                // Debugging guard (keep your original check)
                if (row < 0 || col < 0)
                    MessageBox.Show($"Error at index {i}: row={row}, col={col}");

                Grid.SetRow(square, row);
                Grid.SetColumn(square, col);

                RealBoardGrid.Children.Add(square);
                realBoardSquares.Add(square);
            }
        }

        private string FormatSquareName(string name)
        {
            // Abbreviate common long words
            return name
                .Replace("Avenue", "Ave")
                .Replace("Railroad", "RR")
                .Replace("Community Chest", "Comm.\nChest")
                .Replace("Pennsylvania", "Penn.")
                .Replace("Connecticut", "Conn.")
                .Replace("Mediterranean", "Medit.")
                .Replace("North Carolina", "N. Carolina");
        }


        private Brush GetCornerForeground(Square s)
        {
            switch (s.Type)
            {
                case SquareType.Go: return new SolidColorBrush(Color.FromRgb(198, 40, 40)); // red
                case SquareType.GoToJail: return new SolidColorBrush(Color.FromRgb(198, 40, 40));
                case SquareType.Jail: return new SolidColorBrush(Color.FromRgb(230, 100, 0)); // orange
                case SquareType.FreeParking: return new SolidColorBrush(Color.FromRgb(46, 125, 50)); // green
                default: return Brushes.Black;
            }
        }

        private string GetCornerIcon(Square s)
        {
            switch (s.Type)
            {
                case SquareType.Go: return "▶";
                case SquareType.Jail: return "⛓";
                case SquareType.FreeParking: return "🅿";
                case SquareType.GoToJail: return "🚔";
                default: return "";
            }
        }
        private string GetSquareIcon(Square s)
        {
            switch (s.Type)
            {
                case SquareType.Railroad: return "🚂";
                case SquareType.Utility:
                    return s.Name.Contains("Electric") ? "⚡" : "💧";
                case SquareType.Chance: return "?";
                case SquareType.CommunityChest: return "📦";
                case SquareType.Tax: return "💸";
                default: return "";
            }
        }
        private void ApplySquareBackground(Border square, Square s)
        {
            switch (s.Type)
            {
                case SquareType.Go:
                    square.Background = new SolidColorBrush(Color.FromRgb(255, 249, 196)); // warm cream
                    break;
                case SquareType.Jail:
                    square.Background = new SolidColorBrush(Color.FromRgb(255, 204, 128)); // orange-tan
                    break;
                case SquareType.FreeParking:
                    square.Background = new SolidColorBrush(Color.FromRgb(200, 230, 201)); // green (same as board)
                    break;
                case SquareType.GoToJail:
                    square.Background = new SolidColorBrush(Color.FromRgb(239, 154, 154)); // soft red
                    break;
                case SquareType.Chance:
                    square.Background = new SolidColorBrush(Color.FromRgb(255, 243, 205)); // pale gold
                    break;
                case SquareType.CommunityChest:
                    square.Background = new SolidColorBrush(Color.FromRgb(209, 236, 241)); // pale blue
                    break;
                case SquareType.Tax:
                    square.Background = new SolidColorBrush(Color.FromRgb(248, 215, 218)); // pale red
                    break;
                case SquareType.Railroad:
                    square.Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)); // light gray
                    break;
                case SquareType.Utility:
                    square.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // pale green
                    break;
                default:
                    square.Background = Brushes.White;
                    break;
            }
        }
        private void HandleChance(Player player)
        {
            int eventType = random.Next(0, 5);

            switch (eventType)
            {
                case 0:
                    player.Money += 100;
                    player.LastAction = "+$100 Bank reward";
                    break;

                case 1:
                    player.Money -= 50;
                    player.LastAction = "-$50 Fine";
                    break;

                case 2:
                    player.Position = (player.Position + 3) % 40;
                    player.LastAction = "Move forward 3";
                    break;

                case 3:
                    player.Position = (player.Position - 2 + 40) % 40;
                    player.LastAction = "Move back 2";
                    break;

                case 4:
                    player.Position = 10;
                    player.JailTurns = 3;
                    player.LastAction = "Go to Jail";
                    break;
            }
        }

        private void UpdateRealBoardPlayers()
        {
            foreach (var sq in realBoardSquares)
            {
                if (sq.Child is Grid g)
                {
                    UIElement toRemove = null;
                    foreach (UIElement el in g.Children)
                    {
                        if (el is Border tb && (string)tb.Tag == "tokens")
                        {
                            toRemove = el;
                            break;
                        }
                    }
                    if (toRemove != null)
                        g.Children.Remove(toRemove);
                }
            }

            // Group players by position so multiple tokens show side-by-side
            var byPosition = new Dictionary<int, List<int>>(); // position → list of player indices
            for (int i = 0; i < players.Count; i++)
            {
                int pos = players[i].Position;
                if (!byPosition.ContainsKey(pos))
                    byPosition[pos] = new List<int>();
                byPosition[pos].Add(i);
            }

            Brush[] tokenFills = new Brush[]
            {
        new SolidColorBrush(Color.FromRgb(76,  175, 80)),   // green
        new SolidColorBrush(Color.FromRgb(33,  150, 243)),  // blue
        new SolidColorBrush(Color.FromRgb(244, 67,  54)),   // red
        new SolidColorBrush(Color.FromRgb(156, 39, 176)),   // purple
        new SolidColorBrush(Color.FromRgb(255, 152,  0))    // orange
            };

            foreach (var kvp in byPosition)
            {
                var sqBorder = realBoardSquares[kvp.Key];

                // Ensure the square child is a Grid so we can layer the tokens on top
                if (!(sqBorder.Child is Grid outerGrid))
                {
                    UIElement existing = sqBorder.Child;
                    sqBorder.Child = null;
                    outerGrid = new Grid();
                    outerGrid.Children.Add(existing);
                    sqBorder.Child = outerGrid;
                }

                // Build a small WrapPanel of Ellipse tokens
                WrapPanel tokenPanel = new WrapPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(1),
                    Tag = "tokens"          // so we can find & remove it next turn
                };

                foreach (int pi in kvp.Value)
                {
                    Ellipse token = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = tokenFills[pi % tokenFills.Length],
                        Stroke = Brushes.White,
                        StrokeThickness = 1,
                        Margin = new Thickness(1)
                    };

                    // Tooltip shows player name
                    token.ToolTip = players[pi].Name;

                    tokenPanel.Children.Add(token);
                }

                // Wrap the WrapPanel in a Border so we can tag it
                Border tokenLayer = new Border { Tag = "tokens" };
                tokenLayer.Child = tokenPanel;

                outerGrid.Children.Add(tokenLayer);
            }
        }

        private Brush GetPropertyColor(Square s)
        {
            if (s.Type == SquareType.Railroad)
                return new SolidColorBrush(Color.FromRgb(50, 50, 50));

            if (s.Type == SquareType.Utility)
                return new SolidColorBrush(Color.FromRgb(180, 210, 180));

            if (s.Type != SquareType.Property)
                return Brushes.Transparent;

            // Map by price bracket → correct Monopoly color group
            if (s.Price <= 60) return new SolidColorBrush(Color.FromRgb(139, 69, 19));  // Brown
            if (s.Price <= 120) return new SolidColorBrush(Color.FromRgb(173, 216, 230)); // Light Blue
            if (s.Price <= 160) return new SolidColorBrush(Color.FromRgb(255, 105, 180)); // Pink / Magenta
            if (s.Price <= 200) return new SolidColorBrush(Color.FromRgb(255, 140, 0)); // Orange
            if (s.Price <= 240) return new SolidColorBrush(Color.FromRgb(220, 50, 50)); // Red
            if (s.Price <= 280) return new SolidColorBrush(Color.FromRgb(255, 215, 0)); // Yellow
            if (s.Price <= 320) return new SolidColorBrush(Color.FromRgb(0, 160, 60)); // Green
            return new SolidColorBrush(Color.FromRgb(0, 53, 128)); // Dark Blue
        }

        private void BuildPlayerPanels()
        {
            PlayersPanel.Items.Clear();

            Brush[] tokenFills = new Brush[]
            {
        new SolidColorBrush(Color.FromRgb(76,  175, 80)),   // green
        new SolidColorBrush(Color.FromRgb(33,  150, 243)),  // blue
        new SolidColorBrush(Color.FromRgb(244,  67, 54)),   // red
        new SolidColorBrush(Color.FromRgb(156,  39, 176)),  // purple
        new SolidColorBrush(Color.FromRgb(255, 152,  0))    // orange
            };

            for (int idx = 0; idx < players.Count; idx++)
            {
                var player = players[idx];
                Brush accent = tokenFills[idx % tokenFills.Length];

                // Card border
                Border card = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),  // #0f3460
                    BorderBrush = accent,
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 0, 10, 0),
                    MinWidth = 160
                };

                StackPanel panel = new StackPanel();

                // Header row: color dot + name
                StackPanel header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
                Ellipse dot = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = accent,
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                TextBlock name = new TextBlock
                {
                    Text = player.Name,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 13
                };
                if (player.IsAI)
                {
                    TextBlock aiTag = new TextBlock
                    {
                        Text = " 🤖",
                        Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 204)),
                        FontSize = 11,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    header.Children.Add(dot);
                    header.Children.Add(name);
                    header.Children.Add(aiTag);
                }
                else
                {
                    header.Children.Add(dot);
                    header.Children.Add(name);
                }
                panel.Children.Add(header);

                // Money
                TextBlock money = new TextBlock
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 220, 100)),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };

                // Last action
                TextBlock action = new TextBlock
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 204)),
                    FontSize = 10,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 160,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                // Properties owned
                TextBlock props = new TextBlock
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(130, 170, 220)),
                    FontSize = 10,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 160,
                    Width = 150,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                panel.Children.Add(money);
                panel.Children.Add(action);
                panel.Children.Add(props);

                card.Child = panel;

                player.UI_Money = money;
                player.UI_Action = action;
                player.UI_Properties = props;

                PlayersPanel.Items.Add(card);
            }
        }
    }
}
