using BPSnake.Models.GameCore;
using GridPoint = BPSnake.Models.GameCore.GridPoint;

namespace BPSnake.Services
{
    /// <summary>
    /// Služba detekující kolize.
    /// Ukázka principu Single Responsibility Principle (SRP) – vyčlenění této logiky 
    /// mimo GameEngine udržuje kód čistý a usnadňuje psaní automatizovaných testů (Unit Tests).
    /// </summary>
    internal sealed class CollisionService
    {
        /// <summary>
        /// Vrátí true pokud došlo ke kolizi (se zdí, případnou překážkou nebo tělem hada).
        /// </summary>
        public bool IsCollisionDetected(GameBoard board, Snake snake)
        {
            GridPoint currentHead = snake.Body[0];
            return IsCollisionWithBoundary(board, currentHead) || IsCollisionWithSelf(snake) || IsCollisionWithObstacles(board, currentHead);
        }

        /// <summary>
        /// Kontrola kolize hlavy se zdí (hranice hrací plochy).
        /// </summary>
        private bool IsCollisionWithBoundary(GameBoard board, GridPoint head)
        {
            return head.X < 0 || head.X >= board.Width ||
                   head.Y < 0 || head.Y >= board.Height;
        }

        /// <summary>
        /// Detekuje kolizi hada se zbytkem vlastního těla (podmínka pro Game Over).
        /// </summary>
        /// <returns>True, pokud se hlava překrývá s jakýmkoliv jiným článkem těla.</returns>
        public bool IsCollisionWithSelf(Snake snake)
        {
            var body = snake.Body;
            GridPoint head = body[0];

            // Iterace začíná od indexu 1, čímž záměrně z porovnávání vynecháváme samotnou hlavu na pozici 0.
            for (int i = 1; i < body.Count; i++) {
                if (body[i] == head) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Kontrola kolize se statickými překážkami na mapě.
        /// </summary>
        private bool IsCollisionWithObstacles(GameBoard board, GridPoint head)
        {
            return board.Obstacles.Contains(head);
        }
    }
}
