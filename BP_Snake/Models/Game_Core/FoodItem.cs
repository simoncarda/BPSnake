using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.Game_Core
{
    /// <summary>
    /// Reprezentuje položku jídla ve hře, která má specifickou pozici a hodnotu skóre přidělenou při jejím snězení.
    /// </summary>
    /// <remarks>
    /// Pozice položky jídla je definována strukturou Point a hodnota skóre určuje počet bodů udělených hráči po sebrání.
    /// Výchozí konstruktor inicializuje položku jídla na pozici (10, 10) s hodnotou skóre 5.
    /// </remarks>
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
