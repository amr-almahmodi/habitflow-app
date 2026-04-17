using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using HabitFlow.Data;
using HabitFlow.Models;

namespace HabitFlow
{
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            var all = await App.Database.GetHistoryAsync();
            if (all == null || all.Count == 0)
            {
                HistoryCollectionView.ItemsSource = null;
                return;
            }

            var ordered = all
                .OrderByDescending(h => h.Date)
                .ToList();

            HistoryCollectionView.ItemsSource = ordered;
        }

        // ---- Navigation ----
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
