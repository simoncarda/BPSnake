using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace BP_Snake.Models.Data_Layer
{
    [Table("Scores")]
    internal class GameScore
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Collation("NOCASE")]
        public string PlayerName { get; set; } = "";
        public int Score { get; set; }
    }
}
