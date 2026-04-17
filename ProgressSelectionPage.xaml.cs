using System;
using System.Linq;
using HabitFlow.Models;

namespace HabitFlow;

public partial class ProgressSelectionPage : ContentPage
{
    public ProgressSelectionPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var habits = await App.Database.GetHabitsAsync();

        var summaries = habits
            .GroupBy(h => h.Name)
            .Select(g =>
            {
                var last = g.OrderByDescending(x => x.Date).First();
                return new HabitProgressSummary
                {
                    Name = g.Key,
                    Count = g.Count(),
                    LastStartTime = last.StartTime,
                    LastFinishTime = last.FinishTime,
                    LastDate = last.Date
                };
            })
            .OrderBy(s => s.Name)
            .ToList();

        HabitsCollectionView.ItemsSource = summaries;
    }

    // ?? Called when user taps any habit row
    private async void OnHabitTapped(object sender, TappedEventArgs e)
    {
        var summary = e.Parameter as HabitProgressSummary;
        if (summary == null)
            return;

        await Navigation.PushAsync(new ProgressDetailPage(summary));
    }

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
