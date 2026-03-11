using BPSnake.Models.GameCore;
using GridPoint = BPSnake.Models.GameCore.GridPoint;
using BPSnake.Configuration;

namespace BPSnake.Services
{
    internal sealed class FoodService
    {
        private readonly Random _random;

        public FoodItem CurrentFoodItem { get; private set; } = null!;
        public int FoodEatenInLevel { get; private set; } = 0;

        public FoodService() : this(new Random()) { }

        // Konstruktor pro dependency injection (s vlastní Random instancí)
        public FoodService(Random random)
        {
            _random = random;
        }

        public void Reset()
        {
            FoodEatenInLevel = 0;
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
            // POZNÁMKA: Tento do-while cyklus je jednoduchý, ale pokud by had zabral 
            // 100 % plochy, cyklus bude nekonečný. Protože však plánujeme omezit množství jídla přidáním systému levelů,
            // tak je tento přístup pro účel aplikace dostačující.
            do {
                x = _random.Next(0, board.Width);
                y = _random.Next(0, board.Height);
            } while (snake.Body.Contains(new GridPoint(x, y)) || board.Obstacles.Contains(new GridPoint(x, y)));
            
            CurrentFoodItem = new FoodItem(new GridPoint(x, y), value);
        }

        /// <summary>
        /// Zpracuje událost konzumace potravy, ihned vygeneruje novou a vrátí získané skóre.
        /// </summary>
        public (int scoreValue, bool shouldOpenGate) EatCurrentFood(GameBoard board, Snake snake)
        {
            if (CurrentFoodItem == null) return (0, false);

            int score = CurrentFoodItem.ScoreValue;
            FoodEatenInLevel++;
            bool shouldOpenGate = FoodEatenInLevel >= GameSettings.FoodItemsToOpenGate;
            
            if (!shouldOpenGate) {
                SpawnFood(board, snake);
            } else {
                CurrentFoodItem = null!; // Čekáme na další level
            }

            return (score, shouldOpenGate);
        }
    }
}
