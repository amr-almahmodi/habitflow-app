using System;
using Microsoft.Maui.Controls;

namespace HabitFlow
{
    public partial class MenuListPage : ContentPage
    {
        public MenuListPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        // ---------------- TOP BAR ----------------
        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnAboutTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AboutPage());
        }

        // ---------------- MAIN BUTTONS ----------------
        private async void OnHabitsListClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HabitsListPage());
        }

        private async void OnProgressClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProgressSelectionPage());
        }

        private async void OnHistoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistoryPage());
        }

        // (Reminders handler removed)

        // ---------------- BOTTOM NAV ----------------
        private async void OnHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

        private async void OnMenuTapped(object sender, EventArgs e)
        {
            // already on menu; keep for consistency
        }
    }
}
