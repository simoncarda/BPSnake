using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje konfigurační nastavení pro herní úroveň, včetně jejích rozměrů, překážek a pozice brány.
    /// </summary>
    /// <remarks>
    /// Tato třída slouží k definování vlastností úrovně ve hře, jako je číslo úrovně, pozice překážek a rozměry dané úrovně.
    /// Vlastnost Obstacles obsahuje seznam bodů, které reprezentují umístění překážek v rámci úrovně.
    /// </remarks>
    internal class LevelConfig
    {
        public int LevelNumber { get; set; }
        public List<GridPoint> Obstacles { get; set; } = new List<GridPoint>();
        public GridPoint GatePosition { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
