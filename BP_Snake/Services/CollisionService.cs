using BP_Snake.Models.Game_Core;
using Point = BP_Snake.Models.Game_Core.Point;

namespace BP_Snake.Services
{
    /// <summary>
    /// Služba detekující kolize (zeď, překážka, self-collision).
    /// Oddělení této logiky od GameEngine zlepšuje čitelnost a testovatelnost.
    /// </summary>
    internal sealed class CollisionService
    {
        /// <summary>
        /// Vrátí true pokud došlo ke kolizi (se zdí, překážkou nebo tělem hada).
        /// </summary>
        public bool IsCollisionDetected(GameBoard board, Snake snake)
        {
            Point currentHead = snake.Body[0];
            return IsCollisionWithWall(board, currentHead) || snake.IsSelfCollision() || IsCollisionWithObstacles(board, currentHead);
        }

        /// <summary>
        /// Kontrola kolize hlavy se zdí (hranice hrací plochy).
        /// Brána je výjimka — pokud je otevřená, nepočítá se jako kolize.
        /// </summary>
        private bool IsCollisionWithWall(GameBoard board, Point head)
        {
            if (board.IsGateOpen && head == board.GatePosition) {
                return false;
            }
            return head.X < 0 || head.X >= board.Width ||
                   head.Y < 0 || head.Y >= board.Height;
        }

        /// <summary>
        /// Kontrola kolize se statickými překážkami na mapě.
        /// </summary>
        private bool IsCollisionWithObstacles(GameBoard board, Point head)
        {
            return board.Obstacles.Contains(head);
        }
    }
}
