using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using HabitFlow.Models;

namespace HabitFlow
{
    public partial class ProgressDetailPage : ContentPage
    {
        private readonly HabitProgressSummary _summary;
        private const int targetPerWeek = 3; // default goal: 3 times per week (for monthly chart)

        public ProgressDetailPage(HabitProgressSummary summary)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            _summary = summary;

            if (_summary != null)
            {
                HabitNameHeaderLabel.Text = _summary.Name;
                HabitNameLabel.Text = $"Habit: {_summary.Name}";
                CountLabel.Text = $"Times logged: {_summary.Count}";
                TimeLabel.Text =
                    $"Last time: {_summary.LastStartTime} - {_summary.LastFinishTime} on {_summary.LastDate:dd/MM/yyyy}";
            }

            // load everything (records list + weekly schedule + monthly chart)
            _ = LoadProgressAsync();
        }

        // ------------------------------------------------------------------------
        // LOAD RECORDS + WEEKLY SCHEDULE + MONTHLY CHART
        // ------------------------------------------------------------------------
        private async Task LoadProgressAsync()
        {
            var allHabits = await App.Database.GetHabitsAsync();

            if (_summary == null || allHabits == null || !allHabits.Any())
            {
                SetAllWeekdayLabelsUnchecked();
                SetAllWeekBars(0.0);
                RecordsCollectionView.ItemsSource = null;
                return;
            }

            // All records that belong to this habit name
            var habitRecords = allHabits
                .Where(h => string.Equals(h.Name, _summary.Name, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(h => h.Date)
                .ToList();

            if (!habitRecords.Any())
            {
                SetAllWeekdayLabelsUnchecked();
                SetAllWeekBars(0.0);
                RecordsCollectionView.ItemsSource = null;
                return;
            }

            // Bind the saved records list (for the Edit/Delete cards)
            RecordsCollectionView.ItemsSource = habitRecords;

            // Use the latest record for header + chart reference
            var latest = habitRecords.First();

            HabitNameHeaderLabel.Text = _summary.Name;
            HabitNameLabel.Text = $"Habit: {_summary.Name}";
            CountLabel.Text = $"Times logged: {_summary.Count}";
            TimeLabel.Text =
                $"Last time: {latest.StartTime} - {latest.FinishTime} on {latest.Date:dd/MM/yyyy}";

            // --------------------------------------------------------------------
            // WEEKLY SCHEDULE  (shows the days that the habit is scheduled on)
            // --------------------------------------------------------------------
            // Collect all day codes from DaysOfWeek (e.g., "Mon,Wed,Fri")
            var scheduledCodes = new HashSet<string>(
                habitRecords
                    .SelectMany(h =>
                        (h.DaysOfWeek ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(d => d.Trim()),
                StringComparer.OrdinalIgnoreCase);

            SetAllWeekdayLabelsUnchecked();

            // mark scheduled days with green tick
            if (scheduledCodes.Contains("Mon"))
                SetWeekdayLabelChecked(MonLabel, "Monday");
            if (scheduledCodes.Contains("Tue"))
                SetWeekdayLabelChecked(TueLabel, "Tuesday");
            if (scheduledCodes.Contains("Wed"))
                SetWeekdayLabelChecked(WedLabel, "Wednesday");
            if (scheduledCodes.Contains("Thu"))
                SetWeekdayLabelChecked(ThuLabel, "Thursday");
            if (scheduledCodes.Contains("Fri"))
                SetWeekdayLabelChecked(FriLabel, "Friday");
            if (scheduledCodes.Contains("Sat"))
                SetWeekdayLabelChecked(SatLabel, "Saturday");
            if (scheduledCodes.Contains("Sun"))
                SetWeekdayLabelChecked(SunLabel, "Sunday");

            // --------------------------------------------------------------------
            // MONTHLY CHART  (still based on how many times user logged habit)
            // --------------------------------------------------------------------
            DateTime monthDate = latest.Date;
            var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);

            var weekStarts = new List<DateTime>();
            for (int i = 0; i < 4; i++)
                weekStarts.Add(monthStart.AddDays(i * 7));

            var counts = new int[4];
            for (int i = 0; i < 4; i++)
            {
                var wStart = weekStarts[i];
                var wEnd = (i < 3)
                    ? weekStarts[i].AddDays(6)
                    : monthStart.AddMonths(1).AddDays(-1); // last bucket to month end

                counts[i] = habitRecords.Count(h =>
                    h.Date.Date >= wStart.Date && h.Date.Date <= wEnd.Date);
            }

            double[] percents = new double[4];
            for (int i = 0; i < 4; i++)
                percents[i] = Math.Min(counts[i] / (double)targetPerWeek, 1.0);

            SetWeekBars(percents);
        }

        // ------------------------------------------------------------------------
        // EDIT / DELETE buttons in Saved Records list
        // ------------------------------------------------------------------------
        private async void OnEditHabitClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Habit habit)
            {
                await Navigation.PushAsync(new AddEditHabitPage(habit));
            }
        }

        private async void OnDeleteHabitClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Habit habit)
            {
                bool confirm = await DisplayAlert(
                    "Delete Habit",
                    $"Delete {habit.Name} (scheduled on {habit.DaysOfWeek})?",
                    "Yes", "No");

                if (confirm)
                {
                    await App.Database.DeleteHabitAsync(habit);
                    await LoadProgressAsync();
                }
            }
        }

        // (Optional: if you still have a View button in XAML, you can keep this;
        // if not, it's harmless but unused.)
        private async void OnRecordViewClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Habit record)
            {
                HabitNameLabel.Text = $"Habit: {record.Name}";
                CountLabel.Text = $"Times logged: {_summary.Count}";
                TimeLabel.Text =
                    $"Last time: {record.StartTime} - {record.FinishTime} on {record.Date:dd/MM/yyyy}";
            }
        }

        // ------------------------------------------------------------------------
        // WEEKDAY LABEL HELPERS
        // ------------------------------------------------------------------------
        private void SetAllWeekdayLabelsUnchecked()
        {
            SetWeekdayLabelUnchecked(MonLabel, "Monday");
            SetWeekdayLabelUnchecked(TueLabel, "Tuesday");
            SetWeekdayLabelUnchecked(WedLabel, "Wednesday");
            SetWeekdayLabelUnchecked(ThuLabel, "Thursday");
            SetWeekdayLabelUnchecked(FriLabel, "Friday");
            SetWeekdayLabelUnchecked(SatLabel, "Saturday");
            SetWeekdayLabelUnchecked(SunLabel, "Sunday");
        }

        private void SetWeekdayLabelChecked(Label label, string weekdayName)
        {
            label.Text = $"✔ {weekdayName}";
            label.TextColor = Color.FromRgb(46, 125, 50); // green
        }

        private void SetWeekdayLabelUnchecked(Label label, string weekdayName)
        {
            label.Text = $"? {weekdayName}";
            label.TextColor = Color.FromRgb(120, 120, 120); // grey
        }

        // ------------------------------------------------------------------------
        // MONTHLY CHART HELPERS
        // ------------------------------------------------------------------------
        private void SetAllWeekBars(double percent)
        {
            SetWeekBarWidth(Week1Bar, percent);
            SetWeekBarWidth(Week2Bar, percent);
            SetWeekBarWidth(Week3Bar, percent);
            SetWeekBarWidth(Week4Bar, percent);
        }

        private void SetWeekBars(double[] percents)
        {
            double fullWidth = 220; // adjust for layout if needed
            SetWeekBarWidth(Week1Bar, percents[0], fullWidth);
            SetWeekBarWidth(Week2Bar, percents[1], fullWidth);
            SetWeekBarWidth(Week3Bar, percents[2], fullWidth);
            SetWeekBarWidth(Week4Bar, percents[3], fullWidth);
        }

        private void SetWeekBarWidth(BoxView bar, double percent, double fullWidth)
        {
            bar.Color = Color.FromRgb(46, 125, 50); // green
            bar.WidthRequest = (int)Math.Round(fullWidth * percent);
            bar.HeightRequest = 18;
        }

        private void SetWeekBarWidth(BoxView bar, double percent)
        {
            SetWeekBarWidth(bar, percent, 220);
        }

        // ------------------------------------------------------------------------
        // UTIL
        // ------------------------------------------------------------------------
        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.Date.AddDays(-1 * diff).Date;
        }

        // ------------------------------------------------------------------------
        // NAVIGATION BUTTONS
        // ------------------------------------------------------------------------
        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnAboutTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AboutPage());
        }

        private async void OnHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

        private async void OnMenuTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MenuListPage());
        }
    }
}
