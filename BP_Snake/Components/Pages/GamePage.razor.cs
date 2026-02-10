using BP_Snake.Models;
using BP_Snake.Models.Data_Layer;
using BP_Snake.Models.Game_Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Components.Pages
{
    public partial class GamePage
    {
        private GameEngine _engine = new GameEngine();
        private ElementReference _boardElement; // Reference na herní pole pro nastavení focusu (jinak hra nereaguje, dokud uživatel neklikne do herního pole)

        // proměnné pro interakci s DB
        private DatabaseService _dbService = new DatabaseService();
        private List<GameScore> _highScores = new List<GameScore>();
        private string _playerName = "";
        private string _errorMessage = "";
        private bool _isSaveSuccess = false;
        private bool _showTouchControls = false;

        protected override void OnInitialized()
        {
            // Zobrazí ovládací tlačítka, pokud jsme na iOS nebo Android 
            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS) { 
                _showTouchControls = true;
            } else {
                _showTouchControls = false;
            }

            _engine.OnStateChanged += RefreshUI;
            _engine.LoadNewGame();
        }

        private async Task StartGame()
        {
            _errorMessage = "";
            _isSaveSuccess = false;
            await SetFocus();
            _engine.StartGame();
            await LoadLeaderboard();
        }

        private async void RefreshUI()
        {
            // Přepne vykonávání kódu zpět do hlavního UI vlákna
            await InvokeAsync(() => StateHasChanged());
        }

        private async Task SetFocus()
        {
            await _boardElement.FocusAsync();
        }

        // Řeší vstupy z klávesnice
        private void HandleInput(KeyboardEventArgs e)
        {
            switch (e.Key) {
                case "ArrowUp":
                    _engine.ChangeDirection(Direction.Up);
                    break;
                case "ArrowDown":
                    _engine.ChangeDirection(Direction.Down);
                    break;
                case "ArrowLeft":
                    _engine.ChangeDirection(Direction.Left);
                    break;
                case "ArrowRight":
                    _engine.ChangeDirection(Direction.Right);
                    break;
            }
        }

        private string GetCellClass(int x, int y)
        {
            var p = new Models.Game_Core.Point(x, y);

            if (_engine.CurrentSnake.Body.Count > 0 && _engine.CurrentSnake.Body[0] == p) {
                return "cell snake-head";
            }

            if (_engine.CurrentSnake.Body.Contains(p)) {
                return "cell snake-body";
            }

            if (_engine.CurrentFoodItem.Position == p) {
                return _engine.CurrentFoodItem.ScoreValue >= 10 ? "cell food-bonus" : "cell food";
            }

            if (_engine.CurrentGameBoard.GatePosition == p) {
                return _engine.CurrentGameBoard.IsGateOpen ? "cell gate-open" : "cell gate-closed";
            }

            if (_engine.CurrentGameBoard.Obstacles.Contains(p)) {
                return "cell wall";
            }

            return "cell empty";
        }

        private async Task SaveScore()
        {
            if (_engine.CurrentGameScore == 0) {
                _errorMessage = "Nulové skóre se neukládá! Zkus to znovu.";
                return;
            }
            if (String.IsNullOrWhiteSpace(_playerName)) {
                _errorMessage = "Musíš zadat jméno!";
                return;
            }

            GameScore scoreToSave = new GameScore {
                PlayerName = _playerName.Trim(),
                Score = _engine.CurrentGameScore,
                TotalLevelsCompleted = _engine.TotalLevelsCompleted,
                DateTimeAchieved = _engine.GameOverTime
            };

            // Uložíme do databáze, zároveň získáme výsledek uložení
            SaveResult result = await _dbService.SavePlayerScoreAsync(scoreToSave);
            // Zpracujeme výsledek uložení
            switch (result) {
                case SaveResult.InsertedNew:
                    _errorMessage = "Vítej! Jsi zapsán v tabulce.";
                    _isSaveSuccess = true;
                    break;

                case SaveResult.UpdatedHighScore:
                    _errorMessage = "Gratulace! Překonal jsi svůj rekord!";
                    _isSaveSuccess = true;
                    break;

                case SaveResult.ScoreTooLow:
                    _errorMessage = "Bohužel, tvůj starý rekord byl lepší. Neuloženo.";
                    _isSaveSuccess = false;
                    break;
            }

            await LoadLeaderboard();
        }
        private async Task LoadLeaderboard()
        {
            _highScores = await _dbService.GetScoresAsync();
            StateHasChanged();
        }
    }
}
