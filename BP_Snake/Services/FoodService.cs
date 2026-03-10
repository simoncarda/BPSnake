using BPSnake.Models.GameCore;
using GridPoint = BPSnake.Models.GameCore.GridPoint;
using BPSnake.Configuration;

namespace BPSnake.Services
{
    internal sealed class FoodService
    {
        private readonly Random _random;

        public FoodItem CurrentFoodItem { get; private set; } = null!;

        public FoodService() : this(new Random()) { }

        // Konstruktor pro dependency injection (s vlastní Random instancí)
        public FoodService(Random random)
        {
            _random = random;
        }

        public void Reset()
        {
            CurrentFoodItem = null!; // Bude vzápětí vytvořen pomocí SpawnFood
        }

        /// <summary>
        /// Vytvoří nové jídlo na náhodné pozici na herní desce, přičemž zajišťuje, že se nenachází na pozici obsazené hadem nebo překážkami.
        /// Má 20% šanci na vytvoření bonusového jídla s hodnotou 10, jinak vytvoří standardní jídlo s hodnotou 5.
        /// </summary>
        public void SpawnFood(GameBoard board, Snake snake)
        {
            int x;
            int y;
            int value = (_random.Next(0, 10) < 8) ? GameSettings.NormalFoodScoreValue : GameSettings.BonusFoodScoreValue;
            do {
                x = _random.Next(0, board.Width);
                y = _random.Next(0, board.Height);
            } while (snake.Body.Contains(new GridPoint(x, y)));

            CurrentFoodItem = new FoodItem(new GridPoint(x, y), value);
        }

        /// <summary>
        /// Zpracuje událost snědení jídla, vrátí zisk skóre a informaci o tom, zda se má otevřít brána.
        /// </summary>
        public int EatCurrentFood(GameBoard board, Snake snake)
        {
            int score = CurrentFoodItem.ScoreValue;
            SpawnFood(board, snake);

            return score;
        }
    }
}
