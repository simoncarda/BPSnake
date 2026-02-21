using SQLite;
using BP_Snake.Configuration;

namespace BP_Snake.Models.DataLayer
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
