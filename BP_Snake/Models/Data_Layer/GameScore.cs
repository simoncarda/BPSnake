using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace BP_Snake.Models.Data_Layer
{
    /// <summary>
    /// Reprezentuje záznam o skóre hráče ve hře, včetně jména hráče, dosaženého skóre,
    /// počtu dokončených úrovní a data a času, kdy bylo skóre dosaženo.
    /// </summary>
    /// <remarks>
    /// Tato třída se obvykle používá k ukládání a načítání hráčských skóre z databáze.
    /// Vlastnost PlayerName nerozlišuje velká a malá písmena.
    /// Vlastnost Id slouží jako primární klíč a je automaticky inkrementována.
    /// </remarks>
    [Table("Scores")]
    public class GameScore
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Collation("NOCASE")]
        public string PlayerName { get; set; } = "";
        public int Score { get; set; }
        public int TotalLevelsCompleted { get; set; }
        public DateTime DateTimeAchieved { get; set; }
    }
}
