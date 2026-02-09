using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.Game_Core
{
    internal class GameBoard
    {
        public GameBoard(int levelIndex = 1)
        {
            SetupLevel(LevelData.GetLevel(levelIndex)); // Načtení specifické úrovně podle indexu
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Point> Obstacles { get; private set;} = new List<Point>();
        public Point GatePosition { get; private set; }
        public bool IsGateOpen { get; private set; } = false;

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
