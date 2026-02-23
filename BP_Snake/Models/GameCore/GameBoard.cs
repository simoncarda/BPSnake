

using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje konfigurovatelnou herní plochu pro danou úroveň, včetně jejích rozměrů, překážek a stavu brány.
    /// </summary>
    /// <remarks>
    /// Třída GameBoard se inicializuje se specifickou konfigurací úrovně, což umožňuje dynamické nastavení velikosti plochy, rozmístění překážek a správu brány.
    /// Poskytuje metody pro konfiguraci plochy pro danou úroveň a pro ovládání stavu brány.
    /// Tato třída je určena k tomu, aby sloužila jako hlavní reprezentace herní oblasti pro každou úroveň.
    /// </remarks>
    internal class GameBoard
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public GameBoard()
        {
            SetupLevel(); // Načtení specifické úrovně podle indexu
        }

        public void SetupLevel()
        {
            Width = GameSettings.GameBoardWidth;
            Height = GameSettings.GameBoardHeight;
        }
    }
}
