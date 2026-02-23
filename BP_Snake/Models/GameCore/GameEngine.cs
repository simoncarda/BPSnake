using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Poskytuje základní funkcionalitu pro správu stavu hry, včetně zpracování vstupu hráče, postupu hrou a interakcí mezi herními prvky.
    /// </summary>
    /// <remarks>
    /// Třída GameEngine je zodpovědná za inicializaci a řízení herní smyčky, správu aktuálního stavu hry a zprostředkování interakcí mezi hadem,
    /// položkami jídla a herní plochou. Zpracovává také přechody mezi stavy hry, jako je spuštění, pozastavení, obnovení a ukončení hry.
    /// Tato třída publikuje události pro aktualizaci uživatelského rozhraní (UI) a podporuje uvolňování prostředků prostřednictvím rozhraní IDisposable.
    /// </remarks>
    internal class GameEngine()
    {
        // Stav hry
        public Snake CurrentSnake { get; set; } = null!;
        public GameBoard CurrentGameBoard { get; set; } = null!;
        public GameState CurrentGameState { get; private set; } 

        // UDÁLOST: aktualizace UI
        public event Func<Task>? OnStateChangedAsync;

        /// <summary>
        /// Notifikuje všechny přihlášené posluchače události OnStateChangedAsync, že došlo ke změně stavu hry, a
        /// umožňuje jim reagovat na tuto změnu (například aktualizací uživatelského rozhraní).
        /// </summary>
        private void NotifyStateChanged()
        {
            _ = OnStateChangedAsync?.Invoke();
        }

        /// <summary>
        /// Inicializuje nový herní stav pro začátek nové hry.
        /// Resetuje všechny relevantní proměnné a připraví hru na nový start.
        /// </summary>
        public void LoadNewGame()
        {
            CurrentSnake = new Snake();
            CurrentGameBoard = new GameBoard();
            CurrentGameState = GameState.Menu;
            // Vyvolání události pro aktualizaci UI (načtení hry)
            NotifyStateChanged();
        }

        /// <summary>
        /// Načte nový herní stav a spustí herní smyčku pro začátek nové hry.
        /// </summary>
        public void StartNewGame()
        {
            LoadNewGame();
            CurrentGameState = GameState.Playing;
            NotifyStateChanged();
        }
    }
}
