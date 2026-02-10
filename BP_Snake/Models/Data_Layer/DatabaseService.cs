using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace BP_Snake.Models.Data_Layer
{
    internal class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        private async Task Init()
        {
            if (_database != null) {
                return;
            }

            // Cesta k databázi 
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "SnakeGame.db");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<GameScore>();
        }

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
                    await _database.UpdateAsync(existingPlayer);
                    return SaveResult.UpdatedHighScore;
                }
                return SaveResult.ScoreTooLow;
            } else {
                await _database.InsertAsync(scoreData);
                return SaveResult.InsertedNew;
            }
        }

        public async Task<List<GameScore>> GetScoresAsync()
        {
            await Init();

            // SQL: SELECT * FROM Scores ORDER BY Score DESC LIMIT 10
            return await _database.Table<GameScore>()
                                  .OrderByDescending(s => s.Score)
                                  .Take(10)
                                  .ToListAsync();
        }
        public async Task ClearAllScores()
        {
            await Init();
            await _database.DeleteAllAsync<GameScore>();
        }
        //public async Task DeleteDB()
        //{
        //    await _database.DropTableAsync<GameScore>();
        //    _database = null; // Reset connection to force reinitialization on next access
        //    await Init();
        //}
    }
}
