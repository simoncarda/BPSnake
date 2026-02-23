
namespace BPSnake.Configuration
{
    internal static class GameSettings
    {
        public const int BonusFoodScoreValue = 10; // Počet bodů získaných za snězení bonusového jídla
        public const int NormalFoodScoreValue = 5; // Počet bodů získaných za snězení normálního jídla

        public const int GrowthPerFood = 4; // Počet kroků, o které se had zvětší po snězení jednoho jídla
        public const int BaseGameSpeed = 200; // Základní rychlost hry v milisekundách (počet ms mezi jednotlivými aktualizacemi stavu hry)

        public const int GameBoardWidth = 20; // Šířka herní plochy v počtu políček
        public const int GameBoardHeight = 20; // Výška herní plochy v počtu políček

        public const int InitialSnakeLength = 10; // Počáteční délka hada na začátku každé úrovně
    }
}
