using BPSnake.Models.GameCore;
using BPSnake.Models;

namespace BPSnake.Services
{
    /// <summary>
    /// Služba pro správu celkového stavu herní instance.
    /// Demonstruje návrhový vzor State Machine - drží informaci o tom, co se s hrou aktuálně děje (Menu, Hraní, Pozastaveno, Konec)
    /// a umožňuje čisté oddělení stavu od herní logiky (Enginu).
    /// </summary>
    internal sealed class GameStateService
    {
        // Vlastnosti lze měnit pouze zevnitř pomocí vyhrazených metod (princip zapouzdření)
        public GameState CurrentState { get; private set; } = GameState.Menu;
        public DateTime GameOverTime { get; private set; } = DateTime.MinValue;

        public void SetMenu()
        {
            CurrentState = GameState.Menu;
        }

        public void SetPlaying()
        {
            CurrentState = GameState.Playing;
        }

        public void SetPaused()
        {
            CurrentState = GameState.Paused;
        }

        public void SetGameOver()
        {
            CurrentState = GameState.GameOver;
            GameOverTime = DateTime.Now;
        }

        public bool IsPlaying() => CurrentState == GameState.Playing;
        public bool IsPaused() => CurrentState == GameState.Paused;
    }
}
