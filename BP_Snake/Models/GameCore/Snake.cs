using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje hada v klasické hře „had“, spravuje segmenty jeho těla a směr pohybu.
    /// </summary>
    /// <remarks>
    /// Třída Snake poskytuje metody pro pohyb hada, zvětšování jeho délky a detekci kolizí se sebou samým.
    /// Tělo hada je reprezentováno jako seznam bodů, přičemž první bod odpovídá hlavě. Směr pohybu lze ovládat pomocí vlastnosti CurrentDirection.
    /// Tato třída je určena k použití jako jádro logiky hry a nezajišťuje přímo vykreslování ani uživatelský vstup.
    /// </remarks>
    internal class Snake
    {
        public List<GridPoint> Body { get; set; } = new();
        public Direction CurrentDirection { get; set; }
        public Snake()
        {
            for (int i = 0; i < GameSettings.InitialSnakeLength; i++) {
                Body.Add(new GridPoint(3 - i, 1)); // Vytvoření počátečního těla hada, vždy na pozici (3,1) až (0,1)
            }
            CurrentDirection = Direction.Right; // Výchozí směr pohybu doprava
        }
    }
}
