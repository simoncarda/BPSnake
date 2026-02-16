using BP_Snake.Components.GamePageComponents;
using BP_Snake.Models;
using BP_Snake.Models.DataLayer;
using BP_Snake.Models.GameCore;
using BP_Snake.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Components.Pages
{
    public partial class GamePage1
    {
        private GameEngine _engine = new GameEngine();

        // proměnné pro interakci s DB
        [Inject] private IDatabaseService DbService { get; set; } = default!;
        private List<GameScore> _highScores = new List<GameScore>();
        private string _playerName = "";
        private string _errorMessage = "";
        private bool _isSaveSuccess = false;
        private bool _showTouchControls = false;

        protected override async Task OnInitializedAsync()
        {
            // Zobrazí ovládací tlačítka, pokud jsme na iOS nebo Android 
            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS) {
                _showTouchControls = true;
            } else {
                _showTouchControls = false;
            }

            _engine.OnStateChangedAsync += RefreshUIAsync;
            _engine.LoadNewGame();

            await LoadLeaderboard();
        }

        /// <summary>
        /// Spustí novou herní relaci a resetuje stav hry.
        /// </summary>
        /// <remarks>
        /// Tato metoda vymaže veškeré předchozí chybové zprávy, resetuje indikátor stavu uložení
        /// a po inicializaci nové hry asynchronně načte žebříček nejlepších hráčů (leaderboard).
        /// </remarks>
        /// <returns>Úloha (task), která reprezentuje asynchronní operaci.</returns>
        private async Task StartNewGame()
        {
            _errorMessage = "";
            _isSaveSuccess = false;
            _engine.StartNewGame();
            await LoadLeaderboard();
        }
        private async Task PauseGame()
        {
            _engine.PauseGame();
        }
        private async Task ResumeGame()
        {
            _engine.ResumeGame();
        }

        private Task RefreshUIAsync()
        {
            // Přepne vykonávání kódu zpět do hlavního UI vlákna
            return InvokeAsync(() => StateHasChanged());
        }

        /// <summary>
        /// Zpracovává vstup z klávesnice a aktualizuje směr v enginu na základě stisknutí šipek.
        /// </summary>
        /// <remarks>
        /// Tato metoda reaguje pouze na klávesy ArrowUp (šipka nahoru), ArrowDown (šipka dolů), ArrowLeft (šipka vlevo)
        /// a ArrowRight (šipka vpravo). Ostatní vstupy z klávesnice jsou ignorovány.
        /// </remarks>
        /// <param name="e">The <see cref="KeyboardEventArgs"/> Instance obsahující informace o události klávesnice.</param>
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

        /// <summary>
        /// Určuje název CSS třídy pro buňku na zadaných souřadnicích na základě jejího aktuálního stavu v herní mřížce.
        /// </summary>
        /// <remarks>
        /// Vrácený název css třídy odráží aktuální stav herních prvků na daných souřadnicích, včetně pozice hada, položek jídla, stavu brány a překážek.
        /// Tato metoda se obvykle používá k vykreslení herní plochy s příslušnými vizuálními prvky pro každou buňku.
        /// </remarks>
        /// <param name="x">Od nuly počítaná souřadnice X buňky v rámci herní mřížky.</param>
        /// <param name="y">Od nuly počítaná souřadnice Y buňky v rámci herní mřížky.</param>
        /// <returns>
        /// Řetězec reprezentující název CSS třídy pro danou buňku,
        /// který určuje, zda obsahuje hlavu hada, tělo hada, jídlo, bránu, zeď, nebo zda je prázdná.
        /// </returns>
        private string GetCellClass(int x, int y)
        {
            var p = new Models.GameCore.Point(x, y);

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

        /// <summary>
        /// Uloží aktuální herní skóre hráče, za předpokladu, že je skóre vyšší než nula a je zadáno platné jméno hráče.
        /// </summary>
        /// <remarks>
        /// Pokud je skóre nulové nebo je jméno hráče prázdné (či obsahuje pouze bílé znaky), nastaví se chybová zpráva a skóre se neuloží.
        /// Metoda zpracovává různé výsledky ukládání a poskytuje zpětnou vazbu o tom, zda bylo
        /// skóre úspěšně uloženo, aktualizováno, nebo vyhodnoceno jako příliš nízké pro uložení.
        /// Po uložení načte žebříček, aby odrážel nejnovější výsledky.
        /// </remarks>
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
            SaveResult result = await DbService.SavePlayerScoreAsync(scoreToSave);
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
            _highScores = await DbService.GetScoresAsync();
        }
    }
}
