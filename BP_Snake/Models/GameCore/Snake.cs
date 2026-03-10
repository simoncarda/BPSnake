using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje hada v klasické hře „had“.
    /// </summary>
    internal class Snake
    {
        public List<GridPoint> Body { get; set; } = new();
        public Snake()
        {
            for (int i = 0; i < GameSettings.InitialSnakeLength; i++) {
                Body.Add(new GridPoint(GameSettings.InitialSnakeStartX - i, GameSettings.InitialSnakeStartY));
            }
        }
    }
}
