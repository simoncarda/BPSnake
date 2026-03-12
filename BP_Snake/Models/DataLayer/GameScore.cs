using SQLite;
using BPSnake.Configuration;

namespace BPSnake.Models.DataLayer
{
    /// <summary>
    /// Datový model reprezentující záznam o skóre hráče v databázi SQLite.
    /// Zajišťuje validaci délky jména a definuje databázové schéma pomocí atributů.
    /// Kolace NOCASE zaručuje, že databáze při porovnávání jmen nebude rozlišovat velká a malá písmena.
    /// </summary>
    [Table("Scores")]
    public class GameScore
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _playerName = string.Empty;
        [MaxLength(GameSettings.MaxPlayerNameLength), NotNull]
        [Collation("NOCASE")]
        public string PlayerName { 
            get => _playerName; 
            set {
                ArgumentNullException.ThrowIfNull(value);
                if (value.Length > GameSettings.MaxPlayerNameLength) {
                    throw new ArgumentException($"PlayerName cannot exceed {GameSettings.MaxPlayerNameLength} characters.", nameof(PlayerName));
                }
                _playerName = value;
            }
        }
        public int Score { get; set; }
        public int TotalLevelsCompleted { get; set; }
        public DateTime DateTimeAchieved { get; set; }
    }
}
