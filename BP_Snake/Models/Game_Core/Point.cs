using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.Game_Core
{
    /// <summary>
    /// Struktura pro reprezentaci bodu v 2D prostoru, která obsahuje souřadnice X a Y.
    /// </summary>
    public record struct Point(int X, int Y);
}
