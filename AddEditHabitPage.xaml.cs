using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using HabitFlow.Models;

namespace HabitFlow
{
    public partial class AddEditHabitPage : ContentPage
    {
        private readonly Habit _editingHabit;

        // keeps old calls like new AddEditHabitPage("Football") working
        public AddEditHabitPage(string habitName)
            : this(new Habit
            {
                Name = habitName,
                Date = DateTime.Today
            })
        {
        }

        public AddEditHabitPage(Habit habit = null)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            _editingHabit = habit;

            if (_editingHabit != null)
            {
                // existing habit (edit mode)
                HabitNameEntry.Text = _editingHabit.Name;

                // load times into TimePicker (if they were already saved as strings)
                if (!string.IsNullOrWhiteSpace(_editingHabit.StartTime) &&
                    DateTime.TryParse(_editingHabit.StartTime, out var startDt))
                {
                    StartTimePicker.Time = startDt.TimeOfDay;
                }

                if (!string.IsNullOrWhiteSpace(_editingHabit.FinishTime) &&
                    DateTime.TryParse(_editingHabit.FinishTime, out var finishDt))
                {
                    FinishTimePicker.Time = finishDt.TimeOfDay;
                }

                // load selected days
                LoadSelectedDays(_editingHabit.DaysOfWeek);
            }
            else
            {
                // new habit – give some default times (4–5 pm)
                StartTimePicker.Time = new TimeSpan(16, 0, 0);
                FinishTimePicker.Time = new TimeSpan(17, 0, 0);
            }
        }

        // ---------------- repeat days helpers ----------------

        private void LoadSelectedDays(string days)
        {
            if (string.IsNullOrWhiteSpace(days))
                return;

            var list = days
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim().ToLower())
                .ToList();

            MonCheck.IsChecked = list.Contains("mon");
            TueCheck.IsChecked = list.Contains("tue");
            WedCheck.IsChecked = list.Contains("wed");
            ThuCheck.IsChecked = list.Contains("thu");
            FriCheck.IsChecked = list.Contains("fri");
            SatCheck.IsChecked = list.Contains("sat");
            SunCheck.IsChecked = list.Contains("sun");
        }

        private string GetSelectedDays()
        {
            var days = new List<string>();

            if (MonCheck.IsChecked) days.Add("Mon");
            if (TueCheck.IsChecked) days.Add("Tue");
            if (WedCheck.IsChecked) days.Add("Wed");
            if (ThuCheck.IsChecked) days.Add("Thu");
            if (FriCheck.IsChecked) days.Add("Fri");
            if (SatCheck.IsChecked) days.Add("Sat");
            if (SunCheck.IsChecked) days.Add("Sun");

            return string.Join(",", days);
        }

        // ---------------- SAVE / CANCEL ----------------

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var name = HabitNameEntry.Text?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                await DisplayAlert("Error", "Please enter a habit name.", "OK");
                return;
            }

            var habit = _editingHabit ?? new Habit();

            habit.Name = name;

            // convert TimePicker values to formatted time strings
            habit.StartTime = (DateTime.Today + StartTimePicker.Time).ToString("h:mm tt");
            habit.FinishTime = (DateTime.Today + FinishTimePicker.Time).ToString("h:mm tt");

            // repeat days
            habit.DaysOfWeek = GetSelectedDays();

            // make sure Date is set (used as created/scheduled date)
            if (habit.Date == DateTime.MinValue)
                habit.Date = DateTime.Today;

            await App.Database.SaveHabitAsync(habit);
            await Navigation.PopAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // ---------------- NAVIGATION BAR HANDLERS ----------------

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
