using System;
using SQLite;

namespace HabitFlow.Models
{
    public class Habit
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Habit name
        [SQLite.MaxLength(100)]
        public string Name { get; set; }

        // Store times as simple strings (e.g., "4:00 PM")
        public string StartTime { get; set; }
        public string FinishTime { get; set; }

        // Original date field (can be used as created / base date)
        public DateTime Date { get; set; }

        // New: which days the habit repeats on, e.g. "Mon,Wed,Fri"
        public string DaysOfWeek { get; set; }

        // New: icon path (we don't store it in the DB table)
        [Ignore]
        public string IconSource { get; set; }

        // New: computed next occurrence date (NOT stored in DB)
        [Ignore]
        public DateTime NextDate { get; set; }

        // New: friendly label for that date: "Today", "Tomorrow", or "01/12/2025"
        [Ignore]
        public string NextDisplayDateLabel { get; set; }
    }
}
