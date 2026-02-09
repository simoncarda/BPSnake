using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.Game_Core
{
    internal class Snake
    {
        public Snake()
        {
            Body.Add(new Point(3, 1)); // Hlava hada
            Body.Add(new Point(2, 1));
            Body.Add(new Point(1, 1));
            CurrentDirection = Direction.Right; // Výchozí směr pohybu doprava
        }

        public List<Point> Body { get; set; } = new();
        public Direction CurrentDirection { get; set; }

        public Point GetNextHeadPosition()
        {
            Point head = Body[0]; // Aktuální pozice hlavy hada
            Point nextPosition = head; // Proměnná pro uložení další pozice hlavy, výchozí hodnota je aktuální pozice hlavy

            switch (CurrentDirection)
            {
                case Direction.Up:
                    nextPosition = new Point(head.X, head.Y - 1);
                    break;
                case Direction.Down:
                    nextPosition = new Point(head.X, head.Y + 1);
                    break;
                case Direction.Left:
                    nextPosition = new Point(head.X - 1, head.Y);
                    break;
                case Direction.Right:
                    nextPosition = new Point(head.X + 1, head.Y);
                    break;
            }

            return nextPosition;
        }

        // Metoda pro pohyb hada, s možností růstu
        public void Move(bool grow = false)
        {
            Point nextHeadPosition = GetNextHeadPosition();
            Body.Insert(0, nextHeadPosition); // Přidání nové hlavy na začátek seznamu těla
            if (!grow)
            {
                Body.RemoveAt(Body.Count - 1);
            }
        }

        // Metoda pro detekci kolize aktuální pozice hlavy hada s vlastním tělem
        public bool IsSelfCollision()
        {
            Point head = Body[0];
            for (int i = 1; i < Body.Count; i++) // Přeskočí hlavu v seznamu
            {
                if (Body[i] == head)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
