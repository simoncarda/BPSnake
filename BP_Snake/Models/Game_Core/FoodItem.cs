using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.Game_Core
{
    internal class FoodItem
    {
        public FoodItem(Point position, int scoreValue)
        {
            Position = position;
            ScoreValue = scoreValue;
        }
        public FoodItem()
        {
            Position = new Point(10, 10);
            ScoreValue = 5;
        }

        public Point Position { get; private set; }
        public int ScoreValue { get; private set; }
    }
}
