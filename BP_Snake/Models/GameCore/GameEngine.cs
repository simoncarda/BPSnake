using BPSnake.Configuration;
using BPSnake.Services;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Poskytuje základní funkcionalitu pro správu stavu hry.
    /// </summary>
    /// <remarks>
    /// Třída GameEngine je zodpovědná za inicializaci a řízení herní smyčky, správu aktuálního stavu hry.
    /// Tato třída publikuje události pro aktualizaci uživatelského rozhraní (UI)
    /// </remarks>
    internal class GameEngine(GameStateService gameStateService) : IGameEngine
    {
        // Stav hry (zvenčí pouze pro čtení díky get; a private set;)
        public Snake CurrentSnake { get; private set; } = null!;
        public GameBoard CurrentGameBoard { get; private set; } = null!;
        public GameState CurrentGameState => _gameStateService.CurrentState; 

        // Služby
        private readonly GameStateService _gameStateService = gameStateService;

        // UDÁLOST: aktualizace UI
        public event Func<Task>? OnStateChangedAsync;

        /// <summary>
        /// Notifikuje všechny přihlášené posluchače události OnStateChangedAsync, že došlo ke změně stavu hry, a
        /// umožňuje jim reagovat na tuto změnu (například aktualizací uživatelského rozhraní).
        /// </summary>
        private void NotifyStateChanged() => _ = OnStateChangedAsync?.Invoke();

        /// <summary>
        /// Inicializuje herní objekty a připraví hru ve stavu Menu.
        /// </summary>
        public void LoadNewGame()
        {
            CurrentSnake = new Snake();
            CurrentGameBoard = new GameBoard();
            
            _gameStateService.SetMenu();
            
            // Vyvolání události pro aktualizaci UI (načtení hry)
            NotifyStateChanged();
        }

        /// <summary>
        /// Načte nový herní stav a spustí herní smyčku pro začátek nové hry.
        /// </summary>
        public void StartNewGame()
        {
            LoadNewGame();
            _gameStateService.SetPlaying();
            NotifyStateChanged();
        }

        /// <summary>
        /// Zjistí CSS třídu pro konkrétní buňku herní plochy na základě aktuálního stavu hry.
        /// Přesunutí této logiky do GameEngine ukazuje studentům správné zapouzdření, kde UI
        /// už nezkoumá složité detaily v ostatních objektech, ale jen se zeptá "co je tady?".
        /// </summary>
        public string GetCellCssClass(int x, int y)
        {
            var p = new GridPoint(x, y);

            if (CurrentSnake?.Body.Count > 0 && CurrentSnake.Body[0] == p) {
                return "cell snake-head";
            }

            if (CurrentSnake?.Body.Contains(p) == true) {
                return "cell snake-body";
            }

            return "cell empty";
        }
    }
}
