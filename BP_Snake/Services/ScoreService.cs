namespace BPSnake.Services
{
    /// <summary>
    /// Jednoduchá služba (State Container) pro zapouzdřenou evidenci a výpočet herního skóre jednoho hráče.
    /// Demonstruje Single Responsibility Principle (třída dělá pouze jednu věc).
    /// </summary>
    internal sealed class ScoreService
    {
        public int CurrentScore { get; private set; } = 0;

        public void Increase(int amount)
        {
            if (amount > 0) {
                CurrentScore += amount;
            }
        }

        public void Reset()
        {
            CurrentScore = 0;
        }
    }
}
