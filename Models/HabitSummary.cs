namespace HabitFlow.Models
{
    public class HabitProgressSummary
    {
        public string Name { get; set; }
        public int Count { get; set; }

        // Last saved info (for detail page)
        public string LastStartTime { get; set; }
        public string LastFinishTime { get; set; }
        public DateTime LastDate { get; set; }
    }
}
