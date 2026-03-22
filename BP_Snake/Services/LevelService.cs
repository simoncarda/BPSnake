using BPSnake.Configuration;


namespace BPSnake.Services
{
    /// <summary>
    /// Spravuje logiku postupu mezi úrovněmi. 
    /// Zajišťuje cyklické opakování úrovní po dosažení jejich maximálního konfigurovaného počtu (který se nachází v GameSettings).
    /// </summary>
    internal sealed class LevelService
    {
        public int CurrentLevel { get; private set; } = GameSettings.StartingLevel;
        public int TotalLevelsCompleted { get; private set; } = 0;

        public void Reset()
        {
            CurrentLevel = GameSettings.StartingLevel;
            TotalLevelsCompleted = 0;
        }

        public void MoveToNextLevel()
        {
            TotalLevelsCompleted++;
            CurrentLevel = CurrentLevel >= GameSettings.LevelCount ? GameSettings.StartingLevel : CurrentLevel + 1;
        }
    }
}
