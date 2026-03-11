using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje položku jídla ve hře, která má specifickou pozici a bodovou hodnotu.
    /// </summary>
    /// <remarks>
    /// Instance této třídy jsou po vytvoření neměnné (immutable) zvenčí, což zvyšuje bezpečnost návrhu a zabraňuje nechtěnému posunutí jídla.
    /// </remarks>
    internal class FoodItem
    {
        public FoodItem(GridPoint position, int scoreValue)
        {
            if (scoreValue < 0) throw new ArgumentOutOfRangeException(nameof(scoreValue), "ScoreValue musí být >= 0.");
            Position = position;
            ScoreValue = scoreValue;
        }
        public GridPoint Position { get; private set; }
        public int ScoreValue { get; private set; }
    }
}
