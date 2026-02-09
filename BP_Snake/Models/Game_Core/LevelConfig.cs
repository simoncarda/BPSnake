using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.Game_Core
{
    internal class LevelConfig
    {
        public int LevelNumber { get; set; }
        public List<Point> Obstacles { get; set; } = new List<Point>();
        public Point GatePosition { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
