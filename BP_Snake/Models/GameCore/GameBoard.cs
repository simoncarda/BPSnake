

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje konfigurovatelnou herní plochu pro danou úroveň.
    /// </summary>
    internal class GameBoard
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<GridPoint> Obstacles { get; private set;} = new List<GridPoint>();
        public GridPoint GatePosition { get; private set; }
        public bool IsGateOpen { get; private set; } = false;

        public GameBoard(int levelIndex)
        {
            SetupLevel(LevelData.GetLevel(levelIndex)); // Načtení specifické úrovně podle indexu
        }

        public void SetupLevel(LevelConfig data)
        {
            Obstacles.Clear();
            Width = data.Width;
            Height = data.Height;
            Obstacles.AddRange(data.Obstacles);
            GatePosition = data.GatePosition;
        }
        public void OpenGate()
        {
            IsGateOpen = true;
        }
    }
}
