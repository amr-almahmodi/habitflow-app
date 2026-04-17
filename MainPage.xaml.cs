using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using HabitFlow.Models;
using HabitFlow.Data;

namespace HabitFlow
{
    public partial class MainPage : ContentPage
    {
        // small view-model used for upcoming + all lists
        private class HabitCard
        {
            public Habit Habit { get; set; } = null!;
            public DateTime DisplayDate { get; set; }
            public string IconSource { get; set; } = string.Empty;
            public string WhenText { get; set; } = string.Empty;
        }

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadHabitsForHomeAsync();
        }

        // --------------------------------------------------------------------
        //  LOAD DATA FOR HOME PAGE
        // --------------------------------------------------------------------
        private async Task LoadHabitsForHomeAsync()
        {
            var all = await App.Database.GetHabitsAsync();
            if (all == null || all.Count == 0)
            {
                CurrentHabitCard.IsVisible = false;
                UpcomingHabitsView.ItemsSource = null;
                AllHabitsView.ItemsSource = null;
                return;
            }

            var today = DateTime.Today;

            // Build HabitCard list: compute next date, icon, and text
            var cards = new List<HabitCard>();
            foreach (var h in all)
            {
                var nextDate = ComputeNextOccurrenceDate(h, today);
                var icon = GetIconForHabitName(h.Name);
                var whenText = BuildWhenText(nextDate, h.StartTime, h.FinishTime);

                cards.Add(new HabitCard
                {
                    Habit = h,
                    DisplayDate = nextDate,
                    IconSource = icon,
                    WhenText = whenText
                });
            }

            // sort by next date then start time
            cards = cards
                .OrderBy(c => c.DisplayDate)
                .ThenBy(c => ParseTime(c.Habit.StartTime))
                .ToList();

            // CURRENT habit = first one whose DisplayDate is today, otherwise earliest
            HabitCard currentCard =
                cards.FirstOrDefault(c => c.DisplayDate.Date == today)
                ?? cards.First();

            SetCurrentHabitCard(currentCard);

            // UPCOMING = all cards except current, and only future / today
            var upcoming = cards
                .Where(c => c.Habit.Id != currentCard.Habit.Id &&
                            c.DisplayDate.Date >= today)
                .ToList();
            UpcomingHabitsView.ItemsSource = upcoming;

            // ALL HABITS list at bottom
            AllHabitsView.ItemsSource = cards;
        }

        // decide date that should be shown for this habit
        private DateTime ComputeNextOccurrenceDate(Habit habit, DateTime fromDate)
        {
            // One-time habit (no DaysOfWeek): just use its stored date
            if (string.IsNullOrWhiteSpace(habit.DaysOfWeek))
            {
                return habit.Date.Date;
            }

            // weekly habit with one or more days (Mon,Wed,Fri)
            var daysTokens = habit.DaysOfWeek
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var set = new HashSet<DayOfWeek>();
            foreach (var token in daysTokens)
            {
                switch (token.ToLower())
                {
                    case "mon": set.Add(DayOfWeek.Monday); break;
                    case "tue":
                    case "tues": set.Add(DayOfWeek.Tuesday); break;
                    case "wed": set.Add(DayOfWeek.Wednesday); break;
                    case "thu":
                    case "thur":
                    case "thurs": set.Add(DayOfWeek.Thursday); break;
                    case "fri": set.Add(DayOfWeek.Friday); break;
                    case "sat": set.Add(DayOfWeek.Saturday); break;
                    case "sun": set.Add(DayOfWeek.Sunday); break;
                }
            }

            if (set.Count == 0)
            {
                // fallback: treat as one-time
                return habit.Date.Date;
            }

            // search the next 14 days to be safe
            for (int i = 0; i < 14; i++)
            {
                var d = fromDate.Date.AddDays(i);
                if (set.Contains(d.DayOfWeek))
                    return d;
            }

            // fallback
            return fromDate.Date;
        }

        // "From 4:00 PM - 5:00 PM on Today / Tomorrow / 01/12/2025"
        private string BuildWhenText(DateTime date, string startTime, string finishTime)
        {
            var today = DateTime.Today;
            string dayText;

            if (date.Date == today)
                dayText = "Today";
            else if (date.Date == today.AddDays(1))
                dayText = "Tomorrow";
            else
                dayText = date.ToString("dd/MM/yyyy");

            return $"From {startTime} - {finishTime} on {dayText}";
        }

        private TimeSpan ParseTime(string timeStr)
        {
            return TimeSpan.TryParse(timeStr, out var ts) ? ts : TimeSpan.Zero;
        }

        // Map habit name -> icon image file
        private string GetIconForHabitName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "default_habit.png"; // optional default

            var n = name.Trim().ToLower();

            return n switch
            {
                "football" or "footbal" or "footBall" => "icon_football.png",
                "swimming" => "icon_swimming.png",
                "read" or "reading" => "icon_read.png",
                "gym" => "icon_gym.png",
                "walking" or "walk" => "icon_walking.png",
                "cook" or "cooking" => "icon_cook.png",
                _ => "default_habit.png"
            };
        }

        private void SetCurrentHabitCard(HabitCard card)
        {
            if (card == null || card.Habit == null)
            {
                CurrentHabitCard.IsVisible = false;
                CurrentHabitDoneButton.CommandParameter = null;
                CurrentHabitSkipButton.CommandParameter = null;
                return;
            }

            var habit = card.Habit;

            CurrentHabitCard.IsVisible = true;
            CurrentHabitNameLabel.Text = habit.Name;

            if (string.IsNullOrWhiteSpace(habit.DaysOfWeek))
                CurrentHabitDaysLabel.Text = "Days: One-time";
            else
                CurrentHabitDaysLabel.Text = $"Days: {habit.DaysOfWeek}";

            CurrentHabitTimeLabel.Text = card.WhenText;

            // set icon image
            CurrentHabitIcon.Source = card.IconSource;

            // so handlers know which habit is current
            CurrentHabitDoneButton.CommandParameter = habit;
            CurrentHabitSkipButton.CommandParameter = habit;
        }

        // --------------------------------------------------------------------
        //  DONE / SKIP for CURRENT HABIT
        // --------------------------------------------------------------------
        private async void OnCurrentDoneClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Habit habit)
            {
                await App.Database.SaveHistoryAsync(new HabitHistory
                {
                    HabitName = habit.Name,
                    Date = DateTime.Now,
                    Status = "Done"
                });

                await App.Database.DeleteHabitAsync(habit);
                await LoadHabitsForHomeAsync();
            }
        }

        private async void OnCurrentSkipClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Habit habit)
            {
                await App.Database.SaveHistoryAsync(new HabitHistory
                {
                    HabitName = habit.Name,
                    Date = DateTime.Now,
                    Status = "Missed"
                });

                await App.Database.DeleteHabitAsync(habit);
                await LoadHabitsForHomeAsync();
            }
        }

        // --------------------------------------------------------------------
        //  DONE / SKIP for UPCOMING HABITS (each card)
        // --------------------------------------------------------------------
        private async void OnUpcomingDoneClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Habit habit)
            {
                await App.Database.SaveHistoryAsync(new HabitHistory
                {
                    HabitName = habit.Name,
                    Date = DateTime.Now,
                    Status = "Done"
                });

                await App.Database.DeleteHabitAsync(habit);
                await LoadHabitsForHomeAsync();
            }
        }

        private async void OnUpcomingSkipClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Habit habit)
            {
                await App.Database.SaveHistoryAsync(new HabitHistory
                {
                    HabitName = habit.Name,
                    Date = DateTime.Now,
                    Status = "Missed"
                });

                await App.Database.DeleteHabitAsync(habit);
                await LoadHabitsForHomeAsync();
            }
        }

        // --------------------------------------------------------------------
        //  NAVIGATION HANDLERS
        // --------------------------------------------------------------------
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
