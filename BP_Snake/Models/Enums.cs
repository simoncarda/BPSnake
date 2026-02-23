using System;
using System.Collections.Generic;
using System.Text;

namespace BPSnake.Models
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    public enum GameState
    {
        Playing,
        GameOver,
        Paused,
        Menu
    }
}
