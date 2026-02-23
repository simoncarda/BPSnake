using BPSnake.Models;
using BPSnake.Models.GameCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using GridPoint = BPSnake.Models.GameCore.GridPoint;

namespace BPSnake.Components.Pages
{
    public partial class GamePage
    {
        [Inject] private GameEngine _engine { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {

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

        private Task RefreshUIAsync()
        {
            // Přepne vykonávání kódu zpět do hlavního UI vlákna
            return InvokeAsync(() => StateHasChanged());
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

            return "cell empty";
        }
    }
}
