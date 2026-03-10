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
        [Inject] private IGameEngine _engine { get; set; } = default!;

        private bool _showTouchControls = false;

        protected override async Task OnInitializedAsync()
        {
            // Ukázka síly Blazor Hybrid: Přístup k nativním vlastnostem zařízení (DeviceInfo z .NET MAUI) 
            // přímo z UI logiky pro podmíněné zobrazení dotykového ovládání na mobilech.
            _showTouchControls = DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS;

            _engine.OnStateChangedAsync += RefreshUIAsync;
            _engine.LoadNewGame();
        }

        /// <summary>
        /// Předání povelu ke startu hry enginu.
        /// </summary>
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
        /// Zpracovává vstup z klávesnice.
        /// Využívá moderní C# "switch expression" k mapování kláves na herní směry.
        /// </summary>
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