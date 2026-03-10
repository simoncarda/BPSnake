using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Rozhraní (Interface) definující kontrakt pro herní engine.
    /// Použití rozhraní pomáhá ve výuce vysvětlit principy "Inversion of Control" a usnadňuje testování
    /// nebo budoucí výměnu za jinou implementaci.
    /// </summary>
    internal interface IGameEngine
    {
        // Vlastnosti jsou zvenčí (z pohledu komponent) pouze pro čtení (get),
        // což demonstruje princip zapouzdření (Encapsulation).
        Snake CurrentSnake { get; }
        GameBoard CurrentGameBoard { get; }
        FoodItem CurrentFoodItem { get; }
        int CurrentGameScore { get; }
        int CurrentLevel { get; }
        int TotalLevelsCompleted { get; }
        GameState CurrentGameState { get; }

        // Událost pro asynchronní překreslení Blazor komponent při změně stavu.
        event Func<Task>? OnStateChangedAsync;

        void LoadNewGame();
        void StartNewGame();
        void PauseGame();
        void ResumeGame();
        void GameOver();
        void ChangeDirection(Direction newDirection);

        /// <summary>
        /// Zjistí CSS třídu pro konkrétní buňku herní plochy na základě aktuálního stavu.
        /// Zjednodušuje logiku v uživatelském rozhraní (tím plní princip zapouzdření).
        /// </summary>
        string GetCellCssClass(int x, int y);
    }
}
