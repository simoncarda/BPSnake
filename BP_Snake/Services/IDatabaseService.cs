using System;
using BPSnake.Models;
using BPSnake.Models.DataLayer;

namespace BPSnake.Services
{
    /// <summary>
    /// Interface pro službu zodpovědnou za správu databáze SQLite.
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Event raised when leaderboard data changes (insert/update/clear).
        /// Subscribers should reload cached data when this fires.
        /// </summary>
        event Action? ScoresChanged;
        /// <summary>
        /// Uloží nebo aktualizuje skóre hráče. Vrací informaci o tom, zda byl vytvořen nový záznam,
        /// aktualizován rekord nebo nebylo skóre dostatečně vysoké.
        /// </summary>
        Task<SaveResult> SavePlayerScoreAsync(GameScore scoreData);

        /// <summary>
        /// Načte seznam skóre (top záznamy). Implementace může omezit počet vrácených záznamů.
        /// </summary>
        Task<List<GameScore>> GetScoresAsync();

        /// <summary>
        /// Odstraní všechny záznamy skóre z databáze. Uživatelsky bezpečné pro výuku.
        /// </summary>
        Task ClearAllScoresAsync();
    }
}
