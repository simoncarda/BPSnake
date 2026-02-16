using BP_Snake.Models;
using BP_Snake.Models.Data_Layer;
using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Services
{
    internal interface IDatabaseService
    {
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
        Task ClearAllScores();
    }
}
