

using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje konfigurovatelnou herní plochu.
    /// </summary>
    internal class GameBoard
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public GameBoard()
        {
            SetupLevel();
        }

        public void SetupLevel()
        {
            Width = GameSettings.GameBoardWidth;
            Height = GameSettings.GameBoardHeight;
        }
    }
}
