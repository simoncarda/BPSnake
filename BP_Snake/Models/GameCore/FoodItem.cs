namespace BP_Snake.Models.GameCore
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
        private const int DefaultScore = 5;
        private static readonly GridPoint DefaultPosition = new GridPoint(10, 10);
        public FoodItem(GridPoint position, int scoreValue)
        {
            if (scoreValue < 0) throw new ArgumentOutOfRangeException(nameof(scoreValue), "ScoreValue musí být >= 0.");
            Position = position;
            ScoreValue = scoreValue;
        }
        public FoodItem() : this(DefaultPosition, DefaultScore) { }

        public GridPoint Position { get; private set; }
        public int ScoreValue { get; private set; }
    }
}
