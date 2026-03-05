using BPSnake.Models;
using BPSnake.Models.DataLayer;
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
        [Inject] private IGameEngine _engine { get; set; } = default!;

        private bool _showTouchControls = false;

        protected override async Task OnInitializedAsync()
        {
            // Zobrazí ovládací tlačítka pouze pro mobilní platformy
            _showTouchControls = DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS;

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
        private void StartNewGame() => _engine.StartNewGame();
        private async Task PauseGameAsync()
        {
            _engine.PauseGame();
        }
        private async Task ResumeGameAsync()
        {
            _engine.ResumeGame();
        }

        // Přepne vykonávání kódu zpět do hlavního UI vlákna
        private Task RefreshUIAsync() => InvokeAsync(StateHasChanged);

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
            Direction? newDir = e.Key switch {
                "ArrowUp" => Direction.Up,
                "ArrowDown" => Direction.Down,
                "ArrowLeft" => Direction.Left,
                "ArrowRight" => Direction.Right,
                _ => null
            };

            if (newDir.HasValue) {
                _engine.ChangeDirection(newDir.Value);
            }
        }

        public void Dispose()
        {
            // Ochrana před Memory Leakem: odhlášení události, když je komponenta zničena
            if (_engine != null) {
                _engine.OnStateChangedAsync -= RefreshUIAsync;
            }
        }
    }
}
