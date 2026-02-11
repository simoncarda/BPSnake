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
        private bool _directionChangedInCurrentTick = false; // Pomocná proměnná pro zamezení více změn směru během jednoho ticku
        private int _applesEatenInLevel = 0;
        private int _growBuffer = 0; // Počet kroků, po které se had bude zvětšovat (po snězení jídla)
        private Random _random = new Random();

        // Timer pro řízení rychlosti hry
        private PeriodicTimer? _gameLoopTimer;
        private CancellationTokenSource? _cts;

        // UDÁLOST: aktualizace UI
        public event Action? OnStateChanged;

        public GameEngine()
        {
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
            OnStateChanged?.Invoke();
        }
        public void StartNewGame()
        {
            LoadNewGame();
            CurrentGameState = GameState.Playing;
            _ = StartGameLoop(); // Spustíme novou smyčku, discardujeme vrácený Task, protože nechceme čekat na jeho dokončení
            OnStateChanged?.Invoke();
        }
        public void PauseGame()
        {
            if (CurrentGameState == GameState.Playing) {
                StopGameLoop(); // Zastavíme timer (cancel task)
                //_gameLoopTimer.Change(Timeout.Infinite, Timeout.Infinite); // Pozastavit hru
                CurrentGameState = GameState.Paused;
                OnStateChanged?.Invoke();
            }
        }
        public void ResumeGame()
        {
            if (CurrentGameState == GameState.Paused) {
                _ = StartGameLoop(); // Znovu vytvoříme timer a spustíme smyčku, discardujeme vrácený Task, protože nechceme čekat na jeho dokončení
                CurrentGameState = GameState.Playing;
                OnStateChanged?.Invoke();
            }
        }
        public void GameOver()
        {
            StopGameLoop(); // Zastavíme smyčku
            GameOverTime = DateTime.Now;
            CurrentGameState = GameState.GameOver;
            OnStateChanged?.Invoke();
        }
        private async Task StartGameLoop()
        {
            StopGameLoop(); // Pro jistotu zastavíme běžící smyčku, pokud existuje

            using CancellationTokenSource localCts = new CancellationTokenSource();
            using PeriodicTimer localTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(200)); // Nastavíme interval 200 ms
            _gameLoopTimer = localTimer;
            _cts = localCts;

            try 
            {
                while (await localTimer.WaitForNextTickAsync(localCts.Token)) {
                    GameLogicTick();
                }
            }
            catch (OperationCanceledException) 
            {
                // Smyčka byla zrušena, není třeba nic dělat
            }
        }
        private void StopGameLoop()
        {
            if (_cts != null) {
                _cts.Cancel(); // Zrušíme běžící smyčku
                _cts.Dispose();
                _cts = null;
            }
            if (_gameLoopTimer != null) {
                _gameLoopTimer.Dispose();
                _gameLoopTimer = null;
            }
        }

        // Metoda pro jeden tick timeru - aktualizace stavu hry
        private void GameLogicTick()
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
            if (IsCollisionDetected()) {
                GameOver();
                return;
            }

            // podmínka pro dosažení brány a přechod na další úroveň
            if (CurrentGameBoard.IsGateOpen && CurrentSnake.Body[0] == CurrentGameBoard.GatePosition) {
                LoadNextLevel();
            }

            OnStateChanged?.Invoke();
        }

        // Metoda pro načtení další úrovně
        private void LoadNextLevel()
        {
            TotalLevelsCompleted++;
            // Pokud je aktuální úroveň 4, restartujeme na úroveň 0
            if (CurrentLevel >= 4) {
                CurrentLevel = 1;
            } else {
                CurrentLevel++;
            }
            _applesEatenInLevel = 0;
            CurrentGameBoard = new(CurrentLevel);
            CurrentSnake = new Snake();
            CreateFoodItem();
        }

        // Metoda pro detekci kolize aktuální pozice hlavy hada se zdmi, překážkami nebo sebou
        private bool IsCollisionDetected()
        {
            Point currentHead = CurrentSnake.Body[0];
            return IsCollisionWithWall(currentHead) || CurrentSnake.IsSelfCollision() || IsCollisionWithObstacles(currentHead);
        }

        // Metoda pro detekci kolize pozice bodu se zdmi
        private bool IsCollisionWithWall(Point head)
        {
            if (CurrentGameBoard.IsGateOpen && head == CurrentGameBoard.GatePosition) {
                return false;
            }
            return head.X < 0 || head.X >= CurrentGameBoard.Width || 
                   head.Y < 0 || head.Y >= CurrentGameBoard.Height;
        }

        // Metoda pro detekci kolize pozice bodu (hlavy hada) s překážkami
        private bool IsCollisionWithObstacles(Point head)
        {
            if (CurrentGameBoard.Obstacles.Contains(head)) {
                return true;
            }
            return false;
        }

        // Metoda pro vytvoření nového jídla s náhodnou pozicí a 20% šancí na bonusové jídlo
        private void CreateFoodItem()
        {
            int x;
            int y;
            int value = (_random.Next(0, 10) < 8) ? 5 : 10; // 20% šance na bonusové jídlo s hodnotou 10
            do {
                x = _random.Next(0, CurrentGameBoard.Width);
                y = _random.Next(0, CurrentGameBoard.Height);
            } while (CurrentSnake.Body.Contains(new Point(x, y)) || CurrentGameBoard.Obstacles.Contains(new Point(x, y)));
            CurrentFoodItem = new FoodItem(new Point(x, y), value);
        }

        private void OnFoodEaten()
        {
            _growBuffer = 4;
            CurrentSnake.Move(grow: true);
            CurrentGameScore += CurrentFoodItem.ScoreValue;
            _applesEatenInLevel++;
            if (_applesEatenInLevel >= 5) {
                CurrentGameBoard.OpenGate();
                CurrentFoodItem = new FoodItem(new Point(-1, -1), 0); // "Skrytí" jídla
            } else {
                CreateFoodItem();
            }
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
        }
    }
}
