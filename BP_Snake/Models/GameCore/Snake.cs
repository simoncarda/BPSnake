using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje hada v klasické hře „had“.
    /// </summary>
    internal class Snake
    {
        // List uchovává souřadnice jednotlivých článků. Index 0 představuje vždy hlavu.
        // Kolekce List je vhodná pro snadné vkládání (při růstu) a odebírání (při pohybu).
        public List<GridPoint> Body { get; set; } = new();
        public Snake()
        {
            // Inicializace počátečních článků těla.
            // Odečítáním 'i' se tělo generuje horizontálně za hlavou (směrem doleva).
            for (int i = 0; i < GameSettings.InitialSnakeLength; i++) {
                Body.Add(new GridPoint(GameSettings.InitialSnakeStartX - i, GameSettings.InitialSnakeStartY));
            }
        }
    }
}
