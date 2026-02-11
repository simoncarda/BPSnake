using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    public enum SaveResult
    {
        InsertedNew,      // Nový hráč
        UpdatedHighScore, // Existující hráč překonal rekord
        ScoreTooLow       // Skóre bylo nižší než rekord (neuloženo)
    }
    public enum GameState
    {
        Playing,
        GameOver,
        Paused
    }
}
