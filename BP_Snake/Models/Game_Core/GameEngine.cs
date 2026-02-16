using BP_Snake.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.Game_Core
{
    internal class GameEngine : IDisposable
    {
        public Snake CurrentSnake { get; set; } = new Snake();
        public GameBoard CurrentGameBoard { get; set; } = new GameBoard();
        public FoodItem CurrentFoodItem { get; set; } = new FoodItem();
        public int CurrentGameScore { get; set; } = 0;
        public int CurrentLevel { get; set; } = 1;
        public int TotalLevelsCompleted { get; private set; } = 0; // Počet úrovní dokončených během aktuální hry
        public DateTime GameOverTime { get; private set; } = DateTime.MinValue; // Čas, kdy došlo k Game Over
        public GameState CurrentGameState { get; private set; }
        private const int _baseGameSpeed = 200; // Základní rychlost hry v milisekundách (počet ms mezi jednotlivými aktualizacemi stavu hry)

        private int _currentGameSpeed {
            get {
                return Math.Max(_baseGameSpeed - (20 * TotalLevelsCompleted), 80);
            }
        } // Aktuální rychlost hry, která se může měnit s postupem úrovní
        private bool _directionChangedInCurrentTick = false; // Pomocná proměnná pro zamezení více změn směru během jednoho ticku
        private int _applesEatenInLevel = 0;
        private int _growBuffer = 0; // Počet kroků, po které se had bude zvětšovat (po snězení jídla)
        private readonly GameLoopService _gameLoopService;
        private readonly CollisionService _collisionService;
        private readonly FoodService _foodService;
        private readonly LevelService _levelService;

        // UDÁLOST: aktualizace UI
        public event Func<Task>? OnStateChangedAsync;

        public GameEngine() : this(new GameLoopService(), new CollisionService(), new FoodService(), new LevelService())
        {
        }
        internal GameEngine(GameLoopService gameLoopService, CollisionService collisionService, FoodService foodService, LevelService levelService)
        {
            _gameLoopService = gameLoopService;
            _collisionService = collisionService;
            _foodService = foodService;
            _levelService = levelService;
        }

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
            _ = OnStateChangedAsync?.Invoke();
        }
        public void StartNewGame()
        {
            LoadNewGame();
            CurrentGameState = GameState.Playing;
            _ = StartGameLoop(); // Spustíme novou smyčku, discardujeme vrácený Task, protože nechceme čekat na jeho dokončení
            _ = OnStateChangedAsync?.Invoke();
        }
        public void PauseGame()
        {
            if (CurrentGameState == GameState.Playing) {
                StopGameLoop(); // Zastavíme timer (cancel task)
                CurrentGameState = GameState.Paused;
                _ = OnStateChangedAsync?.Invoke();
            }
        }
        public void ResumeGame()
        {
            if (CurrentGameState == GameState.Paused) {
                _ = StartGameLoop(); // Znovu vytvoříme timer a spustíme smyčku, discardujeme vrácený Task, protože nechceme čekat na jeho dokončení
                CurrentGameState = GameState.Playing;
                _ = OnStateChangedAsync?.Invoke();
            }
        }
        public void GameOver()
        {
            StopGameLoop(); // Zastavíme smyčku
            GameOverTime = DateTime.Now;
            CurrentGameState = GameState.GameOver;
            _ = OnStateChangedAsync?.Invoke();
        }
        private Task StartGameLoop()
        {
            return _gameLoopService.StartAsync(GameLogicTickAsync, _currentGameSpeed);
        }
        private void StopGameLoop()
        {
            _gameLoopService.Stop();
        }

        // Metoda pro jeden tick timeru - aktualizace stavu hry
        private Task GameLogicTickAsync()
        {
            _directionChangedInCurrentTick = false;

            // podmínka pro pohyb při kolizi s jídlem
            if (CurrentSnake.GetNextHeadPosition() == CurrentFoodItem.Position) {
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

            _ = OnStateChangedAsync?.Invoke();
            return Task.CompletedTask;
        }

        // Metoda pro načtení další úrovně
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
        private void RestartGameLoop()
        {
            StopGameLoop();
            if (CurrentGameState == GameState.Playing) {
                _ = StartGameLoop();
            } // Spustíme novou smyčku, discardujeme vrácený Task, protože nechceme čekat na jeho dokončení
        }

        // Metoda pro vytvoření nového jídla s náhodnou pozicí a 20% šancí na bonusové jídlo
        private void CreateFoodItem()
        {
            CurrentFoodItem = _foodService.CreateFoodItem(CurrentGameBoard, CurrentSnake);
        }

        private void OnFoodEaten()
        {
            _growBuffer = 4;
            CurrentSnake.Move(grow: true);

            FoodEatenResult result = _foodService.HandleFoodEaten(CurrentGameBoard, CurrentSnake, CurrentFoodItem, _applesEatenInLevel);
            CurrentGameScore += result.ScoreValue;
            _applesEatenInLevel = result.ApplesEatenInLevel;

            if (result.ShouldOpenGate) {
                CurrentGameBoard.OpenGate();
            }

            CurrentFoodItem = result.NextFoodItem;
        }

        public void ChangeDirection(Direction newDirection)
        {
            if (_directionChangedInCurrentTick) {
                return; // Zabránit více změnám směru během jednoho ticku
            }
            // Zabránit otočení hada o 180 stupňů
            if ((CurrentSnake.CurrentDirection == Direction.Up && newDirection == Direction.Down) ||
                (CurrentSnake.CurrentDirection == Direction.Down && newDirection == Direction.Up) ||
                (CurrentSnake.CurrentDirection == Direction.Left && newDirection == Direction.Right) ||
                (CurrentSnake.CurrentDirection == Direction.Right && newDirection == Direction.Left)) 
            {
                return; 
            }
            CurrentSnake.CurrentDirection = newDirection;
            _directionChangedInCurrentTick = true;
        }

        public void Dispose()
        {
            StopGameLoop(); // Zajistíme, že smyčka bude zastavena a všechny zdroje uvolněny
            _gameLoopService.Dispose();
        }
    }
}
