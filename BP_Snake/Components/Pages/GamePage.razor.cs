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

        protected override async Task OnInitializedAsync()
        {
            _engine.OnStateChangedAsync += RefreshUIAsync;
            _engine.LoadNewGame();
        }

        /// <summary>
        /// Spustí novou herní relaci.
        /// </summary>
        private void StartNewGame() => _engine.StartNewGame();

        // Přepne vykonávání kódu zpět do hlavního UI vlákna
        private Task RefreshUIAsync() => InvokeAsync(StateHasChanged);

        public void Dispose()
        {
            // Ochrana před Memory Leakem: odhlášení události, když je komponenta zničena
            if (_engine != null) {
                _engine.OnStateChangedAsync -= RefreshUIAsync;
            }
        }
    }
}
