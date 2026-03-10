using BPSnake.Configuration;
using BPSnake.Services;

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
    internal class GameEngine(
        GameLoopService gameLoopService, 
        CollisionService collisionService, 
        FoodService foodService,
        GameStateService gameStateService,
        ScoreService scoreService) : IGameEngine, IDisposable
    {
        // Stav hry (zvenčí pouze pro čtení díky get; a private set;)
        public Snake CurrentSnake { get; private set; } = null!;
        public GameBoard CurrentGameBoard { get; private set; } = null!;
        public FoodItem CurrentFoodItem => _foodService.CurrentFoodItem;
        public int CurrentGameScore => _scoreService.CurrentScore;
        public GameState CurrentGameState => _gameStateService.CurrentState; 

        // Služby
        private readonly GameLoopService _gameLoopService = gameLoopService;
        private readonly CollisionService _collisionService = collisionService;
        private readonly FoodService _foodService = foodService;
        private readonly GameStateService _gameStateService = gameStateService;
        private readonly ScoreService _scoreService = scoreService;

        // UDÁLOST: aktualizace UI
        public event Func<Task>? OnStateChangedAsync;

        private int _currentGameSpeed => GameSettings.BaseGameSpeed;
        private bool _directionChangedInCurrentTick = false; // Pomocná proměnná pro zamezení více změn směru během jednoho ticku
        private int _growBuffer = 0;

        /// <summary>
        /// Notifikuje všechny přihlášené posluchače události OnStateChangedAsync, že došlo ke změně stavu hry, a
        /// umožňuje jim reagovat na tuto změnu (například aktualizací uživatelského rozhraní).
        /// </summary>
        private void NotifyStateChanged() => _ = OnStateChangedAsync?.Invoke();

        /// <summary>
        /// Inicializuje nový herní stav pro začátek nové hry.
        /// Resetuje všechny relevantní proměnné a připraví hru na nový start.
        /// </summary>
        public void LoadNewGame()
        {
            StopGameLoop(); // Pro jistotu zastavíme běžící smyčku, pokud existuje
            _foodService.Reset();
            _scoreService.Reset();
            
            CurrentSnake = new Snake();
            CurrentGameBoard = new GameBoard();
            _foodService.SpawnFood(CurrentGameBoard, CurrentSnake);
            
            _gameStateService.SetMenu();
            _growBuffer = 0;
            
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
            _ = StartGameLoop();
            NotifyStateChanged();
        }

        /// <summary>
        /// Zastaví herní smyčku a přepne stav hry na "Paused". Pokud je hra již pozastavena, nebude mít žádný efekt.
        /// </summary>
        public void PauseGame()
        {
            if (_gameStateService.IsPlaying()) {
                StopGameLoop();
                _gameStateService.SetPaused();
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Spustí herní smyčku a přepne stav hry zpět na "Playing". Pokud hra není v stavu "Paused", nebude mít žádný efekt.
        /// </summary>
        public void ResumeGame()
        {
            if (_gameStateService.IsPaused()) {
                _ = StartGameLoop();
                _gameStateService.SetPlaying();
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Ukončí hru: zastaví herní smyčku a přepne stav hry na "GameOver".
        /// </summary>
        public void GameOver()
        {
            StopGameLoop();
            _gameStateService.SetGameOver();
            NotifyStateChanged();
        }

        /// <summary>
        /// Spustí herní smyčku asynchronně s aktuální rychlostí hry. 
        /// Tato metoda zajišťuje, že logika hry bude aktualizována v pravidelných intervalech, které se mohou měnit v závislosti na úrovni a počtu dokončených úrovní.
        /// </summary>
        private Task StartGameLoop() => _gameLoopService.StartAsync(GameLogicTickAsync, _currentGameSpeed);

        /// <summary>
        /// Zastaví herní smyčku a zruší všechny plánované aktualizace stavu hry. 
        /// Tato metoda je volána při pozastavení hry, ukončení hry nebo načtení nové hry, aby se zajistilo, že nebudou probíhat žádné další aktualizace stavu, dokud nebude explicitně znovu spuštěna.
        /// </summary>
        private void StopGameLoop() => _gameLoopService.Stop();

        /// <summary>
        /// Metoda obsahující hlavní logiku hry, která se vykonává při každém "ticku" herní smyčky.
        /// </summary>
        /// <returns> Vrací Task, protože je volána asynchronně z GameLoopService, ale v současné implementaci neprovádí žádné asynchronní operace, takže vrací dokončený Task.</returns>
        private Task GameLogicTickAsync()
        {
            _directionChangedInCurrentTick = false;

            // podmínka pro pohyb při kolizi s jídlem
            if (CurrentFoodItem != null && CurrentSnake.GetNextHeadPosition() == CurrentFoodItem.Position) {
                OnFoodEaten();
            } else if (_growBuffer > 0){
                CurrentSnake.Move(grow: true);
                _growBuffer--;
            } else {
                CurrentSnake.Move();
            }

            // podmínka pro kolizi s překážkami, zdmi či sebou samým
            if (_collisionService.IsCollisionDetected(CurrentGameBoard, CurrentSnake)) {
                GameOver();
                return Task.CompletedTask;
            }

            NotifyStateChanged();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Zpracovává událost, kdy had sní položku jídla, a odpovídajícím způsobem aktualizuje stav hry.
        /// </summary>
        /// <remarks>Tato metoda zvětšuje délku hada, aktualizuje hráčovo skóre a spravuje postup hrou, jako je například otevírání bran při splnění určitých podmínek.</remarks>
        private void OnFoodEaten()
        {
            _growBuffer = GameSettings.GrowthPerFood;
            CurrentSnake.Move(grow: true);

            var score = _foodService.EatCurrentFood(CurrentGameBoard, CurrentSnake);
            _scoreService.Increase(score);
        }

        /// <summary>
        /// Změní směr hada na zadaný směr, pokud nový směr není přímo opačný k aktuálnímu směru nebo pokud již v rámci aktuálního herního kroku nedošlo ke změně směru.
        /// </summary>
        /// <remarks>Tato metoda zajišťuje, že se had nemůže otočit přímo do protisměru (například nahoru a hned dolů) a že je v rámci jednoho herního kroku povolena pouze jedna změna směru.
        /// Pokus o vícenásobnou změnu směru během stejného kroku nebo o změnu do neplatného směru nebude mít žádný vliv.</remarks>
        /// <param name="newDirection">Směr, který se má hadovi nastavit. Tato hodnota nesmí být opačná k aktuálnímu směru.</param>
        public void ChangeDirection(Direction newDirection)
        {
            if (_directionChangedInCurrentTick) {
                    return;
            }
                if (IsOppositeDirection(CurrentSnake.CurrentDirection, newDirection)) {
                return; 
            }
            CurrentSnake.CurrentDirection = newDirection;
            _directionChangedInCurrentTick = true;
        }

        /// <summary>
        /// Vrátí true, pokud jsou dva zadané směry (Enum.Direction) opačné
        /// (například Up a Down nebo Left a Right), jinak vrací false.
        /// </summary>
        /// <returns>True, pokud jsou směry opačné, jinak false.</returns>
        private static bool IsOppositeDirection(Direction dir1, Direction dir2)
        {
            return (dir1 == Direction.Up && dir2 == Direction.Down) ||
                   (dir1 == Direction.Down && dir2 == Direction.Up) ||
                   (dir1 == Direction.Left && dir2 == Direction.Right) ||
                   (dir1 == Direction.Right && dir2 == Direction.Left);
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

            if (CurrentFoodItem != null && CurrentFoodItem.Position == p) {
                return CurrentFoodItem.ScoreValue == GameSettings.BonusFoodScoreValue ? "cell food-bonus" : "cell food";
            }

            return "cell empty";
        }

        public void Dispose()
        {
            StopGameLoop();
            _gameLoopService.Dispose();
        }
    }
}
