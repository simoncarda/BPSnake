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
        /// <summary>
        /// Asynchronní SQLite připojení používané pro všechny operace.
        /// </summary>
        private SQLiteAsyncConnection _database;
        private const string DbName = "SnakeGame.db";

        /// <summary>
        /// Inicializuje SQLite připojení a vytvoří tabulku <see cref="GameScore"/>, pokud ještě neexistuje.
        /// Metoda je bezpečná při opakovaném volání - pokud je připojení již vytvořeno, okamžitě se vrátí.
        /// </summary>
        private async Task Init()
        {
            if (_database != null) {
                return;
            }

            // Cesta k databázi 
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);

            File.Delete(dbPath);

            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<GameScore>();
        }

        /// <summary>
        /// Uloží skóre hráče do databáze.
        /// </summary>
        /// <returns>
        /// <see cref="SaveResult"/> indikující výsledek operace:
        /// - <c>InsertedNew</c> pokud byl vložen nový záznam,
        /// - <c>UpdatedHighScore</c> pokud byl aktualizován stávající záznam na vyšší skóre,
        /// - <c>ScoreTooLow</c> pokud existující skóre je vyšší nebo rovné novému skóre.
        /// </returns>
        /// <remarks>
        /// Pokud již existuje záznam se stejným jménem hráče, provede se porovnání skóre a případná aktualizace.
        /// </remarks>
        public async Task<SaveResult> SavePlayerScoreAsync(GameScore scoreData)
        {
            await Init();
            GameScore existingPlayer = await _database.Table<GameScore>()
                                                      .Where(s => s.PlayerName == scoreData.PlayerName)
                                                      .FirstOrDefaultAsync();

            // Pokud hráč se zadaným jménem existuje, update jeho score (pokud je vyšší, než předchozí), jinak vytvoř nového hráče v tabulce
            if (existingPlayer != null) {
                if (existingPlayer.Score < scoreData.Score) {
                    existingPlayer.Score = scoreData.Score;
                    existingPlayer.DateTimeAchieved = scoreData.DateTimeAchieved;
                    existingPlayer.TotalLevelsCompleted = scoreData.TotalLevelsCompleted;
                    await _database.UpdateAsync(existingPlayer);
                    return SaveResult.UpdatedHighScore;
                }
                return SaveResult.ScoreTooLow;
            } else {
                await _database.InsertAsync(scoreData);
                return SaveResult.InsertedNew;
            }
        }

        /// <summary>
        /// Vrací seznam nejlepších výsledků seřazených sestupně podle skóre.
        /// </summary>
        /// <returns>Seznam až 10 nejlepších <see cref="GameScore"/> záznamů.</returns>
        public async Task<List<GameScore>> GetScoresAsync()
        {
            await Init();

            // SQL: SELECT * FROM Scores ORDER BY Score DESC LIMIT 10
            return await _database.Table<GameScore>()
                                  .OrderByDescending(s => s.Score)
                                  .Take(10)
                                  .ToListAsync();
        }
        /// <summary>
        /// Odstraní všechny záznamy skóre z tabulky.
        /// </summary>
        public async Task ClearAllScores()
        {
            await Init();
            await _database.DeleteAllAsync<GameScore>();
        }
    }
}
