using BP_Snake.Models.GameCore;
using GridPoint = BP_Snake.Models.GameCore.GridPoint;
using BP_Snake.Configuration;

namespace BP_Snake.Services
{
    internal sealed class FoodService
    {
        private readonly Random _random;

        // Konstruktor pro výchozí inicializaci s novou instancí Random
        public FoodService() : this(new Random())
        {
        }

        // Konstruktor pro injektování vlastní instance Random (pro testování nebo specifické scénáře) - contructor chaining pro Dependency Injection
        public FoodService(Random random)
        {
            _random = random;
        }

        /// <summary>
        /// Vytvoří nové jídlo na náhodné pozici na herní desce, přičemž zajišťuje, že se nenachází na pozici obsazené hadem nebo překážkami.
        /// Má 20% šanci na vytvoření bonusového jídla s hodnotou 10, jinak vytvoří standardní jídlo s hodnotou 5.
        /// </summary>
        /// <param name="board">Herní plocha, udává validní pozice, na kterých se může FoodItem vytvořit bez překrytí s překážkou. Udává hranice plochy pro tvorbu FoodItem</param>
        /// <param name="snake">Aktuální instance hada. Využitá pro zajištění toho, že se FoodItem nevytvoří v těle hada.</param>
        /// <returns>Novou instanci FoodItem</returns>
        public FoodItem CreateFoodItem(GameBoard board, Snake snake)
        {
            int x;
            int y;
            int value = (_random.Next(0, 10) < 8) ? GameSettings.NormalFoodScoreValue : GameSettings.BonusFoodScoreValue; // 20% šance na bonusové jídlo s hodnotou 10
            do {
                x = _random.Next(0, board.Width);
                y = _random.Next(0, board.Height);
            } while (snake.Body.Contains(new GridPoint(x, y)) || board.Obstacles.Contains(new GridPoint(x, y)));
            return new FoodItem(new GridPoint(x, y), value);
        }

        /// <summary>
        /// Zpracuje efekty po snědení jídla: zvýší počítadlo, rozhodne se, zda se otevře brána,
        /// a vrátí následující jídlo (nebo prázdné jídlo pokud se má brána otevřít).
        /// </summary>
        public FoodEatenResult HandleFoodEaten(GameBoard board, Snake snake, FoodItem currentFoodItem, int applesEatenInLevel)
        {
            int newApplesEaten = applesEatenInLevel + 1;
            bool shouldOpenGate = newApplesEaten >= GameSettings.ApplesToOpenGate;
            FoodItem? nextFoodItem = shouldOpenGate
                ? null
                : CreateFoodItem(board, snake);

            return new FoodEatenResult(currentFoodItem.ScoreValue, newApplesEaten, shouldOpenGate, nextFoodItem);
        }
    }

    /// <summary>
    /// Výsledek zpracování snědení jídla: kolik skóre bylo získáno, kolik jablek snědeno v úrovni, zda se otevírá brána a další jídlo.
    /// </summary>
    internal readonly record struct FoodEatenResult(int ScoreValue, int ApplesEatenInLevel, bool ShouldOpenGate, FoodItem? NextFoodItem);
}
