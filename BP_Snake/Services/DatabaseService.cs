using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BPSnake.Models;
using BPSnake.Models.DataLayer;
using SQLite;

namespace BPSnake.Services
{
    /// <summary>
    /// Poskytuje jednoduchou asynchronní službu pro práci s lokální SQLite databází hry Snake.
    /// Pro výuku je služba jednoduchá, centralizuje přístup k SQLite.
    /// </summary>
    internal class DatabaseService : IDatabaseService
    {
        public event Action? ScoresChanged;

        /// <summary>
        /// Asynchronní SQLite připojení používané pro všechny operace.
        /// </summary>
        private SQLiteAsyncConnection? _database;
        private const string DbName = "SnakeGame.db";

        // Synchronizace inicializace
        private readonly SemaphoreSlim _initSemaphore = new(1, 1);
        private bool _initialized;

        /// <summary>
        /// Asynchronně inicializuje SQLite připojení a vytvoří tabulku <see cref="GameScore"/>, pokud ještě neexistuje.
        /// Bezpečné pro opakované a současné volání z více vláken.
        /// </summary>
        private async Task InitAsync()
        {
            if (_initialized) {
                return;
            }

            await _initSemaphore.WaitAsync();
            try {
                // Druhá kontrola uvnitř kritické sekce (pro případ, že jiné vlákno inicializaci právě dokončilo)
                if (_initialized) {
                    return;
                }

                string dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);
                _database = new SQLiteAsyncConnection(dbPath);
                await _database.CreateTableAsync<GameScore>();

                _initialized = true;
            }
            finally {
                _initSemaphore.Release();
            }
        }

        /// <summary>
        /// Uloží skóre do databáze. Pokud hráč již existuje, aktualizuje záznam pouze tehdy, 
        /// pokud je nové skóre vyšší než to dosavadní.
        /// </summary>
        public async Task<SaveResult> SavePlayerScoreAsync(GameScore scoreData)
        {
            await InitAsync().ConfigureAwait(false);

            // Vyhledání existujícího hráče podle jména
            var existingPlayer = await _database!
                                       .Table<GameScore>()
                                       .Where(s => s.PlayerName == scoreData.PlayerName)
                                       .FirstOrDefaultAsync();

            if (existingPlayer != null) {
                if (existingPlayer.Score < scoreData.Score) {
                    existingPlayer.Score = scoreData.Score;
                    existingPlayer.DateTimeAchieved = scoreData.DateTimeAchieved;
                    existingPlayer.TotalLevelsCompleted = scoreData.TotalLevelsCompleted;
                    await _database!.UpdateAsync(existingPlayer);
                    ScoresChanged?.Invoke();
                    return SaveResult.UpdatedHighScore;
                }
                return SaveResult.ScoreTooLow;
            } else {
                await _database!.InsertAsync(scoreData);
                ScoresChanged?.Invoke();
                return SaveResult.InsertedNew;
            }
        }

        /// <summary>
        /// Vrací seznam nejlepších výsledků seřazených sestupně podle skóre.
        /// </summary>
        /// <returns>Seznam až 10 nejlepších <see cref="GameScore"/> záznamů.</returns>
        public async Task<List<GameScore>> GetScoresAsync()
        {
            await InitAsync().ConfigureAwait(false);

            return await _database!
                         .Table<GameScore>()
                         .OrderByDescending(s => s.Score)
                         .Take(10)
                         .ToListAsync();
        }

        /// <summary>
        /// Odstraní všechny záznamy skóre z tabulky.
        /// </summary>
        public async Task ClearAllScoresAsync()
        {
            await InitAsync();
            await _database!.DeleteAllAsync<GameScore>();
            ScoresChanged?.Invoke();
        }
    }
}
