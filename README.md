# 🎲 MonoLogic

MonoLogic is a Monopoly-inspired board game application that combines classic gameplay with mathematical analysis. The project allows users not only to play the game, but also to understand probabilities, strategy, and decision-making using data.

---

## 📌 Features

* 🎮 Playable Monopoly-style game
* 🤖 AI player with automatic decisions
* 👥 Support for multiple players
* 💰 Property buying, rent, tax, and jail system
* 🎲 Dice-based movement

### 📊 Analysis Features

* **Monte Carlo Simulation**
  Simulates thousands of turns to estimate how often each square is visited

* **Markov Chain Analysis**
  Calculates long-term probabilities of landing on each square

* **Heatmap Visualization**
  Shows frequently visited squares (green → low, red → high)

* **Entropy Calculation**
  Measures how balanced or concentrated the board probabilities are

* **ROI Ranking**
  Identifies the best properties to buy based on probability, rent, and price

---

## 🎯 Project Goals

* Combine a board game with mathematical modeling
* Help users understand probability in a practical way
* Provide insights for better in-game decisions
* Demonstrate simulation and Markov chain concepts

---

## 🛠️ Technologies Used

* C#
* WPF (Windows Presentation Foundation)
* .NET
* Visual Studio

---

## ▶️ How to Run

### Option 1 — Run from Visual Studio

1. Open the solution file
2. Build the project
3. Press **F5**

### Option 2 — Run Executable

1. Go to:

   ```
   bin/Release/app.publish
   ```
2. Run:

   ```
   MonoLogic.exe
   ```

---
### Option 3 — Download Executable

Download the executable file from the link below:

👉 [Download MonoLogic]: [https://drive.google.com/drive/folders/1gaq_f6SekGcWWUbn5TO69xpHRNlk7Cyf?usp=sharing](url)

1. Extract the files if needed  
2. Run `MonoLogic.exe`  
3. If a security warning appears, click **More Info → Run Anyway**
## 🎮 How to Use

1. Choose number of players and AI settings
2. Click **Start Game**
3. Use **Roll Dice** to play turns
4. Buy properties or skip
5. View player summaries below the board
6. Run analysis tools from the control panel

---

## 📊 Understanding the Analysis

* **Heatmap** shows which squares are most visited
* **Markov Analysis** gives stable long-term probabilities
* **Entropy** shows how evenly probabilities are distributed
* **ROI Ranking** helps identify best investments

---

## 🧠 Project Structure

* `Models/` → game logic (Player, Board, Square)
* `MainWindow.xaml` → UI layout
* `MainWindow.xaml.cs` → game logic + UI interaction
* Analysis methods → simulation, Markov, entropy, ROI

---

## 🚧 Current Status

* Core gameplay implemented
* AI behavior working
* All analysis features implemented
* UI functional (visual improvements possible)

---

## 🔮 Future Improvements

* Better UI design and animations
* Full accessibility improvements
* More advanced AI strategies
* Save/load game functionality
* Multiplayer networking

---

## 👤 Author

Individual project developed as part of a university coursework by Lili Baghoyan

---

## 📄 License

This project is for educational purposes.
