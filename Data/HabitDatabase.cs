using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using HabitFlow.Models;

namespace HabitFlow.Data
{
    public class HabitDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public HabitDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

            // main habits table
            _database.CreateTableAsync<Habit>().Wait();

            // history table (Done / Missed)
            _database.CreateTableAsync<HabitHistory>().Wait();
        }

        // -------- Habits --------

        public Task<List<Habit>> GetHabitsAsync()
        {
            return _database.Table<Habit>()
                            .ToListAsync();
        }

        public Task<int> SaveHabitAsync(Habit habit)
        {
            if (habit.Id != 0)
                return _database.UpdateAsync(habit);

            return _database.InsertAsync(habit);
        }

        public Task<int> DeleteHabitAsync(Habit habit)
        {
            return _database.DeleteAsync(habit);
        }

        public Task<int> ClearAllHabitsAsync()
        {
            return _database.DeleteAllAsync<Habit>();
        }

        // -------- History --------

        public Task<int> SaveHistoryAsync(HabitHistory history)
        {
            return _database.InsertAsync(history);
        }

        public Task<List<HabitHistory>> GetHistoryAsync()
        {
            return _database.Table<HabitHistory>()
                            .OrderByDescending(h => h.Date)
                            .ToListAsync();
        }
    }

    // History row used by HistoryPage
    public class HabitHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string HabitName { get; set; }

        // when it was done / missed
        public DateTime Date { get; set; }

        // "Done" or "Missed"
        public string Status { get; set; }
    }
}
