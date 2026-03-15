using System;
using System.Collections.Generic;
using System.Text;

namespace BPSnake.Models
{
    /// <summary>
    /// Směr pohybu hada, který může být Up (nahoru), Down (dolů), Left (vlevo) nebo Right (vpravo).
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    /// <summary>
    /// Výsledek pokusu o uložení skóre hráče do databáze.
    /// </summary>
    public enum SaveResult
    {
        /// <summary>
        /// Přidán nový záznam pro hráče, který ještě není v databázi, nebo aktualizován existující záznam pro hráče, který překonal svůj předchozí rekord.
        /// </summary>
        InsertedNew,
        /// <summary>
        /// Existující záznam pro hráče byl aktualizován
        /// </summary>
        UpdatedHighScore,
        /// <summary>
        ///  Existující záznam pro hráče nebyl aktualizován, protože nové skóre bylo nižší než jeho předchozí rekord (neuloženo).
        /// </summary>
        ScoreTooLow
    }
    /// <summary>
    /// Indikuje stav hry.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Hra je aktivní a had se pohybuje. V tomto stavu probíhá hlavní herní smyčka, zpracovávají se vstupy hráče, aktualizuje se pozice hada a kontrolují se kolize.
        /// </summary>
        Playing,
        /// <summary>
        /// Hra skončila. V tomto stavu se zastaví herní smyčka a zobrazí se obrazovka s výsledkem a možností začít novou hru.
        /// </summary>
        GameOver,
        /// <summary>
        /// Hra je pozastavena. V tomto stavu se zastaví herní smyčka, ale zůstane zobrazená aktuální pozice hada a herní desky. Hráč může pokračovat ve hře nebo se vrátit do menu.
        /// </summary>
        Paused,
        /// <summary>
        /// Uživatel se nachází v hlavním menu hry
        /// </summary>
        Menu
    }
}
