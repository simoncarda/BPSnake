using BPSnake.Models;
using BPSnake.Models.GameCore;
using BPSnake.Configuration;
using BPSnake.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using GridPoint = BPSnake.Models.GameCore.GridPoint;

namespace BPSnake.Components.Pages
{
    public partial class GamePage
    {
        [Inject] private GameEngine _engine { get; set; } = default!;

        private bool _showTouchControls = false;

        protected override void OnInitialized()
        {
            // Zobrazí ovládací tlačítka, pokud jsme na iOS nebo Android 
            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS) {
                _showTouchControls = true;
            } else {
                _showTouchControls = false;
            }

            _engine.OnStateChangedAsync += RefreshUIAsync;
            _engine.LoadNewGame();
        }

        /// <summary>
        /// Spustí novou herní relaci a resetuje stav hry.
        /// </summary>
        /// <remarks>
        /// Tato metoda vymaže veškeré předchozí chybové zprávy, resetuje indikátor stavu uložení
        /// a po inicializaci nové hry asynchronně načte žebříček nejlepších hráčů (leaderboard).
        /// </remarks>
        /// <returns>Úloha (task), která reprezentuje asynchronní operaci.</returns>
        private void StartNewGame()
        {
            _engine.StartNewGame();
        }
        private void PauseGame()
        {
            _engine.PauseGame();
        }
        private void ResumeGame()
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
            var p = new GridPoint(x, y);

            if (_engine.CurrentSnake.Body.Count > 0 && _engine.CurrentSnake.Body[0] == p) {
                return "cell snake-head";
            }

            if (_engine.CurrentSnake.Body.Contains(p)) {
                return "cell snake-body";
            }

            if (_engine.CurrentFoodItem != null && _engine.CurrentFoodItem.Position == p) {
                return _engine.CurrentFoodItem.ScoreValue == GameSettings.BonusFoodScoreValue ? "cell food-bonus" : "cell food";
            }

            if (_engine.CurrentGameBoard.GatePosition == p) {
                return _engine.CurrentGameBoard.IsGateOpen ? "cell gate-open" : "cell gate-closed";
            }

            if (_engine.CurrentGameBoard.Obstacles.Contains(p)) {
                return "cell wall";
            }

            return "cell empty";
        }
    }
}
