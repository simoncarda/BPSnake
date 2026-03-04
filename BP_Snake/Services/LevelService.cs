using BPSnake.Configuration;


namespace BPSnake.Services
{
    /// <summary>
    /// Jednoduchá služba spravující logiku přechodu mezi úrovněmi.
    /// Zde je centralizované pravidlo, kdy se přejde na další level a jaké jsou hranice.
    /// Aktuálně hra obsahuje 12 úrovní, po dosažení 12. úrovně se přechází zpět na 1. úroveň, což umožňuje nekonečné hraní s opakováním úrovní.
    /// </summary>
    internal sealed class LevelService
    {
        public int CurrentLevel { get; private set; } = 1;
        public int TotalLevelsCompleted { get; private set; } = 0;

        public void Reset()
        {
            CurrentLevel = 1;
            TotalLevelsCompleted = 0;
        }

        public void MoveToNextLevel()
        {
            TotalLevelsCompleted++;
            CurrentLevel = CurrentLevel >= GameSettings.LevelCount ? GameSettings.StartingLevel : CurrentLevel + 1;
        }
    }
}
