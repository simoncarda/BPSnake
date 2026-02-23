
namespace BPSnake.Configuration
{
    internal static class GameSettings
    {
        public const int MaxPlayerNameLength = 10; // Maximální délka jména hráče pro zobrazení ve skóre

        public const int BonusFoodScoreValue = 10; // Počet bodů získaných za snězení bonusového jídla
        public const int NormalFoodScoreValue = 5; // Počet bodů získaných za snězení normálního jídla

        public const int GrowthPerFood = 4; // Počet kroků, o které se had zvětší po snězení jednoho jídla
        public const int BaseGameSpeed = 200; // Základní rychlost hry v milisekundách (počet ms mezi jednotlivými aktualizacemi stavu hry)
        public const int GameSpeedIncreasePerLevel = 20; // O kolik se zrychlí hra s každou dokončenou úrovní (v ms)
        public const int MinGameSpeed = 80; // Minimální rychlost hry (nejrychlejší), aby hra zůstala hratelná

        public const int GameBoardWidth = 20; // Šířka herní plochy v počtu políček
        public const int GameBoardHeight = 20; // Výška herní plochy v počtu políček

        public const int LevelCount = 12; // Celkový počet úrovní v hře
        public const int StartingLevel = 1; // Počáteční úroveň, na které hráč začíná

        public const int InitialSnakeLength = 10; // Počáteční délka hada na začátku každé úrovně

        public const int ApplesToOpenGate = 5; // Počet snědených jablek potřebných k otevření brány do další úrovně
    }
}
