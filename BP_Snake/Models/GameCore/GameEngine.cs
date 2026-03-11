using BPSnake.Configuration;
using BPSnake.Services;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Poskytuje základní funkcionalitu pro správu stavu hry, včetně zpracování vstupu hráče, postupu hrou a interakcí mezi herními prvky.
    /// </summary>
    /// <remarks>
    /// Třída GameEngine je zodpovědná za inicializaci a řízení herní smyčky a správu aktuálního stavu hry. 
    /// Zpracovává také přechody mezi stavy hry, jako je spuštění, pozastavení a obnovení hry.
    /// Tato třída publikuje události pro aktualizaci uživatelského rozhraní (UI) a podporuje uvolňování prostředků prostřednictvím rozhraní IDisposable.
    /// </remarks>
    internal class GameEngine(
        GameLoopService gameLoopService,
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
        private readonly FoodService _foodService = foodService;
        private readonly GameStateService _gameStateService = gameStateService;
        private readonly ScoreService _scoreService = scoreService;

        // UDÁLOST: aktualizace UI
        public event Func<Task>? OnStateChangedAsync;

        private int _currentGameSpeed => GameSettings.BaseGameSpeed;
        // Kritická proměnná chránící před tzv. "fast-click bugem", kdy uživatel stiskne 
        // 2 klávesy rychle za sebou a had by se mohl otočit do sebe dřív, než dojde k vykreslení.
        private bool _directionChangedInCurrentTick = false;
        // Vyrovnávací paměť pro růst hada. Umožňuje hadovi vyrůst o více než 1 článek plynule
        // během několika následujících ticků (kroků).
        private int _growBuffer = 0;

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
        /// Pozastaví hru. Zastaví Timer běžící na pozadí a přepne stav hry na "Paused". Pokud je hra již pozastavena, nebude mít žádný efekt.
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
        /// Spustí herní smyčku a přepne stav hry zpět na "Playing". Pokud hra není ve stavu "Paused", nebude mít žádný efekt.
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
        private Task GameLogicTickAsync()
        {
            _directionChangedInCurrentTick = false;

            // DETEKCE KOLIZE (Collision Detection):
            // Zjišťujeme, zda se budoucí pozice hlavy překrývá s pozicí jídla ještě dříve, než hada reálně posuneme.
            if (CurrentFoodItem != null && CurrentSnake.GetNextHeadPosition() == CurrentFoodItem.Position) {
                OnFoodEaten();
            }

            if (_growBuffer > 0){
                // Zpracování postupné konzumace (růstu). Had roste "plynule" s každým tickem, dokud se buffer nevyprázdní.
                CurrentSnake.Move(grow: true);
                _growBuffer--;
            } else {
                // Běžný pohyb bez růstu
                CurrentSnake.Move();
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
            // Naplníme buffer, aby had v následujících krocích plynule vyrostl
            _growBuffer += GameSettings.GrowthPerFood;

            var score = _foodService.EatCurrentFood(CurrentGameBoard, CurrentSnake);
            _scoreService.Increase(score);
        }

        /// <summary>
        /// Změní směr hada na zadaný směr, pokud nový směr není přímo opačný k aktuálnímu směru nebo pokud již v rámci aktuálního herního kroku nedošlo ke změně směru.
        /// </summary>
        /// <remarks>Tato metoda zajišťuje, že se had nemůže otočit přímo do protisměru (například nahoru a hned dolů) a že je v rámci jednoho herního kroku povolena pouze jedna změna směru.
        /// Pokus o vícenásobnou změnu směru během stejného kroku nebo o změnu do neplatného směru nebude mít žádný vliv.</remarks>
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

            // Rozlišení běžného a bonusového jídla pro CSS stylování na základě hodnoty skóre
            if (CurrentFoodItem != null && CurrentFoodItem.Position == p) {
                return CurrentFoodItem.ScoreValue == GameSettings.BonusFoodScoreValue ? "cell food-bonus" : "cell food";
            }

            return "cell empty";
        }

        /// <summary>
        /// Uvolnění prostředků. Zásadní je zastavit běžící smyčku, 
        /// jinak by na pozadí pokračoval "zombie proces" i po opuštění hry.
        /// </summary>
        public void Dispose()
        {
            StopGameLoop();
            _gameLoopService.Dispose();
        }
    }
}
