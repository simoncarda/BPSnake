using BP_Snake.Configuration;
using BP_Snake.Services;

namespace BP_Snake.Models.GameCore
{
    /// <summary>
    /// Poskytuje základní funkcionalitu pro správu stavu hry, včetně zpracování vstupu hráče, postupu hrou a interakcí mezi herními prvky.
    /// </summary>
    /// <remarks>
    /// Třída GameEngine je zodpovědná za inicializaci a řízení herní smyčky, správu aktuálního stavu hry a zprostředkování interakcí mezi hadem,
    /// položkami jídla a herní plochou. Zpracovává také přechody mezi stavy hry, jako je spuštění, pozastavení, obnovení a ukončení hry.
    /// Tato třída publikuje události pro aktualizaci uživatelského rozhraní (UI) a podporuje uvolňování prostředků prostřednictvím rozhraní IDisposable.
    /// </remarks>
    internal class GameEngine(GameLoopService gameLoopService, CollisionService collisionService, FoodService foodService, LevelService levelService) : IDisposable
    {
        // Konstanty

        // Stav hry
        public Snake CurrentSnake { get; set; } = null!;
        public GameBoard CurrentGameBoard { get; set; } = null!;
        public FoodItem CurrentFoodItem { get; set; } = null!;
        public int CurrentGameScore { get; set; } = 0;
        public int CurrentLevel { get; set; } = 1;
        public int TotalLevelsCompleted { get; private set; } = 0; // Počet úrovní dokončených během aktuální hry
        public DateTime GameOverTime { get; private set; } = DateTime.MinValue; // Čas, kdy došlo k Game Over
        public GameState CurrentGameState { get; private set; } 

        // Služby
        private readonly GameLoopService _gameLoopService = gameLoopService;
        private readonly CollisionService _collisionService = collisionService;
        private readonly FoodService _foodService = foodService;
        private readonly LevelService _levelService = levelService;

        // UDÁLOST: aktualizace UI
        public event Func<Task>? OnStateChangedAsync;

        // Pomocné proměnné pro řízení logiky hry
        private int _currentGameSpeed
        {
            get {
                return Math.Max(GameSettings.BaseGameSpeed - (TotalLevelsCompleted * GameSettings.GameSpeedIncreasePerLevel), GameSettings.MinGameSpeed);
            }
        } // Aktuální rychlost hry, která se může měnit s postupem úrovní
        private bool _directionChangedInCurrentTick = false; // Pomocná proměnná pro zamezení více změn směru během jednoho ticku
        private int _applesEatenInLevel = 0;
        private int _growBuffer = 0; // Počet kroků, po které se had bude zvětšovat (po snězení jídla)

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
            StopGameLoop(); // Pro jistotu zastavíme běžící smyčku, pokud existuje
            CurrentLevel = 1;
            CurrentGameScore = 0;
            CurrentSnake = new Snake();
            CurrentGameBoard = new GameBoard(CurrentLevel);
            CreateFoodItem();
            _applesEatenInLevel = 0;
            CurrentGameState = GameState.Menu;
            _growBuffer = 0;
            TotalLevelsCompleted = 0;
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
            _ = StartGameLoop(); // Spustíme novou smyčku, discardujeme vrácený Task, protože nechceme čekat na jeho dokončení
            NotifyStateChanged();
        }

        /// <summary>
        /// Zastaví herní smyčku a přepne stav hry na "Paused". Pokud je hra již pozastavena, nebude mít žádný efekt.
        /// </summary>
        public void PauseGame()
        {
            if (CurrentGameState == GameState.Playing) {
                StopGameLoop(); // Zastavíme timer (cancel task)
                CurrentGameState = GameState.Paused;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Spustí herní smyčku a přepne stav hry zpět na "Playing". Pokud hra není v stavu "Paused", nebude mít žádný efekt.
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentGameState == GameState.Paused) {
                _ = StartGameLoop(); // Znovu vytvoříme timer a spustíme smyčku, discardujeme vrácený Task, protože nechceme čekat na jeho dokončení
                CurrentGameState = GameState.Playing;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Ukončí hru, zastaví herní smyčku a přepne stav hry na "GameOver". Uloží čas, kdy došlo k Game Over, pro pozdější zobrazení v UI.
        /// </summary>
        public void GameOver()
        {
            StopGameLoop(); // Zastavíme smyčku
            GameOverTime = DateTime.Now;
            CurrentGameState = GameState.GameOver;
            NotifyStateChanged();
        }

        /// <summary>
        /// Spustí herní smyčku asynchronně s aktuální rychlostí hry. 
        /// Tato metoda zajišťuje, že logika hry bude aktualizována v pravidelných intervalech, které se mohou měnit v závislosti na úrovni a počtu dokončených úrovní.
        /// </summary>
        private Task StartGameLoop()
        {
            return _gameLoopService.StartAsync(GameLogicTickAsync, _currentGameSpeed);
        }

        /// <summary>
        /// Zastaví herní smyčku a zruší všechny plánované aktualizace stavu hry. 
        /// Tato metoda je volána při pozastavení hry, ukončení hry nebo načtení nové hry, aby se zajistilo, že nebudou probíhat žádné další aktualizace stavu, dokud nebude explicitně znovu spuštěna.
        /// </summary>
        private void StopGameLoop()
        {
            _gameLoopService.Stop();
        }

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

            // podmínka pro dosažení brány a přechod na další úroveň
            if (CurrentGameBoard.IsGateOpen && CurrentSnake.Body[0] == CurrentGameBoard.GatePosition) {
                LoadNextLevel();
            }

            NotifyStateChanged();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Pokračuje hru na další úroveň, resetuje relevantní herní stav a inicializuje herní desku a hada pro novou úroveň.
        /// </summary>
        /// <remarks>Tato metoda zvyšuje počet dokončených úrovní, načítá další úroveň pomocí služby úrovní (level service), vynuluje počet snědených jablek v aktuální úrovni a restartuje herní smyčku. 
        /// Měla by být volána ve chvíli, kdy hráč dokončí úroveň, aby bylo zajištěno, že stav hry bude správně aktualizován pro další fázi.</remarks>
        private void LoadNextLevel()
        {
            TotalLevelsCompleted++;
            CurrentLevel = _levelService.GetNextLevel(CurrentLevel);
            _applesEatenInLevel = 0;
            CurrentGameBoard = new(CurrentLevel);
            CurrentSnake = new Snake();
            CreateFoodItem();
            RestartGameLoop();
        }

        /// <summary>
        /// Zastaví aktuální herní smyčku a spustí novou s aktualizovanou rychlostí, pokud je hra stále v stavu "Playing".
        /// </summary>
        private void RestartGameLoop()
        {
            StopGameLoop();
            if (CurrentGameState == GameState.Playing) {
                _ = StartGameLoop();
            } // Spustíme novou smyčku, discardujeme vrácený Task, protože nechceme čekat na jeho dokončení
        }

        /// <summary>
        /// Vytvoří a přiřadí novou položku jídla na aktuální herní plochu na základě aktuálního stavu hada.
        /// </summary>
        /// <remarks>Tato metoda využívá službu jídla (food service) ke generování položky jídla, která je umístěna na herní plochu.
        /// Měla by být volána pokaždé, když je potřeba vygenerovat nové jídlo – například poté, co bylo zkonzumováno to předchozí.</remarks>
        private void CreateFoodItem()
        {
            CurrentFoodItem = _foodService.CreateFoodItem(CurrentGameBoard, CurrentSnake);
        }

        /// <summary>
        /// Zpracovává událost, kdy had sní položku jídla, a odpovídajícím způsobem aktualizuje stav hry.
        /// </summary>
        /// <remarks>Tato metoda zvětšuje délku hada, aktualizuje hráčovo skóre a spravuje postup hrou, jako je například otevírání bran při splnění určitých podmínek.
        /// Dále určuje a nastavuje další položku jídla, která se má objevit na herní ploše.</remarks>
        private void OnFoodEaten()
        {
            _growBuffer = GameSettings.GrowthPerFood;
            CurrentSnake.Move(grow: true);

            FoodEatenResult result = _foodService.HandleFoodEaten(CurrentGameBoard, CurrentSnake, CurrentFoodItem, _applesEatenInLevel);
            CurrentGameScore += result.ScoreValue;
            _applesEatenInLevel = result.ApplesEatenInLevel;

            if (result.ShouldOpenGate) {
                CurrentGameBoard.OpenGate();
            }

            CurrentFoodItem = result.NextFoodItem;
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
                return; // Zabránit více změnám směru během jednoho ticku
            }
            // Zabránit otočení hada o 180 stupňů
            if (IsOppositeDirection(CurrentSnake.CurrentDirection, newDirection)) 
            {
                return; 
            }
            CurrentSnake.CurrentDirection = newDirection;
            _directionChangedInCurrentTick = true;
        }

        /// <summary>
        /// Vrátí true, pokud jsou dva zadané směry (Enum.Direction) opačné
        /// (například Up a Down nebo Left a Right), jinak vrací false.
        /// </summary>
        /// <returns>True, pokud jsou směry stejné, jinak false.</returns>
        private static bool IsOppositeDirection(Direction dir1, Direction dir2)
        {
            return (dir1 == Direction.Up && dir2 == Direction.Down) ||
                   (dir1 == Direction.Down && dir2 == Direction.Up) ||
                   (dir1 == Direction.Left && dir2 == Direction.Right) ||
                   (dir1 == Direction.Right && dir2 == Direction.Left);
        }

        public void Dispose()
        {
            StopGameLoop(); // Zajistíme, že smyčka bude zastavena a všechny zdroje uvolněny
            _gameLoopService.Dispose();
        }
    }
}
